using DevExpress.Mvvm;
using MesAdmin.Common.Common;
using MesAdmin.Models;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MesAdmin.Common.Utils;
using DevExpress.Xpf.Grid;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.Xpf;

namespace MesAdmin.ViewModels
{
    public class BAC60PRODUCTION001RVM : ExportViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public BAC60PRODUCTION001LIST Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public BAC60PRODUCTION001 SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public DataTable ChildCollections
        {
            get { return GetProperty(() => ChildCollections); }
            set { SetProperty(() => ChildCollections, value); }
        }
        public DateTime EditDate
        {
            get { return GetProperty(() => EditDate); }
            set { SetProperty(() => EditDate, value); }
        }
        public string Type
        {
            get { return GetValue<string>(); }
            set { SetValue(value, () => Status = value == "연번제외" ? "" : Status); }
        }
        public string Status
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public bool ColVisible
        {
            get { return GetProperty(() => ColVisible); }
            set { SetProperty(() => ColVisible, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        public bool ChildBusy
        {
            get { return GetProperty(() => ChildBusy); }
            set { SetProperty(() => ChildBusy, value); }
        }
        #endregion

        #region Commands
        public ICommand SaveCmd { get; set; }
        public ICommand CellValueChangedCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        public AsyncCommand SelectedItemChangedCmd { get; set; }
        public ICommand<CellMergeEvent> CellMergeCmd { get; set; }
        public ICommand<object> ToExcelCmd { get; set; }
        #endregion

        public BAC60PRODUCTION001RVM()
        {
            EditDate = DateTime.Now;
            ColVisible = true;
            Type = "연번포함";

            SearchCmd = new AsyncCommand(OnSearch);
            SaveCmd = new AsyncCommand(OnSave, () => Collections != null && Collections.Where(o => o.State == EntityState.Modified).Count() > 0);
            SelectedItemChangedCmd = new AsyncCommand(OnSelectedItemChanged, () => SelectedItem != null && SelectedItem.ItemCode != itemCode);
            CellValueChangedCmd = new DelegateCommand(OnCellValueChanged);
            CellMergeCmd = new DelegateCommand<CellMergeEvent>(OnCellMerge);
            ToExcelCmd = new DelegateCommand<object>(OnToExcel);
        }

        public void OnCellMerge(CellMergeEvent pm)
        {
            var e = pm.e;
            var grid = (pm.sender as TableView).Grid;
            
            if (e.Column.FieldName.Contains("ItemCode")) return;

            if (grid.GetCellValue(e.RowHandle1, "ItemCode").ToString() != grid.GetCellValue(e.RowHandle2, "ItemCode").ToString())
            {
                e.Handled = true;
            }
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Run(new Action(SearchCore)).ContinueWith(t =>
            {
                ColVisible = Type == "연번포함" ? true : false;
                IsBusy = false;
            }).ContinueWith(t => { ChildCollections = null; itemCode = ""; });
        }

        public void SearchCore()
        {
            Collections = new BAC60PRODUCTION001LIST(EditDate, Type == "연번포함" ? "A" : "B", Status);
        }

        public Task OnSave()
        {
            IsBusy = true;
            return Task.Run(new Action(() =>
            {
                try
                {
                    Collections.Save();
                    Collections.Where(o => o.State == EntityState.Modified).ToList().ForEach(x => x.State = EntityState.Unchanged);
                    SelectedItem = null;
                    //SearchCore();
                }
                catch (Exception ex)
                {
                    DispatcherService.BeginInvoke(() => MessageBoxService.ShowMessage(ex.Message, "Error", MessageButton.OK, MessageIcon.Error));
                }
            })).ContinueWith(t => IsBusy = false);
        }

        public void OnCellValueChanged()
        {
            if (SelectedItem == null) return;

            if (SelectedItem.State == EntityState.Unchanged)
                SelectedItem.State = EntityState.Modified;
        }

        private string itemCode;
        public Task OnSelectedItemChanged()
        {
            ChildBusy = true;
            return Task.Run(() =>
            {
                ChildCollections = Commonsp.BAC60PRODUCTION001B(EditDate, SelectedItem.ItemCode);
            }).ContinueWith(t => { ChildBusy = false; itemCode = SelectedItem.ItemCode; });
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;
            ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
        }
    }
}