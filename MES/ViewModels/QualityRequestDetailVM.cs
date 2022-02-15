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

namespace MesAdmin.ViewModels
{
    public class QualityRequestDetailVM : ViewModelBase
    {
        #region Services
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
        public IEnumerable<CommonBizPartner> BizPartnerList { get; set; }
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
        public ICommand MouseDoubleClickCmd { get; set; }
        #endregion

        public QualityRequestDetailVM()
        {
            Messenger.Default.Register<string>(this, OnMessage);

            BizAreaCodeList = GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0004");
            if (!string.IsNullOrEmpty(DSUser.Instance.BizAreaCode))
                BizAreaCode = BizAreaCodeList.FirstOrDefault(u => u.MinorCode == DSUser.Instance.BizAreaCode).MinorCode;

            BindingBizPartnerList();
            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now;
            SearchCmd = new AsyncCommand(OnSearch, CanSearch);
            MouseDoubleClickCmd = new DelegateCommand(OnMouseDoubleClick, CanMouseDoubleClick);

            // IQC 용
            BizPartnerList = GlobalCommonBizPartner.Instance.Where(u => (u.BizType == "V" || u.BizType == "CV") && u.IsEnabled == true);
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
            return GlobalCommonBizPartner.Instance.Where(u => (u.BizType == "V" || u.BizType == "CV") && u.IsEnabled == true);
        }

        public bool CanSearch() { return true; }
        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            DataTable dt = new QualityRequestList().GetRequestDetail(QrType, StartDate, EndDate, BizCode, BizAreaCode);
            Collections = dt;
            IsBusy = false;
        }

        public bool CanMouseDoubleClick()
        {
            return SelectedItem != null;
        }
        public void OnMouseDoubleClick()
        {
            DocumentParamter parameter = Parameter as DocumentParamter; // Menu paramter
            string viewName = string.Empty;
            string title = string.Empty;

            switch (QrType)
            {
                case "IQC":
                    viewName = "QualityRequestIQCView";
                    title = "수입검사등록";
                    break;
                case "LQC":
                    viewName = "QualityRequestLQCView";
                    title = "공정검사등록";
                    break;
                case "FQC":
                    viewName = "QualityRequestFQCView";
                    title = "최종검사등록";
                    break;
                case "OQC":
                    viewName = "QualityRequestOQCView";
                    title = "포장검사등록";
                    break;
                default:
                    break;
            }

            string[] pm = { (string)parameter.Item, (string)SelectedItem["QrNo"], SelectedItem["Order"].ToString() };
            string documentId = (string)SelectedItem["QrNo"] + SelectedItem["Order"].ToString();
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
