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
using System.Data.SqlClient;

namespace MesAdmin.ViewModels
{
    public class ExceptionOutVM : ViewModelBase
    {
        #region Services
        IDialogService PopupStockView { get { return GetService<IDialogService>("StockView"); } }
        IDialogService PopupStockMoveView { get { return GetService<IDialogService>("StockMoveView"); } }
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
            set { SetProperty(() => SelectedMoveType, value); }
        }
        public ObservableCollection<CommonMinor> WareHouse { get; set; }
        public CommonMinor SelectedWh
        {
            get { return GetProperty(() => SelectedWh); }
            set { SetProperty(() => SelectedWh, value); }
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
        #endregion

        #region Commands
        public ICommand ShowDialogCmd { get; set; }
        public ICommand<HiddenEditorEvent> HiddenEditorCmd { get; set; }
        public ICommand AddCmd { get; set; }
        public ICommand NewCmd { get; set; }
        public ICommand WhValueChangedCmd { get; set; }
        public DelegateCommand<object> DelCmd { get; set; }
        public AsyncCommand SaveCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        #endregion

        public ExceptionOutVM()
        {
            IsNew = true;
            Collections = new StockMovementDetailList();
            Items = new CommonItemList(); // 품목정보
            
            AddCmd = new DelegateCommand(Add, CanAdd);
            DelCmd = new DelegateCommand<object>(Delete, CanDel);
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            NewCmd = new DelegateCommand(OnNew);
            WhValueChangedCmd = new DelegateCommand(OnWhValueChanged);
            ShowDialogCmd = new DelegateCommand<string>(ShowDialog);
            HiddenEditorCmd = new DelegateCommand<HiddenEditorEvent>(OnHiddenEditor);
            SearchCmd = new AsyncCommand(OnSearch, CanSearch);

            SelectedItems = new ObservableCollection<StockMovementDetail>();
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;

            MoveType = new ObservableCollection<CommonMinor>
            (
                GlobalCommonMinor.Instance.Where(u => u.Ref02 == "OI" && u.IsEnabled == true)
            ); // 예외출고
            WareHouse = new CommonMinorList
            (
                GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0011")
            ); // 창고정보
            WaCode = GlobalCommonWorkAreaInfo.Instance; // 공정정보
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
                if (header.TransType != "OI")
                {
                    DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage("기타출고에 사용할 수 없는 수불유형입니다!"
                                                        , "Information"
                                                        , MessageButton.OK
                                                        , MessageIcon.Information));
                    IsNew = true;
                    return;
                }
            }

            Collections = new StockMovementDetailList(documentNo: DocumentNo);
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
                SelectedMoveType = new CommonMinor { MinorCode = header.MoveType };
                DocumentDate = header.DocumentDate;
                Memo = header.Memo;
                SelectedWh = new CommonMinor { MinorCode = Collections.FirstOrDefault().WhCode };
            }
        }

        bool CanDel(object obj) { return SelectedItems.Count > 0; }
        public void Delete(object obj)
        {
            SelectedItems.ToList().ForEach(u =>
            {
                if (u.State == Common.Common.EntityState.Added)
                    Collections.Remove(u);
                else
                    u.State = u.State == Common.Common.EntityState.Deleted ? Common.Common.EntityState.Unchanged : Common.Common.EntityState.Deleted;
            });
        }

        public void Add()
        {
            Collections.Insert(Collections.Count, new StockMovementDetail
            {
                State = Common.Common.EntityState.Added,
                TransType = SelectedMoveType.Ref02,
                MoveType = SelectedMoveType.MinorCode,
                DCFlag = SelectedMoveType.Ref01,
                BizAreaCode = SelectedWh.Ref01,
                WhCode = SelectedWh.MinorCode,
                StockType = SelectedMoveType.Ref03
            });
        }
        public bool CanAdd()
        {
            return SelectedMoveType != null && SelectedWh != null && IsNew;
        }

        public void ShowDialog(string pm)
        {
            if (pm == "DocumentNo") // 수불현황조회
            {
                var vmMove = ViewModelSource.Create(() => new PopupStockMoveVM("OI")
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
            else // 재고조회
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
                                    State = Common.Common.EntityState.Added,
                                    BizAreaCode = item.BizAreaCode,
                                    ItemCode = item.ItemCode,
                                    ItemName = item.ItemName,
                                    ItemSpec = item.ItemSpec,
                                    WhCode = item.WhCode,
                                    LotNo = item.LotNo,
                                    Qty = item.Qty - item.PickingQty,
                                    BasicUnit = item.BasicUnit,
                                    WaCode = item.WaCode,

                                    TransType = SelectedMoveType.Ref02,
                                    MoveType = SelectedMoveType.MinorCode,
                                    DCFlag = SelectedMoveType.Ref01,
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
        }

        public bool CanSave()
        {
            bool ret = true;
            if (IsNew)
            {
                // 필수 입력값 처리
                foreach (StockMovementDetail item in Collections.Where(u => u.State == Common.Common.EntityState.Added))
                {
                    if (string.IsNullOrEmpty(item.ItemCode) || item.Qty == null)
                    {
                        ret = false;
                        break;
                    }
                }
                if (Collections.Count == 0) return false;
            }
            else
                ret = Collections.Where(u => u.State == Common.Common.EntityState.Deleted).Count() > 0;

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
            DocumentDate = DateTime.Now;
            Memo = null;
        }

        public void OnWhValueChanged()
        {
            if (IsNew)
                Collections.Clear();
        }

        public void OnHiddenEditor(HiddenEditorEvent pm)
        {
            if (pm.e.Column.FieldName != "ItemCode") return;

            TableView view = pm.sender as TableView;
            GridControl grid = view.Grid;

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

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;
            ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
        }
    }
}
