using System.Windows.Data;

namespace MesAdmin.Common.Common
{
    public class SettingBindingExtension : Binding
    {
        public SettingBindingExtension()
        {
            Initialize();
        }

        public SettingBindingExtension(string path)
            : base(path)
        {
            Initialize();
        }

        private void Initialize()
        {
            // 프로그램 종료시 가끔 위치/사이즈 정보를 저장못하는경우 강제로 default setting
            if (MesAdmin.Properties.Settings.Default.Height <= 0 || MesAdmin.Properties.Settings.Default.Width <= 0 ||
                MesAdmin.Properties.Settings.Default.Left <= 0 || MesAdmin.Properties.Settings.Default.Top <= 0)
            {
                MesAdmin.Properties.Settings.Default.Height = 768;
                MesAdmin.Properties.Settings.Default.Width = 1024;
                MesAdmin.Properties.Settings.Default.Left = 300;
                MesAdmin.Properties.Settings.Default.Top = 200;
            }

            this.Source = MesAdmin.Properties.Settings.Default;
            this.Mode = BindingMode.TwoWay;
        }
    }
}
