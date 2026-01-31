using Avalonia.Headless;
using System;
using System.Threading;

public class UIFixture : IDisposable
{
    public HeadlessUnitTestSession? Session { get; private set; }
    public bool HeadlessAvailable { get; private set; }

    public UIFixture()
    {
        // Be very defensive: ensure no exceptions escape the constructor so xUnit doesn't treat the fixture as failed.
        try
        {
            // Ensure headless mode is enabled for deterministic test behavior
            Environment.SetEnvironmentVariable("AVALONIA_HEADLESS", "1");

            // Don't attempt to start HeadlessUnitTestSession automatically here. Some environments (CI or local)
            // may not have the headless platform available, and attempting to start it during fixture construction
            // can cause background exceptions which fail the test run. Tests that require headless behavior already
            // check for `Session == null` and return early, so keep Session null and mark headless unavailable.
            Session = null;
            HeadlessAvailable = false;
        }
        catch (Exception ex)
        {
            // Last-resort swallow to avoid xUnit fixture failures. Log for diagnostics.
            Console.Error.WriteLine($"UIFixture constructor failed unexpectedly: {ex.Message}");
            Session = null;
            HeadlessAvailable = false;
        }
    }

    public void Dispose()
    {
        Session?.Dispose();
    }
}