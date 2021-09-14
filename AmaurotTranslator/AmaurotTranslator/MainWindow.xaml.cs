using System;
using System.Diagnostics;
using System.Windows;
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

        private string sk = "ko";
        private string tk = "ja";
        private const int STATE_K2J = 1;
        private const int STATE_J2K = 2;
        private int currentState = 0;
        private Browser browser;

        public MainWindow()
        {
            InitializeComponent();
            currentState = STATE_K2J;
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

        private void Translate()
        {
            string sentence = tbOriginal.Text;
            string testUrl = $"https://papago.naver.com/?sk={sk}&tk={tk}&st={Uri.EscapeDataString(sentence)}";
            
            browser.webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
            browser.webDriver.Navigate().GoToUrl(testUrl);

            try
            {
                OpenQA.Selenium.IWebElement txtTarget;
                do
                {
                    txtTarget = browser.webDriver.FindElement(By.Id("txtTarget"));
                } while (txtTarget.Text.Equals(""));
                tbTranslated.Text = txtTarget.Text;
                Clipboard.SetText(txtTarget.Text);
            }
            catch (Exception ex)
            {
                Log.Fatal("TRANSLATOR MET UNHANDLED EXCEPTION: {@Exception}, {@GroundZero}",
                        ex,
                        ex.StackTrace);
                tbTranslated.Text = "번역실패";
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
    }
}
