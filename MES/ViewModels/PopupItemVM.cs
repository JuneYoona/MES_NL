using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System;

namespace MesAdmin.ViewModels
{
    public class PopupItemVM : ViewModelBase
    {
        #region Services
        ICurrentWindowService CurrentWindowService { get { return GetService<ICurrentWindowService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public CommonItemList Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public IEnumerable<CommonMinor> BizArea { get; private set; }
        public string EditBizArea
        {
            get { return GetProperty(() => EditBizArea); }
            set { SetProperty(() => EditBizArea, value); }
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
        public CommonItem SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public CommonItem ConfirmItem
        {
            get { return GetProperty(() => ConfirmItem); }
            set { SetProperty(() => ConfirmItem, value); }
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
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public List<UICommand> DialogCmds { get; private set; }
        protected UICommand ConfirmUICmd { get; private set; }
        protected UICommand CancelUICmd { get; private set; }
        public ICommand ConfirmCmd { get; set; }
        public ICommand EditValueChangedCmd { get; set; }
        #endregion

        public PopupItemVM() : this("") { }

        public PopupItemVM(string itemAccount)
        {
            EditItemAcct = itemAccount;
            BizArea = GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0004");
            if (!string.IsNullOrEmpty(DSUser.Instance.BizAreaCode))
                EditBizArea = BizArea.FirstOrDefault(u => u.MinorCode == DSUser.Instance.BizAreaCode).MinorCode;
            OnEditValueChanged();

            // dialog command
            ConfirmUICmd = new UICommand()
            {
                Caption = "확인",
                IsDefault = false,
                IsCancel = false,
                Command = new DelegateCommand(() => ConfirmItem = SelectedItem),
                Id = MessageBoxResult.OK,
            };

            CancelUICmd = new UICommand()
            {
                Caption = "취소",
                IsCancel = true,
                IsDefault = false,
                Id = MessageBoxResult.Cancel,
            };
            DialogCmds = new List<UICommand>() { ConfirmUICmd, CancelUICmd };

            SearchCmd = new AsyncCommand(OnSearch);
            ConfirmCmd = new DelegateCommand(OnConfirm);
            EditValueChangedCmd = new DelegateCommand(OnEditValueChanged);

            OnSearch();
        }

        public void OnEditValueChanged()
        {
            ItemAccount = GlobalCommonMinor.Instance.Where(u => u.MajorCode == "P1001")
                .Where(p => string.IsNullOrEmpty(EditBizArea)? true : p.Ref01 == EditBizArea);
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            while (DispatcherService == null) { System.Threading.Thread.Sleep(TimeSpan.FromSeconds(0.1)); }
            DispatcherService.BeginInvoke(() =>
            {
                Collections = new CommonItemList
                (
                    new CommonItemList(EditBizArea)
                        .Where(p =>
                            string.IsNullOrEmpty(EditItemAcct) ? true : p.ItemAccount == EditItemAcct)
                        .Where(p =>
                            string.IsNullOrEmpty(ItemCode) ? true : p.ItemCode.ToUpper().Contains(ItemCode.ToUpper()))
                        .Where(p =>
                            string.IsNullOrEmpty(ItemName) ? true : p.ItemName.ToUpper().Contains(ItemName.ToUpper()))
                        .Where(p => p.IsEnabled == true)
                );
            });
            IsBusy = false;
        }

        protected void OnConfirm()
        {
            ConfirmItem = SelectedItem;
            CurrentWindowService.Close();
        }
    }
}
