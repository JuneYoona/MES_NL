using System;
using System.Data;
using System.Linq;
using DevExpress.Mvvm;
using MesAdmin.Models;
using System.Collections.Generic;
using System.Windows.Input;
using System.Threading.Tasks;
using MesAdmin.Common.Common;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using DevExpress.Mvvm.POCO;
using MesAdmin.Common.Utils;
using DevExpress.Xpf.Grid;

namespace MesAdmin.ViewModels
{
    public class QualityInspectItemBizVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        IDialogService PopupItemView { get { return GetService<IDialogService>("ItemView"); } }
        #endregion

        #region Public Properties
        public QualityInspectItemList Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public ObservableCollection<QualityInspectItem> SelectedItems
        {
            get { return GetProperty(() => SelectedItems); }
            set { SetProperty(() => SelectedItems, value); }
        }
        public IEnumerable<CommonMinor> ItemAccount { get; set; }
        public IEnumerable<CodeName> QrType { get; set; }
        public string EditItemCode
        {
            get { return GetProperty(() => EditItemCode); }
            set { SetProperty(() => EditItemCode, value); }
        }
        public string EditQrType
        {
            get { return GetProperty(() => EditQrType); }
            set { SetProperty(() => EditQrType, value); }
        }
        public string BizCode
        {
            get { return GetProperty(() => BizCode); }
            set { SetProperty(() => BizCode, value); }
        }
        public IEnumerable<CommonBizPartner> BizPartnerList
        {
            get { return GetProperty(() => BizPartnerList); }
            set { SetProperty(() => BizPartnerList, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        public bool IsSearch
        {
            get { return GetProperty(() => IsSearch); }
            set { SetProperty(() => IsSearch, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public ICommand AddCmd { get; set; }
        public ICommand NewCmd { get; set; }
        public DelegateCommand<object> DelCmd { get; set; }
        public AsyncCommand SaveCmd { get; set; }
        public ICommand<CellValueChangedEvent> CellValueChangedCmd { get; set; }
        public ICommand ShowDialogCmd { get; set; }
        #endregion

        public QualityInspectItemBizVM()
        {
            IsSearch = false;

            ItemAccount = GlobalCommonMinor.Instance.Where(u => u.MajorCode == "P1001");
            QrType = GlobalCommonQrType.Instance;
            EditQrType = "OQC";
            BindingBizPartnerList();

            SearchCmd = new AsyncCommand(OnSearch, CanSearch);
            AddCmd = new DelegateCommand(Add, CanAdd);
            DelCmd = new DelegateCommand<object>(Delete, CanDel);
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            NewCmd = new DelegateCommand(OnNew);
            CellValueChangedCmd = new DelegateCommand<CellValueChangedEvent>(OnCellValueChanged);
            ShowDialogCmd = new DelegateCommand(OnShowDialog);

            SelectedItems = new ObservableCollection<QualityInspectItem>();
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
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
            return GlobalCommonBizPartner.Instance.Where(u => (u.BizType == "C" || u.BizType == "CS") && u.IsEnabled);
        }

        public bool CanSearch()
        {
            return !string.IsNullOrEmpty(EditItemCode) && !string.IsNullOrEmpty(EditQrType) && !string.IsNullOrEmpty(BizCode);
        }
        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            string editQrType = EditQrType;
            string editItemCode = EditItemCode;
            string editBizCode = BizCode;

            Collections = new QualityInspectItemList(editQrType, editItemCode, editBizCode);
            IsBusy = false;
            IsSearch = true;
        }

        public bool CanSave()
        {
            bool ret = true;
            if (Collections == null) return false;
            ret = Collections.Where(u => u.State == EntityState.Deleted || u.State == EntityState.Added || u.State == EntityState.Modified).Count() > 0;
            // 필수 입력값 처리
            foreach (QualityInspectItem item in Collections.Where(u => u.State == EntityState.Added || u.State == EntityState.Modified))
            {
                if (string.IsNullOrEmpty(item.InspectName) || item.Order == null)
                {
                    ret = false;
                    break;
                }
            }
            if (Collections.Count == 0) return false;

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
                Collections.ToList().ForEach(u => { u.QrType = EditQrType; u.ItemCode = EditItemCode; u.BizCode = BizCode; });
                Collections.Save();
                OnSearch();
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

        public bool CanAdd()
        {
            return IsSearch;
        }
        public void Add()
        {
            Collections.Insert(Collections.Count, new QualityInspectItem
            {
                State = EntityState.Added,
                Editor = "TextEdit"
            });
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

        public void OnCellValueChanged(CellValueChangedEvent pm)
        {
            TableView view = pm.sender as TableView;
            GridControl grid = view.Grid;

            if (grid.GetCellValue(pm.e.RowHandle, "State").ToString() == "Unchanged")
                grid.SetCellValue(pm.e.RowHandle, "State", "Modified");
        }

        public void OnNew()
        {
            IsSearch = false;
            EditItemCode = null;
            BizCode = null;
            EditQrType = "OQC";
            Collections = null;
            SelectedItems.Clear();
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
                EditItemCode = vmItem.ConfirmItem.ItemCode;
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