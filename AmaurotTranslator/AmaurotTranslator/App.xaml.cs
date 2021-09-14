using Serilog;
using Serilog.Formatting.Compact;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace AmaurotTranslator
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        public static readonly string Birthdate = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        public static bool makeMiniDump = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetupExceptionHandling();
            InitLogger();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }

        private static void InitLogger()
        {
            Log.Logger = new LoggerConfiguration()
                            .WriteTo.File(formatter: new CompactJsonFormatter(),
                                path: $"./logs/log-{Birthdate}.txt",
                                retainedFileCountLimit: null)
                            .MinimumLevel.Debug()
                            .CreateLogger();
            Log.Debug("Logger initialized");
        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");
                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
                e.SetObserved();
            };
        }

        private void LogUnhandledException(Exception exception, string source)
        {
            SaveMiniDump(exception);
            Environment.Exit(1);
        }

        public static void SaveMiniDump(Exception exception)
        {
            if (makeMiniDump)
            {
                using (FileStream fs = new FileStream($"./logs/log-{Birthdate}.mdmp", FileMode.Create, FileAccess.ReadWrite, FileShare.Write))
                {
                    MiniDump.Write(fs.SafeFileHandle, MiniDump.Option.WithFullMemory, MiniDump.ExceptionInfo.Present);
                }
            }
            else
            {
                if (exception.InnerException != null)
                {
                    Log.Fatal("TERMINATED BY UNHANDLED EXCEPTION: {@Exception}, {@InnerException}, {@GroundZero}",
                        exception,
                        exception.InnerException,
                        exception.InnerException.StackTrace);
                }
                else
                {
                    Log.Fatal("TERMINATED BY UNHANDLED EXCEPTION: {@Exception}, {@GroundZero}",
                        exception,
                        exception.StackTrace);
                }
            }
        }
    }
}
