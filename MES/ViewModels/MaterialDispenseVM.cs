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
    public class MaterialDispenseVM : ViewModelBase
    {
        #region Services
        IDialogService DispenseDetailView { get { return GetService<IDialogService>("DispenseDetailView"); } }
        IDialogService PopupReqView { get { return GetService<IDialogService>("PopupReqView"); } }
        IDialogService StockView { get { return GetService<IDialogService>("StockView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public bool IsNew
        {
            get { return GetProperty(() => IsNew); }
            set { SetProperty(() => IsNew, value); }
        }
        public MaterialDispenseDetail Header
        {
            get { return GetProperty(() => Header); }
            set { SetProperty(() => Header, value); }
        }
        public MaterialDispenseDetailSubList Details
        {
            get { return GetProperty(() => Details); }
            set { SetProperty(() => Details, value); }
        }
        public ObservableCollection<CommonMinor> WareHouse { get; set; }
        public CommonMinor InWhCode
        {
            get { return GetProperty(() => InWhCode); }
            set { SetProperty(() => InWhCode, value); }
        }
        public CommonMinor OutWhCode
        {
            get { return GetProperty(() => OutWhCode); }
            set { SetProperty(() => OutWhCode, value); }
        }
        public ObservableCollection<MaterialDispenseDetailSub> SelectedItems
        {
            get { return GetProperty(() => SelectedItems); }
            set { SetProperty(() => SelectedItems, value); }
        }
        public MaterialDispenseDetailSub SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public CommonItemList Items { get; set; }
        public string EditOutWhCode
        {
            get { return GetProperty(() => EditOutWhCode); }
            set { SetProperty(() => EditOutWhCode, value); }
        }
        public DateTime DspDate
        {
            get { return GetProperty(() => DspDate); }
            set { SetProperty(() => DspDate, value); }
        }
        private List<StockDetail> ExceptStocks { get; set; }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        #endregion

        #region Commands
        public ICommand ShowDialogCmd { get; set; }
        public ICommand AddCmd { get; set; }
        public ICommand NewCmd { get; set; }
        public ICommand WhIndexChangedCmd { get; set; }
        public ICommand ReferReqCmd { get; set; }
        public DelegateCommand<object> DelCmd { get; set; }
        public AsyncCommand SaveCmd { get; set; }
        public ICommand<HiddenEditorEvent> HiddenEditorCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        #endregion

        public MaterialDispenseVM()
        {
            // 품목정보
            Task.Factory.StartNew(() => GlobalCommonItem.Instance).ContinueWith(task => { Items = task.Result; });
            ExceptStocks = new List<StockDetail>();

            AddCmd = new DelegateCommand(Add, CanAdd);
            DelCmd = new DelegateCommand<object>(Delete, CanDel);
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            NewCmd = new DelegateCommand(OnNew);
            WhIndexChangedCmd = new DelegateCommand<string>(OnWhIndexChanged);
            ShowDialogCmd = new DelegateCommand<string>(OnShowDialog);
            HiddenEditorCmd = new DelegateCommand<HiddenEditorEvent>(OnHiddenEditor);
            SearchCmd = new AsyncCommand(OnSearch);
            ReferReqCmd = new DelegateCommand(OnReferReq, CanReferReq);

            SelectedItems = new ObservableCollection<MaterialDispenseDetailSub>();
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;

            WareHouse = new CommonMinorList
            (
                GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0011")
            ); // 창고정보

            Header = new MaterialDispenseDetail();
            Details = new MaterialDispenseDetailSubList();
            DspDate = DateTime.Now;
        }

        private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DelCmd.RaiseCanExecuteChanged();
        }

        public void OnWhIndexChanged(string pm)
        {
            if(IsNew)
                Details.Clear();
        }

        public bool CanReferReq()
        {
            return IsNew;
        }
        public void OnReferReq()
        {
            var vmReq = ViewModelSource.Create(() => new PopupMaterialDispenseReqDetailVM());
            PopupReqView.ShowDialog(
                dialogCommands: vmReq.DialogCmds,
                title: "불출요청내역참조",
                viewModel: vmReq
            );

            if (vmReq.ConfirmItem != null)
            {
                Header = vmReq.ConfirmItem;
                Details.Clear();
            }
        }
        
        public Task OnSearch()
        {
            IsBusy = true;
            
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            IsNew = false;
            string mdNo = Header.MDNo;
            int seq = Header.Seq;
            Header = new MaterialDispenseDetail(mdNo, seq);
            Details = new MaterialDispenseDetailSubList(mdNo: mdNo, seq: seq);
            EditOutWhCode = Details.FirstOrDefault().OutWhCode;
            IsBusy = false;
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

        public bool CanAdd()
        {
            return !string.IsNullOrEmpty(Header.MDNo) && !string.IsNullOrEmpty(EditOutWhCode) && IsNew;
        }
        public void Add()
        {
            Details.Insert(Details.Count, new MaterialDispenseDetailSub
            {
                State = EntityState.Added,
                MDNo = Header.MDNo,
                Seq = Header.Seq,
                DspDate = DspDate,
                OutWhCode = OutWhCode.MinorCode,
                InWhCode = Header.InWhCode,
            });
        }

        public bool CanSave()
        {
            bool ret = true;
            if (IsNew)
            {
                // 필수 입력값 처리
                foreach (MaterialDispenseDetailSub item in Details.Where(u => u.State == EntityState.Added))
                {
                    if (string.IsNullOrEmpty(item.ItemCode) || item.DspQty <= 0 || string.IsNullOrEmpty(item.LotNo))
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
                Details.Save();
                IsNew = false;
                OnSearch();
                // send mesaage to parent view
                Messenger.Default.Send<string>("Refresh");
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

        public void OnShowDialog(string pm)
        {
            if (pm == "StockView") // 재고조회
            {
                // 선택된 로트는 재고 pop up에서 제외
                ExceptStocks.Clear();
                Details.Where(u => !string.IsNullOrEmpty(u.ItemCode)).ToList().ForEach(u => ExceptStocks.Add(new StockDetail
                {
                    BizAreaCode = u.BizAreaCode,
                    ItemCode = u.ItemCode,
                    WhCode = u.OutWhCode,
                    WaCode = u.WaCode,
                    LotNo = u.LotNo,
                }));

                var vm = ViewModelSource.Create(() => new PopupStockVM(OutWhCode, Header.ItemCode, ExceptStocks)); // 재고를 조회할 창고전달
                StockView.ShowDialog(
                    dialogCommands: vm.DialogCmds,
                    title: "재고선택",
                    viewModel: vm
                );

                if (vm.ConfirmItems != null && vm.ConfirmItems.Count > 0)
                {
                    try
                    {
                        int idx = Details.IndexOf(SelectedItem);
                        Details.RemoveAt(idx);
                        
                        foreach (StockDetail item in vm.ConfirmItems)
                        {
                            Details.Insert(idx++,
                                new MaterialDispenseDetailSub
                                {
                                    State = EntityState.Added,
                                    BizAreaCode = item.BizAreaCode,
                                    ItemCode = item.ItemCode,
                                    ItemName = item.ItemName,
                                    ItemSpec = item.ItemSpec,
                                    OutWhCode = item.WhCode,
                                    WaCode = item.WaCode,
                                    LotNo = item.LotNo,
                                    DspQty = item.Qty,
                                    BasicUnit = item.BasicUnit,
                                    DspDate = DspDate,
                                    MDNo = Header.MDNo,
                                    Seq = Header.Seq,
                                    InWhCode = Header.InWhCode,
                                    PIG = item.Remark6,
                                    TSC = item.Remark7,
                                    ExpDate = item.ExpDate,
                                }
                            );
                            // 추가된 재고들 선택된걸 보여주기위해
                            SelectedItems.Add(Details.ElementAt(idx - 1));
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBoxService.ShowMessage(ex.Message, "Information", MessageButton.OK, MessageIcon.Information);
                    }
                }
            }
            else
            { 
                var vm = ViewModelSource.Create(() => new PopupMaterialDispenseDetailVM());

                DispenseDetailView.ShowDialog(
                    dialogCommands: vm.DialogCmds,
                    title: "불출현황 정보",
                    viewModel: vm
                );

                if (vm.ConfirmHeader != null)
                {
                    Header = vm.ConfirmHeader;
                    OnSearch();
                }
            }
        }

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
                grid.SetCellValue(pm.e.RowHandle, "BasicUnit", "");
            }
            else
            {
                grid.SetCellValue(pm.e.RowHandle, "ItemName", item.ItemName);
                grid.SetCellValue(pm.e.RowHandle, "ItemSpec", item.ItemSpec);
                grid.SetCellValue(pm.e.RowHandle, "BasicUnit", item.BasicUnit);
            }
        }

        public void OnNew()
        {
            IsNew = true;
            Header = new MaterialDispenseDetail();
            Details.Clear();
            SelectedItems.Clear();
            EditOutWhCode = null;
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
                MaterialDispenseDetail item = pm.Item as MaterialDispenseDetail;
                Header.MDNo = item.MDNo;
                Header.Seq = item.Seq;
                Task.Factory.StartNew(SearchCore).ContinueWith(task =>
                {
                    ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
                });
            }
        }
    }
}
