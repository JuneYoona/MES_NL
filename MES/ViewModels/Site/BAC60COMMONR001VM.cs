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
    public class BAC60COMMONR001VM : ViewModelBase
    {
        #region Services
        IDialogService PopupItemView { get { return GetService<IDialogService>("PopupItemView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public CommonYieldWE10PerItemList Collection
        {
            get { return GetProperty(() => Collection); }
            set { SetProperty(() => Collection, value); }
        }
        public ObservableCollection<CommonYieldWE10PerItem> SelectedItems
        {
            get { return GetProperty(() => SelectedItems); }
            set { SetProperty(() => SelectedItems, value); }
        }
        public CommonYieldWE10PerItem SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public CommonItemList Items { get; set; }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        #endregion

        #region Commands
        public ICommand NewCmd { get; set; }
        public DelegateCommand<object> DeleteCmd { get; set; }
        public ICommand SaveCmd { get; set; }
        public ICommand CellValueChangedCmd { get; set; }
        public ICommand<HiddenEditorEvent> HiddenEditorCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        public ICommand<string> ShowDialogCmd { get; set; }
        #endregion

        public BAC60COMMONR001VM()
        {
            NewCmd = new DelegateCommand(OnNew);
            DeleteCmd = new DelegateCommand<object>(OnDelete, (object obj) => SelectedItems.Count > 0);
            HiddenEditorCmd = new DelegateCommand<HiddenEditorEvent>(OnHiddenEditor);
            SaveCmd = new DelegateCommand(OnSave, CanSave);
            ShowDialogCmd = new DelegateCommand<string>(ShowDialog);
            CellValueChangedCmd = new DelegateCommand(OnCellValueChanged);
            SearchCmd = new AsyncCommand(OnSearch);

            SelectedItems = new ObservableCollection<CommonYieldWE10PerItem>();
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;

            // 품목정보
            Task.Factory.StartNew(() => GlobalCommonItem.Instance).ContinueWith(task => { Items = task.Result; });
        }

        private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DeleteCmd.RaiseCanExecuteChanged();
        }

        public void OnCellValueChanged()
        {
            if (SelectedItem == null) return;

            if (SelectedItem.State == EntityState.Unchanged)
                SelectedItem.State = EntityState.Modified;
        }

        public bool CanSave()
        {
            bool ret = true;
            // 필수 입력값 처리
            if (Collection == null) return false;
            foreach (CommonYieldWE10PerItem item in Collection.Where(u => u.State == EntityState.Added || u.State == EntityState.Modified))
            {
                if (string.IsNullOrEmpty(item.ItemCode) || string.IsNullOrEmpty(item.ItemCodeCore) || item.Molecule == null || item.Crude == null)
                {
                    ret = false;
                    break;
                }
            }
            //  추가, 수정, 삭제작업이 있을경우
            if (Collection.Where(u => u.State == EntityState.Deleted || u.State == EntityState.Added || u.State == EntityState.Modified).Count() == 0)
                ret = false;

            return ret;
        }
        public void OnSave()
        {
            try
            {
                Collection.Save();
                OnSearch();
            }
            catch (Exception ex)
            {
                string message;
                if (ex is SqlException)
                {
                    SqlException sqlEx = ex as SqlException;
                    if (sqlEx.Number == 2627) message = "이미 등록이 되어있는 품목코드입니다!";
                    else message = ex.Message;
                }
                else message = ex.Message;

                MessageBoxService.ShowMessage(message, "Warning", MessageButton.OK, MessageIcon.Warning);
            }
        }

        public void OnNew()
        {
            CommonYieldWE10PerItem item = new CommonYieldWE10PerItem
            {
                State = Common.Common.EntityState.Added,
                UpdateDate = DateTime.Now,
            };
            Collection.Insert(0, item);
            SelectedItem = item;
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore).ContinueWith(task => IsBusy = false);
        }
        public void SearchCore()
        {
            Collection = new CommonYieldWE10PerItemList();
        }

        public void OnDelete(object obj)
        {
            SelectedItems.ToList().ForEach(u =>
            {
                if (u.State == Common.Common.EntityState.Added)
                    Collection.Remove(u);
                else
                    u.State = u.State == Common.Common.EntityState.Deleted ? Common.Common.EntityState.Unchanged : Common.Common.EntityState.Deleted;
            });
        }

        public void OnHiddenEditor(HiddenEditorEvent pm)
        {
            if (pm.e.Column.FieldName != "ItemCode" && pm.e.Column.FieldName != "ItemCodeCore") return;

            TableView view = pm.sender as TableView;
            GridControl grid = view.Grid;

            CommonItem item = Items.Where(u => u.ItemCode == (string)pm.e.Value).FirstOrDefault();
            if (item == null)
            {
                if (pm.e.Column.FieldName == "ItemCode")
                {
                    grid.SetCellValue(pm.e.RowHandle, "ItemCode", null);
                    grid.SetCellValue(pm.e.RowHandle, "ItemName", null);
                    grid.SetCellValue(pm.e.RowHandle, "ItemSpec", null);
                }
                else
                {
                    grid.SetCellValue(pm.e.RowHandle, "ItemCodeCore", null);
                    grid.SetCellValue(pm.e.RowHandle, "ItemNameCore", null);
                }
            }
            else
            {
                if (pm.e.Column.FieldName == "ItemCode")
                {
                    grid.SetCellValue(pm.e.RowHandle, "ItemName", item.ItemName);
                    grid.SetCellValue(pm.e.RowHandle, "ItemSpec", item.ItemSpec);
                }
                else grid.SetCellValue(pm.e.RowHandle, "ItemNameCore", item.ItemName);
            }
        }

        public void ShowDialog(string pm)
        {
            var vmItem = ViewModelSource.Create(() => new PopupItemVM());
            PopupItemView.ShowDialog(
                dialogCommands: vmItem.DialogCmds,
                title: "품목선택",
                viewModel: vmItem
            );

            if (vmItem.ConfirmItem != null)
            {
                if(pm == "ItemCode") SelectedItems[0].ItemCode = vmItem.ConfirmItem.ItemCode;
                else SelectedItems[0].ItemCodeCore = vmItem.ConfirmItem.ItemCode;

                SelectedItem = null; // for hiding editor
            }
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