using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Core;
using System.Threading;
using System.Windows;

namespace MesAdmin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            // DispatcherService globally
            ServiceContainer.Default.RegisterService("DispatcherService", new DispatcherService());
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                // Window 7 에서 System.Globalization.CultureNotFoundException 예외발생
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ko-KO");
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ko-KO");
            }
            catch { }

            Theme.RegisterPredefinedPaletteThemes();
            ApplicationThemeHelper.ApplicationThemeName = PredefinedThemePalettes.CobaltBlue.Name + Theme.VS2017Light.Name;

            // devexpress optimizing
            System.Reflection.Assembly.Load("DevExpress.Xpf.Printing.v21.2");
            System.Reflection.Assembly.Load("DevExpress.Xpf.DocumentViewer.v21.2.Core");
            System.Reflection.Assembly.Load("DevExpress.Xpf.LayoutControl.v21.2");
            System.Reflection.Assembly.Load("DevExpress.XtraReports.v21.2");

            base.OnStartup(e);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            MesAdmin.Properties.Settings.Default.Save();
        }
    }
}