namespace Utils.ProcessErrorHandling
{
    using System;
    using System.Diagnostics;
    public static class ProcessErrorHandler
    {
        public static void EnableErrorHandling()
        {
            Process.GetCurrentProcess().EnableRaisingEvents = true;

            Process.GetCurrentProcess().Exited += (sender, args) =>
            {
                int exitCode = Process.GetCurrentProcess().ExitCode;
                Console.WriteLine($"Process exited with code: {exitCode} time : {DateTime.Now}");
            };

            Process.GetCurrentProcess().Exited += (sender, args) =>
            {
                int exitCode = Process.GetCurrentProcess().ExitCode;
                Console.WriteLine($"Process will exit with code: {exitCode} time : {DateTime.Now}");
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = (Exception)args.ExceptionObject;
                Console.WriteLine($"Uncaught Exception: {ex.Message} time : {DateTime.Now}");
                Console.Error.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            };

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                Exception ex = args.Exception;
                Console.WriteLine($"Unhandled rejection at {args.Observed} reason: {ex.Message} at {DateTime.Now}");
                Console.Error.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            };

            CancellationTokenSource cts = new();

            Console.CancelKeyPress += (sender, args) =>
            {
                Console.WriteLine($"Process {Environment.ProcessId} has been interrupted time : {DateTime.Now}");
                args.Cancel = true;
                cts.Cancel();
            };

            using (cts.Token.Register(() =>
            {
                Console.WriteLine($"Process {Environment.ProcessId} received a SIGTERM signal time : {DateTime.Now}");
                Environment.Exit(0);
            }))
            {
                // empty...
            }
        }
    }
}