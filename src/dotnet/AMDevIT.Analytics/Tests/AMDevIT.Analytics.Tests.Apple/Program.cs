using Foundation;
using UIKit;

namespace AMDevIT.Analytics.Tests;

internal static class Program
{
    #region Methods

    private static void Main(string[] args) => UIApplication.Main(args, null, typeof(TestAppDelegate));

    #endregion
}

[Register("AnalyticsTestAppDelegate")]
public sealed class TestAppDelegate : UIApplicationDelegate
{
    #region Properties

    public override UIWindow? Window { get; set; }

    #endregion

    #region Methods

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        UIViewController controller = new();
        UITextView output = new(UIScreen.MainScreen.Bounds) { Editable = false, Text = "Running tests…" };
        controller.View = output;
        this.Window = new UIWindow(UIScreen.MainScreen.Bounds) { RootViewController = controller };
        this.Window.MakeKeyAndVisible();
        _ = RunTestsAsync(output);
        return true;
    }

    private static async Task RunTestsAsync(UITextView output)
    {
        string report;
        try
        {
            report = await PlatformTestRunner.RunAsync<AppleTests>();
            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "test-results.txt"), report);
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
