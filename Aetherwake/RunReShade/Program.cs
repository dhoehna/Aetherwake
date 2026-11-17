using Avalonia;
using Avalonia.Controls;
using System;

namespace RunReShade
{
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .SetupWithoutStarting();

            var window = new Window();
            var hwnd = window.TryGetPlatformHandle()?.Handle ?? throw new Exception("Could not get window handle");
            new ReShadeGenerator(hwnd).ApplyShaders("BaseImage\\before.png", args[0], args[1], "BaseImage\\DepthAfter.png");
        }
    }
}
