using DevExpress.Mvvm;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Web.Security;

namespace MesAdmin.Authentication
{
    public class PasswordChangeVM : ViewModelBase, IDataErrorInfo
    {
        #region Services
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public string Password
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        [Required(ErrorMessage = "Password는 8자리 이상 숫자, 영문조합이어야 합니다.")]
        [RegularExpression(@"(?=.*\d)(?=.*[A-Za-z]).{8,}", ErrorMessage = "Password는 8자리 이상 숫자, 영문조합이어야 합니다.")]
        public string NewPassword1
        {
            get { return GetValue<string>(); }
            set { SetValue(value, () => RaisePropertyChanged(nameof(ErrorContent))); }
        }
        public string NewPassword2
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string Error
        {
            get { return string.Empty; }
        }
        public string this[string columnName]
        {
            get
            {
                return IDataErrorInfoHelper.GetErrorText(this, columnName);
            }
        }
        public MembershipUser User
        {
            get { return GetValue<MembershipUser>(); }
            set { SetValue(value); }
        }
        public string ErrorContent
        {
            get
            {
                return Password == NewPassword1 ? "Password가 같습니다. 다른 Password를 입력하세요!" : this["NewPassword1"];
            }
        }
        #endregion

        #region Commands
        public AsyncCommand ChangePasswordCmd { get; set; }
        public ICommand CancelCmd { get; set; }
        #endregion

        public PasswordChangeVM()
        {
            ChangePasswordCmd = new AsyncCommand(OnChangePassword, CanChangePassword);
            CancelCmd = new DelegateCommand(() => Application.Current.MainWindow.Close());
        }

        public bool CanChangePassword() { return string.IsNullOrEmpty(ErrorContent) && NewPassword1 == NewPassword2; }
        public Task OnChangePassword()
        {
            return Task.Run(ChangePasswordCore);
        }
        public void ChangePasswordCore()
        {
            DispatcherService.BeginInvoke(() =>
            {
                User.ChangePassword(Password, NewPassword1);
                Window changePasswordWnd = Application.Current.MainWindow;
                MainWindow mainWnd = new MainWindow();
                Application.Current.MainWindow = mainWnd;

                changePasswordWnd.Close();
                mainWnd.Show();
            });
        }
    }
}