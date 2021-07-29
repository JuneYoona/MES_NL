using System;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;


namespace MesAdmin.ViewModels
{
    public class PopupPurcharseWarehousingVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        ICurrentWindowService CurrentWindowService { get { return GetService<ICurrentWindowService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public string GrNo
        {
            get { return GetProperty(() => GrNo); }
            set { SetProperty(() => GrNo, value); }
        }
        public IEnumerable<PurcharseWarehousing> CollectionsHeader
        {
            get { return GetProperty(() => CollectionsHeader); }
            set { SetProperty(() => CollectionsHeader, value); }
        }
        public PurcharseWarehousing ConfirmHeader
        {
            get { return GetProperty(() => ConfirmHeader); }
            set { SetProperty(() => ConfirmHeader, value); }
        }
        public PurcharseWarehousing SelectedHeader
        {
            get { return GetProperty(() => SelectedHeader); }
            set { SetProperty(() => SelectedHeader, value); }
        }
        public PurcharseWarehousingList CollectionsDetail
        {
            get { return GetProperty(() => CollectionsDetail); }
            set { SetProperty(() => CollectionsDetail, value); }
        }
        public ObservableCollection<PurcharseOrderDetail> SelectedItems
        {
            get { return GetProperty(() => SelectedItems); }
            set { SetProperty(() => SelectedItems, value); }
        }
        public ObservableCollection<PurcharseWarehousing> ConfirmItems
        {
            get { return GetProperty(() => ConfirmItems); }
            set { SetProperty(() => ConfirmItems, value); }
        }
        public IEnumerable<CommonBizPartner> BizPartnerList { get; set; }
        public string SelectedPartner
        {
            get { return GetProperty(() => SelectedPartner); }
            set { SetProperty(() => SelectedPartner, value); }
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

        public PopupPurcharseWarehousingVM()
        {
            BizPartnerList = (new CommonBizPartnerList()).Where(u => u.BizType == "V" || u.BizType == "CV");
            StartDate = DateTime.Now.AddMonths(-2);
            EndDate = DateTime.Now;

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
            CollectionsHeader = new PurcharseWarehousingHeaderList(StartDate, EndDate, GrNo);
            CollectionsHeader = CollectionsHeader
                                .Where(p => string.IsNullOrEmpty(SelectedPartner) ? true : p.BizCode == SelectedPartner);
            CollectionsDetail = null;
            IsBusy = false;
        }

        public void OnMouseDown()
        {
            if (SelectedHeader != null)
                CollectionsDetail = new PurcharseWarehousingList(grNo: SelectedHeader.GrNo);
        }

        public void OnConfirm()
        {
            ConfirmHeader = SelectedHeader;
            CurrentWindowService.Close();
        }
    }
}