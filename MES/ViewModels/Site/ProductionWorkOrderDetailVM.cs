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
        public ObservableCollection<ProductionWorkOrder> SelectedItems
        {
            get { return GetProperty(() => SelectedItems); }
            set { SetProperty(() => SelectedItems, value); }
        }
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
        public object MainViewModel { get; set; }
        public bool LotHeader { get; set; }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public ICommand SaveCmd { get; set; }
        public ICommand ShowDialogCmd { get; set; }
        public ICommand MouseDoubleClickCmd { get; set; }
        public ICommand MouseDownCmd { get; set; }
        public DelegateCommand<object> DelCmd { get; set; }
        public ICommand EditValueChangedCmd { get; set; }
        #endregion

        public ProductionWorkOrderDetailVM()
        {
            Messenger.Default.Register<string>(this, OnMessage);

            BizAreaCode = GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0004");
            if (!string.IsNullOrEmpty(DSUser.Instance.BizAreaCode))
                EditBizAreaCode = BizAreaCode.FirstOrDefault(u => u.MinorCode == DSUser.Instance.BizAreaCode).MinorCode;

            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now.AddMonths(1);

            LotHeader = EditBizAreaCode == "BAC60" ? true : false;

            SaveCmd = new DelegateCommand(OnSave, CanSave);
            SearchCmd = new AsyncCommand(OnSearch, CanSearch);
            DelCmd = new DelegateCommand<object>(Delete, CanDel);
            MouseDoubleClickCmd = new DelegateCommand(OnMouseDoubleClick);
            MouseDownCmd = new DelegateCommand(OnMouseDown);
            EditValueChangedCmd = new DelegateCommand(OnEditValueChanged);

            SelectedItems = new ObservableCollection<ProductionWorkOrder>();
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
        }

        private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DelCmd.RaiseCanExecuteChanged();
        }

        public bool CanSave()
        {
            if (Collections == null) return false;
            return Collections.Where(u => u.State == MesAdmin.Common.Common.EntityState.Deleted).Count() > 0;
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
                if (u.State == MesAdmin.Common.Common.EntityState.Added)
                    Collections.Remove(u);
                else
                    u.State = u.State == MesAdmin.Common.Common.EntityState.Deleted ? MesAdmin.Common.Common.EntityState.Unchanged : MesAdmin.Common.Common.EntityState.Deleted;
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
            string editWaCode = EditWaCode;

            Collections = new ProductionWorkOrderList(
                new ProductionWorkOrderList(EditBizAreaCode, StartDate, EndDate)
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

            string viewName = EditBizAreaCode == "BAC60" ? "ProductionWorkOrderNLView" : "BAC90PP001C";
            string title = string.Empty;

            string pm = SelectedItem.OrderNo;
            string documentId = SelectedItem.OrderNo;
            IDocument document = FindDocument(documentId);
            if (document == null)
            {
                ((MainViewModel)MainViewModel).TabLoadingOpen();
                document = DocumentManagerService.CreateDocument(viewName, new DocumentParamter(EntityMessageType.Changed, pm, MainViewModel), this);
                document.DestroyOnClose = true;
                document.Id = documentId;
                document.Title = "작업지시 등록";
            }
            
            document.Show();
            SelectedItem = null;
        }

        IDocument FindDocument(string documentId)
        {
            foreach (var doc in DocumentManagerService.Documents)
                if (documentId.Equals(doc.Id))
                    return doc;
            return null;
        }

        public void OnEditValueChanged()
        {
            WaCode = GlobalCommonWorkAreaInfo.Instance
                    .Where(u => string.IsNullOrEmpty(EditBizAreaCode) ? true : u.BizAreaCode == EditBizAreaCode);
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
