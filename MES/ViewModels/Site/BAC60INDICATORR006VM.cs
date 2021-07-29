﻿using System;
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
    public class BAC60INDICATORR006VM : ExportViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public object MainViewModel { get; set; }
        public DataTable Collection
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
        public string WaName
        {
            get { return GetProperty(() => WaName); }
            set { SetProperty(() => WaName, value); }
        }
        public ObservableCollection<SeriesItem> ChartDataSource
        {
            get { return GetProperty(() => ChartDataSource); }
            set { SetProperty(() => ChartDataSource, value); }
        }
        public DataRowView SelectedItem
        {
            get { return GetProperty(() => SelectedItem); }
            set { SetProperty(() => SelectedItem, value); }
        }
        public bool IsBusy
        {
            get { return GetProperty(() => IsBusy); }
            set { SetProperty(() => IsBusy, value); }
        }
        public DateTime SYYYYMM
        {
            get { return GetProperty(() => SYYYYMM); }
            set { SetProperty(() => SYYYYMM, value); }
        }
        public DateTime EYYYYMM
        {
            get { return GetProperty(() => EYYYYMM); }
            set { SetProperty(() => EYYYYMM, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        public ICommand<object> ToExcelCmd { get; set; }
        #endregion

        public BAC60INDICATORR006VM()
        {
            SYYYYMM = DateTime.Now;
            EYYYYMM = DateTime.Now;
            SearchCmd = new AsyncCommand(OnSearch, () => !string.IsNullOrEmpty(WaCode));
            ToExcelCmd = new DelegateCommand<object>(base.OnToExcel);

            // 공정정보
            WaCollection = new CommonWorkAreaInfoList("BAC60");
            WaCode = "WE60";

            ChartDataSource = new ObservableCollection<SeriesItem>();
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore).ContinueWith(task => { IsBusy = false; });
        }

        public void SearchCore()
        {
            try
            {
                Collection = Commonsp.BAC60INDICATORR006S(SYYYYMM.ToString("yyyyMM"), EYYYYMM.ToString("yyyyMM"), WaCode);
                CreateChart();
            }
            catch (Exception ex)
            {
                DispatcherService.BeginInvoke(() =>
                {
                    Collection = null;
                    ChartDataSource.Clear();
                    MessageBoxService.ShowMessage(ex.Message, "Information", MessageButton.OK, MessageIcon.Information);
                });
            }
        }

        public void CreateChart()
        {
            DispatcherService.BeginInvoke(() => {
                try
                {
                    if (Collection == null)
                    {
                        ChartDataSource.Clear();
                        return;
                    }

                    WaName = Collection.AsEnumerable().First()["WaName"].ToString();
                    ChartDataSource.Clear();
                    Collection.AsEnumerable().ToList().ForEach(u =>
                        ChartDataSource.Add(
                            new SeriesItem
                            {
                                Name = "",
                                ArgumentData = u["ItemName"].ToString(),
                                ValueData = u["tot_cost"].ToString(),
                                ValueDataSecondary = u["mat_cost"].ToString(),
                                LotNo = u["ItemCode"].ToString(),
                            }
                        )
                    );

                    // 최소 30개
                    for (int i = 0; i < 30 - Collection.AsEnumerable().Count(); i++)
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
