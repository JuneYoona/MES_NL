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
    public class BAC60INDICATORR002VM : ViewModelBase
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
        public ObservableCollection<ItemInfo> Type { get; set; }
        public string SelectedType
        {
            get { return GetProperty(() => SelectedType); }
            set { SetProperty(() => SelectedType, value); }
        }
        public double MinY
        {
            get { return GetProperty(() => MinY); }
            set { SetProperty(() => MinY, value); }
        }
        public double MaxY
        {
            get { return GetProperty(() => MaxY); }
            set { SetProperty(() => MaxY, value); }
        }

        public double Avg
        {
            get { return GetProperty(() => Avg); }
            set { SetProperty(() => Avg, value); }
        }
        public double StdDev
        {
            get { return GetProperty(() => StdDev); }
            set { SetProperty(() => StdDev, value); }
        }
        public double Sig3
        {
            get { return GetProperty(() => Sig3); }
            set { SetProperty(() => Sig3, value); }
        }
        public double Max
        {
            get { return GetProperty(() => Max); }
            set { SetProperty(() => Max, value); }
        }
        public double Min
        {
            get { return GetProperty(() => Min); }
            set { SetProperty(() => Min, value); }
        }
        public double Avg_M_Sig3
        {
            get { return GetProperty(() => Avg_M_Sig3); }
            set { SetProperty(() => Avg_M_Sig3, value); }
        }
        public double Avg_P_Sig3
        {
            get { return GetProperty(() => Avg_P_Sig3); }
            set { SetProperty(() => Avg_P_Sig3, value); }
        }
        public double? USL
        {
            get { return GetProperty(() => USL); }
            set { SetProperty(() => USL, value); }
        }
        public double LSL
        {
            get { return GetProperty(() => LSL); }
            set { SetProperty(() => LSL, value); }
        }
        public double? Cp
        {
            get { return GetProperty(() => Cp); }
            set { SetProperty(() => Cp, value); }
        }
        public double? Cpu
        {
            get { return GetProperty(() => Cpu); }
            set { SetProperty(() => Cpu, value); }
        }
        public double Cpl
        {
            get { return GetProperty(() => Cpl); }
            set { SetProperty(() => Cpl, value); }
        }
        public double Cpk
        {
            get { return GetProperty(() => Cpk); }
            set { SetProperty(() => Cpk, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand SearchCmd { get; set; }
        #endregion

        public BAC60INDICATORR002VM()
        {
            int daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            EndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, daysInMonth);
            SearchCmd = new AsyncCommand(OnSearch, () => !string.IsNullOrEmpty(ItemCode));

            // 공정정보
            WaCollection = new CommonWorkAreaInfoList("BAC60").Where(u => u.WaCode == "WE50");
            WaCode = "WE50";

            Type = new ObservableCollection<ItemInfo>();
            Type.Add(new ItemInfo { Text = "Purity", Value = "Purity" });
            Type.Add(new ItemInfo { Text = "Tg", Value = "Tg" });
            SelectedType = "Purity";

            ChartDataSource = new ObservableCollection<SeriesItem>();
            MinY = 99;
            MaxY = 100;
        }

        public Task OnSearch()
        {
            IsBusy = true;
            return Task.Factory.StartNew(SearchCore).ContinueWith(task => { IsBusy = false; CreateChart(); });
        }

        public void SearchCore()
        {
            Collection = new QualityResultTable(ItemCode, StartDate, EndDate, "FQC").Collections;
        }

        public void CreateChart()
        {
            DispatcherService.BeginInvoke(() => {
                try
                {
                    if (Collection == null || Collection.AsEnumerable().Count() == 0)
                    {
                        ChartDataSource.Clear();
                        return;
                    }

                    // 관리도 결과값 계산
                    IEnumerable<double> rows = Collection.Select()
                        .Where(x => x["" + SelectedType + ""] != DBNull.Value && x["" + SelectedType + ""].ToString() != "" && Convert.ToDouble(x["" + SelectedType + ""]) > 0)
                        .Select(c => Convert.ToDouble(c["" + SelectedType + ""]));

                    Avg = rows.Average();
                    StdDev = CalculateStdDev(rows);
                    Sig3 = StdDev * 3;
                    Max = rows.Max();
                    Min = rows.Min();
                    Avg_M_Sig3 = Avg - Sig3;
                    Avg_P_Sig3 = Avg + Sig3;

                    DataRow dr = Commonsp.GetItemSpec(ItemCode, SelectedType);

                    double ret;
                    USL = double.TryParse(dr[0].ToString(), out ret) ? ret : (double?)null;
                    LSL = double.TryParse(dr[1].ToString(), out ret) ? ret : 0;
                    Cp = (USL - LSL) / (6 * StdDev);
                    Cpu = (USL - Avg) / Sig3;
                    Cpl = (Avg - LSL) / Sig3;
                    if (SelectedType == "Td(5% wt. of loss temp.)")
                        Cpk = Cpl;
                    else
                        Cpk = Math.Min(Convert.ToDouble(Cp), Convert.ToDouble(Cpu));

                    MinY = Math.Min(LSL, Avg_M_Sig3);
                    MaxY = USL == null ? Avg_P_Sig3 : Math.Max((double)USL, Avg_P_Sig3);

                    ChartDataSource.Clear();
                    Collection.AsEnumerable().Where(x => x["" + SelectedType + ""] != DBNull.Value && x["" + SelectedType + ""].ToString() != "").ToList().ForEach(u =>
                        ChartDataSource.Add(
                            new SeriesItem
                            {
                                Name = SelectedType,
                                ArgumentData = (string)u["Lot No."],
                                ValueData = (string)u["" + SelectedType + ""],
                                LotNo = (string)u["Lot No."],
                            }
                        )
                    );

                    // 최소 50개
                    for (int i = 0; i < 50 - Collection.AsEnumerable().Count(); i++)
                    {
                        ChartDataSource.Add(
                            new SeriesItem
                            {
                                Name = SelectedType,
                                ArgumentData = i.ToString(),
                                LotNo = i.ToString(),
                            }
                        );
                    }
                }
                catch { }
            });
        }

        private double CalculateStdDev(IEnumerable<double> values)
        {
            double ret = 0;

            if (values.Count() > 0)
            {
                //Compute the Average
                double avg = values.Average();
                //Perform the Sum of (value-avg)^2
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                //Put it all together
                ret = Math.Sqrt(sum / (values.Count() - 1));
            }
            return ret;
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