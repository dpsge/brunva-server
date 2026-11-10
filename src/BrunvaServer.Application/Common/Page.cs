namespace BrunvaServer.Application.Common;

public sealed record Page<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount);

public static class PageWindow
{
    public const int DefaultSize = 20;
    public const int MaxSize = 100;

    public static (int Page, int Size) Normalize(int page, int pageSize)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedSize = pageSize < 1 ? DefaultSize : pageSize > MaxSize ? MaxSize : pageSize;
        return (normalizedPage, normalizedSize);
    }
}
