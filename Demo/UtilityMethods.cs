using FluentResults;

namespace Demo;
public static class UtilityMethods
{
    public static void PrintError(Result result)
    {
        Console.WriteLine($"Error: {string.Join(" ", result.Errors.Select(e => e.Message))}");
    }
}
