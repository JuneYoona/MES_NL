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
using MesAdmin.Reports;

namespace MesAdmin.ViewModels
{
    public class ExceptionMoveVM : ViewModelBase
    {
        #region Services
        IDialogService PopupStockView { get { return GetService<IDialogService>("StockView"); } }
        IDialogService PopupStockMoveView { get { return GetService<IDialogService>("StockMoveView"); } }
        IDialogService PopupItemView { get { return GetService<IDialogService>("ItemView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public bool IsNew
        {
            get { return GetProperty(() => IsNew); }
            set { SetProperty(() => IsNew, value); }
        }
        public string DocumentNo
        {
            get { return GetProperty(() => DocumentNo); }
            set { SetProperty(() => DocumentNo, value); }
        }
        public ObservableCollection<CommonMinor> MoveType { get; set; }
        public CommonMinor SelectedMoveType
        {
            get { return GetProperty(() => SelectedMoveType); }
            set { SetProperty(() => SelectedMoveType, value, () => RaisePropertiesChanged("ShowPrint") ); }
        }
        public ObservableCollection<CommonMinor> WareHouse { get; set; }
        public CommonMinor SelectedWh
        {
            get { return GetProperty(() => SelectedWh); }
            set { SetProperty(() => SelectedWh, value); }
        }
        public ObservableCollection<CommonMinor> TransWareHouse { get; set; }
        public CommonMinor SelectedTransWh
        {
            get { return GetProperty(() => SelectedTransWh); }
            set { SetProperty(() => SelectedTransWh, value); }
        }
        public DateTime DocumentDate
        {
            get { return GetProperty(() => DocumentDate); }
            set { SetProperty(() => DocumentDate, value); }
        }
        public string Memo
        {
            get { return GetProperty(() => Memo); }
            set { SetProperty(() => Memo, value); }
        }
        public StockMovementDetailList Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public ObservableCollection<StockMovementDetail> SelectedItems
        {
            get { return GetProperty(() => SelectedItems); }
            set { SetProperty(() => SelectedItems, value); }
        }
        public StockMovementDetail SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public ObservableCollection<CommonWorkAreaInfo> StockWaCode
        {
            get { return GetProperty(() => StockWaCode); }
            set { SetProperty(() => StockWaCode, value); }
        }
        public ObservableCollection<CommonWorkAreaInfo> WaCode
        {
            get { return GetProperty(() => WaCode); }
            set { SetProperty(() => WaCode, value); }
        }
        public CommonItemList Items { get; set; }
        private List<StockDetail> ExceptStocks { get; set; }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        public bool ShowPrint{ get{return SelectedMoveType != null && SelectedMoveType.MinorCode == "T01";} }
        #endregion

        #region Commands
        public ICommand ShowDialogCmd { get; set; }
        public ICommand AddCmd { get; set; }
        public ICommand NewCmd { get; set; }
        public ICommand WhIndexChangedCmd { get; set; }
        public ICommand MoveTypeIndexChangedCmd { get; set; }
        public DelegateCommand<object> DelCmd { get; set; }
        public AsyncCommand SaveCmd { get; set; }
        public ICommand<HiddenEditorEvent> HiddenEditorCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        public ICommand PrintCmd { get; set; }
        #endregion

        #region Report
        MaterialReturn report;
        #endregion

        public ExceptionMoveVM()
        {
            IsNew = true;
            Collections = new StockMovementDetailList();
            Items = new CommonItemList(); // 품목정보
            
            AddCmd = new DelegateCommand(Add, CanAdd);
            DelCmd = new DelegateCommand<object>(Delete, CanDel);
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            NewCmd = new DelegateCommand(OnNew);
            MoveTypeIndexChangedCmd = new DelegateCommand(OnMoveTypeIndexChanged);
            WhIndexChangedCmd = new DelegateCommand<string>(OnWhIndexChanged);
            ShowDialogCmd = new DelegateCommand<string>(ShowDialog);
            HiddenEditorCmd = new DelegateCommand<HiddenEditorEvent>(OnHiddenEditor);
            SearchCmd = new AsyncCommand(OnSearch, CanSearch);
            PrintCmd = new DelegateCommand(OnPrint, () => !string.IsNullOrEmpty(DocumentNo));

            SelectedItems = new ObservableCollection<StockMovementDetail>();
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;

            MoveType = new CommonMinorList
            (
                GlobalCommonMinor.Instance.Where(u => u.Ref02 == "ST" && u.IsEnabled == true)
            ); // 재고이동
            WareHouse = new CommonMinorList
            (
                GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0011")
            ); // 창고정보
            TransWareHouse = new CommonMinorList
            (
                GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0011")
            ); // 이동창고정보
            StockWaCode = GlobalCommonWorkAreaInfo.Instance; // 재고창고소유 공정정보
            WaCode = GlobalCommonWorkAreaInfo.Instance; // 이동창고소유 공정정보
            DocumentDate = DateTime.Now;
            ExceptStocks = new List<StockDetail>();
        }

        private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DelCmd.RaiseCanExecuteChanged();
        }

        public bool CanSearch()
        {
            return !string.IsNullOrEmpty(DocumentNo) && IsNew;
        }
        public Task OnSearch()
        {
            IsBusy = true;
            IsNew = false;
            return Task.Factory.StartNew(SearchCore).ContinueWith(task => IsBusy = false);
        }
        public void SearchCore()
        {
            StockMovementHeader header = new StockMovementHeader(DocumentNo);
            if (!string.IsNullOrEmpty(header.DocumentNo))
            {
                if (header.TransType != "ST")
                {
                    DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage("재고이동에 사용할 수 없는 수불유형입니다!"
                                                        , "Information"
                                                        , MessageButton.OK
                                                        , MessageIcon.Information));
                    IsNew = true;
                    return;
                }
            }

            Collections = new StockMovementDetailList(documentNo: DocumentNo);
            Collections = new StockMovementDetailList(Collections.Where(u => u.DCFlag == "C"));
            if (Collections.Count == 0)
            {
                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage("수불정보가 없습니다!"
                                                            , "Information"
                                                            , MessageButton.OK
                                                            , MessageIcon.Information));
                IsNew = true;
            }
            else
            {
                SelectedWh = new CommonMinor { MinorCode = Collections.FirstOrDefault().WhCode };
                SelectedTransWh = new CommonMinor { MinorCode = Collections.FirstOrDefault().TransWhCode };
                SelectedMoveType = new CommonMinor { MinorCode = header.MoveType };
                DocumentDate = header.DocumentDate;
                Memo = header.Memo;
            }
        }

