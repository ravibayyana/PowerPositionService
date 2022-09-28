namespace PowerPositionService.Utils;

public static class StringExt
{
    public static string AddThreadId(this string msg)
    {
        return $"tid:{Thread.CurrentThread.ManagedThreadId}  {msg}";
    }
}