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
using System.Data.SqlClient;
using MesAdmin.Common.Utils;
using DevExpress.Xpf.Grid;

namespace MesAdmin.ViewModels
{
    public class SalesBizItemVM : ViewModelBase
    {
        #region Services
        IDialogService PopupItemView { get { return GetService<IDialogService>("PopupItemView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public SalesBizItemList Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public ObservableCollection<SalesBizItem> SelectedItems
        {
            get { return GetProperty(() => SelectedItems); }
            set { SetProperty(() => SelectedItems, value); }
        }
        public SalesBizItem SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public IEnumerable<CommonBizPartner> BizPartnerList { get; set; }
        public string BizCode
        {
            get { return GetProperty(() => BizCode); }
            set { SetProperty(() => BizCode, value); }
        }
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
        public ObservableCollection<CommonMinor> BizUnit
        {
            get { return GetProperty(() => BizUnit); }
            set { SetProperty(() => BizUnit, value); }
        }
        public CommonItemList Items { get; set; }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        #endregion

        #region Commands
        public ICommand<string> ShowDialogCmd { get; set; }
        public ICommand AddCmd { get; set; }
        public DelegateCommand<object> DelCmd { get; set; }
        public AsyncCommand SaveCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        public ICommand CellValueChangedCmd { get; set; }
        public ICommand<HiddenEditorEvent> HiddenEditorCmd { get; set; }
        #endregion

        public SalesBizItemVM()
        {
            BizPartnerList = (new CommonBizPartnerList()).Where(u => u.BizType == "C" || u.BizType == "CV");
            SelectedItems = new ObservableCollection<SalesBizItem>();
            BizUnit = new CommonMinorList(majorCode: "P1100");
            // 품목정보
            Task.Factory.StartNew(() => GlobalCommonItem.Instance).ContinueWith(task => { Items = task.Result; });
            
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            SearchCmd = new AsyncCommand(OnSearch);
            AddCmd = new DelegateCommand(OnAdd);
            DelCmd = new DelegateCommand<object>(Delete, CanDel);
            ShowDialogCmd = new DelegateCommand<string>(ShowDialog);
            CellValueChangedCmd = new DelegateCommand(OnCellValueChanged);
            HiddenEditorCmd = new DelegateCommand<HiddenEditorEvent>(OnHiddenEditor);

            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
            OnSearch();
        }

        private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DelCmd.RaiseCanExecuteChanged();
        }

        public void OnCellValueChanged()
        {
            if (SelectedItem.State == EntityState.Unchanged)
                SelectedItem.State = EntityState.Modified;
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
                if(pm == "Find")
                    ItemCode = vmItem.ConfirmItem.ItemCode;
                else
                { 
                    SelectedItem.ItemCode = vmItem.ConfirmItem.ItemCode;
                    SelectedItem.ItemName = vmItem.ConfirmItem.ItemName;
                    SelectedItem.ItemSpec = vmItem.ConfirmItem.ItemSpec;
                }
            }
        }

        public bool CanSave()
        {
            bool ret = false;

            if (Collections.Count() == 0) return false;
            if (Collections.Where(u => u.State == EntityState.Deleted).Count() > 0)
                ret = true;

            if (Collections.Where(u => u.State == EntityState.Modified).Count() > 0)
                ret = true;

            // 필수 입력값 처리
            foreach (var item in Collections.Where(u => u.State == EntityState.Added))
            {
                if (string.IsNullOrEmpty(item.BizCode) || string.IsNullOrEmpty(item.ItemCode))
                {
                    ret = false;
                    break;
                }
                else
                    ret = true;
            }
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
                Collections.Save();
                OnSearch();
            }
            catch (Exception ex)
            {
                string message;
                if (ex is SqlException)
                {
                    SqlException sqlEx = ex as SqlException;
                    if (sqlEx.Number == 2627)
                        message = "이미 등록이 되어있습니다!";
                    else
                        message = ex.Message;
                }
                else
                {
                    message = ex.Message;
                }
                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(message
                                                    , "Information"
                                                    , MessageButton.OK
                                                    , MessageIcon.Information));
            }
            IsBusy = false;
        }

        public void OnAdd()
        {
            Collections.Insert(0, new SalesBizItem
            {
                State = EntityState.Added
            });
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            string bizCode = BizCode;
            string itemCode = ItemCode;
            string itemName = ItemName;

            Collections = new SalesBizItemList(bizCode: bizCode, itemCode: itemCode, itemName: itemName);
            IsBusy = false;
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
            }
            else
            {
                grid.SetCellValue(pm.e.RowHandle, "ItemName", item.ItemName);
                grid.SetCellValue(pm.e.RowHandle, "ItemSpec", item.ItemSpec);
            }
        }
    }
}
