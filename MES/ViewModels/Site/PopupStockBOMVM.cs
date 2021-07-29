using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace MesAdmin.ViewModels
{
    public class PopupStockBOMVM : ViewModelBase
    {
        #region Services
        ICurrentWindowService CurrentWindowService { get { return GetService<ICurrentWindowService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public CommonMinor WhCode
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
        public string PrntItemCode
        {
            get { return GetProperty(() => PrntItemCode); }
            set { SetProperty(() => PrntItemCode, value); }
        }
        public CommonItemList Items { get; set; }
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
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public List<UICommand> DialogCmds { get; private set; }
        protected UICommand ConfirmUICmd { get; private set; }
        protected UICommand CancelUICmd { get; private set; }
        public ICommand ConfirmCmd { get; set; }
        #endregion

        public PopupStockBOMVM(string prntItemCode, List<StockDetail> exceptStocks)
        {
            PrntItemCode = prntItemCode;

            if (!string.IsNullOrEmpty(PrntItemCode))
            {
                Items = new CommonItemList();
                ItemName = Items.Where(u => u.ItemCode == PrntItemCode).FirstOrDefault().ItemName;
            }
            
            SelectedItems = new ObservableCollection<StockDetail>();
            ConfirmItems = new ObservableCollection<StockDetail>();
            
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
            Collections = new StockDetailBOMList(PrntItemCode);

            if (ExceptStocks.Count() != 0)
                Collections = Collections.Except(ExceptStocks);

            IsBusy = false;
        }

        protected void OnConfirm()
        {
            ConfirmItems = SelectedItems;
            ConfirmItem = SelectedItem;
            CurrentWindowService.Close();
        }
    }
}
