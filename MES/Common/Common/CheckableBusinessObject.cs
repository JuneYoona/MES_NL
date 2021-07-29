using DevExpress.Mvvm;

namespace MesAdmin.Common.Common
{
    public abstract class CheckableBusinessObject : ValidationBase
    {
        public bool IsChecked
        {
            get { return GetProperty(() => IsChecked); }
            set { SetProperty(() => IsChecked, value); }
        }
    }
}
