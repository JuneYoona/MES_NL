// ** Article and associated source code originally published by Graeme Grant @ https://www.codeproject.com/Articles/1208414/Silent-ClickOnce-Installer-for-WPF-Winforms-in-Csharp-VB
using System;
using System.Deployment.Application;

namespace MesAdmin.Common.Services
{
    public class UpdateProgressChangedEventArgs : EventArgs
    {
        public DeploymentProgressChangedEventArgs ProgressChangedEventArgs { get; }
        public string StatusString { get; }

        public UpdateProgressChangedEventArgs(DeploymentProgressChangedEventArgs args)
        {
            ProgressChangedEventArgs = args;
            StatusString = string.Format("New version downloading {0:D}%", args.ProgressPercentage);
        }

        public override string ToString() => StatusString;

        private string GetProgressString(DeploymentProgressState state)
        {
            if (state == DeploymentProgressState.DownloadingApplicationFiles)
                return "application files";

            if (state == DeploymentProgressState.DownloadingApplicationInformation)
                return "application manifest";

            return "deployment manifest";
        }
    }
}