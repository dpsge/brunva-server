namespace BrunvaServer.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public DateTimeOffset CreatedAt { get; protected set; }

    public DateTimeOffset UpdatedAt { get; protected set; }

    public void MarkCreated(DateTimeOffset now)
    {
        if (CreatedAt == default)
            CreatedAt = now;
        UpdatedAt = now;
    }

    public void MarkUpdated(DateTimeOffset now) => UpdatedAt = now;
}
