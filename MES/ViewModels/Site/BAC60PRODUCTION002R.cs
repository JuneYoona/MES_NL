using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using MesAdmin.Common.Common;
using MesAdmin.Common.Utils;
using MesAdmin.Models;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MesAdmin.ViewModels
{
    public class BAC60PRODUCTION002RVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        IDialogService PopupItemView { get { return GetService<IDialogService>(); } }
        IDialogService BAC60PRODUCTION003C { get { return GetService<IDialogService>("BAC60PRODUCTION003C"); } }
        #endregion

        #region Public Properties
        public BAC60PRODUCTION002LIST Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public ObservableCollection<BAC60PRODUCTION002> SelectedItems { get; } = new ObservableCollection<BAC60PRODUCTION002>();
        public BAC60PRODUCTION002 SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public DateTime PlanYear
        {
            get { return GetProperty(() => PlanYear); }
            set { SetProperty(() => PlanYear, value); }
        }
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
        public CommonItemList Items { get; set; }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        #endregion

        #region Commands
        public ICommand AddCmd { get; set; }
        public DelegateCommand<object> DelCmd { get; set; }
        public AsyncCommand SaveCmd { get; set; }
        public ICommand<HiddenEditorEvent> HiddenEditorCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        public ICommand CellValueChangedCmd { get; set; }
        public ICommand ShowItemDialogCmd { get; set; }
        public ICommand CopyMonthCmd { get; set; }
        #endregion

        public BAC60PRODUCTION002RVM()
        {
            PlanYear = DateTime.Now;
            // 품목정보
            Task.Factory.StartNew(() => GlobalCommonItem.Instance).ContinueWith(task => { Items = task.Result; });

            AddCmd = new DelegateCommand(OnAdd);
            DelCmd = new DelegateCommand<object>(OnDelete, CanDel);
            SearchCmd = new AsyncCommand(OnSearch);
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            ShowItemDialogCmd = new DelegateCommand<string>(OnShowItemDialog);
            HiddenEditorCmd = new DelegateCommand<HiddenEditorEvent>(OnHiddenEditor);
            CellValueChangedCmd = new DelegateCommand(OnCellValueChanged);
            CopyMonthCmd = new DelegateCommand(OnCopyMonth);
        }

        public bool CanSave()
        {
            if (Collections == null) return false;

            bool ret = Collections.Where(u => u.State != EntityState.Unchanged).Count() > 0;

            // 필수 입력값 처리
            foreach (var item in Collections.Where(u => u.State == EntityState.Added))
            {
                if (string.IsNullOrEmpty(item.ItemCode) || item.PlanMonth <= 0 || item.Qty <= 0 || item.Revision <= 0)
                {
                    ret = false;
                    break;
                }
            }

            return ret;
        }
        public Task OnSave()
        {
            IsBusy = true;
            return Task.Run(() =>
            {
                try
                {
                    Collections.Save();
                    OnSearch();
                }
                catch (Exception ex)
                {
                    string message;
                    if (ex is SqlException exception)
                    {
                        message = exception.Number == 2627 ? "이미 등록이 되어있습니다!" : ex.Message;
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
            }).ContinueWith(t => IsBusy = false);
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore).ContinueWith(t => IsBusy = false);
        }
        public void SearchCore()
        {
            Collections = new BAC60PRODUCTION002LIST(PlanYear.Year, ItemCode);
        }

        public void OnAdd()
        {
            int idx = Collections.IndexOf(SelectedItem);
            Collections.Insert(idx + 1, new BAC60PRODUCTION002
            {
                State = EntityState.Added,
                PlanYear = PlanYear.Year,
                PlanMonth = DateTime.Now.Month,
                Revision = 1,
                UpdateDate = DateTime.Now
            });
        }

        public void OnCellValueChanged()
        {
            if (SelectedItem == null) return;

            if (SelectedItem.State == EntityState.Unchanged)
                SelectedItem.State = EntityState.Modified;
        }

        bool CanDel(object obj) { return SelectedItems.Count > 0; }
        public void OnDelete(object obj)
        {
            SelectedItems.ToList().ForEach(u =>
            {
                if (u.State == EntityState.Added)
                    Collections.Remove(u);
                else
                    u.State = u.State == EntityState.Deleted ? EntityState.Unchanged : EntityState.Deleted;
            });
        }

        public void OnShowItemDialog(string pm)
        {
            var vmItem = ViewModelSource.Create(() => new PopupItemVM("15"));
            PopupItemView.ShowDialog(
                dialogCommands: vmItem.DialogCmds,
                title: "품목선택",
                viewModel: vmItem
            );

            if (vmItem.ConfirmItem != null)
            {
                if (pm == "RowData")
                {
                    SelectedItem.ItemCode = vmItem.ConfirmItem.ItemCode;
                    SelectedItem.ItemName = vmItem.ConfirmItem.ItemName;
                    SelectedItem.ItemSpec = vmItem.ConfirmItem.ItemSpec;
                    SelectedItem.BasicUnit = vmItem.ConfirmItem.BasicUnit;

                    SelectedItem = null; // for hiding editor
                }
                else
                {
                    ItemCode = vmItem.ConfirmItem.ItemCode;
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

        public void OnCopyMonth()
        {
            BAC60PRODUCTION003CVM vm = new BAC60PRODUCTION003CVM();
            UICommand retCmt = BAC60PRODUCTION003C.ShowDialog(dialogCommands: vm.DialogCmds, title: "BAC60PRODUCTION002R", viewModel: vm);
            if (retCmt != null && retCmt.IsDefault == true)
            {
                if (MessageBoxService.ShowMessage("저장하시겠습니까?", "Question", MessageButton.YesNo, MessageIcon.Question) == MessageResult.Yes)
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            IsBusy = true;
                            Commonsp.BAC60PRODUCTION003C(vm.SourceDate.Value, vm.TargetDate.Value);
                        }
                        catch (Exception ex)
                        {
                            string message;
                            if (ex is SqlException exception)
                            {
                                message = exception.Number == 2627 ? "이미 등록이 되어있습니다!" : ex.Message;
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
                    }).ContinueWith(t => OnSearch());
                }
            }
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;
            Task.Factory.StartNew(SearchCore).ContinueWith(task =>
            {
                ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
            });
        }
    }
}