        public void ShowDialog(string pm)
        {
            if (pm == "DocumentNo") // 수불현황조회
            {
                var vmMove = ViewModelSource.Create(() => new PopupStockMoveVM("ST")
                {
                    MoveType = MoveType
                }); // // Dialog 수불구분과 수불유형 정의(예외출고 내역만)

                PopupStockMoveView.ShowDialog(
                    dialogCommands: vmMove.DialogCmds,
                    title: "수불현황 정보",
                    viewModel: vmMove
                );

                if (vmMove.ConfirmHeader != null && vmMove.CollectionsDetail != null)
                {
                    DocumentNo = vmMove.ConfirmHeader.DocumentNo;
                    OnSearch();
                }
            }
            else if (pm == "StockView") // 재고조회
            {
                // 선택된 로트는 재고 pop up에서 제외
                ExceptStocks.Clear();
                var temp = Collections.GroupBy(row => new { row.BizAreaCode, row.ItemCode, row.LotNo, row.WhCode, row.WaCode })
                                        .Select(g => new { BizAreaCode = g.Key.BizAreaCode, ItemCode = g.Key.ItemCode, LotNo = g.Key.LotNo, WhCode = g.Key.WhCode, WaCode = g.Key.WaCode });
                foreach (var item in temp)
                {
                    ExceptStocks.Add(new StockDetail
                    {
                        BizAreaCode = item.BizAreaCode,
                        ItemCode = item.ItemCode,
                        WhCode = item.WhCode,
                        WaCode = item.WaCode ?? "",
                        LotNo = item.LotNo,
                    });
                }

                var vmItem = ViewModelSource.Create(() => new PopupStockVM(SelectedWh, "", ExceptStocks)); // 재고를 조회할 창고전달
                PopupStockView.ShowDialog(
                    dialogCommands: vmItem.DialogCmds,
                    title: "재고선택",
                    viewModel: vmItem
                );

                if (vmItem.ConfirmItems != null && vmItem.ConfirmItems.Count > 0)
                {
                    try
                    {
                        int idx = Collections.IndexOf(SelectedItem);
                        Collections.RemoveAt(idx);

                        foreach (StockDetail item in vmItem.ConfirmItems)
                        {
                            Collections.Insert(idx++,
                                new StockMovementDetail
                                {
                                    State = EntityState.Added,
                                    BizAreaCode = item.BizAreaCode,
                                    ItemCode = item.ItemCode,
                                    ItemName = item.ItemName,
                                    ItemSpec = item.ItemSpec,
                                    WhCode = item.WhCode,
                                    LotNo = item.LotNo,
                                    Qty = item.Qty - item.PickingQty,
                                    BasicUnit = item.BasicUnit,
                                    WaCode = item.WaCode,
                                    TransItemCode = SelectedMoveType.MinorCode == "T65" ? null : item.ItemCode, // 품목계정간 재고이동
                                    TransLotNo = SelectedMoveType.MinorCode == "T65" ? null : item.LotNo, // 품목계정간 재고이동
                                    TransWaCode = SelectedMoveType.MinorCode == "T61" ? item.WaCode : "", // 품목간 재고이동

                                    TransType = SelectedMoveType.Ref02,
                                    MoveType = SelectedMoveType.MinorCode,
                                    DCFlag = SelectedMoveType.Ref01,
                                    TransWhCode = SelectedTransWh.MinorCode,
                                    StockType = SelectedMoveType.Ref03
                                }
                            );
                            // 추가된 재고들 선택된걸 보여주기위해
                            SelectedItems.Add(Collections.ElementAt(idx - 1));
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBoxService.ShowMessage(ex.Message, "Information", MessageButton.OK, MessageIcon.Information);
                    }
                }
            }
            else // 품목조회
            {
                var vmItem = ViewModelSource.Create(() => new PopupItemVM());
                PopupItemView.ShowDialog(
                    dialogCommands: vmItem.DialogCmds,
                    title: "품목선택",
                    viewModel: vmItem
                );

                if (vmItem.ConfirmItem != null)
                {
                    foreach (StockMovementDetail item in SelectedItems)
                    {
                        item.TransItemCode = vmItem.ConfirmItem.ItemCode;
                        SelectedItem = null; // for hiding editor
                    }
                }
            }
        }

        bool CanDel(object obj) { return SelectedItems.Count > 0; }
        public void Delete(object obj)
        {
            SelectedItems.ToList().ForEach(u =>
            {
                if (u.State == EntityState.Added)
                    Collections.Remove(u);
                else
                    u.State = u.State == EntityState.Deleted ? EntityState.Unchanged : EntityState.Deleted;
            });
        }

        public void Add()
        {
            if (SelectedMoveType.MinorCode == "T01") // 창고간이동
            {
                if (SelectedWh.MinorCode == SelectedTransWh.MinorCode)
                {
                    MessageBoxService.ShowMessage("이동창고를 다른 창고로 선택하세요!"
                        , "Information"
                        , MessageButton.OK
                        , MessageIcon.Information);
                    return;
                }
            }
            // 품목간재고이동 = 창고변경 안됨, 공정변경안됨
            if (SelectedMoveType.MinorCode == "T61")
            {
                if (SelectedWh.MinorCode != SelectedTransWh.MinorCode)
                {
                    MessageBoxService.ShowMessage("품목간재고이동은 창고가 같아야합니다!"
                        , "Information"
                        , MessageButton.OK
                        , MessageIcon.Information);
                    return;
                }
            }
            Collections.Insert(Collections.Count, new StockMovementDetail
            {
                State = EntityState.Added,
                TransType = SelectedMoveType.Ref02,
                MoveType = SelectedMoveType.MinorCode,
                DCFlag = SelectedMoveType.Ref01,
                BizAreaCode = SelectedWh.Ref01,
                WhCode = SelectedWh.MinorCode,
                TransWhCode = SelectedTransWh.MinorCode,
                StockType = SelectedMoveType.Ref03
            });

            StockWaCode = GlobalCommonWorkAreaInfo.Instance; // 재고창고소유 공정정보
            WaCode = new ObservableCollection<CommonWorkAreaInfo>
            (
                GlobalCommonWorkAreaInfo.Instance.Where(u => u.WhCode == SelectedTransWh.MinorCode)
            ); // 이동창고소유 공정정보
        }
        public bool CanAdd()
        {
            return SelectedMoveType != null && SelectedWh != null && SelectedTransWh != null && IsNew;
        }

        public bool CanSave()
        {
            bool ret = true;
            if (IsNew)
            {
                // 필수 입력값 처리
                foreach (StockMovementDetail item in Collections.Where(u => u.State == EntityState.Added))
                {
                    if (string.IsNullOrEmpty(item.ItemCode) || item.Qty == null || item.TransItemCode == null || item.TransLotNo == null || item.TransLotNo.Trim() == "")
                    {
                        ret = false;
                        break;
                    }
                }
                if (Collections.Count == 0) return false;
            }
            else
                ret = Collections.Where(u => u.State == EntityState.Deleted).Count() > 0;

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
                    Collections.ToList().ForEach(u => { u.DocumentDate = DocumentDate; u.Memo = Memo; });
                    DocumentNo = Collections.Save();
                }
                else
                {
                    Collections.Save();
                }

                var task = OnSearch();
                while (!task.IsCompleted) IsNew = false; // 비동기 실행때문에 제일 마지막처리
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

        public void OnNew()
        {
            IsNew = true;
            DocumentNo = null;
            Collections.Clear();
            SelectedMoveType = null;
            SelectedWh = null;
            SelectedTransWh = null;
            DocumentDate = DateTime.Now;
            Memo = null;
        }

        public void OnWhIndexChanged(string pm)
        {
            if (IsNew)
            {
                if (pm == "WhCode")
                    SelectedTransWh = SelectedWh;
                Collections.Clear();
            }
        }

        public void OnMoveTypeIndexChanged()
        {
            if (IsNew)
            {
                Collections.Clear();
                SelectedWh = null;
                SelectedTransWh = null;
            }
        }

        public void OnHiddenEditor(HiddenEditorEvent pm)
        {
            if (pm.e.Column.FieldName != "ItemCode" && pm.e.Column.FieldName != "TransItemCode") return;

            TableView view = pm.sender as TableView;
            GridControl grid = view.Grid;

            string fieldName = pm.e.Column.FieldName;
            if (fieldName == "ItemCode")
            {
                CommonItem item = Items.Where(u => u.ItemCode == (string)pm.e.Value).FirstOrDefault();
                if (item == null)
                {
                    grid.SetCellValue(pm.e.RowHandle, "ItemCode", null);
                    grid.SetCellValue(pm.e.RowHandle, "ItemName", null);
                    grid.SetCellValue(pm.e.RowHandle, "ItemSpec", null);
                    grid.SetCellValue(pm.e.RowHandle, "BasicUnit", null);
                }
                else
                {
                    grid.SetCellValue(pm.e.RowHandle, "ItemName", item.ItemName);
                    grid.SetCellValue(pm.e.RowHandle, "ItemSpec", item.ItemSpec);
                    grid.SetCellValue(pm.e.RowHandle, "BasicUnit", item.BasicUnit);
                }
            }
            else // TransItemCode
            {
                CommonItem item = Items.Where(u => u.ItemCode == (string)pm.e.Value).FirstOrDefault();
                if (item == null)
                {
                    grid.SetCellValue(pm.e.RowHandle, "TransItemCode", null);
                }
            }
        }

        public void OnPrint()
        {
            report = new MaterialReturn();
            report.Parameters["DocumentNo"].Value = DocumentNo;
            DevExpress.Xpf.Printing.PrintHelper.ShowPrintPreview(System.Windows.Application.Current.MainWindow, report);
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;
            ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
        }
    }
}