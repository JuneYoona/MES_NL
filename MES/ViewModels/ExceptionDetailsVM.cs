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

namespace MesAdmin.ViewModels
{
    public class ExceptionDetailsVM : ExportViewModelBase
    {
        #region Services
        IDialogService PopupItemView { get { return GetService<IDialogService>("ItemView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        #endregion

        #region Public Properties
        public ObservableCollection<CommonMinor> ItemAccount { get; set; }
        public CommonMinor SelectedItemAcct
        {
            get { return GetProperty(() => SelectedItemAcct); }
            set { SetProperty(() => SelectedItemAcct, value); }
        }
        public IEnumerable<CommonMinor> MoveType { get; set; }
        public CommonMinor SelectedMoveType
        {
            get { return GetProperty(() => SelectedMoveType); }
            set { SetProperty(() => SelectedMoveType, value); }
        }
        public IEnumerable<CommonMinor> TransType { get; set; }
        public CommonMinor SelectedTransType
        {
            get { return GetProperty(() => SelectedTransType); }
            set { SetProperty(() => SelectedTransType, value); }
        }
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
        public StockMovementDetailList Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public ObservableCollection<StockMovementDetail> SelectedItems
        {
            get { return GetProperty(() => SelectedItems); }
            set { SetProperty(() => SelectedItems, value); }
        }
        public IEnumerable<CommonMinor> BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value); }
        }
        public string EditBizAreaCode
        {
            get { return GetProperty(() => EditBizAreaCode); }
            set { SetProperty(() => EditBizAreaCode, value); }
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
        public ICommand<object> ToExcelCmd { get; set; }
        #endregion

        public ExceptionDetailsVM()
        {
            BizAreaCode = GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0004");
            if (!string.IsNullOrEmpty(DSUser.Instance.BizAreaCode))
                EditBizAreaCode = BizAreaCode.FirstOrDefault(u => u.MinorCode == DSUser.Instance.BizAreaCode).MinorCode;

            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now;

            TransType = new CommonMinorList("I0002").Where(u => u.IsEnabled == true);
            MoveType = new CommonMinorList("I0001").Where(u => u.IsEnabled == true);
            ItemAccount = new CommonMinorList("P1001");

            SearchCmd = new AsyncCommand(OnSearch, CanSearch);
            ToExcelCmd = new DelegateCommand<object>(base.OnToExcel);
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
            Collections = new StockMovementDetailList
            (
                startDate: StartDate,
                endDate: EndDate,
                itemCode: ItemCode,
                itemAccount: SelectedItemAcct == null ? "" : SelectedItemAcct.MinorCode,
                transType: SelectedTransType == null ? "" : SelectedTransType.MinorCode,
                moveType: SelectedMoveType == null ? "" : SelectedMoveType.MinorCode,
                bizAreaCode: EditBizAreaCode
            );
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
