using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using MesAdmin.Common.Common;
using MesAdmin.Common.Utils;
using MesAdmin.Models;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MesAdmin.ViewModels
{
    public class CommonBOMVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        IDialogService PopupItemView { get { return GetService<IDialogService>(); } }
        #endregion

        #region Public Properties
        public CommonBillOfMaterialList Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public ObservableCollection<CommonBillOfMaterial> SelectedItems { get; } = new ObservableCollection<CommonBillOfMaterial>();
        public CommonBillOfMaterial FocusedItem
        {
            get { return GetProperty(() => FocusedItem); }
            set { SetProperty(() => FocusedItem, value); }
        }
        public string ItemCode
        {
            get { return GetProperty(() => ItemCode); }
            set { SetProperty(() => ItemCode, value); }
        }
        public string Direction
        {
            get { return GetProperty(() => Direction); }
            set { SetProperty(() => Direction, value); }
        }
        public DateTime CheckDate
        {
            get { return GetProperty(() => CheckDate); }
            set { SetProperty(() => CheckDate, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        public CommonItemList Items { get; set; }
        #endregion

        #region Commands
        public ICommand<object> AddCmd { get; set; }
        public DelegateCommand<object> DeleteCmd { get; set; }
        public ICommand SaveCmd { get; set; }
        public ICommand SyncErpCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        public ICommand EditCmd { get; set; }
        public ICommand CellValueChangedCmd { get; set; }
        public ICommand<TreeListView> ShowDialogCmd { get; set; }
        public ICommand<HiddenTreeListEditorEvent> HiddenEditorCmd { get; set; }
        #endregion

        public CommonBOMVM()
        {
            Direction = "정전개";
            DeleteCmd = new DelegateCommand<object>(OnDelete, CanDelete);
            AddCmd = new DelegateCommand<object>(OnAdd, CanAdd);
            SearchCmd = new AsyncCommand(OnSearch);
            SaveCmd = new DelegateCommand(OnSave, CanSave);
            CellValueChangedCmd = new DelegateCommand(OnCellValueChanged);
            HiddenEditorCmd = new DelegateCommand<HiddenTreeListEditorEvent>(OnHiddenEditor);
            ShowDialogCmd = new DelegateCommand<TreeListView>(ShowDialog);
            SyncErpCmd = new DelegateCommand(OnSyncErp);
            CheckDate = DateTime.Now;

            Items = new CommonItemList(); // 품목정보
        }

        public void OnCellValueChanged()
        {
            if (FocusedItem.State == EntityState.Unchanged)
                FocusedItem.State = EntityState.Modified;
        }
        
        public bool CanSave()
        {
            bool ret = true;
            if (Collections == null) return false;
            // 필수 입력값 처리
            foreach (CommonBillOfMaterial item in Collections.Where(u => u.State == EntityState.Added || u.State == EntityState.Modified))
            {
                if (item.StartDate == null || string.IsNullOrEmpty(item.CItemCode))
                {
                    ret = false;
                    break;
                }
            }
            //  추가, 삭제, 수정작업이 있을경우
            if (Collections.Where(u => u.State == EntityState.Deleted || u.State == EntityState.Added || u.State == EntityState.Modified).Count() == 0)
                ret = false;

            return ret && Direction == "정전개";
        }
        public void OnSave()
        {
            try
            {
                Collections.Save();
                OnSearch();
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowMessage(ex.Message);
            }
        }

        public bool CanAdd(object sender) { return FocusedItem != null && Direction == "정전개"; }
        public void OnAdd(object sender)
        {
            if (string.IsNullOrEmpty(FocusedItem.CItemCode))
            {
                MessageBoxService.ShowMessage("품목코드를 먼저 선택하세요!", "Information", MessageButton.OK, MessageIcon.Information);
                return;
            }

            Collections.Add(new CommonBillOfMaterial
            {
                State = EntityState.Added,
                PItemCode = FocusedItem.CItemCode,
                KeyFieldName = new Random().Next().ToString(), // 임시
                ParentFieldName = FocusedItem.KeyFieldName,
                PUnit = FocusedItem.CUnit,
                RecursionLevel = FocusedItem.RecursionLevel + 1,
            });

            TreeListControl grid = sender as TreeListControl;
            TreeListView view = grid.View;

            int[] i = grid.GetSelectedRowHandles();
            view.ExpandNode(i[0]);
        }

        public Task OnSearch()
        {
            IsBusy = true;
            FocusedItem = null;
            Collections = null;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            Collections = new CommonBillOfMaterialList(ItemCode, CheckDate, Direction == "정전개" ? "forward" : "reverse");
            IsBusy = false;
        }

        public void ShowDialog(TreeListView view)
        {
            var vmItem = ViewModelSource.Create(() => new PopupItemVM());
            PopupItemView.ShowDialog(
                dialogCommands: vmItem.DialogCmds,
                title: "품목선택",
                viewModel: vmItem
            );

            if (vmItem.ConfirmItem != null)
            {
                if (view == null)
                {
                    ItemCode = vmItem.ConfirmItem.ItemCode;
                }
                else
                {
                    FocusedItem.CItemCode = vmItem.ConfirmItem.ItemCode;
                    view.HideEditor();
                }
            }
        }

        public void OnHiddenEditor(HiddenTreeListEditorEvent pm)
        {
            if (pm.e.Column.FieldName != "CItemCode") return;

            TreeListView view = pm.sender as TreeListView;
            TreeListNode tn = view.GetNodeByRowHandle(pm.e.RowHandle);

            CommonItem item = Items.Where(u => u.ItemCode == (string)pm.e.Value).FirstOrDefault();
            if (item == null)
            {
                view.SetNodeValue(tn, "CItemCode", null);
                view.SetNodeValue(tn, "ItemName", null);
                view.SetNodeValue(tn, "ItemSpec", null);
                view.SetNodeValue(tn, "CUnit", null);
            }
            else
            {
                int cnt = Collections.Where(u => u.PItemCode == FocusedItem.PItemCode && u.CItemCode == item.ItemCode).Count();

                if (cnt != 1)
                {
                    MessageBoxService.ShowMessage("같은 품목이 존재합니다!", "Warning", MessageButton.OK, MessageIcon.Warning);
                    view.SetNodeValue(tn, "CItemCode", null);
                    return;
                }

                if (FocusedItem.PItemCode == FocusedItem.CItemCode)
                {
                    MessageBoxService.ShowMessage("모품목코드와 동일합니다!", "Warning", MessageButton.OK, MessageIcon.Information);
                    view.SetNodeValue(tn, "CItemCode", null);
                    return;
                }

                view.SetNodeValue(tn, "ItemName", item.ItemName);
                view.SetNodeValue(tn, "ItemSpec", item.ItemSpec);
                view.SetNodeValue(tn, "CUnit", item.BasicUnit);
            }
        }

        bool CanDelete(object obj) { return FocusedItem != null && FocusedItem.RecursionLevel > 0 && Direction == "정전개"; }
        public void OnDelete(object obj)
        {
            SelectedItems.ToList().ForEach(u =>
            {
                if (u.RecursionLevel == 0) return;
                if (u.State == EntityState.Added)
                    Collections.Remove(u);
                else
                    u.State = u.State == EntityState.Deleted ? EntityState.Unchanged : EntityState.Deleted;
            });
        }

        public void OnSyncErp()
        {
            if (Collections == null) Collections = new CommonBillOfMaterialList();
            Collections.SyncErp();
            MessageBoxService.ShowMessage("BOM 정보가 동기화 되었습니다.", "Information", MessageButton.OK, MessageIcon.Information);
            OnSearch();
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;
            Task.Factory.StartNew(SearchCore).ContinueWith(task =>
            {
                ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
            });
        }
    }
}