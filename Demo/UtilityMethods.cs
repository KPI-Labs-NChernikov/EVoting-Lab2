using FluentResults;

namespace Demo;
public static class UtilityMethods
{
    public static void PrintError<T>(Result<T> result)
    {
        PrintError(result.ToResult());
    }

    public static void PrintError(Result result)
    {
        Console.WriteLine($"Error: {string.Join(" ", result.Errors.Select(e => e.Message))}");
    }
}
