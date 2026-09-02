using Android.App;
using Android.OS;
using Android.Widget;

namespace AMDevIT.Analytics.Tests;

[Activity(Label = "Analytics Tests", MainLauncher = true, Exported = true)]
public sealed class MainActivity : Activity
{
    #region Methods

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        TextView output = new(this) { Text = "Running tests…" };
        base.OnCreate(savedInstanceState);
        this.SetContentView(output);
        _ = this.RunTestsAsync(output);
    }

    private async Task RunTestsAsync(TextView output)
    {
        string report;
        try
        {
            report = await PlatformTestRunner.RunAsync<AndroidTests>();
            File.WriteAllText(Path.Combine(this.FilesDir!.AbsolutePath, "test-results.txt"), report);
        }
        catch (Exception exception)
        {
            report = $"TEST HOST FAILED: {exception}";
        }

        Console.WriteLine(report);
        output.Text = report;
    }

    #endregion
}
