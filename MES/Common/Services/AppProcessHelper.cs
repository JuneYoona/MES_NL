// ** Article and associated source code originally published by Graeme Grant @ https://www.codeproject.com/Articles/1208414/Silent-ClickOnce-Installer-for-WPF-Winforms-in-Csharp-VB
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Windows;
using System.Xml;
using System.Xml.Linq;

namespace MesAdmin.Common.Services
{
    internal static class AppProcessHelper
    {
        private static Process process;
        public static Process GetProcess
        {
            get
            {
                return process ?? (process = new Process
                {
                    StartInfo =
                    {
                        FileName = GetShortcutPath2(), UseShellExecute = true
                    }
                });
            }
        }

        public static string GetShortcutPath()
            => $@"{Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Programs),
                    GetPublisher(),
                    GetDeploymentInfo().Name.Replace(".application", ""))}.appref-ms";

        public static string GetShortcutPath2()
        {
            XDocument xDocument;
            using (MemoryStream memoryStream = new MemoryStream(AppDomain.CurrentDomain.ActivationContext.DeploymentManifestBytes))
            using (XmlTextReader xmlTextReader = new XmlTextReader(memoryStream))
            {
                xDocument = XDocument.Load(xmlTextReader);
            }
            var description = xDocument.Root.Elements().Where(p => p.Name.LocalName == "description").First();
            var publisher = description.Attributes().Where(a => a.Name.LocalName == "publisher").First();
            var product = description.Attributes().Where(a => a.Name.LocalName == "product").First();

            return $@"{Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.Programs),
                        publisher.Value,
                        product.Value,
                        product.Value.Replace(".application", ""))}.appref-ms";
        }

        private static ActivationContext ActivationContext
           => AppDomain.CurrentDomain.ActivationContext;

        public static string GetPublisher()
        {
            XDocument xDocument;
            using (var memoryStream = new MemoryStream(ActivationContext.DeploymentManifestBytes))
            using (var xmlTextReader = new XmlTextReader(memoryStream))
                xDocument = XDocument.Load(xmlTextReader);

            if (xDocument.Root == null)
                return null;

            return xDocument.Root
                            .Elements().First(e => e.Name.LocalName == "description")
                            .Attributes().First(a => a.Name.LocalName == "publisher")
                            .Value;
        }

        public static ApplicationId GetDeploymentInfo()
            => (new ApplicationSecurityInfo(ActivationContext)).DeploymentId;

        private static Mutex instanceMutex;
        public static bool SetSingleInstance()
        {
            bool createdNew;
            instanceMutex = new Mutex(true, @"Local\" + Process.GetCurrentProcess().MainModule.ModuleName, out createdNew);
            return createdNew;
        }

        public static bool ReleaseSingleInstance()
        {
            if (instanceMutex == null) return false;

            instanceMutex.Close();
            instanceMutex = null;

            return true;
        }

        private static bool isRestartDisabled;
        private static bool canRestart;

        public static void BeginReStart()
        {
            // make sure we have the process before we start shutting down
            var proc = GetProcess;

            // Note that we can restart only if not
            canRestart = !isRestartDisabled;

            // Start the shutdown process
            Application.Current.Shutdown();
        }

        public static void ReStart()
        {
            // make sure we have the process before we start shutting down
            var proc = GetProcess;

            try
            {
                if (process != null) process.Start();
            }
            catch { }
            // Start the shutdown process
            Application.Current.Shutdown();
        }

        public static void PreventRestart(bool state = true)
        {
            isRestartDisabled = state;
            if (state) canRestart = false;
        }

        public static void RestartIfRequired(int exitCode = 0)
        {
            // make sure to release the instance
            ReleaseSingleInstance();

            if (canRestart && process != null)
                //app is restarting...
                process.Start();
            else
                // app is stopping...
                Application.Current.Shutdown(exitCode);
        }
    }
}