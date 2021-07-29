using System;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using System.Threading.Tasks;
using DevExpress.Mvvm.POCO;
using MesAdmin.Common.Utils;
using DevExpress.Xpf.Grid;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Data.SqlClient;
using MesAdmin.Common.Common;

namespace MesAdmin.ViewModels
{
    public class SalesPlanVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        IDialogService PopupItemView { get { return GetService<IDialogService>(); } }
        #endregion

        #region Public Properties
        public SalesPlanList Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public ObservableCollection<SalesPlan> SelectedItems
        {
            get { return GetProperty(() => SelectedItems); }
            set { SetProperty(() => SelectedItems, value); }
        }
        public SalesPlan SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public IEnumerable<CommonBizPartner> BizPartnerList { get; set; }
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
        public ICommand SaveCmd { get; set; }
        public ICommand<HiddenEditorEvent> HiddenEditorCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        public ICommand ShowDialogCmd { get; set; }
        #endregion

        public SalesPlanVM()
        {
            PlanYear = DateTime.Now;
            // 품목정보
            Task.Factory.StartNew(() => GlobalCommonItem.Instance).ContinueWith(task => { Items = task.Result; });

            AddCmd = new DelegateCommand(OnAdd);
            DelCmd = new DelegateCommand<object>(OnDelete, CanDel);
            SearchCmd = new AsyncCommand(OnSearch);
            SaveCmd = new DelegateCommand(OnSave, CanSave);
            ShowDialogCmd = new DelegateCommand<string>(ShowDialog);
            HiddenEditorCmd = new DelegateCommand<HiddenEditorEvent>(OnHiddenEditor);

            SelectedItems = new ObservableCollection<SalesPlan>();
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;

            BindingBizPartnerList();
        }

        private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DelCmd.RaiseCanExecuteChanged();
        }

        // 시간이 많이 걸리는 작업이어서 async binding
        private async void BindingBizPartnerList()
        {
            var task = Task<IEnumerable<CommonBizPartner>>.Factory.StartNew(LoadingBizPartnerList);
            await task;

            if (task.IsCompleted)
            {
                BizPartnerList = task.Result;
            }
        }

        private IEnumerable<CommonBizPartner> LoadingBizPartnerList()
        {
            return (new CommonBizPartnerList()).Where(u => u.BizType.Substring(0, 1) == "C" && u.IsEnabled == true);
        }

        public bool CanSave()
        {
            bool ret = true;

            if (Collections == null) return false;
            ret = Collections.Where(u => u.State == Common.Common.EntityState.Deleted || u.State == Common.Common.EntityState.Added).Count() > 0;

            // 필수 입력값 처리
            foreach (SalesPlan item in Collections.Where(u => u.State == Common.Common.EntityState.Added))
            {
                if (string.IsNullOrEmpty(item.ItemCode) || string.IsNullOrEmpty(item.BizCode) || item.PlanMonth <= 0 || item.Qty <= 0 || item.Account <= 0)
                {
                    ret = false;
                    break;
                }
            }

            if (Collections.Count == 0) return false;

            return ret;
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

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            Collections = new SalesPlanList(PlanYear.ToString("yyyy"), BizCode, ItemCode);
            IsBusy = false;
        }

        public void OnAdd()
        {
            int idx = Collections.IndexOf(SelectedItem);
            Collections.Insert(idx + 1, new SalesPlan
            {
                State = MesAdmin.Common.Common.EntityState.Added,
                PlanYear = int.Parse(PlanYear.ToString("yyyy")),
                ApplyDate = DateTime.Now,
                UpdateDate = DateTime.Now
            });
        }

        bool CanDel(object obj) { return SelectedItems.Count > 0; }
        public void OnDelete(object obj)
        {
            SelectedItems.ToList().ForEach(u =>
            {
                if (u.State == MesAdmin.Common.Common.EntityState.Added)
                    Collections.Remove(u);
                else
                    u.State = u.State == MesAdmin.Common.Common.EntityState.Deleted ? MesAdmin.Common.Common.EntityState.Unchanged : MesAdmin.Common.Common.EntityState.Deleted;
            });
        }

        public void ShowDialog(string pm)
        {
            var vmItem = ViewModelSource.Create(() => new PopupItemVM("15"));
            PopupItemView.ShowDialog(
                dialogCommands: vmItem.DialogCmds,
                title: "품목선택",
                viewModel: vmItem
            );

            if (vmItem.ConfirmItem != null)
            {
                if (pm == "Filtering")
                    ItemCode = vmItem.ConfirmItem.ItemCode;
                else
                {
                    SelectedItem.ItemCode = vmItem.ConfirmItem.ItemCode;
                    SelectedItem.ItemName = vmItem.ConfirmItem.ItemName;
                    SelectedItem.ItemSpec = vmItem.ConfirmItem.ItemSpec;
                    SelectedItem.BasicUnit = vmItem.ConfirmItem.BasicUnit;
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
