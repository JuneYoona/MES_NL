using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using MesAdmin.Common.Common;
using System.Threading.Tasks;
using DevExpress.Mvvm.POCO;

namespace MesAdmin.ViewModels
{
    public class ProductionWorkOrderVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDialogService PopupItemView { get { return GetService<IDialogService>(); } }
        #endregion

        #region Public Properties
        public ProductionWorkOrderList Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public ProductionWorkOrder SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public IEnumerable<CommonWorkAreaInfo> WaCollections
        {
            get { return GetProperty(() => WaCollections); }
            set { SetProperty(() => WaCollections, value); }
        }
        public DateTime StartDate
        {
            get { return GetProperty(() => StartDate); }
            set { SetProperty(() => StartDate, value); }
        }
        public IEnumerable<CommonMinor> BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value); }
        }
        public string EditBizAreaCode
        {
            get { return GetProperty(() => EditBizAreaCode); }
            set { SetProperty(() => EditBizAreaCode, value); }
        }
        public DateTime EndDate
        {
            get { return GetProperty(() => EndDate); }
            set { SetProperty(() => EndDate, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        #endregion

        #region Commands
        public ICommand NewCmd { get; set; }
        public ICommand DeleteCmd { get; set; }
        public ICommand SaveCmd { get; set; }
        public ICommand CellValueChangedCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        public ICommand ShowDialogCmd { get; set; }
        #endregion

        public ProductionWorkOrderVM()
        {
            BizAreaCode = GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0004");
            if (!string.IsNullOrEmpty(DSUser.Instance.BizAreaCode))
                EditBizAreaCode = BizAreaCode.FirstOrDefault(u => u.MinorCode == DSUser.Instance.BizAreaCode).MinorCode;

            StartDate = DateTime.Now.AddYears(-1);
            EndDate = DateTime.Now;
            WaCollections = new CommonWorkAreaInfoList(EditBizAreaCode).Where(u => u.WorkOrderFlag == "Y");

            NewCmd = new DelegateCommand(OnNew);
            DeleteCmd = new DelegateCommand(OnDelete);
            SearchCmd = new AsyncCommand(OnSearch);
            SaveCmd = new DelegateCommand(OnSave, CanSave);
            CellValueChangedCmd = new DelegateCommand(OnCellValueChanged);
            ShowDialogCmd = new DelegateCommand<string>(ShowDialog);

            OnSearch();
        }

        public bool CanSave()
        {
            if (Collections == null) return false;
            return Collections.Where(u => u.State != EntityState.Unchanged).Count() > 0;
        }
        public void OnSave()
        {
            try
            {
                Collections.Save();
                Collections.InitializeList();
            }
            catch (Exception ex)
            {
                MessageBoxService.ShowMessage(ex.Message, "Information", MessageButton.OK, MessageIcon.Information);
            }
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            Collections = new ProductionWorkOrderList(EditBizAreaCode, StartDate, EndDate);
            IsBusy = false;
        }

        public void OnNew()
        {
            if (string.IsNullOrEmpty(EditBizAreaCode))
            {
                MessageBoxService.ShowMessage("공장정보가 비어있습니다. 재로그인하세요!", "Information", MessageButton.OK, MessageIcon.Information);
                return;
            }
            ProductionWorkOrder order = new ProductionWorkOrder
            {
                BizAreaCode = EditBizAreaCode,
                OrderDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                State = EntityState.Added,
                IsEnd = 'N'
            };
            Collections.Insert(0, order);
            SelectedItem = order;
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

        public void OnCellValueChanged()
        {
            if (SelectedItem.State == EntityState.Unchanged)
                SelectedItem.State = EntityState.Modified;
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
                SelectedItem.ItemCode = vmItem.ConfirmItem.ItemCode;
                SelectedItem.ItemName = vmItem.ConfirmItem.ItemName;
                SelectedItem.BasicUnit = vmItem.ConfirmItem.BasicUnit;
            }
        }
    }
}