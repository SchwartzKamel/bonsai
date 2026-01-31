using Avalonia.Headless;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Tests;

public abstract class UIAvaloniaTestBase : IDisposable
{
    protected HeadlessUnitTestSession? Session { get; }

    protected UIAvaloniaTestBase(HeadlessUnitTestSession? session)
    {
        Session = session;
    }

    public void Dispose()
    {
    }

    protected async Task DispatchAsync(Func<Task> action)
    {
        if (Session is null) throw new InvalidOperationException("Headless session not available.");
        await Session.Dispatch(action, CancellationToken.None);
    }

    protected void Dispatch(Action action)
    {
        if (Session is null) throw new InvalidOperationException("Headless session not available.");
        Session.Dispatch(() => { action(); return Task.CompletedTask; }, CancellationToken.None).Wait();
    }
}