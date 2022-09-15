using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DevExpress.Xpf.Core;
using DevExpress.Mvvm;
using System.Diagnostics;
using System.Deployment.Application;
using MesAdmin.Authentication;

namespace MesAdmin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : DevExpress.Xpf.Core.ThemedWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
        }

        public static implicit operator MainWindow(PasswordChangeView v)
        {
            throw new NotImplementedException();
        }
    }

    public class MainWindowViewModel : ViewModelBase
    {
        public bool DemoVisible { get; set; }
        public ICommand HelpCmd { get; set; }
        public ICommand ExeDemoCmd { get; set; }

        public MainWindowViewModel()
        {
            HelpCmd = new DelegateCommand(OnHelp);
            ExeDemoCmd = new DelegateCommand(OnExeDemo);

            DemoVisible = ApplicationDeployment.IsNetworkDeployed ? true : false;
        }

        public void OnHelp()
        {
            System.Windows.Forms.Help.ShowHelp(null, System.IO.Directory.GetCurrentDirectory() + @"\MES-NL.chm");
        }

        public void OnExeDemo()
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo(System.IO.Directory.GetCurrentDirectory() + @"\DemoMes.exe");
                info.UseShellExecute = false;
                Process p = Process.Start(info);
            }
            catch { }
        }
    }
}