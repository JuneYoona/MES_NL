using System;
using MesAdmin.Common.Common;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using System.Collections.Generic;
using DevExpress.Mvvm.POCO;
using System.Threading.Tasks;

namespace MesAdmin.ViewModels
{
    public class PurcharseWarehousingDetailsVM : ViewModelBase
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
        public IEnumerable<PurcharseWarehousing> Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public PurcharseWarehousing SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public PurcharseWarehousing FocusedRow
        {
            get { return GetProperty(() => FocusedRow); }
            set { SetProperty(() => FocusedRow, value); }
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
        #endregion

        public PurcharseWarehousingDetailsVM()
        {
            Messenger.Default.Register<string>(this, OnMessage);
            
            StartDate = DateTime.Now.AddMonths(-6);
            EndDate = DateTime.Now;
            BizPartnerList = (new CommonBizPartnerList()).Where(u => u.BizType == "V" || u.BizType == "CV");
            OnSearch();

            SearchCmd = new AsyncCommand(OnSearch);
            ShowDialogCmd = new DelegateCommand(OnShowDialog);
            MouseDoubleClickCmd = new DelegateCommand(OnMouseDoubleClick);
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

            Collections = new PurcharseWarehousingList(startDate: StartDate, endDate: EndDate);
            Collections = Collections
                            .Where(u => string.IsNullOrEmpty(bizCode) ? true : u.BizCode == bizCode)
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

            string documentId = SelectedItem.GrNo;
            IDocument document = FindDocument(documentId);
            if (document == null)
            {
                document = DocumentManagerService.CreateDocument("PurcharseWarehousingView", new DocumentParamter(EntityMessageType.Changed, SelectedItem), this);
                document.DestroyOnClose = true;
                document.Id = documentId;
                document.Title = "구매입고 등록";
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
    }
}
