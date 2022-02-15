using System;
using System.Data;
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
    public class PopupSalesOrderDetailVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        ICurrentWindowService CurrentWindowService { get { return GetService<ICurrentWindowService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public string SoNo
        {
            get { return GetProperty(() => SoNo); }
            set { SetProperty(() => SoNo, value); }
        }
        public IEnumerable<SalesOrderDetail> Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public ObservableCollection<SalesOrderDetail> SelectedItems
        {
            get { return GetProperty(() => SelectedItems); }
            set { SetProperty(() => SelectedItems, value); }
        }
        public ObservableCollection<SalesOrderReqDetail> ConfirmItems
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
        public ObservableCollection<CodeName> CloseFlagCollections { get; set; }
        public string CloseFlag
        {
            get { return GetProperty(() => CloseFlag); }
            set { SetProperty(() => CloseFlag, value); }
        }
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
        #endregion

        public PopupSalesOrderDetailVM() : this("", "") { }
        public PopupSalesOrderDetailVM(string bizPartner, string soType)
        {
            CloseFlagCollections = GlobalCommonCloseFlag.Instance;
            CloseFlag = "N";
            BizPartnerList = (new CommonBizPartnerList()).Where(u => u.BizType == "C" || u.BizType == "CV");
            SelectedPartner = bizPartner;
            SoType = soType;
            StartDate = DateTime.Now.AddMonths(-2);
            EndDate = DateTime.Now;
            SelectedItems = new ObservableCollection<SalesOrderDetail>();
            ConfirmItems = new ObservableCollection<SalesOrderReqDetail>();

            // dialog command
            ConfirmUICmd = new UICommand()
            {
                Caption = "확인",
                IsDefault = false,
                IsCancel = false,
                Command = new DelegateCommand(OnConfirm),
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

            OnSearch();
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            string soNo = SoNo;
            string soType = SoType;
            string bizCode = SelectedPartner;
            string closeFlag = CloseFlag;

            Collections = new SalesOrderDetailList(startDate: StartDate, endDate: EndDate);
            Collections = Collections
                            .Where(p => string.IsNullOrEmpty(soNo) ? true : p.SoNo == soNo)
                            .Where(p => string.IsNullOrEmpty(bizCode) ? true : p.ShipTo == bizCode)
                            .Where(p => string.IsNullOrEmpty(soType) ? true : p.SoType == soType)
                            .Where(p => string.IsNullOrEmpty(closeFlag) ? true : p.CloseFlag == closeFlag);
            IsBusy = false;
        }

        public void OnConfirm()
        {
            foreach (var item in SelectedItems)
            {
                ConfirmItems.Add(new SalesOrderReqDetail
                {
                    State = EntityState.Added,
                    ShipTo = item.ShipTo,
                    ItemCode = item.ItemCode,
                    ItemName = item.ItemName,
                    ItemSpec = item.ItemSpec,
                    CustItemCode = item.CustItemCode,
                    Qty = item.Qty,
                    UnitPrice = item.UnitPrice,
                    BasicUnit = item.BasicUnit,
                    VATRate = item.VATRate,
                    ExchangeRate = item.ExchangeRate,
                    NetAmtLocal = item.NetAmtLocal,
                    Currency = item.Currency,
                    SoNo = item.SoNo,
                    SoSeq = item.Seq,
                    WhCode = item.WhCode,
                });
            }

            CurrentWindowService.Close();
        }
    }
}
