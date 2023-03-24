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
    public class PopupSalesDlvyNoteVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        ICurrentWindowService CurrentWindowService { get { return GetService<ICurrentWindowService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public IEnumerable<SalesDlvyNoteHeader> CollectionsHeader
        {
            get { return GetProperty(() => CollectionsHeader); }
            set { SetProperty(() => CollectionsHeader, value); }
        }
        public SalesDlvyNoteHeader SelectedHeader
        {
            get { return GetProperty(() => SelectedHeader); }
            set { SetProperty(() => SelectedHeader, value); }
        }
        public SalesDlvyNoteHeader ConfirmHeader
        {
            get { return GetProperty(() => ConfirmHeader); }
            set { SetProperty(() => ConfirmHeader, value); }
        }
        public SalesDlvyNoteDetailList CollectionsDetail
        {
            get { return GetValue<SalesDlvyNoteDetailList>(); }
            set { SetValue(value); }
        }
        public IEnumerable<CommonBizPartner> BizPartnerList
        {
            get { return GetProperty(() => BizPartnerList); }
            set { SetProperty(() => BizPartnerList, value); }
        }
        public string BizCode
        {
            get { return GetProperty(() => BizCode); }
            set { SetProperty(() => BizCode, value); }
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
        public AsyncCommand MouseDownCmd { get; set; }
        #endregion

        public PopupSalesDlvyNoteVM() : this("N", "N") { }
        public PopupSalesDlvyNoteVM(string packing, string salesPost)
        {
            BizAreaCodeList = GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0004" && u.IsEnabled == true);
            if (!string.IsNullOrEmpty(DSUser.Instance.BizAreaCode))
                BizAreaCode = DSUser.Instance.BizAreaCode;

            // 업체정보가져오기
            Task<IEnumerable<CommonBizPartner>>.Factory.StartNew(LoadingBizPartnerList)
                .ContinueWith(task => { BizPartnerList = task.Result; });

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
            MouseDownCmd = new AsyncCommand(OnMouseDown);

            Task.Factory.StartNew(SearchCore);
        }

        private IEnumerable<CommonBizPartner> LoadingBizPartnerList()
        {
            return GlobalCommonBizPartner.Instance.Where(u => u.BizType == "C" || u.BizType == "CS");
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            string soType = SoType;
            string bizCode = BizCode;
            string packingFlag = PackingFlag;
            string postFlag = PostFlag;

            CollectionsHeader = new SalesDlvyNoteHeaderList(BizAreaCode, StartDate, EndDate);
            CollectionsHeader = CollectionsHeader
                                .Where(p => p.PostFlag == postFlag)
                                .Where(p => p.PackingFlag == packingFlag)
                                .Where(p => string.IsNullOrEmpty(soType) ? true : p.SoType == soType)
                                .Where(p => string.IsNullOrEmpty(bizCode) ? true : p.ShipTo == bizCode);
            CollectionsDetail = null;
            IsBusy = false;
        }

        public Task OnMouseDown()
        {
            return Task.Run(new Action(() =>
            {
                if (SelectedHeader != null)
                    CollectionsDetail = new SalesDlvyNoteDetailList(dnNo: SelectedHeader.DnNo);
            }));
        }

        public void OnConfirm()
        {
            ConfirmHeader = SelectedHeader;
            CurrentWindowService.Close();
        }
    }
}