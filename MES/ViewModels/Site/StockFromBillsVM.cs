using System;
using System.Linq;
using DevExpress.Mvvm;
using MesAdmin.Models;
using DevExpress.Mvvm.POCO;
using System.Threading.Tasks;
using System.Data;
using MesAdmin.Common.Common;
using DevExpress.Xpf.Grid;

namespace MesAdmin.ViewModels
{
    public class StockFromBillsVM : ExportViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDialogService PopupItemView { get { return GetService<IDialogService>(); } }
        #endregion

        #region Public Properties
        public SiteView SiteView { get; set; }
        public DataTable Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public string ItemCode
        {
            get { return GetProperty(() => ItemCode); }
            set { SetProperty(() => ItemCode, value); }
        }
        public string ItemName
        {
            get { return GetProperty(() => ItemName); }
            set { SetProperty(() => ItemName, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        #endregion

        #region Commands
        public ICommand<string> ShowDialogCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        public ICommand<object> EndGroupingCmd { get; set; }
        #endregion

        public StockFromBillsVM()
        {
            SiteView = new SiteView();

            ShowDialogCmd = new DelegateCommand<string>(ShowDialog);
            SearchCmd = new AsyncCommand(OnSearch, CanSearch);
            EndGroupingCmd = new DelegateCommand<object>(OnEndGrouping);
        }

        public bool CanSearch()
        {
            return !string.IsNullOrEmpty(ItemCode);
        }
        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            string itemCode = ItemCode;

            Collections = SiteView.GetStockFromBills(itemCode);
            IsBusy = false;
        }

        public void OnEndGrouping(object pm)
        {
            TableView view = pm as TableView;
            GridControl grid = view.Grid;

            for (int i = 0; i < grid.VisibleRowCount; i++)
            {
                int rowHandle = grid.GetRowHandleByVisibleIndex(i);
                if (grid.IsGroupRowHandle(rowHandle) && grid.GetRowLevelByRowHandle(rowHandle) == 0)
                {
                    grid.ExpandGroupRow(rowHandle);
                }
            }
        }

        public void ShowDialog(string pm)
        {
            var vmItem = ViewModelSource.Create(() => new PopupItemVM("15"));
            PopupItemView.ShowDialog(
                dialogCommands: vmItem.DialogCmds,
                title: "품목선택",
                viewModel: vmItem
            );

            if (vmItem.ConfirmItem != null)
            {
                ItemCode = vmItem.ConfirmItem.ItemCode;
                ItemName = vmItem.ConfirmItem.ItemName;
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
