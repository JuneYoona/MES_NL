using System;
using MesAdmin.Common.Common;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MesAdmin.ViewModels
{
    public class StockDailyVM : ExportViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        #endregion

        #region Public Properties
        public StockDailyList Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public IEnumerable<CommonMinor> BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value); }
        }
        public IEnumerable<CommonMinor> WhCode
        {
            get { return GetProperty(() => WhCode); }
            set { SetProperty(() => WhCode, value); }
        }
        public string EditBizAreaCode
        {
            get { return GetProperty(() => EditBizAreaCode); }
            set { SetProperty(() => EditBizAreaCode, value); }
        }
        public string EditWhCode
        {
            get { return GetProperty(() => EditWhCode); }
            set { SetProperty(() => EditWhCode, value); }
        }
        public IEnumerable<CommonMinor> ItemAccount
        {
            get { return GetProperty(() => ItemAccount); }
            set { SetProperty(() => ItemAccount, value); }
        }
        public string EditItemAcct
        {
            get { return GetProperty(() => EditItemAcct); }
            set { SetProperty(() => EditItemAcct, value); }
        }
        public DateTime EditDate
        {
            get { return GetProperty(() => EditDate); }
            set { SetProperty(() => EditDate, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        public string View { get; set; }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public ICommand<object> ToExcelCmd { get; set; }
        public ICommand EditValueChangedCmd { get; set; }
        #endregion

        public StockDailyVM()
        {
            BizAreaCode = GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0004");
            if (!string.IsNullOrEmpty(DSUser.Instance.BizAreaCode))
                EditBizAreaCode = BizAreaCode.FirstOrDefault(u => u.MinorCode == DSUser.Instance.BizAreaCode).MinorCode;
            EditDate = DateTime.Now;

            SearchCmd = new AsyncCommand(OnSearch);
            ToExcelCmd = new DelegateCommand<object>(base.OnToExcel);
            EditValueChangedCmd = new DelegateCommand(OnEditValueChanged);
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            string bizAreaCode = EditBizAreaCode;
            string whCode = EditWhCode;
            string itemAccount = EditItemAcct;

            if (View == "StockDailyView")
            {
                Collections = new StockDailyList(EditDate);
                Collections = new StockDailyList
                (
                    Collections
                        .Where(u => string.IsNullOrEmpty(bizAreaCode) ? true : u.BizAreaCode == bizAreaCode)
                        .Where(u => string.IsNullOrEmpty(whCode) ? true : u.WhCode == whCode)
                        .Where(u => string.IsNullOrEmpty(itemAccount) ? true : u.ItemAccount == itemAccount)
                );
            }
            else
            {
                Collections = new StockDailtyListItem(EditDate);
                Collections = new StockDailtyListItem
                (
                    Collections
                        .Where(u => string.IsNullOrEmpty(bizAreaCode) ? true : u.BizAreaCode == bizAreaCode)
                        .Where(u => string.IsNullOrEmpty(whCode) ? true : u.WhCode == whCode)
                        .Where(u => string.IsNullOrEmpty(itemAccount) ? true : u.ItemAccount == itemAccount)
                );
            }
            IsBusy = false;
        }

        public void OnEditValueChanged()
        {
            WhCode = GlobalCommonMinor.Instance
                    .Where(u => u.MajorCode == "I0011")
                    .Where(u => string.IsNullOrEmpty(EditBizAreaCode) ? true : u.Ref01 == EditBizAreaCode);

            ItemAccount = GlobalCommonMinor.Instance
                    .Where(u => u.MajorCode == "P1001")
                    .Where(u => string.IsNullOrEmpty(EditBizAreaCode) ? true : u.Ref01 == EditBizAreaCode);
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;
            View = pm.Item as string;
            Task.Factory.StartNew(SearchCore).ContinueWith(task =>
            {
                ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
            });
        }
    }
}
