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
    public class PopupStockMoveVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        ICurrentWindowService CurrentWindowService { get { return GetService<ICurrentWindowService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public string DocumentNo
        {
            get { return GetProperty(() => DocumentNo); }
            set { SetProperty(() => DocumentNo, value); }
        }
        public string TransType { get; set; }
        public IEnumerable<StockMovementHeader> CollectionsHeader
        {
            get { return GetProperty(() => CollectionsHeader); }
            set { SetProperty(() => CollectionsHeader, value); }
        }
        public StockMovementHeader SelectedHeader
        {
            get { return GetProperty(() => SelectedHeader); }
            set { SetProperty(() => SelectedHeader, value); }
        }
        public StockMovementHeader ConfirmHeader
        {
            get { return GetProperty(() => ConfirmHeader); }
            set { SetProperty(() => ConfirmHeader, value); }
        }
        public StockMovementDetailList CollectionsDetail
        {
            get { return GetProperty(() => CollectionsDetail); }
            set { SetProperty(() => CollectionsDetail, value); }
        }
        public DateTime FromDate
        {
            get { return GetProperty(() => FromDate); }
            set { SetProperty(() => FromDate, value); }
        }
        public DateTime ToDate
        {
            get { return GetProperty(() => ToDate); }
            set { SetProperty(() => ToDate, value); }
        }
        public IEnumerable<CommonMinor> BizArea { get; private set; }
        public string EditBizArea
        {
            get { return GetProperty(() => EditBizArea); }
            set { SetProperty(() => EditBizArea, value); }
        }
        public IEnumerable<CommonMinor> MoveType { get; set; }
        public string EditMoveType
        {
            get { return GetProperty(() => EditMoveType); }
            set { SetProperty(() => EditMoveType, value); }
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

        public PopupStockMoveVM() : this("") { }
        public PopupStockMoveVM(string transType)
        {
            TransType = transType;
            FromDate = DateTime.Now.AddMonths(-1);
            ToDate = DateTime.Now;

            // default value binding
            BizArea = GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0004");
            if (!string.IsNullOrEmpty(DSUser.Instance.BizAreaCode))
                EditBizArea = BizArea.FirstOrDefault(u => u.MinorCode == DSUser.Instance.BizAreaCode).MinorCode;

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
            string bizAreaCode = EditBizArea;
            string moveType = EditMoveType;
            string documentNo = DocumentNo;

            CollectionsHeader = new StockMovementHeaderList(FromDate, ToDate, TransType);
            CollectionsHeader = CollectionsHeader
                                .Where(p => string.IsNullOrEmpty(bizAreaCode) ? true : p.BizAreaCode == bizAreaCode)
                                .Where(p => string.IsNullOrEmpty(moveType) ? true : p.MoveType == moveType)
                                .Where(p => string.IsNullOrEmpty(documentNo) ? true : p.DocumentNo == documentNo);
            CollectionsDetail = null;
            IsBusy = false;
        }

        public void OnMouseDown()
        {
            if (SelectedHeader != null)
                CollectionsDetail = new StockMovementDetailList ( documentNo : SelectedHeader.DocumentNo );
        }

        public void OnConfirm()
        {
            ConfirmHeader = SelectedHeader;
            CurrentWindowService.Close();
        }
    }
}
