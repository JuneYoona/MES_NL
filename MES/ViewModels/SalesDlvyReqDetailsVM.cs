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
    public class SalesDlvyReqDetailsVM : ExportViewModelBase
    {
        #region Services
        IDialogService PopupItemView { get { return GetService<IDialogService>("ItemView"); } }
        IDocumentManagerService DocumentManagerService { get { return GetService<IDocumentManagerService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
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
        public string BizCode
        {
            get { return GetProperty(() => BizCode); }
            set { SetProperty(() => BizCode, value); }
        }
        public IEnumerable<CommonBizPartner> BizCodeList
        {
            get { return GetProperty(() => BizCodeList); }
            set { SetProperty(() => BizCodeList, value); }
        }
        public IEnumerable<SalesDlvyReqDetail> Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public SalesDlvyReqDetail SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public DataTable Details
        {
            get { return GetProperty(() => Details); }
            set { SetProperty(() => Details, value); }
        }
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
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public ICommand ShowItemDialogCmd { get; set; }
        public ICommand MouseDoubleClickCmd { get; set; }
        public ICommand MouseDownCmd { get; set; }
        public ICommand<object> ToExcelCmd { get; set; }
        #endregion

        public SalesDlvyReqDetailsVM()
        {
            Messenger.Default.Register<string>(this, OnMessage);

            BizAreaCodeList = GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0004" && u.IsEnabled == true);
            if (!string.IsNullOrEmpty(DSUser.Instance.BizAreaCode)) BizAreaCode = DSUser.Instance.BizAreaCode;

            StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            EndDate = DateTime.Now.AddMonths(1);
            // 업체정보가져오기
            Task.Run(() => { return GlobalCommonBizPartner.Instance.Where(u => u.BizType == "C" || u.BizType == "CS" && u.IsEnabled == true); })
                .ContinueWith(t => { BizCodeList = t.Result; });

            SearchCmd = new AsyncCommand(OnSearch);
            ShowItemDialogCmd = new DelegateCommand(OnShowItemDialog);
            MouseDoubleClickCmd = new DelegateCommand(OnMouseDoubleClick);
            MouseDownCmd = new DelegateCommand(OnMouseDown);
            ToExcelCmd = new DelegateCommand<object>(OnToExcel);
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

            var collections = new SalesDlvyReqDetailList(startDate: StartDate, endDate: EndDate, bizAreaCode: BizAreaCode);
            Collections = collections
                            .Where(u => string.IsNullOrEmpty(bizCode) ? true : u.ShipTo == bizCode)
                            .Where(u => string.IsNullOrEmpty(itemCode) ? true : u.ItemCode == itemCode);

            IsBusy = false;
        }

        public void OnShowItemDialog()
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
                document = DocumentManagerService.CreateDocument("SalesOrderReqView", new DocumentParamter(EntityMessageType.Changed, SelectedItem), this);
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
            Task.Run(SearchCore).ContinueWith(task =>
            {
                ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
            });
        }
    }
}
