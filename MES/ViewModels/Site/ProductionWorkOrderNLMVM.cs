using System;
using System.Data;
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
using System.Data.SqlClient;

namespace MesAdmin.ViewModels
{
    public class ProductionWorkOrderNLMVM : ViewModelBase
    {
        #region Services
        IDialogService PopupItemView { get { return GetService<IDialogService>("ItemView"); } }
        IDialogService PopupStockView { get { return GetService<DevExpress.Mvvm.IDialogService>("StockView"); } }
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
        public ProductionWorkOrderDetail Header
        {
            get { return GetProperty(() => Header); }
            set { SetProperty(() => Header, value); }
        }
        public IEnumerable<CommonWorkAreaInfo> WaCollections
        {
            get { return GetProperty(() => WaCollections); }
            set { SetProperty(() => WaCollections, value); }
        }
        public ProductionWorkOrderList Details
        {
            get { return GetProperty(() => Details); }
            set { SetProperty(() => Details, value); }
        }
        public ObservableCollection<ProductionWorkOrder> SelectedItems { get; } = new ObservableCollection<ProductionWorkOrder>();
        public ProductionWorkOrder SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public ObservableCollection<CommonMinor> WhCode { get; set; }
        public string ItemCode
        {
            get { return GetProperty(() => ItemCode); }
            set { SetProperty(() => ItemCode, value); }
        }
        public string ItemName
        {
            get { return GetProperty(() => ItemName); }
            set { SetProperty(() => ItemName, value); }
        }
        public string ItemSpec
        {
            get { return GetProperty(() => ItemSpec); }
            set { SetProperty(() => ItemSpec, value); }
        }
        public string Remark2
        {
            get { return GetProperty(() => Remark2); }
            set { SetProperty(() => Remark2, value); }
        }
        public string Remark3
        {
            get { return GetProperty(() => Remark3); }
            set { SetProperty(() => Remark3, value); }
        }
        public string BasicUnit
        {
            get { return GetProperty(() => BasicUnit); }
            set { SetProperty(() => BasicUnit, value); }
        }
        public string WaCode
        {
            get { return GetProperty(() => WaCode); }
            set { SetProperty(() => WaCode, value); }
        }
        public decimal LotQty
        {
            get { return GetProperty(() => LotQty); }
            set { SetProperty(() => LotQty, value); }
        }
        public DateTime OrderDate
        {
            get { return GetProperty(() => OrderDate); }
            set { SetProperty(() => OrderDate, value); }
        }
        void UpdateIsEnabled()
        {
            RaisePropertyChanged(() => IsEnabled);
        }
        public bool IsEnabled
        {
            get { return Details.Count == 0 && IsNew; }
        }
        public string Title
        {
            get { return GetProperty(() => Title); }
            set { SetProperty(() => Title, value); }
        }
        public CommonItemList Items { get; set; }
        #endregion

        #region Commands
        public ICommand ShowDialogCmd { get; set; }
        public ICommand<HiddenEditorEvent> HiddenEditorCmd { get; set; }
        public ICommand<CellValueChangedEvent> CellValueChangedCmd { get; set; }
        public ICommand ShowStockCmd { get; set; }
        public ICommand NewCmd { get; set; }
        public DelegateCommand<object> DelCmd { get; set; }
        public ICommand AddCmd { get; set; }
        public AsyncCommand SaveCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        #endregion

        public ProductionWorkOrderNLMVM()
        {
            WaCode = "WE30";

            // 품목정보
            Task.Factory.StartNew(() => GlobalCommonItem.Instance).ContinueWith(task => { Items = task.Result; });
            Header = new ProductionWorkOrderDetail();
            OrderDate = DateTime.Now;
            Details = new ProductionWorkOrderList();
            IsNew = true;
            WaCollections = new CommonWorkAreaInfoList("BAC60").Where(u => u.WorkOrderFlag == "Y" && u.WaCode != "WE10");

            AddCmd = new DelegateCommand(OnAdd, CanAdd);
            DelCmd = new DelegateCommand<object>(Delete, CanDel);
            ShowDialogCmd = new DelegateCommand(OnShowDialog);
            ShowStockCmd = new DelegateCommand(OnShowStock);
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            NewCmd = new DelegateCommand(OnNew);
        }

        public bool CanSave()
        {
            bool ret = true;
            if (IsNew)
            {
                if (Details.Count == 0) return false;
                // 필수 입력값 처리
                foreach (var item in Details.Where(u => u.State == EntityState.Added))
                {
                    if (item.OrderQty <= 0 || string.IsNullOrEmpty(item.EqpCode))
                    {
                        ret = false;
                        break;
                    }
                }
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
                if (string.IsNullOrEmpty(Remark2) || string.IsNullOrEmpty(Remark3))
                    throw new Exception("로트번호 헤더와 순번은 필수 입력값입니다!");

                if (WaCode == "WE30")
                {
                    // 정규식 검사(품번 4자리 + 년월 2자리 + 일 2자리)
                    string ptn = @"^[a-zA-Z0-9]{4}[a-zA-Z]{2}\d{2}";
                    if (!System.Text.RegularExpressions.Regex.IsMatch(Remark2, ptn))
                        throw new Exception("로트번호 헤더가 잘못되었습니다.(정규식)");
                }

                // 로트번호, 가용재고 그룹핑 후 입력재고 비교
                decimal orderQty = Details.Sum(u => u.OrderQty);

                if (orderQty > LotQty)
                    throw new Exception("재고가 부족합니다!");

                foreach (ProductionWorkOrder order in Details)
                {
                    ProductionWorkOrderNL item = new ProductionWorkOrderNL
                    {
                        BizAreaCode = order.BizAreaCode,
                        OrderDate = order.OrderDate,
                        WaCode = order.WaCode,
                        EqpCode = order.EqpCode,
                        ItemCode = order.ItemCode,
                        OrderQty = order.BasicUnit == "kg" ? order.OrderQty * 1000 : order.OrderQty,
                        Remark = order.Remark,
                        Remark2 = Remark2,
                        Remark3 = Remark3
                    };

                    item.Save("BAC60");

                    Header.State = EntityState.Added;
                    Header.OrderNo = item.OrderNo;
                    Header.Qty = order.OrderQty;

                    ProductionWorkOrderDetailList itemList = new ProductionWorkOrderDetailList();
                    itemList.Add(Header);
                    itemList.Save();

                    order.State = EntityState.Unchanged;
                    order.OrderNo = item.OrderNo;

                    IsNew = false;
                }
            }
            catch (Exception ex)
            {
                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(ex.Message
                                                    , "Information"
                                                    , MessageButton.OK
                                                    , MessageIcon.Information));
            }

