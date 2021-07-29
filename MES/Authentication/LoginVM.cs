using System.Windows;
using DevExpress.Mvvm;
using System.Windows.Input;
using System.Web.Security;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data.Common;
using System;
using System.Collections.Generic;
using System.Web.Profile;
using MesAdmin.Models;
using MesAdmin.Common.Services;
using System.Deployment.Application;

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
            set { SetProperty(() => UserId, value); }
        }
        public string Password
        {
            get { return GetProperty(() => Password); }
            set { SetProperty(() => Password, value); }
        }
        public string SelectedDB
        {
            get { return GetProperty(() => SelectedDB); }
            set { SetProperty(() => SelectedDB, value); }
        }
        public List<DBName> DBNameCollections
        {
            get { return GetProperty(() => DBNameCollections); }
            set { SetProperty(() => DBNameCollections, value); }
        }
        public List<BizArea> BizAreaCollections
        {
            get { return GetProperty(() => BizAreaCollections); }
            set { SetProperty(() => BizAreaCollections, value); }
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
        public SilentUpdater UpdateService { get; }
        #endregion

        #region Commands
        public AsyncCommand LoginCmd { get; set; }
        public ICommand CancelCmd { get; set; }
        #endregion

        public LoginVM()
        {
            LoginCmd = new AsyncCommand(OnLogin, CanLogin);
            CancelCmd = new DelegateCommand(OnCancel);

            IsChecked = string.IsNullOrEmpty(Properties.Settings.Default.UserId) ? false : true;
            UserId = Properties.Settings.Default.UserId;
            
            DBNameCollections = new List<DBName> { new DBName { Name = "DSNL_MES" }, new DBName { Name = "DSNL_TEST" } };
            if (Properties.Settings.Default.DBName != "")
                SelectedDB = Properties.Settings.Default.DBName;
            else
                SelectedDB = "DSNL_MES";

            BizAreaCollections = new List<BizArea>
            {
                new BizArea { Code = "BAC60", Name = "OLED" },
                new BizArea { Code = "BAC90", Name = "BPDL" },
            };
            if (Properties.Settings.Default.BizAreaCode == "")
                EditBizArea = "BAC60";
            else
                EditBizArea = Properties.Settings.Default.BizAreaCode;

            if (!IsInDesignMode) UpdateApplication();
        }

        public bool CanLogin() { return !string.IsNullOrEmpty(UserId) && !string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(SelectedDB); }
        public Task OnLogin() 
        {
            return Task.Factory.StartNew(LoginCore);
        }
        public void LoginCore()
        {
            if (!TryOpenConnection()) return;

            if (Membership.ValidateUser(UserId, Password) == true)
            {
                DispatcherService.BeginInvoke(() =>
                {
                    Window loginWnd = Application.Current.MainWindow;
                    MainWindow mainWnd = new MainWindow();
                    Application.Current.MainWindow = mainWnd;

                    loginWnd.Close();
                    mainWnd.Show();
                });

                CreateGlobalValue();
            }
            else
            {
                IsEnabled = true;
                Message = "ID 와 Password를 확인하세요!";
            }
        }

        public void OnCancel()
        {
            Application.Current.MainWindow.Close();
        }

        public bool TryOpenConnection()
        {
            try
            {
                IsEnabled = false;
                Message = "DataBase 연결중......";
                Database db = new DatabaseProviderFactory().Create(SelectedDB);
                using (DbConnection con = db.CreateConnection())
                {
                    con.Open();
                    return true;
                }
            }
            catch
            {
                IsEnabled = true;
                Message = "연결에 실패하였습니다.";
                return false;
            }
        }

        public void CreateGlobalValue()
        {
            ProfileBase profile = ProfileBase.Create(UserId);
            
            DSUser.Instance.UserID = UserId;
            DSUser.Instance.UserName = profile.GetPropertyValue("KorName").ToString();
            DSUser.Instance.BizAreaCode = EditBizArea;
            DBInfo.Instance.Name = SelectedDB;

            Properties.Settings.Default.UserId = IsChecked ? UserId : "";
            Properties.Settings.Default.DBName = SelectedDB;
            Properties.Settings.Default.BizAreaCode = EditBizArea;
            Properties.Settings.Default.Save();

            // 재접속처리
            ProviderFactory.Instance = null;
            GlabalCommonLayout.Instance = null;
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

    public class DBName
    {
        public string Name { get; set; }
    }

    public class BizArea
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
