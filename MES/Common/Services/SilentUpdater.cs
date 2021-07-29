// ** Article and associated source code originally published by Graeme Grant @ https://www.codeproject.com/Articles/1208414/Silent-ClickOnce-Installer-for-WPF-Winforms-in-Csharp-VB
using System;
using System.ComponentModel;
using System.Deployment.Application;
using System.Timers;

namespace MesAdmin.Common.Services
{
    // original version here: http://www.codeproject.com/Articles/489763/Silently-updatable-single-instance-WPF-ClickOnce-a
    // cleaned & modernized code; changed to single instance class
    public sealed class SilentUpdater : INotifyPropertyChanged
    {
        private static volatile SilentUpdater instance;
        public static SilentUpdater Instance { get { return instance ?? (instance = new SilentUpdater()); } }

        private bool updateAvailable;
        public bool UpdateAvailable
        {
            get { return updateAvailable; }
            internal set { updateAvailable = value; RaisePropertyChanged(nameof(UpdateAvailable)); }
        }

        private Timer Timer { get; }
        private ApplicationDeployment ApplicationDeployment { get; }
        private bool Processing { get; set; }

        public event EventHandler<UpdateProgressChangedEventArgs> ProgressChanged;
        public event EventHandler<EventArgs> Completed;
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private SilentUpdater()
        {
            if (!ApplicationDeployment.IsNetworkDeployed) return;
            ApplicationDeployment = ApplicationDeployment.CurrentDeployment;

            // progress
            ApplicationDeployment.UpdateProgressChanged += (s, e) =>
            {
                if (ProgressChanged != null)
                    ProgressChanged.Invoke(this, new UpdateProgressChangedEventArgs(e));
            };

            // completed
            ApplicationDeployment.UpdateCompleted += (s, e) =>
            {
                Processing = false;
                if (e.Cancelled || e.Error != null)
                    return;

                UpdateAvailable = true;
                if (Completed != null)
                    Completed.Invoke(sender: this, e: null);
            };

            // checking
            Timer = new Timer(10 * 60 * 1000);
            Timer.Elapsed += (s, e) =>
            {
                if (Processing) return;
                Processing = true;
                try
                {
                    if (ApplicationDeployment.CheckForUpdate(false))
                        ApplicationDeployment.UpdateAsync();
                    else
                        Processing = false;
                }
                catch (Exception)
                {
                    Processing = false;
                }
            };

            Timer.Start();
        }
    }
}