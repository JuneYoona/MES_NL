using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Printing;
using DevExpress.XtraReports.UI;
using MesAdmin.Common.Common;
using MesAdmin.Models;
using MesAdmin.Reports;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MesAdmin.ViewModels
{
    public class SalesLabelPrintHistoryVM : ViewModelBase
    {
        #region Services
        DevExpress.Mvvm.IDialogService PopupItemView { get { return GetService<DevExpress.Mvvm.IDialogService>("ItemView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        IDocumentManagerService DocumentManagerService { get { return GetService<IDocumentManagerService>(); } }
        #endregion

        #region Public Properties
        public DataTable Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public DataRowView SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public SalesLabelPrintHistoryList Details
        {
            get { return GetProperty(() => Details); }
            set { SetProperty(() => Details, value); }
        }
        public DataTable InspDetails
        {
            get { return GetProperty(() => InspDetails); }
            set { SetProperty(() => InspDetails, value); }
        }
        public SalesLabelPrintHistory SelectedDetail
        {
            get { return GetProperty(() => SelectedDetail); }
            set { SetProperty(() => SelectedDetail, value); }
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
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public ICommand ShowItemDialogCmd { get; set; }
        public ICommand PrintCmd { get; set; }
        public ICommand PrintPackingCmd { get; set; }        
        public AsyncCommand MouseDownCmd { get; set; }
        public DelegateCommand<object> DelCmd { get; set; }
        public AsyncCommand SaveCmd { get; set; }
        #endregion

        #region Report
        XtraReport report;
        #endregion

        public SalesLabelPrintHistoryVM()
        {
            BindingBizPartnerList();
            // 수주형태
            SoTypeList = new SalesOrderTypeConfigList().Where(u => u.IsEnabled == true);

            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now.AddMonths(1);

            ShowItemDialogCmd = new DelegateCommand(OnShowDialog);
            PrintCmd = new DelegateCommand(OnPrint, () => SelectedItem != null);
            PrintPackingCmd = new DelegateCommand(OnPrintPacking, () => SelectedItem != null);
            SearchCmd = new AsyncCommand(OnSearch, CanSearch);
            MouseDownCmd = new AsyncCommand(OnMouseDown);
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            DelCmd = new DelegateCommand<object>(Delete, CanDel);
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
            return new CommonBizPartnerList().Where(u => u.BizType.Substring(0, 1) == "C");
        }

        public bool CanSearch() { return true; }
        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore).ContinueWith(t => IsBusy = false);
        }
        public void SearchCore()
        {
            Collections = new SalesOrderDlvyHeaderTable(StartDate, EndDate, SoType, ItemCode, BizCode).Collections;
            Details = null;
            InspDetails = null;
        }

        public Task OnMouseDown()
        {
            DetailBusy = true;
            SelectedDetail = null;
            return Task.Factory.StartNew(MouseDownCore).ContinueWith(t => DetailBusy = false);
        }
        public void MouseDownCore()
        {
            if (SelectedItem == null) return;

            string reqNo = (string)SelectedItem.Row["ReqNo"];
            int reqSeq = int.Parse(SelectedItem.Row["ReqSeq"].ToString());

            Details = new SalesLabelPrintHistoryList(reqNo, reqSeq);
            InspDetails = Commonsp.usps_sales_PrintInspection(reqNo, reqSeq);
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

        bool CanDel(object obj) { return SelectedDetail != null; }
        public void Delete(object obj)
        {
            Details.Where(u => SelectedDetail.LotNo == u.LotNo).ToList().ForEach(u =>
            {
                    u.State = u.State == EntityState.Deleted ? EntityState.Unchanged : EntityState.Deleted;
            });
        }

        public bool CanSave()
        {
            if (Details == null) return false;
            return Details.Where(u => u.State == EntityState.Deleted).Count() > 0;
        }
        public Task OnSave()
        {
            DetailBusy = true;
            return Task.Factory.StartNew(SaveCore);
        }
        public void SaveCore()
        {
            try
            {
                Details.Save();
            }
            catch (Exception ex)
            {
                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(ex.Message
                                                    , "Information"
                                                    , MessageButton.OK
                                                    , MessageIcon.Information));
            }
            OnMouseDown();
        }

        public void OnPrint()
        {
            report = new SalesCheckList();
            report.Parameters["ReqNo"].Value = (string)SelectedItem.Row["ReqNo"];
            PrintHelper.ShowPrintPreview(System.Windows.Application.Current.MainWindow, report);
        }

        public void OnPrintPacking()
        {
            report = new PackingOrderBAC60();
            report.Parameters["ReqNo"].Value = (string)SelectedItem.Row["ReqNo"];
            report.Parameters["ReqSeq"].Value = int.Parse(SelectedItem.Row["ReqSeq"].ToString());
            PrintHelper.ShowPrintPreview(System.Windows.Application.Current.MainWindow, report);
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;
            Task.Factory.StartNew(SearchCore).ContinueWith(task =>
            {
                ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
            });
        }
    }
}