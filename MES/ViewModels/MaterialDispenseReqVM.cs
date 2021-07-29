using System;
using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using MesAdmin.Common.Common;
using System.Collections.Generic;
using System.Collections.Specialized;
using DevExpress.Mvvm.POCO;
using System.Threading.Tasks;
using MesAdmin.Common.Utils;
using DevExpress.Xpf.Grid;
using System.Data.SqlClient;

namespace MesAdmin.ViewModels
{
    public class MaterialDispenseReqVM : ViewModelBase
    {
        #region Services
        IDialogService PopupItemView { get { return GetService<IDialogService>("PopupItemView"); } }
        IDialogService ReqView { get { return GetService<IDialogService>("ReqView"); } }
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public bool IsNew
        {
            get { return GetProperty(() => IsNew); }
            set { SetProperty(() => IsNew, value); }
        }
        public ObservableCollection<CommonMinor> WareHouse { get; set; }
        public CommonMinor SelectedWh
        {
            get { return GetProperty(() => SelectedWh); }
            set { SetProperty(() => SelectedWh, value); }
        }
        public MaterialDispenseHeader Header
        {
            get { return GetProperty(() => Header); }
            set { SetProperty(() => Header, value); }
        }
        public MaterialDispenseDetailList Details
        {
            get { return GetProperty(() => Details); }
            set { SetProperty(() => Details, value); }
        }
        public ObservableCollection<MaterialDispenseDetail> SelectedItems
        {
            get { return GetProperty(() => SelectedItems); }
            set { SetProperty(() => SelectedItems, value); }
        }
        public CommonItemList Items { get; set; }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        #endregion

        #region Commands
        public ICommand ShowDialogCmd { get; set; }
        public ICommand<HiddenEditorEvent> HiddenEditorCmd { get; set; }
        public ICommand AddCmd { get; set; }
        public ICommand NewCmd { get; set; }
        public DelegateCommand<object> DelCmd { get; set; }
        public AsyncCommand SaveCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        #endregion

        public MaterialDispenseReqVM()
        {
            Items = new CommonItemList(); // 품목정보
            
            AddCmd = new DelegateCommand(Add, CanAdd);
            DelCmd = new DelegateCommand<object>(Delete, CanDel);
            SaveCmd = new AsyncCommand(OnSave, CanSave);
            NewCmd = new DelegateCommand(OnNew);
            ShowDialogCmd = new DelegateCommand<string>(ShowDialog);
            HiddenEditorCmd = new DelegateCommand<HiddenEditorEvent>(OnHiddenEditor);
            SearchCmd = new AsyncCommand(OnSearch, CanSearch);

            SelectedItems = new ObservableCollection<MaterialDispenseDetail>();
            SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;

            WareHouse = new CommonMinorList
            (
                GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0011")
            ); // 창고정보

            IsNew = true;
            Header = new MaterialDispenseHeader();
            Details = new MaterialDispenseDetailList();
            Header.ReqDate = DateTime.Now;
        }

        private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DelCmd.RaiseCanExecuteChanged();
        }

        public bool CanSave()
        {
            bool ret = true;
            if (IsNew)
            {
                // 필수 입력값 처리
                foreach (MaterialDispenseDetail item in Details.Where(u => u.State == Common.Common.EntityState.Added))
                {
                    if (string.IsNullOrEmpty(item.ItemCode) || item.ReqQty <= 0)
                    {
                        ret = false;
                        break;
                    }
                }
                if (Details.Count == 0) return false;
            }
            else
                ret = Details.Where(u => u.State == Common.Common.EntityState.Deleted).Count() > 0;

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
                if (IsNew)
                {
                    Header.Save();
                    Details.ToList().ForEach(u => u.MDNo = Header.MDNo);
                }
                Details.Save();
                var task = OnSearch();
                while (!task.IsCompleted) IsNew = false; // 비동기 실행때문에 제일 마지막처리
                // send mesaage to parent view
                Messenger.Default.Send<string>("Refresh");
            }
            catch (Exception ex)
            {
                if (ex is SqlException && IsNew)
                    Header.Delete();

                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(ex.Message
                                                    , "Information"
                                                    , MessageButton.OK
                                                    , MessageIcon.Information));
            }
            IsBusy = false;
        }

        public bool CanSearch()
        {
            return !string.IsNullOrEmpty(Header.MDNo) && IsNew;
        }
        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            IsNew = false;
            Details = new MaterialDispenseDetailList(Header.MDNo);
            
            if (Details.Count == 0)
            {
                DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage("불출요청정보가 없습니다!"
                                                        , "Information"
                                                        , MessageButton.OK
                                                        , MessageIcon.Information));
                IsNew = true;
            }
            else Header = new MaterialDispenseHeader(Header.MDNo);

            IsBusy = false;
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

        public bool CanAdd()
        {
            return Header.InWhCode != null && Header.ReqDate != null && Header.DlvyDate != null && IsNew;
        }
        public void Add()
        {
            Details.Insert(Details.Count, new MaterialDispenseDetail
            {
                State = Common.Common.EntityState.Added,
                InWhCode = Header.InWhCode
            });
        }

        public void ShowDialog(string pm)
        {
            if(pm == "PopupItemView") // 품목조회
            {
                var vmItem = ViewModelSource.Create(() => new PopupItemVM());
                PopupItemView.ShowDialog(
                    dialogCommands: vmItem.DialogCmds,
                    title: "품목선택",
                    viewModel: vmItem
                );

                if (vmItem.ConfirmItem != null)
                {
                    SelectedItems[0].ItemCode = vmItem.ConfirmItem.ItemCode;
                    SelectedItems[0].ItemName = vmItem.ConfirmItem.ItemName;
                    SelectedItems[0].ItemSpec = vmItem.ConfirmItem.ItemSpec;
                    SelectedItems[0].BasicUnit = vmItem.ConfirmItem.BasicUnit;
                }
            }
            else
            {
                var vm = ViewModelSource.Create(() => new PopupMaterialDispenseReqVM());
                ReqView.ShowDialog(
                    dialogCommands: vm.DialogCmds,
                    title: "불출요청현황 정보",
                    viewModel: vm
                );

                if (vm.ConfirmHeader != null)
                {
                    Header.MDNo = (string)vm.ConfirmHeader["MDNo"];
                    OnSearch();
                }
            }
        }

        bool CanDel(object obj) { return SelectedItems.Count > 0; }
        public void Delete(object obj)
        {
            if (SelectedItems != null)
            {
                if (IsNew) // 입력모드
                {
                    SelectedItems.ToList().ForEach(item => Details.Remove(item));
                }
                else // 수정모드
                {
                    SelectedItems.ToList().ForEach(item =>
                        item.State = item.State == Common.Common.EntityState.Deleted ?
                        Common.Common.EntityState.Unchanged :
                        Common.Common.EntityState.Deleted
                    );
                }
            }
        }

        public void OnNew()
        {
            IsNew = true;
            Header = new MaterialDispenseHeader();
            Details.Clear();
            SelectedItems.Clear();
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;

            if (pm.Type == EntityMessageType.Added)
            {
                ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
            }
            else
            {
                MaterialDispenseDetail item = pm.Item as MaterialDispenseDetail;
                Header.MDNo = item.MDNo;
                Task.Factory.StartNew(SearchCore).ContinueWith(task =>
                {
                    ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
                });
            }
        }
    }
}
