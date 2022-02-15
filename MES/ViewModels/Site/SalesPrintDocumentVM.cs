using System;
using System.Windows.Input;
using DevExpress.Mvvm;
using MesAdmin.Models;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using MesAdmin.Reports;
using DevExpress.Xpf.Core;
using System.Reflection;
using MesAdmin.Common.Common;

namespace MesAdmin.ViewModels
{
    public class SalesPrintDocumentVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        IDocumentManagerService DocumentManagerService { get { return GetService<IDocumentManagerService>(); } }
        #endregion

        #region Public Properties
        public IEnumerable<object> Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public object SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public DataTable Details
        {
            get { return GetProperty(() => Details); }
            set { SetProperty(() => Details, value); }
        }
        public IEnumerable<CommonBizPartner> BizCodeList
        {
            get { return GetProperty(() => BizCodeList); }
            set { SetProperty(() => BizCodeList, value); }
        }
        public IEnumerable<SalesOrderTypeConfig> SoTypeList { get; set; }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        public bool DetailBusy
        {
            get { return GetProperty(() => DetailBusy); }
            set { SetProperty(() => DetailBusy, value); }
        }
        public string SoType
        {
            get { return GetProperty(() => SoType); }
            set { SetProperty(() => SoType, value); }
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
        public string BizCode
        {
            get { return GetProperty(() => BizCode); }
            set { SetProperty(() => BizCode, value); }
        }
        public string ItemCode
        {
            get { return GetProperty(() => ItemCode); }
            set { SetProperty(() => ItemCode, value); }
        }
        public string MajorCode { get { return "I0011"; } }
        public SalesPrintDocument salesPrintDocument { get; set; }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public ICommand ShowDialogCmd { get; set; }
        public ICommand PrintIVCmd { get; set; }
        public ICommand PrintPLCmd { get; set; }
        public AsyncCommand MouseDownCmd { get; set; }
        #endregion

        #region Report
        Invoice rptInvoice;
        PackingList rptPackingList;
        #endregion

        public SalesPrintDocumentVM()
        {
            BindingBizPartnerList();
            // 수주형태
            SoTypeList = new SalesOrderTypeConfigList().Where(u => u.IsEnabled == true);

            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now.AddMonths(1);
           // OnSearch();

            PrintIVCmd = new DelegateCommand(OnPrintIV, CanPrintIV);
            PrintPLCmd = new DelegateCommand(OnPrintPL, CanPrintPL);
            SearchCmd = new AsyncCommand(OnSearch, CanSearch);
            MouseDownCmd = new AsyncCommand(OnMouseDown);
        }

        // 시간이 많이 걸리는 작업이어서 async binding
        private async void BindingBizPartnerList()
        {
            var task = Task<IEnumerable<CommonBizPartner>>.Factory.StartNew(LoadingBizPartnerList);
            await task;

            if (task.IsCompleted)
            {
                BizCodeList = task.Result;
            }
        }

        private IEnumerable<CommonBizPartner> LoadingBizPartnerList()
        {
            return (new CommonBizPartnerList()).Where(u => u.BizType.Substring(0, 1) == "C");
        }

        public bool CanSearch() { return true; }
        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            salesPrintDocument = new SalesPrintDocument(StartDate, EndDate, SoType, BizCode);
            Collections = salesPrintDocument.SalesOrderReqHeader;
       
            Details = null;
            IsBusy = false;
        }

        public Task OnMouseDown()
        {
            DetailBusy = true;
            return Task.Factory.StartNew(MouseDownCore);
        }
        public void MouseDownCore()
        {
            try
            {
                if (SelectedItem != null && salesPrintDocument != null)
                    Details = salesPrintDocument.GetReqDatail(SelectedItem.GetType().GetProperty("ReqNo").GetValue(SelectedItem).ToString());
            }
            catch { }

            DetailBusy = false;
        }

        public bool CanPrintIV()
        {
            return SelectedItem != null;
        }
        public void OnPrintIV()
        {
            if (SelectedItem == null) return;

            rptInvoice = new Invoice();
            rptInvoice.Parameters["ReqNo"].Value = SelectedItem.GetType().GetProperty("ReqNo").GetValue(SelectedItem).ToString();
            rptInvoice.CreateDocument(false);

            var preview = new DocumentPreviewControlEx { DocumentSource = rptInvoice };

            var wnd = new DXWindow() { Content = preview, Title = "Invoice" };
            wnd.Show();
        }

        public bool CanPrintPL()
        {
            return SelectedItem != null;
        }
        public void OnPrintPL()
        {
            if (SelectedItem == null) return;

            rptPackingList = new PackingList();
            rptPackingList.Parameters["ReqNo"].Value = SelectedItem.GetType().GetProperty("ReqNo").GetValue(SelectedItem).ToString();
            rptPackingList.CreateDocument(false);

            var preview = new DocumentPreviewControlEx { DocumentSource = rptPackingList };

            var wnd = new DXWindow() { Content = preview, Title = "Packing List" };
            wnd.Show();
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
