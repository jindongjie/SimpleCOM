using Avalonia;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace SimpleCOM
{
    internal abstract class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static int Main(string[] args)
        {
            var builder = BuildAvaloniaApp();

            if (!args.Contains("--drm")) return builder.StartWithClassicDesktopLifetime(args);
            SilenceConsole();
            return builder.StartLinuxDrm(args: args, card: GetCard(), scaling: GetScaling());

            string GetCard()
            {
                var idx = Array.IndexOf(args, "--card");
                if (idx != -1 && args.Length > idx + 1)
                {
                    return args[idx + 1];
                }
                return "/dev/dri/card0"; // 默认使用 /dev/dri/card0
            }

            double GetScaling()
            {
                var idx = Array.IndexOf(args, "--scaling");
                if (idx != 0 && args.Length > idx + 1 &&
                    double.TryParse(args[idx + 1], NumberStyles.Any, CultureInfo.InvariantCulture, out var scaling))
                    return scaling;
                return 1;
            }
        }

        private static void SilenceConsole()
        {
            new Thread(DisableConsolePrint)
            { IsBackground = true }.Start();
        }

        private static void DisableConsolePrint()
        {
            Console.CursorVisible = false;
            while (true) Console.ReadKey(true);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        private static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