            IsBusy = false;
        }

        public bool CanAdd()
        {
             return IsNew && !string.IsNullOrEmpty(ItemCode) && !string.IsNullOrEmpty(WaCode) && !string.IsNullOrEmpty(Header.LotNo);
        }
        public void OnAdd()
        {
            CommonItem item = Items.Where(u => u.ItemCode == ItemCode).FirstOrDefault();
            if (item == null) return;

            Details.Insert(Details.Count, new ProductionWorkOrder
            {
                State = EntityState.Added,
                BizAreaCode = "BAC60",
                ItemCode = ItemCode,
                ItemName = item.ItemName,
                ItemSpec = item.ItemSpec,
                BasicUnit = Header.BasicUnit,
                WaCode = WaCode,
                OrderDate = OrderDate,
                Remark2 = Header.LotNo
            });

            UpdateIsEnabled();
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

            UpdateIsEnabled();
        }

        public bool CanSearch()
        {
            return !string.IsNullOrEmpty(Header.OrderNo);
        }
        public Task OnSearch()
        {
            IsBusy = true;
            IsNew = false;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            //string orderNo = Header.OrderNo;

            //Details = new ProductionWorkOrderDetailList(orderNo);
            //if (Details.Count == 0)
            //{
            //    DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage("작업지시내역 정보가 없습니다!"
            //                                            , "Information"
            //                                            , MessageButton.OK
            //                                            , MessageIcon.Information));
            //    IsNew = true;
            //}
            //else
            //    Header = new ProductionWorkOrderNL(orderNo);

            //IsBusy = false;
            //UpdateIsEnabled();
        }

        public void OnNew()
        {
            IsNew = true;
            ItemCode = "";
            ItemName = "";
            Remark2 = "";
            Remark3 = "";
            LotQty = 0;
            Header = new ProductionWorkOrderDetail();
            OrderDate = DateTime.Now;
            Details.Clear();
            UpdateIsEnabled();
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
                ItemName = vmItem.ConfirmItem.ItemName;
                ItemSpec = vmItem.ConfirmItem.ItemSpec;
                BasicUnit = vmItem.ConfirmItem.BasicUnit;
            }
        }

        public void OnShowStock()
        {
            var vmItem = ViewModelSource.Create(() =>
            new PopupStockBOMVM(ItemCode, null));

            PopupStockView.ShowDialog(
                dialogCommands: vmItem.DialogCmds,
                title: "재고선택",
                viewModel: vmItem
            );
            try
            {
                if (vmItem.ConfirmItem != null)
                {
                    Remark2 = vmItem.ConfirmItem.LotNo;
                    Remark3 = Commonsp.GetLotCountWE30(ItemCode, vmItem.ConfirmItem.ItemCode, vmItem.ConfirmItem.LotNo, WaCode);
                }
            }
            catch
            {
                MessageBoxService.ShowMessage("로트헤더를 자동으로 생성할 수 없습니다. 수동으로 입력하세요", "Information", MessageButton.OK, MessageIcon.Information);
            }
            try
            { 
                if (vmItem.ConfirmItems != null && vmItem.ConfirmItems.Count > 0)
                {

                    Header.BizAreaCode = "BAC60";
                    Header.ItemCode = vmItem.ConfirmItems[0].ItemCode;
                    Header.ItemName = vmItem.ConfirmItems[0].ItemName;
                    Header.ItemSpec = vmItem.ConfirmItems[0].ItemSpec;
                    Header.LotNo = vmItem.ConfirmItems[0].LotNo;
                    LotQty = vmItem.ConfirmItems[0].Qty - vmItem.ConfirmItems[0].PickingQty;
                    Header.BasicUnit = vmItem.ConfirmItems[0].BasicUnit;
                    Header.WhCode = vmItem.ConfirmItems[0].WhCode;
                    Header.WaCode = vmItem.ConfirmItems[0].WaCode;
                }
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowMessage(ex.Message, "Information", MessageButton.OK, MessageIcon.Information);
            }
        }

        public void OnCellValueChanged(CellValueChangedEvent pm)
        {
            if (pm.e.Column.FieldName != "OrderQty") return;
            try
            {
                TableView view = pm.sender as TableView;
                GridControl grid = view.Grid;

                grid.UpdateTotalSummary();
            }
            catch (Exception ex) { MessageBoxService.ShowMessage(ex.Message); }
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;
            ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
        }
    }
}