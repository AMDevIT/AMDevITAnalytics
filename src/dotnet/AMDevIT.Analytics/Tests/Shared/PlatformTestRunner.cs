using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace AMDevIT.Analytics.Tests;

internal static class PlatformTestRunner
{
    #region Methods

    internal static async Task<string> RunAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>()
        where T : new()
    {
        StringBuilder report = new();
        int failed = 0;
        int total = 0;

        foreach (MethodInfo method in typeof(T).GetMethods().Where(method => method.IsDefined(typeof(TestMethodAttribute))).OrderBy(method => method.Name))
        {
            total++;
            try
            {
                object? result = method.Invoke(new T(), null);
                if (result is Task task) await task.WaitAsync(TimeSpan.FromSeconds(15));
                report.AppendLine($"PASS {method.Name}");
            }
            catch (Exception exception)
            {
                failed++;
                report.AppendLine($"FAIL {method.Name}: {(exception is TargetInvocationException ? exception.InnerException : exception)}");
            }
        }

        if (total == 0) throw new InvalidOperationException("No platform tests were discovered.");
        report.AppendLine($"TOTAL {total}; FAILED {failed}");
        return report.ToString();
    }

    #endregion
}
