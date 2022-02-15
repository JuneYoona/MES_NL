using System;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;
using MesAdmin.Models;
using MesAdmin.Common.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Input;
using DevExpress.Mvvm.POCO;

namespace MesAdmin.ViewModels
{
    public class QualityRequestDetailFromERPVM : ViewModelBase
    {
        #region Services
        IDialogService PopupItemView { get { return GetService<IDialogService>("ItemView"); } }
        IDialogService PopupQualityResultIMRView { get { return GetService<IDialogService>("QualityResultIMRView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        IDocumentManagerService DocumentManagerService { get { return GetService<IDocumentManagerService>(); } }
        #endregion

        #region Public Properties
        public MainViewModel MainViewModel { get { return (MainViewModel)((ISupportParentViewModel)this).ParentViewModel; } }
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
        public ObservableCollection<Column> Columns { get; private set; }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        public string QrType
        {
            get
            {
                string qrType = string.Empty;
                DocumentParamter pm = Parameter as DocumentParamter;
                if (pm.Type == EntityMessageType.Added)
                    qrType = (string)pm.Item;
                return qrType;
            }
        }
        public string BizAreaCode
        {
            get
            {
                string bizAreaCode = string.Empty;
                DocumentParamter pm = Parameter as DocumentParamter;
                if (pm.Type == EntityMessageType.Added)
                    bizAreaCode = (string)pm.BizAreaCode;
                return bizAreaCode;
            }
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
        public IEnumerable<CommonBizPartner> BizCodeList
        {
            get { return GetProperty(() => BizCodeList); }
            set { SetProperty(() => BizCodeList, value); }
        }
        public string ItemCode
        {
            get { return GetProperty(() => ItemCode); }
            set { SetProperty(() => ItemCode, value); }
        }
        public string Visibility
        {
            get { return GetProperty(() => Visibility); }
            set { SetProperty(() => Visibility, value); }
        }
        public bool VisibleBiz
        {
            get { return GetProperty(() => VisibleBiz); }
            set { SetProperty(() => VisibleBiz, value); }
        }
        public bool VisibleDn
        {
            get { return GetProperty(() => VisibleDn); }
            set { SetProperty(() => VisibleDn, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public AsyncCommand Search2Cmd { get; set; }
        public ICommand ShowIMRCmd { get; set; }
        public ICommand ShowDialogCmd { get; set; }
        public ICommand MouseDoubleClickCmd { get; set; }
        #endregion

        public QualityRequestDetailFromERPVM()
        {
            Messenger.Default.Register<string>(this, OnMessage);

            Columns = new ObservableCollection<Column>();

            BindingBizPartnerList();
            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now;
            SearchCmd = new AsyncCommand(OnSearch, CanSearch);
            Search2Cmd = new AsyncCommand(OnSearch2, () => !string.IsNullOrEmpty(ItemCode));
            ShowIMRCmd = new DelegateCommand(OnShowIMR, () => Collections != null && Collections.Rows.Count > 0);
            ShowDialogCmd = new DelegateCommand(OnShowDialog);
            MouseDoubleClickCmd = new DelegateCommand(OnMouseDoubleClick, CanMouseDoubleClick);
        }

        // 시간이 많이 걸리는 작업이어서 async binding
        private async void BindingBizPartnerList()
        {
            var task = Task.Run(() => { return GlobalCommonBizPartner.Instance; });
            await task;

            if (task.IsCompleted)
            {
                BizCodeList = task.Result;
            }
        }

        public bool CanSearch() { return true; }
        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            DataTable dt = new QualityRequestFromERPList().GetRequestDetail(QrType, StartDate, EndDate, BizCode, BizAreaCode);
            Collections = dt;
            IsBusy = false;
        }

        public Task OnSearch2()
        {
            IsBusy = true;
            return Task.Factory.StartNew(Search2Core).ContinueWith(task => IsBusy = false);
        }
        public void Search2Core()
        {
            DataTable dt = new QualityRequestFromERPList().GetResultDetail("IQC", ItemCode, StartDate, EndDate, BizCode);

            Columns.Clear();
            if (dt != null)
            {
                foreach (DataColumn col in dt.Columns)
                {
                    Columns.Add(
                        new Column
                        {
                            FieldName = col.ColumnName,
                            Width = col.ColumnName == "차수" || col.ColumnName == "단위" ? 50 : 100,
                            Settings = SettingsType.Default
                        });
                }
            }

            Collections = dt;
        }

        public bool CanMouseDoubleClick()
        {
            return SelectedItem != null;
        }
        public void OnMouseDoubleClick()
        {
            DocumentParamter parameter = Parameter as DocumentParamter; // Menu paramter
            string viewName = "QualityRequestIQCVFromERPView";
            string title = "수입검사등록";
            
            string qrNo = Collections.Columns.Contains("QrNo") ? (string)SelectedItem["QrNo"] : (string)SelectedItem["검사요청번호"];
            string order = Collections.Columns.Contains("Order") ? (string)SelectedItem["Order"] : (string)SelectedItem["차수"];

            string[] pm = { (string)parameter.Item, qrNo, order };
            string documentId = qrNo + order;
            IDocument document = MainViewModel.FindDocument(documentId);
            if (document == null)
            {
                MainViewModel.TabLoadingOpen();
                document = MainViewModel.CreateDocument(viewName, title, new DocumentParamter(EntityMessageType.Changed, pm, BizAreaCode, MainViewModel));
                document.DestroyOnClose = true;
                document.Id = documentId;
            }

            document.Show();
            SelectedItem = null;
        }

        public void OnShowDialog()
        {
            var vmItem = ViewModelSource.Create(() => new PopupItemVM("35"));
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

        public void OnShowIMR()
        {
            var vmPopup = ViewModelSource.Create(() => new QualityResultIMRVM(Collections, "IQC"));
            PopupQualityResultIMRView.ShowDialog(
                dialogCommands: vmPopup.DialogCmds,
                title: "검사현황 관리도",
                viewModel: vmPopup
            );
        }

        void OnMessage(string pm)
        {
            if (pm == "Refresh")
                OnSearch();
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (IsInDesignMode) return;

            // 거래처 검색 control, column binding(IQC만 사용, 출하로트는 FQC)
            Visibility = QrType == "IQC" ? "Visible" : "Hidden";
            VisibleBiz = QrType == "IQC" ? true : false;
            VisibleDn = QrType == "FQC" ? true : false;

            Task.Run(SearchCore).ContinueWith(task => MainViewModel.TabLoadingClose());
        }
    }
}