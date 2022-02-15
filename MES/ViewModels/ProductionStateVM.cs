using System;
using System.Data;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using MesAdmin.Common.Common;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MesAdmin.ViewModels
{
    public class ProductionStateVM : ExportViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        #endregion

        #region Public Properties
        public ProductionInputRecordList Collections
        {
            get { return GetProperty(() => Collections); }
            set { SetProperty(() => Collections, value); }
        }
        public ProductionInputRecord SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public ProductionOutputRecordList OutputRecords
        {
            get { return GetProperty(() => OutputRecords); }
            set { SetProperty(() => OutputRecords, value); }
        }
        public DateTime StartDate
        {
            get { return GetProperty(() => StartDate); }
            set { SetProperty(() => StartDate, value); }
        }
        public DateTime EndDate
        {
            get { return GetProperty(() => EndDate); }
            set { SetProperty(() => EndDate, value); }
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
        public string LotNo
        {
            get { return GetProperty(() => LotNo); }
            set { SetProperty(() => LotNo, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        public bool OLED { get { return DSUser.Instance.BizAreaCode == "BAC60"; } }

        public ObservableCollection<ItemInfo> Type { get; set; }
        public string SelectedType
        {
            get { return GetProperty(() => SelectedType); }
            set { SetProperty(() => SelectedType, value); }
        }
        #endregion

        #region Commands
        public ICommand DeleteCmd { get; set; }
        public ICommand SaveCmd { get; set; }
        public AsyncCommand SearchCmd { get; set; }
        public ICommand<object> ToExcelCmd { get; set; }
        public ICommand MouseDownCmd { get; set; }
        #endregion

        public ProductionStateVM()
        {
            BizAreaCodeList = GlobalCommonMinor.Instance.Where(u => u.MajorCode == "I0004");
            if (!string.IsNullOrEmpty(DSUser.Instance.BizAreaCode))
                BizAreaCode = BizAreaCodeList.FirstOrDefault(u => u.MinorCode == DSUser.Instance.BizAreaCode).MinorCode;

            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now;

            Type = new ObservableCollection<ItemInfo>();
            Type.Add(new ItemInfo { Text = "선행검사출하품", Value = "SkyBlue" });
            Type.Add(new ItemInfo { Text = "선행검사완료품", Value = "Green" });

            SearchCmd = new AsyncCommand(OnSearch);
            DeleteCmd = new DelegateCommand(OnDelete);
            SaveCmd = new DelegateCommand(OnSave, CanSave);
            MouseDownCmd = new DelegateCommand(OnMouseDown);
            ToExcelCmd = new DelegateCommand<object>(base.OnToExcel);
        }

        public void OnMouseDown()
        {
            if (SelectedItem != null)
                OutputRecords = new ProductionOutputRecordList(SelectedItem.OrderNo, SelectedItem.Seq);
        }
        
        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore);
        }
        public void SearchCore()
        {
            Collections = new ProductionInputRecordList(StartDate, EndDate, bizAreaCode: BizAreaCode, waCode: WaCode, lotNo: LotNo, color: SelectedType);
            OutputRecords = null;
            IsBusy = false;
        }

        public void OnDelete()
        {
            if (SelectedItem == null) return;

            SelectedItem.State =
                SelectedItem.State == EntityState.Deleted ? EntityState.Unchanged : EntityState.Deleted;
        }

        public bool CanSave()
        {
            if (Collections == null) return false;
            return Collections.Where(u => u.State == EntityState.Deleted).Count() > 0 ;
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
