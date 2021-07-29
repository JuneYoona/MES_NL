using System;
using System.Collections.ObjectModel;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using DevExpress.Mvvm.POCO;
using System.Threading.Tasks;
using System.Data;
using MesAdmin.Common.Common;

namespace MesAdmin.ViewModels
{
    public class StockMoveReportVM : ExportViewModelBase
    {
        #region Services
        IDialogService PopupItemView { get { return GetService<IDialogService>("ItemView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        #endregion

        #region Public Properties
        public ObservableCollection<CommonMinor> ItemAccount { get; set; }
        public string SelectedItemAcct
        {
            get { return GetProperty(() => SelectedItemAcct); }
            set { SetProperty(() => SelectedItemAcct, value); }
        }
        public DateTime BasicDate
        {
            get { return GetProperty(() => BasicDate); }
            set { SetProperty(() => BasicDate, value); }
        }
        public string ItemCode
        {
            get { return GetProperty(() => ItemCode); }
            set { SetProperty(() => ItemCode, value); }
        }
        public DataTable Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public StockMoveReport StockMoveReport { get; set; }
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

        public StockMoveReportVM()
        {
            StockMoveReport = new StockMoveReport();
            BasicDate = DateTime.Now;
            ItemAccount = new CommonMinorList("P1001");

            SearchCmd = new AsyncCommand(OnSearch, CanSearch);
            ToExcelCmd = new DelegateCommand<object>(base.OnToExcel);
            ShowDialogCmd = new DelegateCommand(OnShowDialog);
        }

        public bool CanSearch()
        {
            return !string.IsNullOrEmpty(SelectedItemAcct) && BasicDate != null;
        }
        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            Collections = StockMoveReport.GetCollections(BasicDate, SelectedItemAcct, ItemCode);
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
            ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
        }
    }
}
