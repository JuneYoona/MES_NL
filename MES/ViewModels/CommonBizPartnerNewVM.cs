using System;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using System.Web.Security;
using System.Collections.ObjectModel;
using MesAdmin.Common.Common;
using System.Threading.Tasks;

namespace MesAdmin.ViewModels
{
    public class CommonBizPartnerNewVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public CommonBizPartner CommonBizPartnerItem
        {
            get { return GetProperty(() => CommonBizPartnerItem); }
            set { SetProperty(() => CommonBizPartnerItem, value); }
        }
        public CommonBizPartnerList Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public EntityMessageType Status
        {
            get { return GetProperty(() => Status); }
            set { SetProperty(() => Status, value); }
        }
        public CommonMinorList BizType
        {
            get { return GetProperty(() => BizType); }
            set { SetProperty(() => BizType, value); }
        }
        public ObservableCollection<CodeName> VATFlag
        {
            get { return GetProperty(() => VATFlag); }
            set { SetProperty(() => VATFlag, value); }
        }
        public CommonMinorList Currency
        {
            get { return GetProperty(() => Currency); }
            set { SetProperty(() => Currency, value); }
        }
        public bool? IsEnabled
        {
            get { return GetProperty(() => IsEnabled); }
            set { SetProperty(() => IsEnabled, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SaveCmd { get; set; }
        #endregion

        public CommonBizPartnerNewVM()
        {
            BizType = new CommonMinorList("B9005");
            Currency = new CommonMinorList("BZ006");
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            VATFlag = GlobalCommonIncude.Instance;
            Collections = new CommonBizPartnerList();
        }

        public bool CanSave()
        {
            bool ret = false;
            if (!string.IsNullOrEmpty(CommonBizPartnerItem.BizName) && !string.IsNullOrEmpty(CommonBizPartnerItem.BizType))
                ret = true;

            return ret;
        }
        public Task OnSave()
        {
            return Task.Factory.StartNew(SaveCore);
        }
        private void SaveCore()
        {
            IsEnabled = false;
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(0.5));
            DispatcherService.BeginInvoke(() =>
            {
                try
                {
                    Collections.Save();
                    // send mesaage to parent view
                    CommonBizPartner copyObj = CommonBizPartnerItem.DeepCloneReflection();
                    copyObj.State = EntityState.Unchanged;
                    Messenger.Default.Send(new EntityMessage<CommonBizPartner>(copyObj, Status));
                    Status = EntityMessageType.Changed;
                    CommonBizPartnerItem.State = EntityState.Modified;
                }
                catch (Exception ex)
                {
                    MessageBoxService.ShowMessage(ex.Message, "Information", MessageButton.OK, MessageIcon.Information);
                }
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
            if (Status == EntityMessageType.Added)
            {
                CommonBizPartnerItem = new CommonBizPartner();
                CommonBizPartnerItem.State = EntityState.Added;
                CommonBizPartnerItem.IsEnabled = true;
            }
            else
            {
                CommonBizPartnerItem = new CommonBizPartnerList((string)info.Item).FirstOrDefault();
                CommonBizPartnerItem.State = EntityState.Modified;    
            }
            Collections.Add(CommonBizPartnerItem);
        }
    }
}
