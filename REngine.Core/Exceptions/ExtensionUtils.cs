using System.Text;

namespace REngine.Core.Exceptions;

public static class ExtensionUtils
{
    public static string GetFullString(this Exception ex)
    {
        var err = new StringBuilder();
        err.AppendLine($"{ex.GetType().Name}: {ex.Message}");
        err.AppendLine($"StackTrace: {ex.StackTrace}");

        var innerEx = ex.InnerException;
        while (innerEx is not null)
        {
            err.AppendLine("---- INNER EXCEPTION ----");
            err.AppendLine($"{innerEx.GetType().Name}: {innerEx.Message}");
            err.AppendLine($"StackTrace: {innerEx.StackTrace}");
            innerEx = innerEx.InnerException;
        }

        return err.ToString();
    }
}