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
using System.Collections.ObjectModel;

namespace MesAdmin.ViewModels
{
    public class QualityPrecedenceLotListVM : ViewModelBase
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
        public DataRowView SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public DataTable Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        public ObservableCollection<ItemInfo> Type { get; set; }
        public string SelectedType
        {
            get { return GetProperty(() => SelectedType); }
            set { SetProperty(() => SelectedType, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public ICommand ShowDialogCmd { get; set; }
        public ICommand MouseDoubleClickCmd { get; set; }
        #endregion

        public QualityPrecedenceLotListVM()
        {
            Messenger.Default.Register<string>(this, OnMessage);

            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now.AddMonths(1);

            Type = new ObservableCollection<ItemInfo>();
            Type.Add(new ItemInfo { Text = "선행검사출하품", Value = "NotComplete" });
            Type.Add(new ItemInfo { Text = "선행검사완료품", Value = "Complete" });

            SearchCmd = new AsyncCommand(OnSearch);
            ShowDialogCmd = new DelegateCommand(OnShowDialog);
            MouseDoubleClickCmd = new DelegateCommand(OnMouseDoubleClick);

            // 업체정보가져오기
            Task<IEnumerable<CommonBizPartner>>.Factory.StartNew(LoadingBizPartnerList)
                .ContinueWith(task => { BizPartnerList = task.Result; });
        }

        private IEnumerable<CommonBizPartner> LoadingBizPartnerList()
        {
            return GlobalCommonBizPartner.Instance.Where(u => u.BizType.Substring(0, 1) == "C");
        }

        public void OnMouseDoubleClick()
        {
            if (SelectedItem == null) return;

            string documentId = (string)SelectedItem["QrNo"];
            IDocument document = FindDocument(documentId);
            if (document == null)
            {
                ((MainViewModel)MainViewModel).TabLoadingOpen();
                document = DocumentManagerService.CreateDocument("QualityPrecedenceLotView", new DocumentParamter(EntityMessageType.Changed, (string)SelectedItem["QrNo"], MainViewModel), this);
                document.DestroyOnClose = true;
                document.Id = documentId;
                document.Title = "선행로트 등록";
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

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore).ContinueWith(task => IsBusy = false);
        }
        public void SearchCore()
        {
            DateTime startDate = StartDate;
            DateTime endDate = EndDate;
            string bizCode = BizCode;
            string itemCode = ItemCode;
            string state = SelectedType;

            Collections = new QualityPrecedenceLot().GetQualityPrecedenceLotList(startDate, endDate, itemCode, bizCode, state);
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
