using System;
using System.Data;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using System.Web.Security;
using System.Collections.ObjectModel;
using MesAdmin.Common.Common;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using System.Windows;

namespace MesAdmin.ViewModels
{
    class NetUserNewVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public NetUser NetUserItem
        {
            get { return GetProperty(() => NetUserItem); }
            set { SetProperty(() => NetUserItem, value); }
        }
        public NetRoles LueNetRoles
        {
            get { return GetProperty(() => LueNetRoles); }
            set { SetProperty(() => LueNetRoles, value); }
        }
        public NetRole SelectedRole
        {
            get { return GetProperty(() => SelectedRole); }
            set { SetProperty(() => SelectedRole, value); }
        }
        public EntityMessageType Status
        {
            get { return GetProperty(() => Status); }
            set { SetProperty(() => Status, value); }
        }
        public bool? IsEnabled 
        {
            get { return GetProperty(() => IsEnabled); }
            set { SetProperty(() => IsEnabled, value); }
        }
        public List<CommonWorkAreaInfo> WaCodes
        {
            get { return GetProperty(() => WaCodes); }
            set { SetProperty(() => WaCodes, value); }
        }
        public List<object> SelectedWaCode
        {
            get { return GetProperty(() => SelectedWaCode); }
            set { SetProperty(() => SelectedWaCode, value); }
        }
        public bool RoleEdit
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SaveCmd { get; set; }
        public ICommand AddRoleCmd { get; set; }
        public ICommand DelRoleCmd { get; set; }
        public ICommand<Popup> OpenedCmd { get; set; }
        #endregion

        public NetUserNewVM()
        {
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            AddRoleCmd = new DelegateCommand(OnAddRole);
            DelRoleCmd = new DelegateCommand(OnDelRole);
            OpenedCmd = new DelegateCommand<Popup>(OnOpened);

            // LookUpEdit data source
            LueNetRoles = NetRoles.Select();

            // 공정정보 세팅
            WaCodes = new CommonWorkAreaInfoList("").Where(o => o.IsEnabled).ToList();
            
            // 추가작업자 및 검사자 세팅
            var minors = GlobalCommonMinor.Instance.Where(o => o.MajorCode == "A0T01" && o.IsEnabled);
            foreach (CommonMinor item in minors)
            {
                WaCodes.Add(new CommonWorkAreaInfo { WaCode = item.MinorCode, WaName = item.MinorName });
            }
        }

        public void OnAddRole()
        {
            NetUserItem.Roles.Add(new NetRole() { State = EntityState.Added });
        }

        public void OnDelRole()
        {
            if (SelectedRole == null) return;

            if (SelectedRole.State == EntityState.Added)
                NetUserItem.Roles.Remove(SelectedRole);
            else
            {
                SelectedRole.State = SelectedRole.State == EntityState.Deleted ? 
                    SelectedRole.State = EntityState.Unchanged : 
                    SelectedRole.State = EntityState.Deleted;
            }
        }

        bool CanSave() 
        {
            NetUserItem.ValidateProperty(NetUserItem.Password, "Password");
            return NetUserItem.IsValid && !string.IsNullOrEmpty(NetUserItem.UserName) && !string.IsNullOrEmpty(NetUserItem.Profile.KorName) && !string.IsNullOrEmpty(NetUserItem.Password);
        }
        public Task OnSave()
        {
            return Task.Factory.StartNew(SaveCore);
        }
        private void SaveCore()
        {
            IsEnabled = false;
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(0.5));

            NetUserItem.Profile.WorkParts = SelectedWaCode == null ? "" : string.Join(",", SelectedWaCode);

            DispatcherService.BeginInvoke(() =>
            {
                if (Status == EntityMessageType.Added) // 추가
                {
                    try
                    {
                        // Membsership, Profile save
                        MembershipUser newUser = Membership.CreateUser
                        (
                            username: NetUserItem.UserName,
                            password: NetUserItem.Password,
                            email: NetUserItem.Email
                        );
                        NetUserItem.Profile.Save(NetUserItem.UserName);
                    
                        // roles save
                        string err = NetUsers.AddRoles
                        (
                            (Guid)newUser.ProviderUserKey, 
                            NetUserItem.Roles.Where(u => u.State == EntityState.Added)
                        );
                        if (err != string.Empty)
                            MessageBoxService.ShowMessage(err);
                    }
                    catch (MembershipCreateUserException e)
                    {
                        MessageBoxService.ShowMessage(GetErrorMessage(e.StatusCode));
                    }
                }
                else // 수정
                {
                    try
                    {
                        MembershipUser user = Membership.GetUser(NetUserItem.UserName);
                        user.ChangePassword(user.GetPassword(), NetUserItem.Password);
                        user.Email = NetUserItem.Email;
                        user.IsApproved = NetUserItem.IsApproved;
                        // Membsership, Profile save
                        Membership.UpdateUser(user);
                        NetUserItem.Profile.Save();

                        // roles save
                        string err = NetUsers.AddRoles
                        (
                            NetUserItem.UserId,
                            NetUserItem.Roles.Where(u => u.State == EntityState.Added)
                        );
                        NetUsers.DelteRoles
                        (
                            NetUserItem.UserId,
                            NetUserItem.Roles.Where(u => u.State == EntityState.Deleted)
                        );
                        if (err != string.Empty)
                            MessageBoxService.ShowMessage(err);
                    
                    }
                    catch (System.Configuration.Provider.ProviderException e)
                    {
                        MessageBoxService.ShowMessage(e.Message);
                    }
                }

                // refresh
                NetUserItem = NetUsers.Select(userName: NetUserItem.UserName)
                                      .FirstOrDefault(); 
            
                // send mesaage to parent view
                Messenger.Default.Send(new EntityMessage<NetUser>(NetUserItem, Status));
                Status = EntityMessageType.Changed;
            });
            IsEnabled = true;
        }

        // popup move with window
        public void OnOpened(Popup sender)
        {
            Window win = Application.Current.MainWindow;
            EventHandler handler = null;

            win.LocationChanged += handler = (s, e) =>
            {
                if (sender.IsOpen)
                {
                    var mi = typeof(Popup).GetMethod("UpdatePosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    mi.Invoke(sender, null);
                }
                else win.LocationChanged -= handler;
            };
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);

            if (ViewModelBase.IsInDesignMode) return;
            DocumentParamter info = (DocumentParamter)Parameter;
            if (info == null) throw new InvalidOperationException();

            RoleEdit = DSUser.Instance.RoleName.Contains("admin") ? true : false;
            Status = info.Type;

            NetUserItem = NetUsers.Select(userName: (string)info.Item).FirstOrDefault() ??
                new NetUser
                {
                    Roles = new ObservableCollection<NetRole>(),
                    Profile = new NetProfile(),
                    IsApproved = true
                };

            if (NetUserItem.Profile.WorkParts != null)
            {
                object[] userWorkPart = NetUserItem.Profile.WorkParts.Split(',');
                SelectedWaCode = new List<object>();
                SelectedWaCode = new List<object>(userWorkPart);
            }
        }

        public string GetErrorMessage(MembershipCreateStatus status)
        {
            switch (status)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "사용자 아이디가 이미 존재합니다.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "Email이 이미 존재합니다.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
    }
}
