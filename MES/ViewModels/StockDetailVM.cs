using System;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using MesAdmin.Common.Common;
using System.Data;

namespace MesAdmin.ViewModels
{
    public class StockDetailVM : ExportViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public StockDetailList Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public StockDetail SelectedItem
        {
            get { return GetValue<StockDetail>(); }
            set { SetValue(value); }
        }
        public IEnumerable<CommonMinor> BizAreaCodeList
        {
            get { return GetProperty(() => BizAreaCodeList); }
            set { SetProperty(() => BizAreaCodeList, value); }
        }
        public string BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value, () => { WhCode = ""; ItemAccount = ""; }); }
        }
        public string WhCode
        {
            get { return GetProperty(() => WhCode); }
            set { SetProperty(() => WhCode, value); }
        }
        public string ItemAccount
        {
            get { return GetProperty(() => ItemAccount); }
            set { SetProperty(() => ItemAccount, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public AsyncCommand SaveCmd { get; set; }
        public ICommand<object> ToExcelCmd { get; set; }
        public ICommand CellValueChangedCmd { get; set; }
        #endregion

        public StockDetailVM()
        {
            BizAreaCodeList = GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0004");
            if (!string.IsNullOrEmpty(DSUser.Instance.BizAreaCode))
                BizAreaCode = BizAreaCodeList.FirstOrDefault(u => u.MinorCode == DSUser.Instance.BizAreaCode).MinorCode;

            SearchCmd = new AsyncCommand(OnSearch);
            ToExcelCmd = new DelegateCommand<object>(base.OnToExcel);
            CellValueChangedCmd = new DelegateCommand(OnCellValueChanged);
            SaveCmd = new AsyncCommand(OnSave, () => Collections != null && Collections.Where(o => o.State != EntityState.Unchanged).Count() > 0);
        }

        public Task OnSave()
        {
            IsBusy = true;
            return Task.Run(() =>
            {
                try
                {
                    Collections.Save();
                    SearchCore();
                }
                catch (Exception ex)
                {
                    DispatcherService.BeginInvoke(() =>
                        MessageBoxService.ShowMessage(ex.Message, "Error", MessageButton.OK, MessageIcon.Error)
                    );
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
            string bizAreaCode = BizAreaCode;
            string whCode = WhCode;
            string itemAccount = ItemAccount;

            Collections = new StockDetailList(bizAreaCode, whCode, itemAccount);
        }

        public void OnCellValueChanged()
        {
            if (SelectedItem == null) return;

            if (SelectedItem.State == EntityState.Unchanged)
                SelectedItem.State = EntityState.Modified;
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