using System;
using System.Diagnostics;
using System.Windows;
using OpenQA.Selenium.Chrome;

namespace AmaurotTranslator
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        ChromeOptions chromeOptions = new ChromeOptions();
        public MainWindow()
        {
            InitializeComponent();
            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            chromeOptions.AddArguments("--headless");
            Context.Instance.browser = new ChromeDriver(driverService, chromeOptions);

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
            string sk = "ko";
            string tk = "ja";
            string sentence = tbOriginal.Text;
            string testUrl = $"https://papago.naver.com/?sk={sk}&tk={tk}&st={Uri.EscapeDataString(sentence)}";
            Context.Instance.browser.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
            Context.Instance.browser.Navigate().GoToUrl(testUrl);
            try
            {
                OpenQA.Selenium.IWebElement txtTarget;
                do
                {
                    txtTarget = Context.Instance.browser.FindElementById("txtTarget");
                } while (txtTarget.Text.Equals(""));
                tbTranslated.Text = txtTarget.Text;

            }
            catch (Exception ex)
            {
                tbTranslated.Text = "번역실패";
            }
        }
        private void ReTranslate()
        {
            string sk = "ja";
            string tk = "ko";
            string sentence = tbTranslated.Text;
            string testUrl = $"https://papago.naver.com/?sk={sk}&tk={tk}&st={Uri.EscapeDataString(sentence)}";
            Context.Instance.browser.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
            Context.Instance.browser.Navigate().GoToUrl(testUrl);
            try
            {
                OpenQA.Selenium.IWebElement txtTarget;
                do
                {
                    txtTarget = Context.Instance.browser.FindElementById("txtTarget");
                } while (txtTarget.Text.Equals(""));
                tbReTranslated.Text = txtTarget.Text;

            }
            catch (Exception ex)
            {
                tbReTranslated.Text = "번역실패";
            }
        }

        private void tbOriginal_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (!tbOriginal.Text.Equals(""))
                {
                    Translate();
                    ReTranslate();
                }
            }
        }
    }
}
