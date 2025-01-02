using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Xpf.Grid;

namespace MesAdmin.ViewModels
{
    public class PopupStockVM : ViewModelBase
    {
        #region Services
        ICurrentWindowService CurrentWindowService { get { return GetService<ICurrentWindowService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public string WhCode
        {
            get { return GetProperty(() => WhCode); }
            set { SetProperty(() => WhCode, value); }
        }
        public ObservableCollection<CommonMinor> ItemAccount { get; set; }
        public string EditItemAcct
        {
            get { return GetProperty(() => EditItemAcct); }
            set { SetProperty(() => EditItemAcct, value); }
        }
        public IEnumerable<StockDetail> Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public StockDetail SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public StockDetail ConfirmItem
        {
            get { return GetProperty(() => ConfirmItem); }
            set { SetProperty(() => ConfirmItem, value); }
        }
        public ObservableCollection<StockDetail> SelectedItems
        {
            get { return GetProperty(() => SelectedItems); }
            set { SetProperty(() => SelectedItems, value); }
        }
        public ObservableCollection<StockDetail> ConfirmItems
        {
            get { return GetProperty(() => ConfirmItems); }
            set { SetProperty(() => ConfirmItems, value); }
        }
        private IEnumerable<StockDetail> ExceptStocks
        {
            get { return GetProperty(() => ExceptStocks) ?? new List<StockDetail>(); }
            set { SetProperty(() => ExceptStocks, value); }
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
        public bool IsEnabled
        {
            get { return GetProperty(() => IsEnabled); }
            set { SetProperty(() => IsEnabled, value); }
        }
        public string Summary
        {
            get { return GetProperty(() => Summary); }
            set { SetProperty(() => Summary, value); }
        }
        public string Count
        {
            get { return GetProperty(() => Count); }
            set { SetProperty(() => Count, value); }
        }
        public bool SumVisible
        {
            get { return GetProperty(() => SumVisible); }
            set { SetProperty(() => SumVisible, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public List<UICommand> DialogCmds { get; private set; }
        protected UICommand ConfirmUICmd { get; private set; }
        protected UICommand CancelUICmd { get; private set; }
        public ICommand ConfirmCmd { get; set; }
        public ICommand<object> SelectionChangedCmd { get; set; }
        #endregion

        public PopupStockVM(string whCode) : this(whCode, "") { }
        public PopupStockVM(string whCode, string itemCode) : this(whCode, itemCode, null) { }
        public PopupStockVM(string whCode, string itemCode, List<StockDetail> exceptStocks) : this(whCode, itemCode, exceptStocks, "") { }
        public PopupStockVM(string whCode, string itemCode, List<StockDetail> exceptStocks, string itemAcct)
        {
            WhCode = whCode;
            ItemCode = itemCode;
            EditItemAcct = itemAcct;

            SelectedItems = new ObservableCollection<StockDetail>();
            ConfirmItems = new ObservableCollection<StockDetail>();

            if (itemCode == "")
                IsEnabled = true;
            else
                IsEnabled = false;

            ItemAccount = new CommonMinorList("P1001");

            // dialog command
            ConfirmUICmd = new UICommand()
            {
                Caption = "확인",
                IsDefault = false,
                IsCancel = false,
                Command = new DelegateCommand(() => { ConfirmItem = SelectedItem; ConfirmItems = SelectedItems; }),
                Id = MessageBoxResult.OK,
            };

            CancelUICmd = new UICommand()
            {
                Caption = "취소",
                IsCancel = true,
                IsDefault = false,
                Id = MessageBoxResult.Cancel,
            };
            DialogCmds = new List<UICommand>() { ConfirmUICmd, CancelUICmd };

            SearchCmd = new AsyncCommand(OnSearch);
            ConfirmCmd = new DelegateCommand(OnConfirm);
            SelectionChangedCmd = new DelegateCommand<object>(OnSelectionChanged);
            ExceptStocks = exceptStocks;
            OnSearch();
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            Collections = new StockDetailList(whCode: WhCode, itemAccount: EditItemAcct)
                    .Where(p =>
                        string.IsNullOrEmpty(ItemCode) ? true : p.ItemCode.ToUpper().Contains(ItemCode.ToUpper()))
                    .Where(p =>
                        string.IsNullOrEmpty(ItemName) ? true : p.ItemName.ToUpper().Contains(ItemName.ToUpper()))
                    .Where(p => (p.Qty + p.QrQty) - p.PickingQty > 0);

            if (ExceptStocks.Count() != 0)
                Collections = Collections.Except(ExceptStocks);

            IsBusy = false;
        }

        public void OnSelectionChanged(object sender)
        {
            TableView view = sender as TableView;
            GridControl grid = view.Grid;
            IList<GridCell> selectedCells = view.GetSelectedCells();

            decimal summary = 0;
            decimal count = 0;
            decimal temp = 0;
            foreach (var item in selectedCells)
            {
                var cellValue = grid.GetCellValue(item.RowHandle, item.Column);
                bool res = decimal.TryParse(cellValue.ToString(), out temp);
                if (res) summary += temp;
                count += 1;
            }

            Count = count > 0 ? "개수 : " + count.ToString() : "";
            Summary = summary > 0 ? "합계 : " + summary.ToString() : "";
            SumVisible = summary > 0 ? true : false;
        }

        protected void OnConfirm()
        {
            ConfirmItems = SelectedItems;
            ConfirmItem = SelectedItem;
            CurrentWindowService.Close();
        }
    }
}