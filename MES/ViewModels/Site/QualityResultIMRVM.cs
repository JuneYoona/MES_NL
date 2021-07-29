using System;
using System.Linq;
using DevExpress.Mvvm;
using MesAdmin.Common.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using System.Collections.Generic;
using MesAdmin.Models;
using System.Data.Common;

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
        public string Gate
        {
            get { return GetProperty(() => Gate); }
            set { SetProperty(() => Gate, value); }
        }
        public ObservableCollection<ItemInfo> Type { get; set; }
        public string SelectedType
        {
            get { return GetProperty(() => SelectedType); }
            set { SetProperty(() => SelectedType, value); }
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
        public double GridSpacing
        {
            get { return GetProperty(() => GridSpacing); }
            set { SetProperty(() => GridSpacing, value); }
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

            ChartDataSource = new ObservableCollection<SeriesItem>();

            CancelUICmd = new UICommand()
            {
                Caption = "닫기",
                IsCancel = true,
                IsDefault = false,
                Id = MessageBoxResult.Cancel,
            };
            DialogCmds = new List<UICommand>() {CancelUICmd };

            Type = new ObservableCollection<ItemInfo>();
            Type.Add(new ItemInfo { Text = "Purity", Value = "Purity" });
            if (gate == "FQC")
            {
                Type.Add(new ItemInfo { Text = "Tg", Value = "Tg" });
                Type.Add(new ItemInfo { Text = "Td(5% wt. of loss temp.)", Value = "Td(5% wt. of loss temp.)" });
            }
            SelectedType = "Purity";

            SearchCmd = new DelegateCommand(OnSearch, () => true);
            OnSearch();
        }

        public void OnSearch()
        {
            Opacity = 0.55m;
            
            if (!ChartSource.Columns.Contains(SelectedType))
            {
                MessageBoxService.ShowMessage("선택하신 검사항목으로 검사값이 없습니다!", "Information", MessageButton.OK, MessageIcon.Information);
                return;
            }

            try
            {
                // 관리도 결과값 계산
                IEnumerable<double> rows = ChartSource.Select()
                    .Where(x => x["" + SelectedType + ""] != DBNull.Value && x["" + SelectedType + ""].ToString() != "" && Convert.ToDouble(x["" + SelectedType + ""]) > 0)
                    .Select(c => Convert.ToDouble(c["" + SelectedType + ""]));

                Avg = rows.Average();
                StdDev = CalculateStdDev(rows);
                Sig3 = StdDev * 3;
                Max = rows.Max();
                Min = rows.Min();
                Avg_M_Sig3 = Avg - Sig3;
                Avg_P_Sig3 = Avg + Sig3;

                string itemCode = ChartSource.AsEnumerable().First()["품목코드"].ToString();
                DataRow dr = GetItemSpec(itemCode, SelectedType);

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
            }
            catch (Exception ex)
            {  
                MessageBoxService.ShowMessage(ex.Message, "Information", MessageButton.OK, MessageIcon.Information);
                return;
            }

            ChartDataSource.Clear();

            // 차트 옵션 세팅
            if (SelectedType == "Purity") GridSpacing = 0.02;
            else if (SelectedType == "Tg") GridSpacing = 0.5;
            else GridSpacing = 2;

            MinY = Math.Min(LSL, Avg_M_Sig3);
            MaxY = USL == null ? Avg_P_Sig3 : Math.Max((double)USL, Avg_P_Sig3);

            ChartSource.AsEnumerable().Where(x => x["" + SelectedType + ""] != DBNull.Value && x["" + SelectedType + ""].ToString() != "").ToList().ForEach(u =>
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

        private DataRow GetItemSpec(string itemCode, string type)
        {
            Database db = ProviderFactory.Instance;
            string sql;

            if (Gate == "IQC") sql = "SELECT UP_RATE, DOWN_RATE FROM [ERPSERVER].[DSNL].dbo.Q_INSPECTION_DS_ITEM WHERE ITEM_CD = @ItemCode AND INSP_ITEM_CD = 'RE001'";
            else sql = "SELECT UpRate, DownRate FROM quality_InspectItem WHERE ItemCode = @ItemCode AND InspectName = @Type";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCom, "@ItemCode", DbType.String, itemCode);
            db.AddInParameter(dbCom, "@Type", DbType.String, type);
            DataSet ds = db.ExecuteDataSet(dbCom);

            return ds.Tables[0].Rows.Count == 0 ? null : ds.Tables[0].Rows[0];
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
}
