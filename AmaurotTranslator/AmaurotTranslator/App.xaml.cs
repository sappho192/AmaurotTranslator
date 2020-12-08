using System.Windows;

namespace AmaurotTranslator
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (Context.Instance.browser != null)
            {
                Context.Instance.browser.Dispose();
            }
        }
    }
}
