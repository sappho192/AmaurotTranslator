using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Net;
using System.Diagnostics;
using Microsoft.Win32;
using OpenQA.Selenium;
using Serilog;

namespace AmaurotTranslator
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isTranslatorBusy = false;
        private bool isUIInitialized = false;

        private string sk = "ko";
        private string tk = "ja";
        private const int STATE_K2J = 1;
        private const int STATE_J2K = 2;
        private const string chromeUserKeyName = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe";
        private const string chromeLocalKeyName = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe";
        private int currentState = 0;
        private Browser browser;

        public MainWindow()
        {
            InitializeComponent();
            isUIInitialized = true;
            LoadUserSettings();
            currentState = STATE_K2J;

            // Calculate log folder size
            UpdateLogFolderSize();

            string latestDriverVersion = getLatestDriverVersion();
            string currentChromeVersion = getCurrentChromeVersion();
            if (!latestDriverVersion.Equals(currentChromeVersion))
            {
                MessageBox.Show($"최신 크롬을 사용하고 있지 않습니다. 크롬을 업데이트하지 않으면 번역기가 동작하지 않을 수 있어요.");
            }

            browser = Browser.Instance();

            // Following code will watch automatically kill chromeDriver.exe
            // WatchDogMain.exe is from my repo: https://github.com/sappho192/WatchDogDotNet
            var pid = Process.GetCurrentProcess().Id;
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "WatchDogMain.exe";
            info.Arguments = $"{pid}";
            info.WorkingDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            Process watchdogProcess = Process.Start(info);
        }

        private void UpdateLogFolderSize()
        {
            lbLogSize.Content = $"로그: {FormatBytes(GetDirectorySize("./logs"))}";
        }

        private string getLatestDriverVersion()
        {
            using (var client = new WebClient())
            {
                var result = client.DownloadString("https://chromedriver.storage.googleapis.com/LATEST_RELEASE");
                return result;
            }
        }

        private string getCurrentChromeVersion()
        {
            string result = string.Empty;
            object path;
            path = Registry.GetValue(chromeUserKeyName, "", null);
            if (path != null)
            {
                Console.WriteLine("Chrome: " + FileVersionInfo.GetVersionInfo(path.ToString()).FileVersion);
                result = FileVersionInfo.GetVersionInfo(path.ToString()).FileVersion;
            } else
            {
                path = Registry.GetValue(chromeLocalKeyName, "", null);
                if (path != null)
                {
                    Console.WriteLine("Chrome: " + FileVersionInfo.GetVersionInfo(path.ToString()).FileVersion);
                    result = FileVersionInfo.GetVersionInfo(path.ToString()).FileVersion;
                }
            }
            return result; 
        }

        private void LoadUserSettings()
        {
            var globalOpacity = Properties.Settings.Default.globalOpacity;
            grOriginal.Opacity = globalOpacity;
            grTranslated.Opacity = globalOpacity;
            grReTranslated.Opacity = globalOpacity;
            slOpacity.Value = globalOpacity;
            mainWindow.Top = Properties.Settings.Default.globalPosTop;
            mainWindow.Left = Properties.Settings.Default.globalPosLeft;
        }

        private void Translate()
        {
            string sentence = tbOriginal.Text;
            string testUrl = $"https://papago.naver.com/?sk={sk}&tk={tk}&st={Uri.EscapeDataString(sentence)}";
            
            browser.webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            browser.webDriver.Navigate().GoToUrl(testUrl);

            string translated = string.Empty;
            try
            {
                OpenQA.Selenium.IWebElement txtTarget;
                do
                {
                    txtTarget = browser.webDriver.FindElement(By.Id("txtTarget"));
                } while (txtTarget.Text.Equals(""));
                translated = txtTarget.Text;
                tbTranslated.Text = translated;
            }
            catch (Exception ex)
            {
                Log.Fatal("TRANSLATOR MET UNHANDLED EXCEPTION: {@Exception}, {@GroundZero}",
                        ex,
                        ex.StackTrace);
                tbTranslated.Text = "번역실패";
            }
            
            try
            {
                Clipboard.Clear();
                Clipboard.SetDataObject(translated);
                //Clipboard.SetText(txtTarget.Text);
            }
            catch (Exception ex)
            {
                Log.Fatal("TRANSLATOR MET UNHANDLED EXCEPTION: {@Exception}, {@GroundZero}",
                    ex,
                    ex.StackTrace);
            }
        }
        private void ReTranslate()
        {
            string sentence = tbTranslated.Text;
            string testUrl = $"https://papago.naver.com/?sk={tk}&tk={sk}&st={Uri.EscapeDataString(sentence)}";
            browser.webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
            browser.webDriver.Navigate().GoToUrl(testUrl);

            try
            {
                OpenQA.Selenium.IWebElement txtTarget;
                do
                {
                    txtTarget = browser.webDriver.FindElement(By.Id("txtTarget"));
                } while (txtTarget.Text.Equals(""));
                tbReTranslated.Text = txtTarget.Text;

            }
            catch (Exception ex)
            {
                Log.Fatal("TRANSLATOR MET UNHANDLED EXCEPTION: {@Exception}, {@GroundZero}",
                    ex,
                    ex.StackTrace);
                tbReTranslated.Text = "번역실패";
            }
        }

        public static long GetDirectorySize(string path)
        {
            long size = 0;
            DirectoryInfo dirInfo = new(path);

            foreach (FileInfo fi in dirInfo.GetFiles("*", SearchOption.AllDirectories))
            {
                size += fi.Length;
            }

            return size;
        }

        public static string FormatBytes(long bytes)
        {
            const int scale = 1024;
            string[] orders = new string[] { "GB", "MB", "KB", "Bytes" };
            long max = (long)Math.Pow(scale, orders.Length - 1);

            foreach (string order in orders)
            {
                if (bytes > max)
                    return string.Format("{0:##.##}{1}", decimal.Divide(bytes, max), order);

                max /= scale;
            }
            return "0B";
        }

        private void tbOriginal_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (!tbOriginal.Text.Equals("") && (isTranslatorBusy == false))
                {
                    isTranslatorBusy = true;
                    Translate();
                    ReTranslate();
                    isTranslatorBusy = false;
                    UpdateLogFolderSize();
                }
            }
        }

        private void Window_PreviewLostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            Window window = (Window)sender;
            window.Topmost = true;
        }

        private void btExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btJKSwitch_Click(object sender, RoutedEventArgs e)
        {
            if(currentState == STATE_K2J)
            {
                currentState = STATE_J2K;
                btJKSwitch.Content = "한→일";
                sk = "ja";
                tk = "ko";
                tbSk.Text = "번역하고 싶은 말을 적어보세요. (일본어)";
                tbTk.Text = "번역된 문장이 적히는 곳입니다. (한국어)";
                tbTk2Sk.Text = "번역된 한국어를 다시 일본어로 번역한 결과입니다.";
                tbOriginal.Text = "";
                tbTranslated.Text = "";
                tbReTranslated.Text = "";
            } else
            {
                currentState = STATE_K2J;
                btJKSwitch.Content = "일→한";
                sk = "ko";
                tk = "ja";
                tbSk.Text = "번역하고 싶은 말을 적어보세요. (한국어)";
                tbTk.Text = "번역된 문장이 적히는 곳입니다. (일본어)";
                tbTk2Sk.Text = "번역된 일본어를 다시 한국어로 번역한 결과입니다.";
                tbOriginal.Text = "";
                tbTranslated.Text = "";
                tbReTranslated.Text = "";
            }
        }

        private void slOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(isUIInitialized)
            {
                Properties.Settings.Default.globalOpacity = e.NewValue;
                Properties.Settings.Default.Save();
            }
            if (grOriginal != null)
            {
                grOriginal.Opacity = e.NewValue;
            }
            if (grTranslated != null)
            {
                grTranslated.Opacity = e.NewValue;
            }
            if (grReTranslated != null)
            {
                grReTranslated.Opacity = e.NewValue;
            }
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if(isUIInitialized)
            {
                if (mainWindow.WindowState.Equals(WindowState.Normal))
                {
                    Properties.Settings.Default.globalPosTop = mainWindow.Top;
                    Properties.Settings.Default.globalPosLeft = mainWindow.Left;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void btClearLog_Click(object sender, RoutedEventArgs e)
        {
            string[] filePaths = Directory.GetFiles(App.logPath);
            foreach (string filePath in filePaths)
            {
                var name = new FileInfo(filePath).Name;
                name = name.ToLower();
                if (name != App.logFile && name != App.logChromeFile)
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (IOException ex)
                    {
                        Log.Debug($"{ex.Message}");
                    }
                }
            }

            UpdateLogFolderSize();
        }
    }
}
