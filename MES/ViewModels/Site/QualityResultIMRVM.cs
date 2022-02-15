using DevExpress.Mvvm;
using MesAdmin.Common.Common;
using MesAdmin.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MesAdmin.ViewModels
{
    public class QualityResultIMRVM : ExportViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public ObservableCollection<SeriesItem> ChartDataSource
        {
            get { return GetProperty(() => ChartDataSource); }
            set { SetProperty(() => ChartDataSource, value); }
        }
        public DataTable ChartSource { get; set; }
        public string ItemCode
        {
            get { return GetProperty(() => ItemCode); }
            set { SetProperty(() => ItemCode, value); }
        }
        public string Gate
        {
            get { return GetProperty(() => Gate); }
            set { SetProperty(() => Gate, value); }
        }
        public ObservableCollection<InspectItem> InspectItems { get; set; }
        public InspectItem InspectItem
        {
            get { return GetProperty(() => InspectItem); }
            set { SetProperty(() => InspectItem, value); }
        }
        public decimal? Opacity
        {
            get { return GetProperty(() => Opacity); }
            set { SetProperty(() => Opacity, value); }
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
        public ICommand SearchCmd { get; set; }
        protected UICommand CancelUICmd { get; private set; }
        public List<UICommand> DialogCmds { get; private set; }
        #endregion

        public QualityResultIMRVM() : this(null, "") { }
        public QualityResultIMRVM(DataTable chartSource, string gate)
        {
            ChartSource = chartSource.AsEnumerable().OrderBy(o => o.Field<DateTime>("검사일")).CopyToDataTable();
            Gate = gate;
            ItemCode = ChartSource.AsEnumerable().First()["품목코드"].ToString();

            #region 검사항목 가져오기
            DataTable dt = Commonsp.GetInpectItem(ItemCode, Gate);
            InspectItems = new ObservableCollection<InspectItem>();

            foreach (DataRow dr in dt.Rows)
            {
                InspectItems.Add(
                    new InspectItem
                    {
                        InspectName = dr["InspectName"].ToString(),
                        DownRate = dr["DownRate"].ToString(),
                        UpRate = dr["UpRate"].ToString(),
                    });
            }

            InspectItem = InspectItems[0];
            #endregion

            ChartDataSource = new ObservableCollection<SeriesItem>();

            CancelUICmd = new UICommand()
            {
                Caption = "닫기",
                IsCancel = true,
                IsDefault = false,
                Id = MessageBoxResult.Cancel,
            };
            DialogCmds = new List<UICommand>() {CancelUICmd };
            SearchCmd = new DelegateCommand(OnSearch, () => true);
            OnSearch();
        }

        public void OnSearch()
        {
            Opacity = 0.55m;

            try
            {
                // 관리도 결과값 계산
                IEnumerable<double> rows = ChartSource.Select()
                    .Where(x => !string.IsNullOrEmpty(x["" + InspectItem.InspectName + ""].ToString()) && Convert.ToDouble(x["" + InspectItem.InspectName + ""]) > 0)
                    .Select(c => Convert.ToDouble(c["" + InspectItem.InspectName + ""]));

                if (rows.Count() == 0) return;

                Avg = rows.Average();
                StdDev = CalculateStdDev(rows);
                Sig3 = StdDev * 3;
                Max = rows.Max();
                Min = rows.Min();
                Avg_M_Sig3 = Avg - Sig3;
                Avg_P_Sig3 = Avg + Sig3;

                double ret;
                USL = double.TryParse(InspectItem.UpRate, out ret) ? ret : (double?)null;
                LSL = double.TryParse(InspectItem.DownRate, out ret) ? ret : 0;
                Cp = (USL - LSL) / (6 * StdDev);
                Cpu = (USL - Avg) / Sig3;
                Cpl = (Avg - LSL) / Sig3;
                if (InspectItem.InspectName == "Td(5% wt. of loss temp.)")
                    Cpk = Cpl;
                else
                    Cpk = Math.Min(Convert.ToDouble(Cp), Convert.ToDouble(Cpu));
            }
            catch (Exception ex)
            {  
                MessageBoxService.ShowMessage(ex.Message, "Information", MessageButton.OK, MessageIcon.Information);
                return;
            }

            ChartDataSource.Clear();

            MinY = Math.Min(LSL, Avg_M_Sig3);
            MaxY = USL == null ? Avg_P_Sig3 : Math.Max((double)USL, Avg_P_Sig3);

            ChartSource.AsEnumerable().Where(x => !string.IsNullOrEmpty(x["" + InspectItem.InspectName + ""].ToString())).ToList().ForEach(u =>
                ChartDataSource.Add(
                    new SeriesItem
                    {
                        Name = InspectItem.InspectName,
                        ArgumentData = (string)u["Lot No."],
                        ValueData = (string)u["" + InspectItem.InspectName + ""],
                        LotNo = (string)u["Lot No."],
                    }
                )
            );

            Opacity = 1;
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
    }

    public class SeriesItem
    {
        public string Name { get; set; }
        public string ArgumentData { get; set; }
        public string ValueData { get; set; }
        public string ValueDataSecondary { get; set; }
        public string LotNo { get; set; }
    }

    public class InspectItem
    {
        public string InspectName { get; set; }
        public string DownRate { get; set; }
        public string UpRate { get; set; }
    }
}