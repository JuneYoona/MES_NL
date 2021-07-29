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
using MesAdmin.Reports;
using System.Data.SqlClient;

namespace MesAdmin.ViewModels
{
    public class BAC90PP001CVM : ViewModelBase
    {
        #region Services
        IDialogService PopupStockView { get { return GetService<DevExpress.Mvvm.IDialogService>("StockView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public bool IsNew
        {
            get { return GetProperty(() => IsNew); }
            set { SetProperty(() => IsNew, value, () => RaisePropertyChanged(() => IsEnabled)); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        public ProductionWorkOrderNL Header
        {
            get { return GetProperty(() => Header); }
            set { SetProperty(() => Header, value); }
        }
        public IEnumerable<CommonWorkAreaInfo> WaCollections
        {
            get { return GetProperty(() => WaCollections); }
            set { SetProperty(() => WaCollections, value); }
        }
        public ProductionWorkOrderDetailList Details
        {
            get { return GetProperty(() => Details); }
            set { SetProperty(() => Details, value, () => RaisePropertyChanged(() => IsEnabled)); }
        }
        public ObservableCollection<ProductionWorkOrderDetail> SelectedItems
        {
            get { return GetProperty(() => SelectedItems); }
            set { SetProperty(() => SelectedItems, value); }
        }
        public ProductionWorkOrderDetail SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public ObservableCollection<CommonMinor> WhCode { get; set; }
        private List<StockDetail> ExceptStocks { get; set; }
        public bool IsEnabled
        {
            get { return Details == null && Details.Count > 0; }
        }
        #endregion

        #region Commands
        public ICommand ShowDialogCmd { get; set; }
        public ICommand<HiddenEditorEvent> HiddenEditorCmd { get; set; }
        public ICommand ShowStockCmd { get; set; }
        public ICommand NewCmd { get; set; }
        public DelegateCommand DelCmd { get; set; }
        public ICommand AddCmd { get; set; }
        public AsyncCommand SaveCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        #endregion

        public BAC90PP001CVM()
        {
            Header = new ProductionWorkOrderNL();
            Header.OrderDate = DateTime.Now;
            ExceptStocks = new List<StockDetail>();
            WaCollections = new CommonWorkAreaInfoList("BAC90").Where(u => u.WorkOrderFlag == "Y");

            SearchCmd = new AsyncCommand(OnSearch, () => !string.IsNullOrEmpty(Header.OrderNo));
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            AddCmd = new DelegateCommand(OnAdd, CanAdd);
            DelCmd = new DelegateCommand(Delete, () => SelectedItems.Count > 0);
            ShowStockCmd = new DelegateCommand(OnShowStock);
            NewCmd = new DelegateCommand(OnNew);
            HiddenEditorCmd = new DelegateCommand<HiddenEditorEvent>(OnHiddenEditor);

            SelectedItems = new ObservableCollection<ProductionWorkOrderDetail>();
        }

        public bool CanSave()
        {
            if (Details == null) return false;

            bool ret = true;
            if (IsNew)
            {
                if (Details.Count == 0) return false;
                // 필수 입력값 처리
                foreach (var item in Details.Where(u => u.State == EntityState.Added))
                {
                    if (string.IsNullOrEmpty(item.ItemCode) || item.Qty <= 0 || string.IsNullOrEmpty(item.LotNo))
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
            return Task.Factory.StartNew(SaveCore).ContinueWith(task => IsBusy = false);
        }
        public void SaveCore()
        {
            try
            {
                if (IsNew)
                {
                    // 로트번호, 가용재고 그룹핑 후 입력재고 비교
                    var temp = Details.GroupBy(row => new { row.LotNo, row.AvailableQty, row.ItemCode })
                                        .Select(g => new { LotNo = g.Key.LotNo, AvailableQty = g.Key.AvailableQty, Qty = g.Sum(x => x.Qty) });
                    foreach (var item in temp)
                    {
                        if (item.Qty > item.AvailableQty)
                            throw new Exception(item.LotNo + "의 재고를 확인하세요!");
                    }

                    Header.Save("BAC90");
                    Details.ToList().ForEach(u => u.OrderNo = Header.OrderNo);
                }

                Details.Save();
                var task = OnSearch();
                while (!task.IsCompleted) IsNew = false; // 비동기 실행때문에 제일 마지막처리
                // send mesaage to parent view
                Messenger.Default.Send<string>("Refresh");
            }
            catch (Exception ex)
            {
                if (ex is SqlException && IsNew)
                    Header.Delete();

                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(ex.Message
                                                    , "Information"
                                                    , MessageButton.OK
                                                    , MessageIcon.Information));
            }
        }

        public bool CanAdd()
        {
            return IsNew && !string.IsNullOrEmpty(Header.ItemCode) && !string.IsNullOrEmpty(Header.WaCode) && !string.IsNullOrEmpty(Header.EqpCode);
        }
        public void OnAdd()
        {
            try
            {
                if (Header.WaCode == "WB20" && Details == null) Header.CreateLotNoWB20();

                Details = Details ?? new ProductionWorkOrderDetailList();
                Details.Insert(Details.Count, new ProductionWorkOrderDetail
                {
                    State = EntityState.Added,
                    BizAreaCode = "BAC90",
                });
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowMessage(ex.Message, "Information", MessageButton.OK, MessageIcon.Information);
            }
        }

        public void Delete()
        {
            SelectedItems.ToList().ForEach(u =>
            {
                if (u.State == EntityState.Added)
                    Details.Remove(u);
                else
                    u.State = u.State == EntityState.Deleted ? EntityState.Unchanged : EntityState.Deleted;
            });

            if (Details.Count == 0)
            {
                Details = null;
                Header.LotNo = "";
            }
        }

        public Task OnSearch()
        {
            IsBusy = true;
            IsNew = false;
            return Task.Factory.StartNew(SearchCore).ContinueWith(task => IsBusy = false);
        }
        public void SearchCore()
        {
            string orderNo = Header.OrderNo;

            Details = new ProductionWorkOrderDetailList(orderNo);
            if (Details.Count == 0)
            {
                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage("작업지시내역 정보가 없습니다!"
                                                        , "Information"
                                                        , MessageButton.OK
                                                        , MessageIcon.Information));
                IsNew = true;
            }
            else
                Header = new ProductionWorkOrderNL("BAC90", orderNo);

        }

        public void OnNew()
        {
            IsNew = true;
            Header = new ProductionWorkOrderNL();
            Header.ItemCode = "";
            Header.EqpCode = "";
            Header.OrderDate = DateTime.Now;
            Details = null;
        }

        public void OnShowStock()
        {
            // 선택된 로트는 재고 pop up에서 제외
            ExceptStocks.Clear();
            Details.Where(u => !string.IsNullOrEmpty(u.ItemCode)).ToList().ForEach(u => ExceptStocks.Add(new StockDetail
            {
                BizAreaCode = u.BizAreaCode,
                ItemCode = u.ItemCode,
                WhCode = u.WhCode,
                WaCode = u.WaCode,
                LotNo = u.LotNo,
            }));

            var vmItem = ViewModelSource.Create(() =>
            new PopupStockBOMVM(Header.ItemCode, ExceptStocks));

            PopupStockView.ShowDialog(
                dialogCommands: vmItem.DialogCmds,
                title: "재고선택",
                viewModel: vmItem
            );

            try
            {
                if (vmItem.ConfirmItems != null && vmItem.ConfirmItems.Count > 0)
                {

                    int idx = Details.IndexOf(SelectedItem);
                    Details.RemoveAt(idx);

                    foreach (StockDetail item in vmItem.ConfirmItems)
                    {
                        // TSC default value setting
                        if (item.ItemCode == "MBA114" || item.ItemCode == "MBA115" || item.ItemCode == "MBA116") item.Remark4 = "100";
                        if (item.ItemCode == "MBA117") item.Remark4 = "10";

                        Details.Insert(idx++,
                            new ProductionWorkOrderDetail
                            {
                                State = Common.Common.EntityState.Added,
                                BizAreaCode = item.BizAreaCode,
                                ItemCode = item.ItemCode,
                                ItemName = item.ItemName,
                                ItemSpec = item.ItemSpec,
                                WhCode = item.WhCode,
                                WaCode = item.WaCode,
                                LotNo = item.LotNo,
                                Qty = item.Qty - item.PickingQty,
                                AvailableQty = item.Qty - item.PickingQty,
                                BasicUnit = item.BasicUnit,
                                TSC = item.Remark4,
                                PIG = item.Remark5,
                                ExpDate = string.IsNullOrEmpty(item.Remark6) ? (DateTime?)null : DateTime.Parse(item.Remark6),
                            }
                        );
                        // 추가된 재고들 선택된걸 보여주기위해
                        SelectedItems.Add(Details.ElementAt(idx - 1));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowMessage(ex.Message, "Information", MessageButton.OK, MessageIcon.Information);
            }
        }

        public void OnHiddenEditor(HiddenEditorEvent pm)
        {
            if (pm.e.Column.FieldName != "Solution" && pm.e.Column.FieldName != "TSC") return;
            try
            {
                TableView view = pm.sender as TableView;
                GridControl grid = view.Grid;

                decimal temp;
                bool ret;

                if (pm.e.Column.FieldName == "Solution")
                    ret = decimal.TryParse(grid.GetCellValue(pm.e.RowHandle, "TSC").ToString(), out temp);
                else ret = decimal.TryParse(grid.GetCellValue(pm.e.RowHandle, "Solution").ToString(), out temp);
                
                if (ret)
                {
                    grid.SetCellValue(pm.e.RowHandle, "AC", decimal.Parse(pm.e.Value.ToString()) * temp / 100);
                }

                // Active compound summary를 구하고 계산 수량을 재계산한다.
                if (Header.TSC == null) return;

                decimal ac;
                decimal sumAC = 0;
                foreach (var item in Details)
                {
                    ret = decimal.TryParse(item.AC, out ac);
                    if (ret) sumAC += ac;
                }

                // 실제 투입무게 계산
                decimal solution;
                decimal sumCAL = 0;
                decimal cal;
                foreach (var item in Details.Where(o => o.ItemCode != "TBSS01" && o.ItemCode != "TBSS02"))
                {
                    ret = decimal.TryParse(item.Solution, out solution);
                    if (ret)
                    {
                        cal = solution * Convert.ToDecimal(Header.TSC) / sumAC * Header.OrderQty / 100;
                        item.CAL = cal.ToString();
                        sumCAL += cal;
                    }
                }

                foreach (var item in Details.Where(o => o.ItemCode == "TBSS01" || o.ItemCode == "TBSS02"))
                {
                    item.CAL = ((Header.OrderQty - sumCAL) / 2).ToString();
                }
            }
            catch { }
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;
            if (pm.Type == EntityMessageType.Added)
            {
                IsNew = true;
                ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
            }
            else
            {
                Header.OrderNo = pm.Item as string;
                Task.Factory.StartNew(SearchCore).ContinueWith(task =>
                {
                    ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
                });
            }
        }
    }
}