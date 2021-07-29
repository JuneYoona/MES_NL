using System;
using MesAdmin.Common.Common;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using System.Collections.Generic;
using DevExpress.Mvvm.POCO;
using System.Threading.Tasks;
using System.Data;

namespace MesAdmin.ViewModels
{
    public class SalesOrderReqDetailsVM : ViewModelBase
    {
        #region Services
        IDialogService PopupItemView { get { return GetService<IDialogService>("ItemView"); } }
        IDocumentManagerService DocumentManagerService { get { return GetService<IDocumentManagerService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        #endregion

        #region Public Properties
        public object MainViewModel { get; set; }
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
        public string BizCode
        {
            get { return GetProperty(() => BizCode); }
            set { SetProperty(() => BizCode, value); }
        }
        public IEnumerable<CommonBizPartner> BizPartnerList
        {
            get { return GetProperty(() => BizPartnerList); }
            set { SetProperty(() => BizPartnerList, value); }
        }
        public IEnumerable<SalesOrderReqDetail> Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public SalesOrderReqDetail SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public DataTable Details
        {
            get { return GetProperty(() => Details); }
            set { SetProperty(() => Details, value); }
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
        public ICommand MouseDoubleClickCmd { get; set; }
        public ICommand MouseDownCmd { get; set; }
        #endregion

        public SalesOrderReqDetailsVM()
        {
            Messenger.Default.Register<string>(this, OnMessage);

            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now.AddMonths(1);

            SearchCmd = new AsyncCommand(OnSearch);
            ShowDialogCmd = new DelegateCommand(OnShowDialog);
            MouseDoubleClickCmd = new DelegateCommand(OnMouseDoubleClick);
            MouseDownCmd = new DelegateCommand(OnMouseDown);

            BindingBizPartnerList();
        }

        // 시간이 많이 걸리는 작업이어서 async binding
        private async void BindingBizPartnerList()
        {
            var task = Task<IEnumerable<CommonBizPartner>>.Factory.StartNew(LoadingBizPartnerList);
            await task;

            if (task.IsCompleted)
            {
                BizPartnerList = task.Result;
            }
        }

        private IEnumerable<CommonBizPartner> LoadingBizPartnerList()
        {
            return (new CommonBizPartnerList()).Where(u => u.BizType.Substring(0, 1) == "C");
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            string bizCode = BizCode;
            string itemCode = ItemCode;

            Collections = new SalesOrderReqDetailList(startDate: StartDate, endDate: EndDate);
            Collections = Collections
                            .Where(u => string.IsNullOrEmpty(bizCode) ? true : u.ShipTo == bizCode)
                            .Where(u => string.IsNullOrEmpty(itemCode) ? true : u.ItemCode == itemCode);
                            
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

        public void OnMouseDoubleClick()
        {
            if (SelectedItem == null) return;

            string documentId = SelectedItem.ReqNo;
            IDocument document = FindDocument(documentId);
            if (document == null)
            {
                ((MainViewModel)MainViewModel).TabLoadingOpen();
                document = DocumentManagerService.CreateDocument("SalesOrderReqNLView", new DocumentParamter(EntityMessageType.Changed, SelectedItem, MainViewModel), this);
                document.DestroyOnClose = true;
                document.Id = documentId;
                document.Title = "출하요청 등록";
            }
            document.Show();
            SelectedItem = null;
        }

        public void OnMouseDown()
        {
            if (SelectedItem != null)
                Details = new SalesOrderDlvyTable((string)SelectedItem.ReqNo, (int)SelectedItem.Seq).Collections;
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
