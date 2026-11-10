using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BrunvaServer.Api.Filters;
using BrunvaServer.Api.Middleware;
using BrunvaServer.Api.Problems;
using BrunvaServer.Api.Security;
using BrunvaServer.Application.Abstractions;
using BrunvaServer.Infrastructure.Persistence;
using BrunvaServer.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBrunva(builder.Configuration);
builder.Services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddScoped<IOrganizationContext, OrganizationContext>();
builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);
builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ResultMappingFilter>();
        options.Filters.Add<OrganizationContextFilter>();
    })
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks().AddDbContextCheck<BrunvaDbContext>(tags: ["ready"]);

var auth = builder.Configuration.GetSection(AuthSettings.Section).Get<AuthSettings>() ?? new AuthSettings();
if (Encoding.UTF8.GetByteCount(auth.SigningKey) < 32)
    throw new InvalidOperationException("Jwt:SigningKey must be at least 32 bytes.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = auth.Issuer,
            ValidateAudience = true,
            ValidAudience = auth.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(auth.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "sub"
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
    options.OnRejected = async (context, cancellationToken) =>
    {
        var problem = ProblemResults.Details(
            context.HttpContext,
            StatusCodes.Status429TooManyRequests,
            "rate_limited",
            "Too many requests",
            "Try again later.");
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/problem+json";
        await context.HttpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
    };
});

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapOpenApi();
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<BrunvaDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();

public partial class Program;
