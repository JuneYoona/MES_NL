using System;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using MesAdmin.Common.Common;
using System.Collections.Generic;
using System.Collections.Specialized;
using DevExpress.Mvvm.POCO;
using System.Threading.Tasks;
using MesAdmin.Common.Utils;
using DevExpress.Xpf.Grid;

namespace MesAdmin.ViewModels
{
    public class SalesOrderReqVM : ViewModelBase
    {
        #region Services
        IDialogService PopupSalesOrderDetailView { get { return GetService<IDialogService>("PopupSalesOrderDetailView"); } }
        IDialogService PopupSalesOrderReqView { get { return GetService<IDialogService>("PopupSalesOrderReqView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public SalesOrderReqHeader Header
        {
            get { return GetProperty(() => Header); }
            set { SetProperty(() => Header, value); }
        }
        public SalesOrderReqDetailList Details
        {
            get { return GetProperty(() => Details); }
            set { SetProperty(() => Details, value); }
        }
        public ObservableCollection<SalesOrderReqDetail> SelectedItems
        {
            get { return GetProperty(() => SelectedItems); }
            set { SetProperty(() => SelectedItems, value); }
        }
        public ObservableCollection<CommonMinor> WhCode { get; set; }
        public IEnumerable<CommonBizPartner> BizPartnerList { get; set; }
        public CommonBizPartner SelectedPartner
        {
            get { return GetProperty(() => SelectedPartner); }
            set { SetProperty(() => SelectedPartner, value); }
        }
        public IEnumerable<SalesOrderTypeConfig> OrderType { get; set; }
        #endregion

        #region Commands
        public bool IsNew
        {
            get { return GetProperty(() => IsNew); }
            set { SetProperty(() => IsNew, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        public ICommand ShowDialogCmd { get; set; }
        public ICommand BizPartneChangedCmd { get; set; }
        public ICommand NewCmd { get; set; }
        public DelegateCommand<object> DelCmd { get; set; }
        public AsyncCommand SaveCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        public ICommand ReferSoCmd { get; set; }
        public ICommand<CellValueChangedEvent> CellValueChangedCmd { get; set; }
        #endregion

        public SalesOrderReqVM()
        {
            BizPartnerList = (new CommonBizPartnerList()).Where(u => u.BizType == "C" || u.BizType == "CV");
            WhCode = new CommonMinorList
            (
                GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0011")
            ); // 창고정보
            OrderType = (new SalesOrderTypeConfigList()).Where(u => u.IsEnabled == true); // 수주형태
            IsNew = true;
            DelCmd = new DelegateCommand<object>(Delete, CanDel);
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            SearchCmd = new AsyncCommand(OnSearch, CanSearch);
            ReferSoCmd = new DelegateCommand(OnReferSo, CanReferSo);
            BizPartneChangedCmd = new DelegateCommand(OnBizPartneChanged);
            NewCmd = new DelegateCommand(OnNew);
            ShowDialogCmd = new DelegateCommand(OnShowDialog);
            CellValueChangedCmd = new DelegateCommand<CellValueChangedEvent>(OnCellValueChanged);

            SelectedItems = new ObservableCollection<SalesOrderReqDetail>();
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;

            Header = new SalesOrderReqHeader();
            Details = new SalesOrderReqDetailList();
        }

        private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DelCmd.RaiseCanExecuteChanged();
        }

        public bool CanSave()
        {
            bool ret = true;
            if (IsNew)
            {
                // 필수 입력값 처리
                foreach (var item in Details.Where(u => u.State == EntityState.Added))
                {
                    if (string.IsNullOrEmpty(item.WhCode) || item.Qty <= 0)
                    {
                        ret = false;
                        break;
                    }
                }
                if (Details.Count == 0) return false;
                if (Header.ReqDate == null) return false;
                if (string.IsNullOrEmpty(Header.MoveType)) return false;
            }
            else
                ret = Details.Where(u => u.State == EntityState.Deleted).Count() > 0;

            return ret;
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
                if (IsNew)
                {
                    Header.Currency = Details.FirstOrDefault().Currency;
                    Header.Save();
                    Details.ToList().ForEach(u => u.ReqNo = Header.ReqNo);
                }
                Details.Save();
                SearchCore();
                IsNew = false;
                // send mesaage to parent view
                Messenger.Default.Send<string>("Refresh");
            }
            catch (Exception ex)
            {
                if (IsNew)
                    Header.Delete();

                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(ex.Message
                                                    , "Information"
                                                    , MessageButton.OK
                                                    , MessageIcon.Information));
            }
            IsBusy = false;
        }

        public bool CanReferSo()
        {
            return !string.IsNullOrEmpty(Header.ShipTo) && !string.IsNullOrEmpty(Header.SoType) && Header.ReqDate != null && IsNew;
        }
        public void OnReferSo()
        {
            var vmOrderDetail = ViewModelSource.Create(() => new PopupSalesOrderDetailVM(Header.ShipTo, Header.SoType));
            PopupSalesOrderDetailView.ShowDialog(
                dialogCommands: vmOrderDetail.DialogCmds,
                title: "수주내역참조",
                viewModel: vmOrderDetail
            );

            if (vmOrderDetail.ConfirmItems.Count > 0)
            {
                foreach (var item in vmOrderDetail.ConfirmItems)
                {
                    Details.Add(item);
                }
                Header.MoveType = (new SalesOrderTypeConfigList()).Where(u => u.SoType == Header.SoType).FirstOrDefault().MoveType;
            }
        }

        public void OnShowDialog()
        {
            var vmReq = ViewModelSource.Create(() => new PopupSalesOrderReqVM());
            PopupSalesOrderReqView.ShowDialog(
                dialogCommands: vmReq.DialogCmds,
                title: "출하요청내역 조회",
                viewModel: vmReq
            );

            if (vmReq.ConfirmHeader != null)
            {
                Header.ReqNo = vmReq.ConfirmHeader.ReqNo;
                OnSearch();
                IsNew = false;
             }
        }

        bool CanDel(object obj) { return SelectedItems.Count > 0; }
        public void Delete(object obj)
        {
            SelectedItems.ToList().ForEach(u =>
            {
                if (u.State == EntityState.Added)
                    Details.Remove(u);
                else
                    u.State = u.State == EntityState.Deleted ? EntityState.Unchanged : EntityState.Deleted;
            });
        }

        public void OnBizPartneChanged()
        {
            if(IsNew)
            {
                Details.Clear();
                Header.MoveType = "";
            }
        }

        public void OnNew()
        {
            IsNew = true;
            Header = new SalesOrderReqHeader();
            Details.Clear();
        }

        public bool CanSearch()
        {
            return !string.IsNullOrEmpty(Header.ReqNo);
        }
        public Task OnSearch()
        {
            IsBusy = true;
            IsNew = false;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            string reqNo = Header.ReqNo;
            Header = new SalesOrderReqHeader(reqNo);
            Header.ReqNo = reqNo;
            Details = new SalesOrderReqDetailList(reqNo);
            IsBusy = false;

            if (Details.Count == 0)
                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage("출하요청 정보가 없습니다!"
                                                        , "Information"
                                                        , MessageButton.OK
                                                        , MessageIcon.Information));
        }

        // 단가, 환율에의한 출고금액 계산
        public void OnCellValueChanged(CellValueChangedEvent pm)
        {
            if (pm.e.Column.FieldName != "Qty" && pm.e.Column.FieldName != "UnitPrice" && pm.e.Column.FieldName != "ExchangeRate") return;

            TableView view = pm.sender as TableView;
            GridControl grid = view.Grid;
            int rh = pm.e.RowHandle;

            grid.SetCellValue(rh, "NetAmtLocal", (decimal)grid.GetCellValue(rh, "Qty") * (decimal)grid.GetCellValue(rh, "UnitPrice") * (decimal)grid.GetCellValue(rh, "ExchangeRate"));
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;
            if (pm.Type == EntityMessageType.Added) return;

            SalesOrderReqDetail item = pm.Item as SalesOrderReqDetail;
            Header.ReqNo = item.ReqNo;
            OnSearch();
            IsNew = false;
        }
    }
}
