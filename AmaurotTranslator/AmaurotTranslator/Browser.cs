using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
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
            chromeOptions.AddArguments("--headless");
            new DriverManager().SetUpDriver(new ChromeConfig());
            var driverService = ChromeDriverService.CreateDefaultService();
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
