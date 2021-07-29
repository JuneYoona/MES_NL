using System;
using System.Collections.ObjectModel;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using DevExpress.Mvvm.POCO;
using System.Threading.Tasks;
using MesAdmin.Common.Common;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace MesAdmin.ViewModels
{
    public class ProductionMonthlyVM : ViewModelBase
    {
        #region Services
        IDialogService PopupItemView { get { return GetService<IDialogService>("ItemView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        #endregion

        #region Public Properties
        public ObservableCollection<CommonMinor> ItemAccount { get; set; }
        public string EditItemAcct
        {
            get { return GetProperty(() => EditItemAcct); }
            set { SetProperty(() => EditItemAcct, value); }
        }
        public string ItemCode
        {
            get { return GetProperty(() => ItemCode); }
            set { SetProperty(() => ItemCode, value); }
        }
        public ProductionInputRecord ProductionInputRecord { get; set; }
        public DataTable Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public IEnumerable<CommonWorkAreaInfo> WaCode
        {
            get { return GetProperty(() => WaCode); }
            set { SetProperty(() => WaCode, value); }
        }
        public string EditWaCode
        {
            get { return GetProperty(() => EditWaCode); }
            set { SetProperty(() => EditWaCode, value); }
        }
        public DateTime YYYYMM
        {
            get { return GetProperty(() => YYYYMM); }
            set { SetProperty(() => YYYYMM, value); }
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

        public ProductionMonthlyVM()
        {
            ProductionInputRecord = new ProductionInputRecord();
            WaCode = GlobalCommonWorkAreaInfo.Instance;
            ItemAccount = new CommonMinorList("P1001");
            YYYYMM = DateTime.Now;

            SearchCmd = new AsyncCommand(OnSearch, CanSearch);
            ShowDialogCmd = new DelegateCommand(OnShowDialog);
        }

        public bool CanSearch()
        {
            return !IsBusy;
        }
        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            string yyyymm = YYYYMM.ToString("yyyyMM");
            string itemCode = ItemCode;
            string itemAccount = EditItemAcct;
            string waCode = EditWaCode;

            Collections = ProductionInputRecord.ProductionMonthlyReport(yyyymm, itemCode, itemAccount, waCode);
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

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;
            Task.Factory.StartNew(SearchCore).ContinueWith(task =>
            {
                ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
            });
        }
    }
}
