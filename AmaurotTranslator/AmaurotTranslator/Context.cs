using OpenQA.Selenium.Chrome;

namespace AmaurotTranslator
{
    public class Context
    {
        private Context() { }
        private static Context _instance = null;
        public static Context Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Context();
                }
                return _instance;
            }
        }

        public ChromeDriver browser = null;
    }
}
