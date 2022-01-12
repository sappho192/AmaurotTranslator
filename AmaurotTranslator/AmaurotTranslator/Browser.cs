using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace AmaurotTranslator
{
    public class Browser
    {
        public readonly IWebDriver webDriver;
        private static Browser _browser = null;
        private ChromeOptions chromeOptions = new ChromeOptions();

        private Browser()
        {
            chromeOptions.AddArguments("disable-infobars", "--headless", "--no-sandbox", "--disable-gpu", "--disable-dev-shm-usage");
            //chromeOptions.AddArguments("--headless", "--no-sandbox", "--disable-gpu");
            new DriverManager().SetUpDriver(new ChromeConfig());
            var driverService = ChromeDriverService.CreateDefaultService();
            if (File.Exists("DEBUG_MODE.txt") || File.Exists("DEBUG_MODE.txt.txt"))
            {
                driverService.LogPath = App.logChromeFilePath;
            }
            driverService.HideCommandPromptWindow = true;
            webDriver = new ChromeDriver(driverService, chromeOptions);
        }

        public static Browser Instance()
        {
            if(_browser == null)
            {
                _browser = new Browser();
            }
            return _browser;
        }

        ~Browser()
        {
            if(_browser != null)
            {
                if(webDriver != null)
                {
                    webDriver.Quit();
                    webDriver.Dispose();
                }
            }
        }
    }
}
