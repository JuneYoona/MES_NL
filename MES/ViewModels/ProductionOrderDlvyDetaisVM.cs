using System;
using System.Windows.Input;
using DevExpress.Mvvm;
using MesAdmin.Models;
using MesAdmin.Common.Common;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using DevExpress.Mvvm.POCO;
using DevExpress.XtraReports.UI;

namespace MesAdmin.ViewModels
{
    public class ProductionOrderDlvyDetaisVM : ViewModelBase
    {
        #region Services
        IDialogService PopupItemView { get { return GetService<IDialogService>("ItemView"); } }
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
        public DataTable Details
        {
            get { return GetProperty(() => Details); }
            set { SetProperty(() => Details, value); }
        }
        public IEnumerable<CommonBizPartner> BizPartnerList
        {
            get { return GetProperty(() => BizPartnerList); }
            set { SetProperty(() => BizPartnerList, value); }
        }
        public IEnumerable<SalesOrderTypeConfig> OrderType { get; set; }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
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
        public ICommand ShowDialogCmd { get; set; }
        public AsyncCommand MouseDownCmd { get; set; }
        public ICommand PrintCOACmd { get; set; }
        #endregion

        public ProductionOrderDlvyDetaisVM()
        {
            BindingBizPartnerList();
            OrderType = new SalesOrderTypeConfigList().Where(u => u.IsEnabled == true); // 수주형태

            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now.AddMonths(1);
            ShowDialogCmd = new DelegateCommand(OnShowDialog);
            SearchCmd = new AsyncCommand(OnSearch, CanSearch);
            MouseDownCmd = new AsyncCommand(OnMouseDown);
            PrintCOACmd = new DelegateCommand(OnPrintCOA, CanPrintCOA);
        }

        // 시간이 많이 걸리는 작업이어서 async binding
        private async void BindingBizPartnerList()
        {
            var task = Task<IEnumerable<CommonBizPartner>>.Factory.StartNew(LoadingBizPartnerLiss);
            await task;

            if (task.IsCompleted)
            {
                BizPartnerList = task.Result;
            }
        }

        private IEnumerable<CommonBizPartner> LoadingBizPartnerLiss()
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
            Collections = new SalesOrderDlvyHeaderTable(StartDate, EndDate, SoType, ItemCode, BizCode).Collections;
            Details = null;
            IsBusy = false;
        }

        public Task OnMouseDown()
        {
            return Task.Factory.StartNew(MouseDownCore);
        }
        public void MouseDownCore()
        {
            if (SelectedItem != null)
                Details = new SalesOrderDlvyTable((string)SelectedItem.Row["ReqNo"], int.Parse(SelectedItem.Row["ReqSeq"].ToString())).Collections;
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

        public bool CanPrintCOA()
        {
            return SelectedItem != null;
        }
        public void OnPrintCOA()
        {
            // Minor Code에 등록된 성적서를 가져온다
            CommonMinor minor = new CommonMinorList(majorCode: "COAForm").Where(u => u.MinorCode == (string)SelectedItem.Row["BizCode"]).FirstOrDefault();
            string reportName = minor == null || string.IsNullOrEmpty(minor.MinorCode) ? "COAForDefault" : minor.Ref01;
            
            Type type = Type.GetType("MesAdmin.Reports." + reportName, true);
            XtraReport report = (XtraReport)Activator.CreateInstance(type);
            
            try
            {
                foreach (DataRow item in Details.Rows)
                {
                    XtraReport temp = (XtraReport)Activator.CreateInstance(type);
                    temp.Parameters["ReqNo"].Value = (string)SelectedItem.Row["ReqNo"];
                    temp.Parameters["ReqSeq"].Value = int.Parse(SelectedItem.Row["ReqSeq"].ToString());
                    temp.Parameters["ItemCode"].Value = (string)SelectedItem.Row["ItemCode"];
                    temp.Parameters["LotNo"].Value = (string)item["LOT_NO"];
                    temp.Parameters["Qty"].Value = decimal.Parse(item["GI_QTY"].ToString());
                    temp.CreateDocument();
                    report.Pages.AddRange(temp.Pages);
                }
            }
            catch(Exception ex)
            {
                MessageBoxService.ShowMessage(ex.Message, "Information", MessageButton.OK, MessageIcon.Information);
            }

            DevExpress.Xpf.Printing.PrintHelper.ShowPrintPreview(System.Windows.Application.Current.MainWindow, report);
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
