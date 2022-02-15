using System;
using System.Windows.Input;
using DevExpress.Mvvm;
using MesAdmin.Models;
using MesAdmin.Common.Common;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MesAdmin.ViewModels
{
    public class ProductionWorkOrderDetailVM : ViewModelBase
    {
        #region Services
        IDialogService PopupItemView { get { return GetService<IDialogService>("ItemView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        IDocumentManagerService DocumentManagerService { get { return GetService<IDocumentManagerService>(); } }
        #endregion

        #region Public Properties
        public ProductionWorkOrderList Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public ObservableCollection<ProductionWorkOrder> SelectedItems { get; } = new ObservableCollection<ProductionWorkOrder>();
        public ProductionWorkOrder SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public ProductionWorkOrderDetailList Details
        {
            get { return GetProperty(() => Details); }
            set { SetProperty(() => Details, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
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
        public string MajorCode { get { return "I0011"; } }
        public IEnumerable<CommonMinor> BizAreaCodeList
        {
            get { return GetProperty(() => BizAreaCodeList); }
            set { SetProperty(() => BizAreaCodeList, value); }
        }
        public string BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value); }
        }
        public string WaCode
        {
            get { return GetProperty(() => WaCode); }
            set { SetProperty(() => WaCode, value); }
        }
        public MainViewModel MainViewModel { get { return (MainViewModel)((ISupportParentViewModel)this).ParentViewModel; } }
        public bool LotHeader { get; set; }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public ICommand SaveCmd { get; set; }
        public ICommand ShowDialogCmd { get; set; }
        public ICommand MouseDoubleClickCmd { get; set; }
        public ICommand MouseDownCmd { get; set; }
        public DelegateCommand<object> DelCmd { get; set; }
        #endregion

        public ProductionWorkOrderDetailVM()
        {
            Messenger.Default.Register<string>(this, OnMessage);

            BizAreaCodeList = GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0004");
            if (!string.IsNullOrEmpty(DSUser.Instance.BizAreaCode))
                BizAreaCode = BizAreaCodeList.FirstOrDefault(u => u.MinorCode == DSUser.Instance.BizAreaCode).MinorCode;

            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now.AddMonths(1);

            LotHeader = BizAreaCode == "BAC60" ? true : false;

            SaveCmd = new DelegateCommand(OnSave, CanSave);
            SearchCmd = new AsyncCommand(OnSearch, CanSearch);
            DelCmd = new DelegateCommand<object>(Delete, CanDel);
            MouseDoubleClickCmd = new DelegateCommand(OnMouseDoubleClick);
            MouseDownCmd = new DelegateCommand(OnMouseDown);
        }

        public bool CanSave()
        {
            if (Collections == null) return false;
            return Collections.Where(u => u.State == EntityState.Deleted).Count() > 0;
        }
        public void OnSave()
        {
            try
            {
                Collections.Save();
                OnSearch();
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowMessage(ex.Message, "Information", MessageButton.OK, MessageIcon.Information);
            }
        }

        bool CanDel(object obj) { return SelectedItems.Count > 0; }
        public void Delete(object obj)
        {
            SelectedItems.ToList().ForEach(u =>
            {
                if (u.State == EntityState.Added)
                    Collections.Remove(u);
                else
                    u.State = u.State == EntityState.Deleted ? EntityState.Unchanged : EntityState.Deleted;
            });
        }

        public bool CanSearch() { return true; }
        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            string editWaCode = WaCode;

            Collections = new ProductionWorkOrderList(
                new ProductionWorkOrderList(BizAreaCode, StartDate, EndDate)
                                .Where(p => 
                                    string.IsNullOrEmpty(editWaCode) ? true : p.WaCode == editWaCode)
            );
                
            Details = null;
            IsBusy = false;
        }

        public void OnMouseDown()
        {
            Details = null;
            if (SelectedItem != null)
                Details = new ProductionWorkOrderDetailList(SelectedItem.OrderNo);
        }

        public void OnMouseDoubleClick()
        {
            if (SelectedItem == null) return;

            string viewName = BizAreaCode == "BAC60" ? "ProductionWorkOrderNLView" : "BAC90PP001C";
            string title = string.Empty;

            string pm = SelectedItem.OrderNo;
            string documentId = SelectedItem.OrderNo;
            IDocument document = MainViewModel.FindDocument(documentId);
            if (document == null)
            {
                MainViewModel.TabLoadingOpen();
                document = MainViewModel.CreateDocument(viewName, "작업지시 등록", new DocumentParamter(EntityMessageType.Changed, pm, MainViewModel));
                document.DestroyOnClose = true;
                document.Id = documentId;
            }

            document.Show();
            SelectedItem = null;
        }

        void OnMessage(string pm)
        {
            if (pm == "Refresh")
                OnSearch();
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (IsInDesignMode) return;

            Task.Run(SearchCore).ContinueWith(task => MainViewModel.TabLoadingClose());
        }
    }
}
