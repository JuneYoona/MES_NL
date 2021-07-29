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
    public class SalesOrderVM : ViewModelBase
    {
        #region Services
        DevExpress.Mvvm.IDialogService PopupItemView { get { return GetService<DevExpress.Mvvm.IDialogService>("PopupItemView"); } }
        DevExpress.Mvvm.IDialogService PopupSOView { get { return GetService<DevExpress.Mvvm.IDialogService>("PopupSOView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
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
        public SalesOrderHeader Header
        {
            get { return GetProperty(() => Header); }
            set { SetProperty(() => Header, value); }
        }
        public SalesOrderDetailList Details
        {
            get { return GetProperty(() => Details); }
            set { SetProperty(() => Details, value); }
        }
        public ObservableCollection<SalesOrderDetail> SelectedItems
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
        public ObservableCollection<CodeName> VATFlag
        {
            get { return GetProperty(() => VATFlag); }
            set { SetProperty(() => VATFlag, value); }
        }
        public IEnumerable<SalesOrderTypeConfig> OrderType { get; set; }
        public string Memo
        {
            get { return GetProperty(() => Memo); }
            set { SetProperty(() => Memo, value); }
        }
        public CommonItemList Items { get; set; }
        #endregion

        #region Commands
        public ICommand ShowDialogCmd { get; set; }
        public ICommand<HiddenEditorEvent> HiddenEditorCmd { get; set; }
        public ICommand AddCmd { get; set; }
        public ICommand NewCmd { get; set; }
        public DelegateCommand<object> DeleteCmd { get; set; }
        public AsyncCommand SaveCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        public ICommand EditValueChangedCmd { get; set; }
        public ICommand<CellValueChangedEvent> CellValueChangedCmd { get; set; }
        #endregion

        public SalesOrderVM()
        {
            BizPartnerList = (new CommonBizPartnerList()).Where(u => u.BizType == "C" || u.BizType == "CV");
            VATFlag = GlobalCommonIncude.Instance;
            WhCode = new CommonMinorList
            (
                GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0011")
            ); // 창고정보
            // 품목정보
            Task.Factory.StartNew(() => GlobalCommonItem.Instance).ContinueWith(task => { Items = task.Result; });
            OrderType = (new SalesOrderTypeConfigList()).Where(u => u.IsEnabled == true); // 수주형태
            IsNew = true;

            AddCmd = new DelegateCommand(Add, CanAdd);
            DeleteCmd = new DelegateCommand<object>(OnDelete, CanDelete);
            ShowDialogCmd = new DelegateCommand<string>(ShowDialog);
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            SearchCmd = new AsyncCommand(OnSearch, CanSearch);
            HiddenEditorCmd = new DelegateCommand<HiddenEditorEvent>(OnHiddenEditor);
            NewCmd = new DelegateCommand(OnNew);
            EditValueChangedCmd = new DelegateCommand(OnEditValueChanged);
            CellValueChangedCmd = new DelegateCommand<CellValueChangedEvent>(OnCellValueChanged);

            SelectedItems = new ObservableCollection<SalesOrderDetail>();
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;

            Header = new SalesOrderHeader();
            Details = new SalesOrderDetailList();
        }

        public void OnEditValueChanged()
        {
            if (SelectedPartner == null) return;

            Header.VATFlag = SelectedPartner.VATFlag;
            Header.VATRate = SelectedPartner.VATRate;
            Header.Currency = SelectedPartner.Currency;
            Header.SoDate = DateTime.Now;
            if (SelectedPartner.Currency == "KRW")
                Header.ExchangeRate = 1;
        }

        private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DeleteCmd.RaiseCanExecuteChanged();
        }

        public bool CanSave()
        {
            bool ret = true;
            if (IsNew)
            {
                // 필수 입력값 처리
                foreach (SalesOrderDetail item in Details.Where(u => u.State == EntityState.Added))
                {
                    if (string.IsNullOrEmpty(item.ItemCode) || item.Qty <= 0 || item.WhCode == null)
                    {
                        ret = false;
                        break;
                    }
                }
                if (Details.Count == 0) return false;
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
                if(IsNew)
                { 
                    // details 수주금액, 부가세 금액 sum
                    Header.NetAmt = Details.Sum(u => u.NetAmt);
                    Header.VATAmt = Details.Sum(u => u.VATAmt);
                    Header.NetAmtLocal = Header.NetAmt * Header.ExchangeRate;
                    Header.VATAmtLocal = Header.VATAmt * Header.ExchangeRate;

                    Header.BillTo = Header.ShipTo;
                    Header.Save();
                    Details.ToList().ForEach(u =>
                    {
                        u.SoNo = Header.SoNo;
                        u.ShipTo = Header.ShipTo;
                        u.ExchangeRate = Header.ExchangeRate.Value;
                    });
                    IsNew = false;
                }
                Details.Save();
                OnSearch();
                // send mesaage to parent view
                Messenger.Default.Send<string>("Refresh");
            }
            catch (Exception ex)
            {
                if(IsNew)
                    Header.Delete();

                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(ex.Message
                                                    , "Information"
                                                    , MessageButton.OK
                                                    , MessageIcon.Information));
            }
            IsBusy = false;
        }

        public bool CanSearch()
        {
            return !string.IsNullOrEmpty(Header.SoNo);
        }
        public Task OnSearch()
        {
            IsBusy = true;
            IsNew = false;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            string soNo = Header.SoNo;
            Header = new SalesOrderHeader(soNo);
            Header.SoNo = soNo;
            Details = new SalesOrderDetailList(soNo);
            IsBusy = false;

            if(Details.Count == 0)
                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage("수주정보가 없습니다!"
                                                        , "Information"
                                                        , MessageButton.OK
                                                        , MessageIcon.Information));
        }

        bool CanDelete(object obj) { return SelectedItems.Count > 0; }
        public void OnDelete(object obj)
        {
            SelectedItems.ToList().ForEach(u =>
            {
                if (u.State == EntityState.Added)
                    Details.Remove(u);
                else
                    u.State = u.State == EntityState.Deleted ? EntityState.Unchanged : EntityState.Deleted;
            });
        }

        // 품목코드 선택에 의해 품목명, SPEC, Basicunit 추가
        public void OnHiddenEditor(HiddenEditorEvent pm)
        {
            if (pm.e.Column.FieldName != "ItemCode") return;

            TableView view = pm.sender as TableView;
            GridControl grid = view.Grid;

            CommonItem item = Items.Where(u => u.ItemCode == (string)pm.e.Value).FirstOrDefault();
            if (item == null)
            {
                grid.SetCellValue(pm.e.RowHandle, "ItemCode", "");
                grid.SetCellValue(pm.e.RowHandle, "ItemName", "");
                grid.SetCellValue(pm.e.RowHandle, "ItemSpec", "");
            }
            else
            {
                grid.SetCellValue(pm.e.RowHandle, "ItemName", item.ItemName);
                grid.SetCellValue(pm.e.RowHandle, "ItemSpec", item.ItemSpec);
                grid.SetCellValue(pm.e.RowHandle, "BasicUnit", item.BasicUnit);
            }
        }

        // 수주량과 단가에 의해 수주금액, 부가세금액 자동 계산
        public void OnCellValueChanged(CellValueChangedEvent pm)
        {
            if (pm.e.Column.FieldName != "Qty" && pm.e.Column.FieldName != "UnitPrice") return;

            TableView view = pm.sender as TableView;
            GridControl grid = view.Grid;
            int rh = pm.e.RowHandle;

            grid.SetCellValue(rh, "NetAmt", (decimal)grid.GetCellValue(rh, "Qty") * (decimal)grid.GetCellValue(rh, "UnitPrice"));
            grid.SetCellValue(rh, "VATAmt", (decimal)grid.GetCellValue(rh, "NetAmt") * (decimal)grid.GetCellValue(rh, "VATRate") / 100);
        }

        public bool CanAdd()
        {
             return SelectedPartner != null && Header.ReqDlvyDate != null && Header.ExchangeRate != null && !string.IsNullOrEmpty(Header.SoType) && IsNew;
        }
        public void Add()
        {
            Details.Insert(Details.Count, new SalesOrderDetail
            {
                State = EntityState.Added,
                VATRate = Header.VATRate,
                ReqDlvyDate = Header.ReqDlvyDate
            });
        }

        public void ShowDialog(string pm)
        {
            if (pm == "PopupItemView") // 품목조회
            {
                var vmItem = ViewModelSource.Create(() => new PopupItemVM());
                PopupItemView.ShowDialog(
                    dialogCommands: vmItem.DialogCmds,
                    title: "품목선택",
                    viewModel: vmItem
                );

                if (vmItem.ConfirmItem != null)
                {
                    SelectedItems[0].ItemCode = vmItem.ConfirmItem.ItemCode;
                    SelectedItems[0].ItemName = vmItem.ConfirmItem.ItemName;
                    SelectedItems[0].ItemSpec = vmItem.ConfirmItem.ItemSpec;
                    SelectedItems[0].BasicUnit = vmItem.ConfirmItem.BasicUnit;
                    SelectedItems[0].WhCode = vmItem.ConfirmItem.OutWhCode;
                }
            }
            else
            {
                var vmSO = ViewModelSource.Create(() => new PopupSalesOrderVM());
                PopupSOView.ShowDialog(
                    dialogCommands: vmSO.DialogCmds,
                    title: "수주현황 정보",
                    viewModel: vmSO
                );

                if (vmSO.ConfirmHeader != null)
                {
                    Header.SoNo = vmSO.ConfirmHeader.SoNo;
                    OnSearch();
                }
            }
        }

        public void OnNew()
        {
            IsNew = true;
            Header = new SalesOrderHeader();
            Details.Clear();
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;
            if (pm.Type == EntityMessageType.Added) return;

            SalesOrderDetail item = pm.Item as SalesOrderDetail;
            Header.SoNo = item.SoNo;
            OnSearch();
            IsNew = false;
        }
    }
}
