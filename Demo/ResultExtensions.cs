using FluentResults;
using static Demo.UtilityMethods;

namespace Demo;
public static class ResultExtensions
{
    public static void PrintErrorIfFailed<T>(this Result<T> result)
    {
        PrintErrorIfFailed(result.ToResult());
    }

    public static void PrintErrorIfFailed(this Result result)
    {
        if (result.IsFailed)
        {
            PrintError(result);
        }
    }
}
