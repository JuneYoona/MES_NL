using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DevExpress.Mvvm;
using MesAdmin.Models;
using MesAdmin.Common.Common;
using DevExpress.Mvvm.POCO;
using System.Threading.Tasks;
using System.Data;
using System.Linq;
using System.Collections.Generic;

namespace MesAdmin.ViewModels
{
    public class BAC60INDICATORR004VM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public object MainViewModel { get; set; }
        public IEnumerable<ProductionInputRecord> Collection
        {
            get { return GetProperty(() => Collection); }
            set { SetProperty(() => Collection, value); }
        }
        public IEnumerable<CommonWorkAreaInfo> WaCollection
        {
            get { return GetProperty(() => WaCollection); }
            set { SetProperty(() => WaCollection, value); }
        }
        public string WaCode
        {
            get { return GetProperty(() => WaCode); }
            set { SetProperty(() => WaCode, value); }
        }
        public ObservableCollection<SeriesItem> ChartDataSource
        {
            get { return GetProperty(() => ChartDataSource); }
            set { SetProperty(() => ChartDataSource, value); }
        }
        public ProductionInputRecord SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
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
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        #endregion

        public BAC60INDICATORR004VM()
        {
            int daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            EndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, daysInMonth);
            SearchCmd = new AsyncCommand(OnSearch, () => !string.IsNullOrEmpty(ItemCode));

            // 공정정보
            WaCollection = new CommonWorkAreaInfoList("BAC60");

            ChartDataSource = new ObservableCollection<SeriesItem>();
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore).ContinueWith(task => { IsBusy = false; CreateChart(); });
        }

        public void SearchCore()
        {
            Collection = new ProductionInputRecordList(StartDate, EndDate, bizAreaCode: "BAC60", waCode: WaCode).Where(o => o.ItemCode == ItemCode).OrderBy(o => o.ProductOrderNo);
        }

        public void CreateChart()
        {
            DispatcherService.BeginInvoke(() => {
                try
                {
                    if (Collection == null || Collection.Count() == 0)
                    {
                        ChartDataSource.Clear();
                        return;
                    }

                    ItemName = Collection.First().ItemName;

                    ChartDataSource.Clear();
                    Collection.ToList().ForEach(u =>
                        ChartDataSource.Add(
                            new SeriesItem
                            {
                                Name = "",
                                ArgumentData = u.LotNo,
                                ValueData = u.Qty.ToString(),
                                ValueDataSecondary = u.WorkTime.ToString(),
                                LotNo = u.LotNo,
                            }
                        )
                    );

                    // 최소 30개
                    for (int i = 0; i < 30 - Collection.Count(); i++)
                    {
                        ChartDataSource.Add(
                            new SeriesItem
                            {
                                Name = "",
                                ArgumentData = i.ToString(),
                                LotNo = i.ToString(),
                            }
                        );
                    }
                }
                catch { }
            });
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;
            MainViewModel = pm.ParentViewmodel;

            ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
        }
    }
}
