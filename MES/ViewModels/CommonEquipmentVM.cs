using System;
using System.Data;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using MesAdmin.Common.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MesAdmin.ViewModels
{
    public class CommonEquipmentVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public CommonEquipmentList Collections
        {
            get { return GetValue<CommonEquipmentList>(); }
            set { SetValue(value, () => RaisePropertyChanged(nameof(BACCodeEnabled))); }
        }
        public CommonEquipment SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public IEnumerable<CommonMinor> BizAreaCodeList
        {
            get { return GetProperty(() => BizAreaCodeList); }
            set { SetProperty(() => BizAreaCodeList, value); }
        }
        public string BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value); }
        }
        public string WaCode
        {
            get { return GetProperty(() => WaCode); }
            set { SetProperty(() => WaCode, value); }
        }
        public string EqpCode
        {
            get { return GetProperty(() => EqpCode); }
            set { SetProperty(() => EqpCode, value); }
        }
        public string EqpName
        {
            get { return GetProperty(() => EqpName); }
            set { SetProperty(() => EqpName, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        public bool BACCodeEnabled { get { return Collections.Where(o => o.State == EntityState.Added).Count() == 0; } }
        #endregion

        #region Commands
        public ICommand NewCmd { get; set; }
        public ICommand DeleteCmd { get; set; }
        public ICommand SaveCmd { get; set; }
        public ICommand CellValueChangedCmd { get; set; }
        public ICommand SyncErpCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        public ICommand EditCmd { get; set; }
        #endregion

        public CommonEquipmentVM()
        {
            BizAreaCodeList = GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0004");
            if (!string.IsNullOrEmpty(DSUser.Instance.BizAreaCode))
                BizAreaCode = BizAreaCodeList.FirstOrDefault(u => u.MinorCode == DSUser.Instance.BizAreaCode).MinorCode;

            NewCmd = new DelegateCommand(OnNew);
            DeleteCmd = new DelegateCommand(OnDelete);
            SaveCmd = new DelegateCommand(OnSave, CanSave);
            CellValueChangedCmd = new DelegateCommand(OnCellValueChanged);
            SearchCmd = new AsyncCommand(OnSearch);
            SyncErpCmd = new DelegateCommand(OnSyncErp);
        }

        public void OnCellValueChanged()
        {
            if (SelectedItem.State == EntityState.Unchanged)
                SelectedItem.State = EntityState.Modified;
        }

        public void OnNew()
        {
            CommonEquipment item = new CommonEquipment
            {
                State = EntityState.Added,
                BizAreaCode = BizAreaCode,
                UpdateDate = DateTime.Now,
                EqpState = "W",
                IsMonitor = false,
                IsEnabled = true
            };
            Collections.Insert(0, item);
            SelectedItem = item;
        }

        public void OnDelete()
        {
            if (SelectedItem == null) return;

            if (SelectedItem.State == EntityState.Added)
                Collections.Remove(SelectedItem);
            else
                SelectedItem.State =
                    SelectedItem.State == EntityState.Deleted ? EntityState.Unchanged : EntityState.Deleted;
        }

        public bool CanSave()
        {
            return true;
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
                MessageBoxService.ShowMessage(ex.Message, "Information", MessageButton.OK, MessageIcon.Information);
            }
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore).ContinueWith(t => IsBusy = false);
        }
        public void SearchCore()
        {
            string bizAreaCode = BizAreaCode;
            string eqpName = EqpName;
            string waCode = WaCode;

            if (Collections == null)
                Collections = new CommonEquipmentList();
            while (DispatcherService == null) { System.Threading.Thread.Sleep(TimeSpan.FromSeconds(0.1)); }

            Collections = null;
            Collections = new CommonEquipmentList
            (
                new CommonEquipmentList()
                    .Where(p =>
                        string.IsNullOrEmpty(eqpName) ? true : p.EqpName.ToUpper().Contains(EqpName.ToUpper()))
                    .Where(p =>
                        string.IsNullOrEmpty(bizAreaCode) ? true : p.BizAreaCode == bizAreaCode)
                    .Where(p =>
                        string.IsNullOrEmpty(waCode) ? true : p.WaCode == waCode)
            );

            Collections.CollectionChanged += (s, e) => RaisePropertyChanged(nameof(BACCodeEnabled));
        }

        public void OnSyncErp()
        {
            Collections.SyncErp();
            MessageBoxService.ShowMessage("설비정보가 동기화 되었습니다.", "Information", MessageButton.OK, MessageIcon.Information);
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