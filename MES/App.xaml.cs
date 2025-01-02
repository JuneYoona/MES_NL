using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Core;
using System;
using System.Threading;
using System.Windows;
using System.Deployment.Application;

namespace MesAdmin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            SplashScreenManager.Create(() => new SplashScreen1(), new DXSplashScreenViewModel
            {
                IsIndeterminate = true,
                Title = "DS Neolux MES",
                Subtitle = ApplicationDeployment.IsNetworkDeployed ? ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString() : "21.2.3.67",
                Logo = new Uri("pack://application:,,,/Resources/Images/App-5.png")
            }).ShowOnStartup();

            // DispatcherService globally
            ServiceContainer.Default.RegisterService("DispatcherService", new DispatcherService());
            // Download excel globally
            ServiceContainer.Default.RegisterService("SaveFileDialogServiceXlsx", new SaveFileDialogService
            {
                RestorePreviouslySelectedDirectory = true,
                DefaultExt = ".xlsx",
                Filter = "Excel (.xlsx)|*.xlsx",
                FilterIndex = 1
            });
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