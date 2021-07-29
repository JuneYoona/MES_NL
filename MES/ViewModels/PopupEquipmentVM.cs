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
    public class PopupEquipmentVM : ViewModelBase
    {
        #region Services
        ICurrentWindowService CurrentWindowService { get { return GetService<ICurrentWindowService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public CommonEquipmentList Collections
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
        public IEnumerable<CommonWorkAreaInfo> WorkAreaInfo
        {
            get { return GetProperty(() => WorkAreaInfo); }
            set { SetProperty(() => WorkAreaInfo, value); }
        }
        public string EditWorkAreaInfo
        {
            get { return GetProperty(() => EditWorkAreaInfo); }
            set { SetProperty(() => EditWorkAreaInfo, value); }
        }
        public CommonEquipment SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public CommonEquipment ConfirmItem
        {
            get { return GetProperty(() => ConfirmItem); }
            set { SetProperty(() => ConfirmItem, value); }
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
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public List<UICommand> DialogCmds { get; private set; }
        protected UICommand ConfirmUICmd { get; private set; }
        protected UICommand CancelUICmd { get; private set; }
        public ICommand ConfirmCmd { get; set; }
        public ICommand EditValueChangedCmd { get; set; }
        #endregion

        public PopupEquipmentVM() : this("") { }
        public PopupEquipmentVM(string bizAreaCode)
        {
            BizArea = GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0004");
            if (!string.IsNullOrEmpty(bizAreaCode))
                EditBizArea = BizArea.FirstOrDefault(u => u.MinorCode == bizAreaCode).MinorCode;
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
            WorkAreaInfo = GlobalCommonWorkAreaInfo.Instance.Where(u => u.IsEnabled == true)
                .Where(p => string.IsNullOrEmpty(EditBizArea) ? true : p.BizAreaCode == EditBizArea);
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
                Collections = new CommonEquipmentList
                (
                    new CommonEquipmentList()
                        .Where(p =>
                            string.IsNullOrEmpty(EditBizArea) ? true : p.BizAreaCode == EditBizArea)
                        .Where(p =>
                            string.IsNullOrEmpty(EqpCode) ? true : p.EqpCode.ToUpper().Contains(EqpCode.ToUpper()))
                        .Where(p =>
                            string.IsNullOrEmpty(EditWorkAreaInfo) ? true : p.WaCode == EditWorkAreaInfo)
                        .Where(p =>
                            string.IsNullOrEmpty(EqpName) ? true : p.EqpName.ToUpper().Contains(EqpName.ToUpper()))
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