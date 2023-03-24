using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using MesAdmin.Common.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace MesAdmin.Models
{
    public class ProductionInputRecord : StateBusinessObject
    {
        public string BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value); }
        }
        public string ProductOrderNo
        {
            get { return GetProperty(() => ProductOrderNo); }
            set { SetProperty(() => ProductOrderNo, value); }
        }
        public string OrderNo
        {
            get { return GetProperty(() => OrderNo); }
            set { SetProperty(() => OrderNo, value); }
        }
        public int Seq
        {
            get { return GetProperty(() => Seq); }
            set { SetProperty(() => Seq, value); }
        }
        public string WhCode
        {
            get { return GetProperty(() => WhCode); }
            set { SetProperty(() => WhCode, value); }
        }
        public string WaCode
        {
            get { return GetProperty(() => WaCode); }
            set { SetProperty(() => WaCode, value); }
        }
        public string EqpCode
        {
            get { return GetProperty(() => EqpCode); }
            set { SetProperty(() => EqpCode, value); }
        }
        public DateTime FinishDate
        {
            get { return GetProperty(() => FinishDate); }
            set { SetProperty(() => FinishDate, value); }
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
        public string LotNo
        {
            get { return GetProperty(() => LotNo); }
            set { SetProperty(() => LotNo, value); }
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
        public string ItemSpec
        {
            get { return GetProperty(() => ItemSpec); }
            set { SetProperty(() => ItemSpec, value); }
        }
        public string BasicUnit
        {
            get { return GetProperty(() => BasicUnit); }
            set { SetProperty(() => BasicUnit, value); }
        }
        public decimal Qty
        {
            get { return GetProperty(() => Qty); }
            set { SetProperty(() => Qty, value); }
        }
        public decimal BadQty
        {
            get { return GetProperty(() => BadQty); }
            set { SetProperty(() => BadQty, value); }
        }
        public decimal LossQty
        {
            get { return GetProperty(() => LossQty); }
            set { SetProperty(() => LossQty, value); }
        }
        public decimal OutQty
        {
            get { return GetProperty(() => OutQty); }
            set { SetProperty(() => OutQty, value); }
        }
        public decimal Yield
        {
            get { return GetProperty(() => Yield); }
            set { SetProperty(() => Yield, value); }
        }
        public string WorkerName
        {
            get { return GetProperty(() => WorkerName); }
            set { SetProperty(() => WorkerName, value); }
        }
        public string Remark1
        {
            get { return GetProperty(() => Remark1); }
            set { SetProperty(() => Remark1, value); }
        }
        public string Remark4
        {
            get { return GetProperty(() => Remark4); }
            set { SetProperty(() => Remark4, value); }
        }
        public string Remark5
        {
            get { return GetProperty(() => Remark5); }
            set { SetProperty(() => Remark5, value); }
        }
        public string UpdateId
        {
            get { return GetProperty(() => UpdateId); }
            set { SetProperty(() => UpdateId, value); }
        }
        public DateTime UpdateDate
        {
            get { return GetProperty(() => UpdateDate); }
            set { SetProperty(() => UpdateDate, value); }
        }
        public string LQCResult
        {
            get { return GetProperty(() => LQCResult); }
            set { SetProperty(() => LQCResult, value); }
        }
        
        // 선행로트 관련 속성
        public string Color
        {
            get { return GetProperty(() => Color); }
            set { SetProperty(() => Color, value); }
        }
        public string Show
        {
            get { return GetProperty(() => Show); }
            set { SetProperty(() => Show, value); }
        }
        public string Tip
        {
            get { return GetProperty(() => Tip); }
            set { SetProperty(() => Tip, value); }
        }
        public int LeadTime
        {
            get { return GetProperty(() => LeadTime); }
            set { SetProperty(() => LeadTime, value); }
        }
        public int PauseTime
        {
            get { return GetProperty(() => PauseTime); }
            set { SetProperty(() => PauseTime, value); }
        }
        public int WorkTime { get; set; }
        public string LeadTime2
        {
            get { return GetProperty(() => LeadTime2); }
            set { SetProperty(() => LeadTime2, value); }
        }
        public IEnumerable<ProductionOutputRecord> OutputRecord { get; set; }

        public DataTable ProductionMonthlyReport(string yyyymm, string itemCode, string itemAccount, string waCode)
        {
            Database db = ProviderFactory.Instance;

            DbCommand dbCom = db.GetSqlStringCommand("SELECT * FROM fn_productionMonthly(@YYYYMM, @ItemCode, @ItemAccount, @WaCode)");
            db.AddInParameter(dbCom, "@YYYYMM", DbType.String, yyyymm);
            db.AddInParameter(dbCom, "@ItemCode", DbType.String, itemCode);
            db.AddInParameter(dbCom, "@ItemAccount", DbType.String, itemAccount);
            db.AddInParameter(dbCom, "@WaCode", DbType.String, waCode);
            DataSet ds = db.ExecuteDataSet(dbCom);

            return ds.Tables[0];
        }
    }

    public class ProductionInputRecordList : ObservableCollection<ProductionInputRecord>
    {
        private object thisLock = new object();
        private DateTime startDate;
        private DateTime endDate;
        private string bizAreaCode;
        private string waCode;
        private string lotNo;
        private string color;

        public ProductionInputRecordList(DateTime startDate, DateTime endDate, string bizAreaCode = "", string waCode = "", string lotNo = "", string color = "")
        {
            this.startDate = startDate;
            this.endDate = endDate;
            this.bizAreaCode = bizAreaCode;
            this.waCode = waCode;
            this.lotNo = lotNo;
            this.color = color;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;

            string sql;
            if (bizAreaCode == "BAC40")
            {
                sql = "SELECT * FROM fn_production_InputRecord_BAC40(@BizAreaCode, @WaCode, @LotNo, @StartDate, @EndDate) ORDER BY OrderNo desc";
            }
            else
            {
                sql = "SELECT * FROM fn_production_InputRecord(@BizAreaCode, @WaCode, @LotNo, @StartDate, @EndDate) ";

                if (!string.IsNullOrEmpty(color))
                {
                    sql += "WHERE Show = 'Visible' AND Color = '" + color + "'";
                }

                sql += " ORDER BY OrderNo desc";
            }

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, bizAreaCode);
            db.AddInParameter(dbCom, "@WaCode", DbType.String, waCode);
            db.AddInParameter(dbCom, "@LotNo", DbType.String, lotNo);
            db.AddInParameter(dbCom, "@StartDate", DbType.Date, startDate.ToShortDateString());
            db.AddInParameter(dbCom, "@EndDate", DbType.Date, endDate.ToShortDateString());
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add
                (
                    new ProductionInputRecord
                    {
                        State = EntityState.Unchanged,
                        BizAreaCode = (string)u["BizAreaCode"],
                        ProductOrderNo = (string)u["ProductOrderNo"],
                        OrderNo = (string)u["OrderNo"],
                        Seq = (int)u["Seq"],
                        WhCode = (string)u["WhCode"],
                        WaCode = (string)u["WaCode"],
                        EqpCode = (string)u["EqpCode"],
                        FinishDate = (DateTime)u["FinishDate"],
                        StartDate = (DateTime)u["StartDate"],
                        EndDate = (DateTime)u["EndDate"],
                        LotNo = (string)u["LotNo"],
                        ItemCode = (string)u["ItemCode"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = (string)u["ItemSpec"],
                        BasicUnit = (string)u["BasicUnit"],
                        Qty = (decimal)u["Qty"],
                        BadQty = (decimal)u["BadQty"],
                        LossQty = (decimal)u["LossQty"],
                        Yield = (decimal)u["Yield"],
                        WorkerName = u["WorkerName"].ToString(),
                        Remark1 = u["Remark1"].ToString(),
                        Remark4 = u["Remark4"].ToString(),
                        Remark5 = u["Remark5"].ToString(),
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"],
                        LeadTime = (int)u["LeadTime"],
                        PauseTime = (int)u["PauseTime"],
                        WorkTime = (int)u["LeadTime"] - (int)u["PauseTime"],
                        LeadTime2 = u["LeadTime2"].ToString(),
                        LQCResult = bizAreaCode == "BAC40" ? u["LQCResult"].ToString() : null,
                        Color = bizAreaCode == "BAC40" ? null : u["Color"].ToString(),
                        Show = bizAreaCode == "BAC40" ? null : u["Show"].ToString(),
                        Tip = bizAreaCode == "BAC40" ? null : u["Tip"].ToString(),
                    }
                )
            );
        }

        public IEnumerable<ProductionOutputRecord> GetOutputRecord(string orderNo, int seq)
        {
            return new ProductionOutputRecordList(orderNo, seq);
        }

        public void Save()
        {
            IEnumerable<ProductionInputRecord> items = this.Items;
            Delete(items.Where(u => u.State == EntityState.Deleted));
        }

        public void Delete(IEnumerable<ProductionInputRecord> items)
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                string str;
                DbCommand dbCom = null;
                try
                {
                    foreach (ProductionInputRecord item in items)
                    {
                        str = "DELETE production_InputRecord WHERE ProductOrderNo = '{0}'";
                        str = string.Format(str, item.OrderNo + item.Seq.ToString());
                        dbCom = db.GetSqlStringCommand(str);
                        db.ExecuteNonQuery(dbCom, trans);
                    }
                    trans.Commit();
                }
                catch (SqlException ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Errors[0].Message);
                }
            }
        }
    }
}