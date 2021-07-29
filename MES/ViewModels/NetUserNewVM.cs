using System;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using System.Web.Security;
using System.Collections.ObjectModel;
using MesAdmin.Common.Common;
using System.Threading.Tasks;
using System.Collections.Generic;

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
        public CommonWorkAreaInfoList WAItems
        {
            get { return GetProperty(() => WAItems); }
            set { SetProperty(() => WAItems, value); }
        }
        public List<object> SelectedWACode
        {
            get { return GetProperty(() => SelectedWACode); }
            set { SetProperty(() => SelectedWACode, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SaveCmd { get; set; }
        public ICommand AddRoleCmd { get; set; }
        public ICommand DelRoleCmd { get; set; }
        #endregion

        public NetUserNewVM()
        {
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            AddRoleCmd = new DelegateCommand(OnAddRole);
            DelRoleCmd = new DelegateCommand(OnDelRole);

            // LookUpEdit data source
            LueNetRoles = NetRoles.Select();

            // 공정정보 세팅
            WAItems = new CommonWorkAreaInfoList("");
            WAItems.Add(new CommonWorkAreaInfo { WaCode = "Sales", WaName = "Sales" });
            WAItems.Add(new CommonWorkAreaInfo { WaCode = "IQC", WaName = "수입검사" });
            WAItems.Add(new CommonWorkAreaInfo { WaCode = "LQC", WaName = "공정검사" });
            WAItems.Add(new CommonWorkAreaInfo { WaCode = "FQC", WaName = "최종검사" });
            WAItems.Add(new CommonWorkAreaInfo { WaCode = "OQC", WaName = "출하검사" });
            WAItems.Add(new CommonWorkAreaInfo { WaCode = "PDABOX", WaName = "라벨검사(OLED)" });
            WAItems.Add(new CommonWorkAreaInfo { WaCode = "PDABPDL", WaName = "라벨검사(BPDL)" });
            WAItems.Add(new CommonWorkAreaInfo { WaCode = "QNReg", WaName = "선행로트등록자" });
            WAItems.Add(new CommonWorkAreaInfo { WaCode = "QNRes", WaName = "선행로트검사자" });
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
            NetUserItem.ValidateProperty(NetUserItem.UserName, "UserName");
            NetUserItem.ValidateProperty(NetUserItem.Password, "Password");
            return NetUserItem.IsValid && !string.IsNullOrEmpty(NetUserItem.Profile.KorName);
        }
        public Task OnSave()
        {
            return Task.Factory.StartNew(SaveCore);
        }
        private void SaveCore()
        {
            IsEnabled = false;
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(0.5));

            if (SelectedWACode == null)
                NetUserItem.Profile.WorkParts = "";
            else
                NetUserItem.Profile.WorkParts = string.Join(",", SelectedWACode);

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

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);

            if (ViewModelBase.IsInDesignMode) return;
            DocumentParamter info = (DocumentParamter)Parameter;
            if (info == null) throw new InvalidOperationException();

            Status = info.Type;
            NetUserItem = NetUsers.Select(userName: (string)info.Item)
                                  .FirstOrDefault() ?? new NetUser { Roles = new ObservableCollection<NetRole>(), Profile = new NetProfile(), IsApproved = true };

            if (NetUserItem.Profile.WorkParts == null) return;

            object[] userWorkPart = NetUserItem.Profile.WorkParts.Split(',');
            SelectedWACode = new List<object>();
            SelectedWACode = new List<Object>((IEnumerable<Object>)userWorkPart);
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
