using System;
using System.Data;
using DevExpress.Mvvm;
using MesAdmin.Models;
using MesAdmin.Common.Common;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Windows.Input;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using MesAdmin.Common.Utils;
using System.Linq;

namespace MesAdmin.ViewModels
{
    public class CommonItemNewVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        IDialogService PopupItemView { get { return GetService<IDialogService>("PopupItemView"); } }
        #endregion

        #region Public Properties
        public CommonItem SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public EntityMessageType Status
        {
            get { return GetProperty(() => Status); }
            set { SetProperty(() => Status, value); }
        }
        public CommonMinorList ItemAccount
        {
            get { return GetProperty(() => ItemAccount); }
            set { SetProperty(() => ItemAccount, value); }
        }
        public CommonMinorList BasicUnit
        {
            get { return GetProperty(() => BasicUnit); }
            set { SetProperty(() => BasicUnit, value); }
        }
        public CommonMinorList WhCode
        {
            get { return GetProperty(() => WhCode); }
            set { SetProperty(() => WhCode, value); }
        }
        public CommonAltItemList AltItems
        {
            get { return GetProperty(() => AltItems); }
            set { SetProperty(() => AltItems, value); }
        }
        public CommonAltItem SelectedAltItem
        {
            get { return GetProperty(() => SelectedAltItem); }
            set { SetProperty(() => SelectedAltItem, value); }
        }
        public CommonItemList Items { get; set; }
        public bool? IsEnabled
        {
            get { return GetProperty(() => IsEnabled); }
            set { SetProperty(() => IsEnabled, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SaveCmd { get; set; }
        public ICommand AddAltItemCmd { get; set; }
        public ICommand DelAltItemCmd { get; set; }
        public ICommand ShowDialogCmd { get; set; }
        public ICommand CellValueChangedCmd { get; set; }
        public ICommand<HiddenEditorEvent> HiddenEditorCmd { get; set; }
        #endregion

        public CommonItemNewVM()
        {
            ItemAccount = new CommonMinorList("P1001");
            BasicUnit = new CommonMinorList("P1100");
            WhCode = new CommonMinorList(majorCode: "I0011");
            Items = new CommonItemList(); // 품목정보

            SaveCmd = new AsyncCommand(OnSave, CanSave);
            AddAltItemCmd = new DelegateCommand(OnAddAltItem, CanAddAltItem);
            DelAltItemCmd = new DelegateCommand(OnDelAltItem, CanDelAltItem);
            ShowDialogCmd = new DelegateCommand<string>(ShowDialog);
            CellValueChangedCmd = new DelegateCommand(OnCellValueChanged);
            HiddenEditorCmd = new DelegateCommand<HiddenEditorEvent>(OnHiddenEditor);
        }

        public bool CanAddAltItem()
        {
            return Status == EntityMessageType.Changed;
        }
        public void OnAddAltItem()
        {
            AltItems.Insert(AltItems.Count, new CommonAltItem
            {
                State = EntityState.Added,
                ItemCode = SelectedItem.ItemCode,
            });
        }

        public bool CanSave()
        {
            bool ret = false;
            if (!string.IsNullOrEmpty(SelectedItem.ItemCode) && !string.IsNullOrEmpty(SelectedItem.ItemName)
                && !string.IsNullOrEmpty(SelectedItem.ItemAccount) && !string.IsNullOrEmpty(SelectedItem.BasicUnit))
                ret = true;

            // 대체품목 필수값
            if (AltItems != null)
            {
                if (AltItems.Where(u => string.IsNullOrEmpty(u.ItemName) == true).Count() != 0)
                    ret = false;
            }

            return ret;
        }
        public Task OnSave()
        {
            return Task.Factory.StartNew(SaveCore);
        }
        private void SaveCore()
        {
            IsEnabled = false;
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(0.5));
            DispatcherService.BeginInvoke(() =>
            {
                try
                {
                    SelectedItem.Save();
                    // 대체품 저장
                    if (AltItems != null)
                    {
                        AltItems.Save();
                        AltItems = new CommonAltItemList(SelectedItem.ItemCode);
                    }

                    // send mesaage to parent view
                    CommonItem copyObj = SelectedItem.DeepCloneReflection();
                    copyObj.State = EntityState.Unchanged;
                    Messenger.Default.Send(new EntityMessage<CommonItem>(copyObj, Status));
                    Status = EntityMessageType.Changed;
                    SelectedItem.State = EntityState.Modified;
                }
                catch (Exception ex)
                {
                    string message;
                    if (ex is SqlException)
                    {
                        SqlException sqlEx = ex as SqlException;
                        if (sqlEx.Number == 2627)
                            message = "이미 등록이 되어있는 품목코드입니다!";
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
            });
            IsEnabled = true;
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
                SelectedAltItem.AltItemCode = vmItem.ConfirmItem.ItemCode;
                SelectedAltItem.ItemName = vmItem.ConfirmItem.ItemName;
                SelectedAltItem.ItemSpec = vmItem.ConfirmItem.ItemSpec;
            }
        }

        public void OnCellValueChanged()
        {
            if (SelectedAltItem.State == EntityState.Unchanged)
                SelectedAltItem.State = EntityState.Modified;
        }

        public void OnHiddenEditor(HiddenEditorEvent pm)
        {
            if (pm.e.Column.FieldName != "AltItemCode") return;

            TableView view = pm.sender as TableView;
            GridControl grid = view.Grid;

            CommonItem item = Items.Where(u => u.ItemCode == (string)pm.e.Value).FirstOrDefault();
            if (item == null)
            {
                grid.SetCellValue(pm.e.RowHandle, "AltItemCode", "");
                grid.SetCellValue(pm.e.RowHandle, "ItemName", "");
                grid.SetCellValue(pm.e.RowHandle, "ItemSpec", "");
            }
            else
            {
                grid.SetCellValue(pm.e.RowHandle, "ItemName", item.ItemName);
                grid.SetCellValue(pm.e.RowHandle, "ItemSpec", item.ItemSpec);
            }
        }

        bool CanDelAltItem() { return SelectedAltItem != null; }
        public void OnDelAltItem()
        {

            if (SelectedAltItem.State == EntityState.Added)
                AltItems.Remove(SelectedAltItem);
            else
                SelectedAltItem.State = SelectedAltItem.State == EntityState.Deleted ? EntityState.Unchanged : EntityState.Deleted;
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);

            if (ViewModelBase.IsInDesignMode) return;
            DocumentParamter info = (DocumentParamter)Parameter;
            if (info == null) throw new InvalidOperationException();

            Status = info.Type;
            if (Status == EntityMessageType.Added)
            {
                SelectedItem = new CommonItem();
                SelectedItem.State = EntityState.Added;
                SelectedItem.IsEnabled = true;
            }
            else
            {
                SelectedItem = ((CommonItem)info.Item).DeepCloneReflection();
                SelectedItem.State = EntityState.Modified;
                AltItems = new CommonAltItemList(SelectedItem.ItemCode);
            }
        }
    }
}
