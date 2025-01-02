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
using DevExpress.Xpf.Editors;
using System.Data.SqlClient;
using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.Sql;

namespace MesAdmin.ViewModels
{
    public class ProductionWorkOrderNLVM : ViewModelBase
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
            set { SetProperty(() => IsNew, value, () => RaisePropertyChanged(() => IsEnabled)); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        public string Title
        {
            get { return GetProperty(() => Title); }
            set { SetProperty(() => Title, value); }
        }
        public string Background
        {
            get { return GetProperty(() => Background); }
            set { SetProperty(() => Background, value); }
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
            get { return Details == null && IsNew; }
        }
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
        public ICommand<EditValueChangedEventArgs> EditValueChangedCmd { get; set; }
        public ICommand PrintLabelCmd { get; set; }
        #endregion

        public ProductionWorkOrderNLVM()
        {
            Title = "로트번호 헤더";
            Header = new ProductionWorkOrderNL();
            Header.OrderDate = DateTime.Now;
            ExceptStocks = new List<StockDetail>();            
            WaCollections = new CommonWorkAreaInfoList("BAC60").Where(u => u.WorkOrderFlag == "Y");

            ShowDialogCmd = new DelegateCommand(OnShowDialog);
            SearchCmd = new AsyncCommand(OnSearch, CanSearch);
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            AddCmd = new DelegateCommand(OnAdd, CanAdd);
            DelCmd = new DelegateCommand<object>(Delete, CanDel);
            ShowStockCmd = new DelegateCommand(OnShowStock);
            NewCmd = new DelegateCommand(OnNew);
            CellValueChangedCmd = new DelegateCommand<CellValueChangedEvent>(OnCellValueChanged);
            EditValueChangedCmd = new DelegateCommand<EditValueChangedEventArgs>(OnEditValueChanged);
            PrintLabelCmd = new DelegateCommand(OnPrintLabel, () => !IsNew);

            SelectedItems = new ObservableCollection<ProductionWorkOrderDetail>();
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
        }

        private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DelCmd.RaiseCanExecuteChanged();
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
            return Task.Factory.StartNew(SaveCore);
        }
        public void SaveCore()
        {
            try
            {
                if (IsNew)
                {
                    // 혼합공정은 헤더가 필요없고 썩이는 로트중 수량이 많은 로트의 헤더를 가져간다.(실적등록시)
                    if (string.IsNullOrEmpty(Header.Remark2) && Header.WaCode != "WE42")
                        throw new Exception("로트번호 헤더가 비어 있습니다.");

                    if (Header.WaCode == "WE30")
                    {
                        // 정규식 검사(품번 4자리 + 년월 2자리 + 일 2자리)
                        string ptn = @"^[a-zA-Z0-9]{4}[a-zA-Z]{2}\d{2}";
                        if(!System.Text.RegularExpressions.Regex.IsMatch(Header.Remark2, ptn))
                            throw new Exception("로트번호 헤더가 잘못되었습니다.(정규식)");
                    }

                    // 로트번호, 가용재고 그룹핑 후 입력재고 비교
                    var temp = Details.GroupBy(row => new { row.LotNo, row.AvailableQty, row.ItemCode })
                                        .Select(g => new { LotNo = g.Key.LotNo, AvailableQty = g.Key.AvailableQty, Qty = g.Sum(x => x.Qty) });
                    foreach (var item in temp)
                    {
                        if (item.Qty > item.AvailableQty)
                            throw new Exception(item.LotNo + "의 재고를 확인하세요!");
                    }

                    Header.Save("BAC60");
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
            IsBusy = false;
        }

        public bool CanAdd()
        {
            return IsNew && !string.IsNullOrEmpty(Header.ItemCode) && !string.IsNullOrEmpty(Header.WaCode) && !string.IsNullOrEmpty(Header.EqpCode);
        }
        public void OnAdd()
        {
            try
            {
                // 합성공정 로트번호는 작업지시에서 생성
                if (Header.WaCode == "WE10") Header.CreateLotNo();

                // 포장공정은 계획량 필수(자동출력때문)
                if (Header.WaCode == "WE60" && Header.OrderQty == 0)
                {
                    throw new Exception("계획량을 입력하세요!(라벨출력용)");
                }

                Details = Details ?? new ProductionWorkOrderDetailList();
                Details.Insert(Details.Count, new ProductionWorkOrderDetail
                {
                    State = EntityState.Added,
                    BizAreaCode = "BAC60",
                });
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowMessage(ex.Message, "Information", MessageButton.OK, MessageIcon.Information);
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

            if (Details.Count == 0)
            {
                Header.Remark2 = "";
                Details = null;
            }
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
                Header = new ProductionWorkOrderNL("BAC60", orderNo);

            IsBusy = false;
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
                Header.ItemCode = vmItem.ConfirmItem.ItemCode;
                Header.ItemName = vmItem.ConfirmItem.ItemName;
                Header.BasicUnit = vmItem.ConfirmItem.BasicUnit;
            }
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
                // 공정별 전공정 로트 채우기
                foreach (StockDetail item in vmItem.ConfirmItems)
                {
                    if (Header.WaCode == "WE20") // 재결정
                    {
                        if (item.ItemAccount == "29" && item.ItemCode.Substring(0, 3) != "SER") // LD12MH131A1 왼쪽 9자리
                            Header.Remark2 = item.LotNo.Substring(0, 9);
                    }

                    if (Header.WaCode == "WE30" || Header.WaCode == "WE47") // 정제/2차 Melting
                    {
                        if (item.ItemAccount == "29" && item.ProcureType == "M") // 반제품이고 외주가공품이 아닌품목
                            Header.Remark2 = item.LotNo;
                    }

                    if (Header.WaCode == "WE40" || Header.WaCode == "WE50") // 분쇄/성형
                    {
                        if (item.ItemAccount == "29")
                            Header.Remark2 = item.LotNo.Substring(0, 9);
                    }

                    if (Header.WaCode == "WE60") // 포장
                    {
                        if (item.ItemAccount == "29") // 오른쪽 11자리(제품로트)
                            Header.Remark2 = item.LotNo.Substring(item.LotNo.Length - 11);
                    }
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

                    int idx = Details.IndexOf(SelectedItem);
                    Details.RemoveAt(idx);

                    foreach (StockDetail item in vmItem.ConfirmItems)
                    {
                        Details.Insert(idx++,
                            new ProductionWorkOrderDetail
                            {
                                State = EntityState.Added,
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
        
        public void OnCellValueChanged(CellValueChangedEvent pm)
        {
            if (pm.e.Column.FieldName != "Qty") return;
            try
            {
                TableView view = pm.sender as TableView;
                GridControl grid = view.Grid;

                grid.UpdateTotalSummary();
            }
            catch (Exception ex) { MessageBoxService.ShowMessage(ex.Message); }
        }

        public void OnEditValueChanged(EditValueChangedEventArgs e)
        {
            if (IsNew && e.NewValue != null && e.NewValue.ToString() == "WE30")
            {
                MessageBoxService.ShowMessage("정제공정은 \"작업지시 등록(정제)\" 메뉴에서 입력하세요!", "Information", MessageButton.OK, MessageIcon.Information);
                Header.WaCode = e.OldValue == null ? null : e.OldValue.ToString();
            }
        }

        protected void OnPrintLabel()
        {
            XtraReport rpt = new Reports.WorkOrderDetail();

            (rpt.DataSource as SqlDataSource).ConnectionName = DBInfo.Instance.Name;
            rpt.Parameters["OrderNo"].Value = Header.OrderNo;
            rpt.CreateDocument();

            DevExpress.Xpf.Printing.PrintHelper.ShowPrintPreview(System.Windows.Application.Current.MainWindow, rpt);
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
