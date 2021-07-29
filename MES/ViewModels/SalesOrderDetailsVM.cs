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
    public class SalesOrderDetailsVM : ViewModelBase
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
        public string CloseFlag
        {
            get { return GetProperty(() => CloseFlag); }
            set { SetProperty(() => CloseFlag, value); }
        }
        public string BizCode
        {
            get { return GetProperty(() => BizCode); }
            set { SetProperty(() => BizCode, value); }
        }
        public IEnumerable<CommonBizPartner> BizPartnerList { get; set; }
        public IEnumerable<SalesOrderDetail> Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public SalesOrderDetailList CheckCollections
        {
            get { return GetProperty(() => CheckCollections); }
            set { SetProperty(() => CheckCollections, value); }
        }
        public SalesOrderDetail SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public ObservableCollection<CodeName> CloseFlagCollections { get; set; }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public AsyncCommand SaveCmd { get; set; }
        public ICommand ShowDialogCmd { get; set; }
        public ICommand MouseDoubleClickCmd { get; set; }
        public ICommand SelectAllCmd { get; set; }
        #endregion

        public SalesOrderDetailsVM()
        {
            Messenger.Default.Register<string>(this, OnMessage);

            CloseFlagCollections = GlobalCommonCloseFlag.Instance;
            StartDate = DateTime.Now.AddMonths(-2);
            EndDate = DateTime.Now;
            BizPartnerList = (new CommonBizPartnerList()).Where(u => u.BizType == "C" || u.BizType == "CV");
            OnSearch();

            SearchCmd = new AsyncCommand(OnSearch);
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            ShowDialogCmd = new DelegateCommand(OnShowDialog);
            MouseDoubleClickCmd = new DelegateCommand(OnMouseDoubleClick);
            SelectAllCmd = new DelegateCommand(OnSelectAll);
        }

        public bool CanSave()
        {
            if (CheckCollections == null) return false;
            return CheckCollections.Where(u => u.IsChecked == true).Count() > 0;
        }
        public Task OnSave()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SaveCore);
        }
        public void SaveCore()
        {
            try
            {
                CheckCollections.Close();
                OnSearch();
            }
            catch (Exception ex)
            {
                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(ex.Message
                                                    , "Information"
                                                    , MessageButton.OK
                                                    , MessageIcon.Information));
            }
            IsBusy = false;
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
            string closeFlag = CloseFlag;

            Collections = new SalesOrderDetailList(startDate: StartDate, endDate: EndDate);
            Collections = Collections
                            .Where(u => string.IsNullOrEmpty(bizCode) ? true : u.ShipTo == bizCode)
                            .Where(u => string.IsNullOrEmpty(itemCode) ? true : u.ItemCode == itemCode)
                            .Where(u => string.IsNullOrEmpty(closeFlag) ? true : u.CloseFlag == closeFlag);

            CheckCollections = new SalesOrderDetailList(startDate: StartDate, endDate: EndDate);
            CheckCollections = new SalesOrderDetailList(
                            CheckCollections
                                .Where(u => u.CloseFlag == "N")
                                .Where(u => string.IsNullOrEmpty(bizCode) ? true : u.ShipTo == bizCode)
                                .Where(u => string.IsNullOrEmpty(itemCode) ? true : u.ItemCode == itemCode));
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

            string documentId = SelectedItem.SoNo;
            IDocument document = FindDocument(documentId);
            if (document == null)
            {
                document = DocumentManagerService.CreateDocument("SalesOrderView", new DocumentParamter(EntityMessageType.Changed, SelectedItem), this);
                document.DestroyOnClose = true;
                document.Id = documentId;
                document.Title = "수주등록";
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

        void OnMessage(string pm)
        {
            if (pm == "Refresh")
                OnSearch();
        }

        public void OnSelectAll()
        {
            CheckCollections.ToList().ForEach(u => u.IsChecked = true);
        }
    }
}
