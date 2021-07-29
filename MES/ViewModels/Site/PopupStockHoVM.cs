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
    public class PopupStockHoVM : ViewModelBase
    {
        #region Services
        ICurrentWindowService CurrentWindowService { get { return GetService<ICurrentWindowService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public IEnumerable<StockDetailHO> Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public StockDetailHO SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public StockDetailHO ConfirmItem
        {
            get { return GetProperty(() => ConfirmItem); }
            set { SetProperty(() => ConfirmItem, value); }
        }
        public ObservableCollection<StockDetailHO> SelectedItems
        {
            get { return GetProperty(() => SelectedItems); }
            set { SetProperty(() => SelectedItems, value); }
        }
        public ObservableCollection<StockDetailHO> ConfirmItems
        {
            get { return GetProperty(() => ConfirmItems); }
            set { SetProperty(() => ConfirmItems, value); }
        }
        private IEnumerable<StockDetailHO> ExceptStocks
        {
            get { return GetProperty(() => ExceptStocks) ?? new List<StockDetailHO>(); }
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
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public List<UICommand> DialogCmds { get; private set; }
        protected UICommand ConfirmUICmd { get; private set; }
        protected UICommand CancelUICmd { get; private set; }
        public ICommand ConfirmCmd { get; set; }
        #endregion

        public PopupStockHoVM() : this(null) { }
        public PopupStockHoVM(List<StockDetailHO> exceptStocks)
        {
            SelectedItems = new ObservableCollection<StockDetailHO>();
            ConfirmItems = new ObservableCollection<StockDetailHO>();

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
            string itemCode = ItemCode;
            string itemName = ItemName;

            Collections = new StockDetailHOList();
            Collections = Collections
                           .Where(p =>
                                string.IsNullOrEmpty(itemCode) ? true : p.ItemCode.ToUpper().Contains(itemCode.ToUpper()))
                            .Where(p =>
                                string.IsNullOrEmpty(itemName) ? true : p.ItemName.ToUpper().Contains(itemName.ToUpper()));

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
