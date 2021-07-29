// ** Article and associated source code originally published by Graeme Grant @ https://www.codeproject.com/Articles/1208414/Silent-ClickOnce-Installer-for-WPF-Winforms-in-Csharp-VB
using System.Reflection;
using System.Windows;

namespace MesAdmin.Common.Services
{
    public static class ApplicationExtension
    {
        public static bool SetSingleInstance(this Application app)
            => AppProcessHelper.SetSingleInstance();

        public static bool ReleaseSingleInstance(this Application app)
            => AppProcessHelper.ReleaseSingleInstance();

        public static void BeginReStart(this Application app)
            => AppProcessHelper.BeginReStart();

        public static void ReStart(this Application app)
            => AppProcessHelper.ReStart();

        public static void PreventRestart(this Application app, bool state = true)
            => AppProcessHelper.PreventRestart(state);

        public static void RestartIfRequired(this Application app)
            => AppProcessHelper.RestartIfRequired();

        public static string Version(this Application app)
            => Assembly.GetEntryAssembly().GetName().Version.ToString();
    }
}