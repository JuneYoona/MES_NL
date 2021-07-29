using System;
using MesAdmin.Common.Common;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using System.Collections.Generic;
using DevExpress.Mvvm.POCO;
using System.Threading.Tasks;

namespace MesAdmin.ViewModels
{
    public class PurcharseWarehousingIQCVM : ViewModelBase
    {
        #region Services
        IDialogService PopupItemView { get { return GetService<IDialogService>("ItemView"); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public DateTime StartDate
        {
            get { return GetProperty(() => StartDate); }
            set { SetProperty(() => StartDate, value); }
        }
        public DateTime EndDate
        {
            get { return GetProperty(() => EndDate); }
            set { SetProperty(() => EndDate, value); }
        }
        public string ItemCode
        {
            get { return GetProperty(() => ItemCode); }
            set { SetProperty(() => ItemCode, value); }
        }
        public string BizCode
        {
            get { return GetProperty(() => BizCode); }
            set { SetProperty(() => BizCode, value); }
        }
        public IEnumerable<CommonBizPartner> BizPartnerList { get; set; }
        public IEnumerable<QualityRequest> Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public ICommand ShowDialogCmd { get; set; }
        #endregion

        public PurcharseWarehousingIQCVM()
        {
            StartDate = DateTime.Now.AddMonths(-6);
            EndDate = DateTime.Now;
            BizPartnerList = (new CommonBizPartnerList()).Where(u => u.BizType == "V" || u.BizType == "CV");
            OnSearch();

            SearchCmd = new AsyncCommand(OnSearch);
            ShowDialogCmd = new DelegateCommand(OnShowDialog);
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            string bizCode = BizCode;
            string itemCode = ItemCode;

            Collections = new QualityRequestList(startDate: StartDate, endDate: EndDate, qrType : "IQC", bizCode : bizCode);
            Collections = Collections
                            .Where(u => u.TransferFlag == false)
                            .Where(u => string.IsNullOrEmpty(itemCode) ? true : u.ItemCode == itemCode);
            IsBusy = false;
        }

        public void OnShowDialog()
        {
            var vmItem = ViewModelSource.Create(() => new PopupItemVM());
            PopupItemView.ShowDialog(
                dialogCommands: vmItem.DialogCmds,
                title: "품목선택",
                viewModel: vmItem
            );

            if (vmItem.ConfirmItem != null)
            {
                ItemCode = vmItem.ConfirmItem.ItemCode;
            }
        }
    }
}
