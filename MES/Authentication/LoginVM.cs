using DevExpress.Mvvm;
using MesAdmin.Common.Services;
using MesAdmin.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Deployment.Application;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Security;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MesAdmin.Authentication
{
    public class LoginVM : ViewModelBase
    {
        #region Services
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public string UserId
        {
            get { return GetProperty(() => UserId); }
            set { SetProperty(() => UserId, value, () => Message = BizAreaList != null ? "" : Message); }
        }
        public string Password
        {
            get { return GetProperty(() => Password); }
            set { SetProperty(() => Password, value, () => Message = BizAreaList != null ? "" : Message); }
        }
        public string SelectedDB
        {
            get { return GetProperty(() => SelectedDB); }
            set { SetProperty(() => SelectedDB, value); }
        }
        public List<string> DBNameList
        {
            get { return GetProperty(() => DBNameList); }
            set { SetProperty(() => DBNameList, value); }
        }
        public List<ItemInfo> BizAreaList
        {
            get { return GetProperty(() => BizAreaList); }
            set { SetProperty(() => BizAreaList, value); }
        }
        public string EditBizArea
        {
            get { return GetProperty(() => EditBizArea); }
            set { SetProperty(() => EditBizArea, value); }
        }
        public bool IsChecked
        {
            get { return GetProperty(() => IsChecked); }
            set { SetProperty(() => IsChecked, value); }
        }
        public string Message
        {
            get { return GetProperty(() => Message); }
            set { SetProperty(() => Message, value); }
        }
        public bool? IsEnabled 
        {
            get { return GetProperty(() => IsEnabled); }
            set { SetProperty(() => IsEnabled, value); }
        }
        public bool Updating
        {
            get { return GetProperty(() => Updating); }
            set { SetProperty(() => Updating, value); }
        }
        public string UpdaterText
        {
            get { return GetProperty(() => UpdaterText); }
            set { SetProperty(() => UpdaterText, value); }
        }
        public int UpdaterPercentage
        {
            get { return GetProperty(() => UpdaterPercentage); }
            set { SetProperty(() => UpdaterPercentage, value); }
        }
        public bool PasswordFocus
        {
            get { return GetProperty(() => PasswordFocus); }
            set { SetProperty(() => PasswordFocus, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand LoginCmd { get; set; }
        public ICommand CancelCmd { get; set; }
        #endregion

        public LoginVM()
        {
            LoginCmd = new AsyncCommand(OnLogin, CanLogin);
            CancelCmd = new DelegateCommand(() => Application.Current.MainWindow.Close());

            IsChecked = string.IsNullOrEmpty(Properties.Settings.Default.UserId) ? false : true;
            UserId = Properties.Settings.Default.UserId;

            #region 사업부 및 database 정보
            DBNameList = new List<string> { "DSNL_MES", "DSNL_TEST" };
            EditBizArea = Properties.Settings.Default.BizAreaCode;
            SelectedDB = Properties.Settings.Default.DBName;

            BizAreaList = new List<ItemInfo>();
            BizAreaList.Add(new ItemInfo { Value = "BAC40", Text = "도전볼" });
            BizAreaList.Add(new ItemInfo { Value = "BAC60", Text = "OLED" });
            BizAreaList.Add(new ItemInfo { Value = "BAC90", Text = "BPDL" });
            BizAreaList.Add(new ItemInfo { Value = "BAC95", Text = "QD" });
            #endregion

            // after loading target control
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => PasswordFocus = !string.IsNullOrEmpty(UserId) ? true : false), DispatcherPriority.Render);

            if (!IsInDesignMode) UpdateApplication();
        }

        public bool CanLogin() { return !string.IsNullOrEmpty(UserId) && !string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(SelectedDB) && !string.IsNullOrEmpty(EditBizArea); }
        public Task OnLogin() 
        {
            return Task.Factory.StartNew(LoginCore);
        }
        public void LoginCore()
        {
            if (!TryOpenConnection()) return;

            if (Membership.ValidateUser(UserId, Password))
            {
                IsEnabled = false;
                Message = "";

                #region Password 만료일 check
                MembershipUser user = Membership.GetUser(UserId);
                DateTime lastPasswordChangedDate = user.LastPasswordChangedDate;
                double diffDay = (DateTime.Now - lastPasswordChangedDate).TotalDays;
                #endregion

                DispatcherService.BeginInvoke(() =>
                {
                    Window loginWnd = Application.Current.MainWindow;
                    Window activeWnd = null;

                    if (diffDay > 90)
                    {
                        activeWnd = new PasswordChangeView();
                        var vm = activeWnd.DataContext as PasswordChangeVM;
                        vm.User = user;
                        vm.Password = Password;
                    }
                    else
                    {
                        activeWnd = new MainWindow();
                    }

                    Application.Current.MainWindow = activeWnd;

                    loginWnd.Close();
                    activeWnd.Show();
                });

                CreateGlobalValue();
            }
            else
            {
                IsEnabled = true;
                Message = "ID 와 Password를 확인하세요!";
            }
        }

        public bool TryOpenConnection()
        {
            try
            {
                IsEnabled = false;
                Message = "Database 연결중......";
                Database db = new DatabaseProviderFactory().Create(SelectedDB);
                using (DbConnection con = db.CreateConnection())
                {
                    con.Open();
                    IsEnabled = true;
                    Message = "";
                    return true;
                }
            }
            catch
            {
                IsEnabled = true;
                Message = "Database 연결에 실패하였습니다.";
                return false;
            }
        }

        public void CreateGlobalValue()
        {
            DBInfo.Instance.Name = SelectedDB;

            // Role 및 사용자 이름 저장
            NetUsers users = NetUsers.Select(UserId);
            List<string> roleNames = new List<string>();

            foreach (var role in users[0].Roles)
            {
                roleNames.Add(role.RoleName);
            }

            DSUser.Instance.UserID = UserId;
            DSUser.Instance.BizAreaCode = EditBizArea;
            DSUser.Instance.UserName = users[0].Profile.KorName;
            DSUser.Instance.RoleName = roleNames;

            Properties.Settings.Default.UserId = IsChecked ? UserId : "";
            Properties.Settings.Default.DBName = SelectedDB;
            Properties.Settings.Default.BizAreaCode = EditBizArea;
            Properties.Settings.Default.Save();

            // 재접속처리
            ProviderFactory.Instance = null;
            GlobalCommonItem.Instance = null;
            GlobalCommonBizPartner.Instance = null;
            GlobalCommonMinor.Instance = null;
            //ProfileBase profile = ProfileBase.Create(UserId);

            //DSUser.Instance.UserID = UserId;
            //DSUser.Instance.UserName = profile.GetPropertyValue("KorName").ToString();
            //DSUser.Instance.BizAreaCode = EditBizArea;
            //DBInfo.Instance.Name = SelectedDB;

            //Properties.Settings.Default.UserId = IsChecked ? UserId : "";
            //Properties.Settings.Default.DBName = SelectedDB;
            //Properties.Settings.Default.BizAreaCode = EditBizArea;
            //Properties.Settings.Default.Save();

            //// 재접속처리
            //ProviderFactory.Instance = null;
            //GlabalCommonLayout.Instance = null;
        }

        private void UpdateApplication()
        {
            try
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
                    ad.CheckForUpdateCompleted += new CheckForUpdateCompletedEventHandler(CheckForUpdateCompleted);
                    ad.CheckForUpdateAsync();
                }
            }
            catch { }
        }

        void CheckForUpdateCompleted(object sender, CheckForUpdateCompletedEventArgs e)
        {
            if (e.UpdateAvailable)
            {
                IsEnabled = false;
                Updating = true;
                BeginUpdate();
            }
        }

        private void BeginUpdate()
        {
            ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
            ad.UpdateCompleted += (s, e) =>
            {
                UpdaterText = "프로그램을 다시 시작하는중...";
                Task.Delay(TimeSpan.FromMilliseconds(2000)).ContinueWith(task => DispatcherService.BeginInvoke(() => Application.Current.ReStart()));
            };

            // Indicate progress in the application's status bar.
            ad.UpdateProgressChanged += (s, e) =>
            {
                UpdaterText = string.Format("새로운 버전이 설치되고 있습니다. {0:D}%", e.ProgressPercentage.ToString());
                UpdaterPercentage = e.ProgressPercentage;
            };

            ad.UpdateAsync();
        }
    }
}
