using System;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;

namespace MesAdmin.ViewModels
{
    public class PopupSalesOrderDlvyVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        ICurrentWindowService CurrentWindowService { get { return GetService<ICurrentWindowService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public string DnNo
        {
            get { return GetProperty(() => DnNo); }
            set { SetProperty(() => DnNo, value); }
        }
        public IEnumerable<SalesOrderDlvyHeader> CollectionsHeader
        {
            get { return GetProperty(() => CollectionsHeader); }
            set { SetProperty(() => CollectionsHeader, value); }
        }
        public SalesOrderDlvyHeader SelectedHeader
        {
            get { return GetProperty(() => SelectedHeader); }
            set { SetProperty(() => SelectedHeader, value); }
        }
        public SalesOrderDlvyHeader ConfirmHeader
        {
            get { return GetProperty(() => ConfirmHeader); }
            set { SetProperty(() => ConfirmHeader, value); }
        }
        public SalesOrderDlvyDetailList CollectionsDetail
        {
            get { return GetProperty(() => CollectionsDetail); }
            set { SetProperty(() => CollectionsDetail, value); }
        }
        public IEnumerable<CommonBizPartner> BizPartnerList { get; set; }
        public string BizCode
        {
            get { return GetProperty(() => BizCode); }
            set { SetProperty(() => BizCode, value); }
        }
        public IEnumerable<SalesOrderTypeConfig> OrderType { get; set; }
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
        public ObservableCollection<CodeName> FlagCollections { get; set; }
        public string PackingFlag
        {
            get { return GetProperty(() => PackingFlag); }
            set { SetProperty(() => PackingFlag, value); }
        }
        public string PostFlag
        {
            get { return GetProperty(() => PostFlag); }
            set { SetProperty(() => PostFlag, value); }
        }
        public bool IsEnabledPost
        {
            get { return GetProperty(() => IsEnabledPost); }
            set { SetProperty(() => IsEnabledPost, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public List<UICommand> DialogCmds { get; private set; }
        protected UICommand ConfirmUICmd { get; private set; }
        protected UICommand CancelUICmd { get; private set; }
        public ICommand ConfirmCmd { get; set; }
        public ICommand MouseDownCmd { get; set; }
        #endregion

        public PopupSalesOrderDlvyVM() : this("N", "N") { }
        public PopupSalesOrderDlvyVM(string packing, string salesPost)
        {
            BizPartnerList = (new CommonBizPartnerList()).Where(u => u.BizType == "C" || u.BizType == "CV");
            OrderType = (new SalesOrderTypeConfigList()).Where(u => u.IsEnabled == true); // 수주형태
            FlagCollections = GlobalCommonPackingFlag.Instance;
            PackingFlag = packing;
            PostFlag = salesPost;
            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now.AddMonths(1);
            IsEnabledPost = packing == "Y";

            // dialog command
            ConfirmUICmd = new UICommand()
            {
                Caption = "확인",
                IsDefault = false,
                IsCancel = false,
                Command = new DelegateCommand(() => ConfirmHeader = SelectedHeader),
                Id = MessageBoxResult.OK,
            };
            CancelUICmd = new UICommand()
            {
                Caption = "취소",
                IsCancel = true,
                IsDefault = false,
                Id = MessageBoxResult.Cancel,
            };
            DialogCmds = new List<UICommand>() { ConfirmUICmd, CancelUICmd };

            // command
            SearchCmd = new AsyncCommand(OnSearch);
            ConfirmCmd = new DelegateCommand(OnConfirm);
            MouseDownCmd = new DelegateCommand(OnMouseDown);

            OnSearch();
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            string dnNo = DnNo;
            string soType = SoType;
            string bizCode = BizCode;
            string packingFlag = PackingFlag;
            string postFlag = PostFlag;

            CollectionsHeader = new SalesOrderDlvyHeaderList(StartDate, EndDate);
            CollectionsHeader = CollectionsHeader
                                .Where(p => p.PostFlag == postFlag)
                                .Where(p => p.PackingFlag == packingFlag)
                                .Where(p => string.IsNullOrEmpty(dnNo) ? true : p.DnNo == dnNo)
                                .Where(p => string.IsNullOrEmpty(soType) ? true : p.SoType == soType)
                                .Where(p => string.IsNullOrEmpty(bizCode) ? true : p.ShipTo == bizCode);
            CollectionsDetail = null;
            IsBusy = false;
        }

        public void OnMouseDown()
        {
            if (SelectedHeader != null)
                CollectionsDetail = new SalesOrderDlvyDetailList(dnNo: SelectedHeader.DnNo);
        }

        public void OnConfirm()
        {
            ConfirmHeader = SelectedHeader;
            CurrentWindowService.Close();
        }
    }
}
