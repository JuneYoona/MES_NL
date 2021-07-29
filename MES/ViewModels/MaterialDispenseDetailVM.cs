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
using MesAdmin.Reports;
using DevExpress.Xpf.Printing;
using DevExpress.XtraReports.UI;

namespace MesAdmin.ViewModels
{
    public class MaterialDispenseDetailVM : ViewModelBase
    {
        #region Services
        DevExpress.Mvvm.IDialogService PopupItemView { get { return GetService<DevExpress.Mvvm.IDialogService>("ItemView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        IDocumentManagerService DocumentManagerService { get { return GetService<IDocumentManagerService>(); } }
        #endregion

        #region Public Properties
        public object MainViewModel { get; set; }
        public IEnumerable<MaterialDispenseDetail> Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public MaterialDispenseDetail SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public MaterialDispenseDetailSubList Details
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
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public ICommand ShowDialogCmd { get; set; }
        public ICommand MouseDoubleClickCmd { get; set; }
        public ICommand MouseDownCmd { get; set; }
        public ICommand PrintCmd { get; set; }
        #endregion

        #region Report
        MaterialDispense report;
        #endregion

        public MaterialDispenseDetailVM()
        {
            Messenger.Default.Register<string>(this, OnMessage);

            StartDate = DateTime.Now.AddMonths(-2);
            EndDate = DateTime.Now;
            ShowDialogCmd = new DelegateCommand(OnShowDialog);
            SearchCmd = new AsyncCommand(OnSearch, CanSearch);
            MouseDoubleClickCmd = new DelegateCommand(OnMouseDoubleClick, CanMouseDoubleClick);
            MouseDownCmd = new DelegateCommand(OnMouseDown);
            PrintCmd = new DelegateCommand(OnPrint);
        }

        public bool CanSearch() { return true; }
        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            string itemCode = ItemCode;
            Collections = new MaterialDispenseDetailList(startDate: StartDate, endDate: EndDate)
                                .Where(u => string.IsNullOrEmpty(itemCode) ? true : u.ItemCode == itemCode)
                                .Where(u => u.PostFlag =="Y");
            Details = null;
            IsBusy = false;
        }

        public void OnMouseDown()
        {
            if (SelectedItem != null)
                Details = new MaterialDispenseDetailSubList(mdNo: SelectedItem.MDNo, seq: SelectedItem.Seq);
        }

        public bool CanMouseDoubleClick()
        {
            return SelectedItem != null;
        }
        public void OnMouseDoubleClick()
        {
            string viewName = string.Empty;
            string title = string.Empty;
             
            var pm = SelectedItem;
            string documentId = SelectedItem.MDNo + SelectedItem.Seq.ToString();
            IDocument document = FindDocument(documentId);
            if (document == null)
            {
                ((MainViewModel)MainViewModel).TabLoadingOpen();
                document = DocumentManagerService.CreateDocument("MaterialDispenseView", new DocumentParamter(EntityMessageType.Changed, pm, MainViewModel), this);
                document.DestroyOnClose = true;
                document.Id = documentId;
                document.Title = "자재불출승인";
            }
            document.Show();
            SelectedItem = null;
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

        public void OnPrint()
        {
            if (SelectedItem == null) return;

            report = new MaterialDispense();
            report.Parameters["MDNo"].Value = SelectedItem.MDNo;

            XtraReport sub = null;

            if (SelectedItem.InWhCode == "WE10") sub = new MaterialDispenseDetails();
            else sub = new MaterialDispenseDetailsBAC90();

            sub.Parameters["MDNo"].Value = SelectedItem.MDNo;
            report.xrSubreport1.ReportSource = sub;
            report.xrSubreport2.ReportSource = sub;

            PrintHelper.ShowPrintPreview(System.Windows.Application.Current.MainWindow, report);
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
