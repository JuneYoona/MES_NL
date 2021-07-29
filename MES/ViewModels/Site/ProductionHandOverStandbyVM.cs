using System;
using MesAdmin.Common.Common;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using System.Collections.Generic;
using DevExpress.Mvvm.POCO;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace MesAdmin.ViewModels
{
    public class ProductionHandOverStandbyVM : ViewModelBase
    {
        #region Services
        IDialogService PopupItemView { get { return GetService<IDialogService>("ItemView"); } }
        IDocumentManagerService DocumentManagerService { get { return GetService<IDocumentManagerService>(); } }
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
        public IEnumerable<StockDetailHO> Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public ObservableCollection<StockDetailHO> SelectedItems
        {
            get { return GetProperty(() => SelectedItems); }
            set { SetProperty(() => SelectedItems, value); }
        }
        public StockDetailHO SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public StockDetailHO FocusedRow
        {
            get { return GetProperty(() => FocusedRow); }
            set { SetProperty(() => FocusedRow, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        public object MainViewModel { get; set; }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public ICommand ShowDialogCmd { get; set; }
        public ICommand AddCmd { get; set; }
        #endregion

        public ProductionHandOverStandbyVM()
        {
            Messenger.Default.Register<string>(this, OnMessage);

            StartDate = DateTime.Now.AddMonths(-6);
            EndDate = DateTime.Now;

            SearchCmd = new AsyncCommand(OnSearch);
            ShowDialogCmd = new DelegateCommand(OnShowDialog);
            AddCmd = new DelegateCommand(OnAdd, CanAdd);

            SelectedItems = new ObservableCollection<StockDetailHO>();
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            string itemCode = ItemCode;
            DateTime startDate = StartDate.Date;
            DateTime endDate = EndDate.Date;

            Collections = new StockDetailHOList();
            Collections = Collections
                           .Where(p => p.FinishDate >= startDate && p.FinishDate <= endDate)
                           .Where(p =>
                                string.IsNullOrEmpty(itemCode) ? true : p.ItemCode.ToUpper() == itemCode.ToUpper());

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

        public bool CanAdd()
        {
            return SelectedItems.Count > 0;
        }
        public void OnAdd()
        {
            ((MainViewModel)MainViewModel).TabLoadingOpen();
            IDocument document;
            document = DocumentManagerService.CreateDocument("ProductionHandOverView", new DocumentParamter(EntityMessageType.Added, SelectedItems, MainViewModel), this);
            document.DestroyOnClose = true;
            document.Id = Guid.NewGuid();
            document.Title = "제품인계 등록";
            document.Show();
            //SelectedItems = null;
        }

        IDocument FindDocument(string documentId)
        {
            foreach (var doc in DocumentManagerService.Documents)
                if (documentId.Equals(doc.Id))
                    return doc;
            return null;
        }

        void OnMessage(string pm)
        {
            if (pm == "Refresh")
                OnSearch();
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;
            MainViewModel = pm.ParentViewmodel;

            Task.Factory.StartNew(SearchCore).ContinueWith(task =>
            {
                ((MainViewModel)MainViewModel).TabLoadingClose();
            });

        }
    }
}
