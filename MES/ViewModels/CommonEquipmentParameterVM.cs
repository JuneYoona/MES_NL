using System;
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
    public class CommonEquipmentParameterVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        IDialogService PopupEqpView { get { return GetService<IDialogService>("EqpView"); } }
        #endregion

        #region Public Properties
        public CommonEquipmentParameterList Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public ObservableCollection<CommonEquipmentParameter> SelectedItems
        {
            get { return GetProperty(() => SelectedItems); }
            set { SetProperty(() => SelectedItems, value); }
        }
        public IEnumerable<CommonMinor> ItemAccount { get; set; }
        public string BizAreaCode { get; set; }
        public string BizAreaName { get; set; }
        public CommonEquipment Equipment
        {
            get { return GetProperty(() => Equipment); }
            set { SetProperty(() => Equipment, value); }
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
        public ObservableCollection<ItemInfo> Type { get; set; }
        public string SelectedType
        {
            get { return GetProperty(() => SelectedType); }
            set { SetProperty(() => SelectedType, value); }
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

        public CommonEquipmentParameterVM()
        {
            IsSearch = false;

            BizAreaCode = DSUser.Instance.BizAreaCode;
            BizAreaName = GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0004" && u.MinorCode == BizAreaCode).FirstOrDefault().MinorName;

            Type = new ObservableCollection<ItemInfo>();
            Type.Add(new ItemInfo { Text = "작업전", Value = "A" });
            Type.Add(new ItemInfo { Text = "작업중", Value = "B" });
            Type.Add(new ItemInfo { Text = "작업종료", Value = "C" });

            SearchCmd = new AsyncCommand(OnSearch, CanSearch);
            AddCmd = new DelegateCommand(Add, CanAdd);
            DelCmd = new DelegateCommand<object>(Delete, CanDel);
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            NewCmd = new DelegateCommand(OnNew);
            CellValueChangedCmd = new DelegateCommand<CellValueChangedEvent>(OnCellValueChanged);
            ShowDialogCmd = new DelegateCommand(OnShowDialog);

            SelectedItems = new ObservableCollection<CommonEquipmentParameter>();
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
        }

        private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DelCmd.RaiseCanExecuteChanged();
        }

        public bool CanSearch()
        {
            return Equipment != null && !string.IsNullOrEmpty(Equipment.EqpCode) && !string.IsNullOrEmpty(SelectedType);
        }
        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            string eqpCode = Equipment.EqpCode;
            string seq = SelectedType;

            Collections = new CommonEquipmentParameterList(eqpCode, seq);
            IsBusy = false;
            IsSearch = true;
        }

        public bool CanSave()
        {
            bool ret = true;
            if (Collections == null) return false;
            ret = Collections.Where(u => u.State == Common.Common.EntityState.Deleted || u.State == Common.Common.EntityState.Added || u.State == Common.Common.EntityState.Modified).Count() > 0;
            // 필수 입력값 처리
            foreach (CommonEquipmentParameter item in Collections.Where(u => u.State == Common.Common.EntityState.Added || u.State == Common.Common.EntityState.Modified))
            {
                if (string.IsNullOrEmpty(item.Parameter) || item.Order == null)
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
            Collections.Insert(Collections.Count, new CommonEquipmentParameter
            {
                State = Common.Common.EntityState.Added,
                BizAreaCode = BizAreaCode,
                EqpCode = Equipment.EqpCode,
                Seq = SelectedType
            });
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
            Equipment = null;
            SelectedType = null;
            Collections = null;
            SelectedItems.Clear();
        }

        public void OnShowDialog()
        {
            var vmItem = ViewModelSource.Create(() => new PopupEquipmentVM(BizAreaCode));
            PopupEqpView.ShowDialog(
                dialogCommands: vmItem.DialogCmds,
                title: "설비선택",
                viewModel: vmItem
            );

            if (vmItem.ConfirmItem != null)
            {
                Equipment = vmItem.ConfirmItem;
                Collections = null;
                IsSearch = false;
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