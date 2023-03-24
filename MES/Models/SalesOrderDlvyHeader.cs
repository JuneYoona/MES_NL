using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using DevExpress.Mvvm;
using System.Linq;

namespace MesAdmin.Models
{
    public class SalesOrderDlvyHeader : ViewModelBase
    {
        public string DnNo
        {
            get { return GetProperty(() => DnNo); }
            set { SetProperty(() => DnNo, value); }
        }
 
        public string SoType
        {
            get { return GetProperty(() => SoType); }
            set { SetProperty(() => SoType, value); }
        }
        public string MoveType
        {
            get { return GetProperty(() => MoveType); }
            set { SetProperty(() => MoveType, value); }
        }
        public string ShipTo
        {
            get { return GetProperty(() => ShipTo); }
            set { SetProperty(() => ShipTo, value); }
        }
        public string DocumentNo
        {
            get { return GetProperty(() => DocumentNo); }
            set { SetProperty(() => DocumentNo, value); }
        }
        public string PostFlag
        {
            get { return GetProperty(() => PostFlag); }
            set { SetProperty(() => PostFlag, value); }
        }
        public string PackingFlag
        {
            get { return GetProperty(() => PackingFlag); }
            set { SetProperty(() => PackingFlag, value); }
        }
        public DateTime? ActualDate
        {
            get { return GetProperty(() => ActualDate); }
            set { SetProperty(() => ActualDate, value); }
        }
        public DateTime? ReqDate
        {
            get { return GetProperty(() => ReqDate); }
            set { SetProperty(() => ReqDate, value); }
        }
        public string Memo
        {
            get { return GetProperty(() => Memo); }
            set { SetProperty(() => Memo, value); }
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
        public string Remark1
        {
            get { return GetProperty(() => Remark1); }
            set { SetProperty(() => Remark1, value); }
        }
        public string Remark2
        {
            get { return GetProperty(() => Remark2); }
            set { SetProperty(() => Remark2, value); }
        }
        public string Remark3
        {
            get { return GetProperty(() => Remark3); }
            set { SetProperty(() => Remark3, value); }
        }
        public string BizName
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string ItemCode
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string ItemName
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string ItemSpec
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public decimal Qty
        {
            get { return GetValue<decimal>(); }
            set { SetValue(value); }
        }
        public string WhCode
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string ReqNo
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public int ReqSeq
        {
            get { return GetValue<int>(); }
            set { SetValue(value); }
        }
        public string BasicUnit
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }

        public SalesOrderDlvyHeader() { }
        public SalesOrderDlvyHeader(string dnNo)
        {
            Database db = ProviderFactory.Instance;
            string sql = "SELECT * FROM sales_OrderDlvy_Header (NOLOCK) WHERE DnNo = @DnNo";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCom, "@DnNo", DbType.String, dnNo);
            DataSet ds = db.ExecuteDataSet(dbCom);
            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
            {
                DnNo = (string)u["DnNo"];
                SoType = (string)u["SoType"];
                MoveType = (string)u["MoveType"];
                ShipTo = (string)u["ShipTo"];
                DocumentNo = u["DocumentNo"].ToString();
                PostFlag = (string)u["PostFlag"];
                PackingFlag = (string)u["PackingFlag"];
                ActualDate = u["ActualDate"] == DBNull.Value ? null : (DateTime?)u["ActualDate"];
                ReqDate = (DateTime)u["ReqDate"];
                Memo = u["Memo"].ToString();
                UpdateId = (string)u["UpdateId"];
                UpdateDate = (DateTime)u["UpdateDate"];
            });
        }

        public void Delete()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                dbCom = db.GetSqlStringCommand(string.Format("DELETE sales_OrderDlvy_Header WHERE DnNo = '{0}'", DnNo));
                db.ExecuteNonQuery(dbCom);
            }
        }

        public void Save()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    dbCom = db.GetStoredProcCommand("usp_sales_OrderDlvy_Header");
                    dbCom.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(dbCom, "@SoType", DbType.String, SoType);
                    db.AddInParameter(dbCom, "@MoveType", DbType.String, MoveType);
                    db.AddInParameter(dbCom, "@ShipTo", DbType.String, ShipTo);
                    db.AddInParameter(dbCom, "@ReqDate", DbType.Date, ReqDate);
                    db.AddInParameter(dbCom, "@Memo", DbType.String, Memo);
                    db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                    db.AddOutParameter(dbCom, "@OutDnNo", DbType.String, 20);
                    db.ExecuteNonQuery(dbCom, trans);
                    DnNo = db.GetParameterValue(dbCom, "@OutDnNo").ToString();
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public void PostDelivery()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                string sql = "Update sales_OrderDlvy_Header SET ActualDate = '{1}', PostFlag = 'Y', UpdateId = '{2}', UpdateDate = getdate()  WHERE DnNo = '{0}'";
                sql = string.Format(sql, DnNo, ActualDate.Value.ToShortDateString(), DSUser.Instance.UserID);
                dbCom = db.GetSqlStringCommand(sql);
                db.ExecuteNonQuery(dbCom);
            }
        }

        public void PostDeliveryCancel()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                string sql = "Update sales_OrderDlvy_Header SET ActualDate = null, PostFlag = 'N', UpdateId = '{2}', UpdateDate = getdate()  WHERE DnNo = '{0}'";
                sql = string.Format(sql, DnNo, ActualDate.Value.ToShortDateString(), DSUser.Instance.UserID);
                dbCom = db.GetSqlStringCommand(sql);
                db.ExecuteNonQuery(dbCom);
            }
        }
    }

    public class SalesOrderDlvyHeaderList : ObservableCollection<SalesOrderDlvyHeader>
    {
        private DateTime startDate;
        private DateTime endDate;

        public SalesOrderDlvyHeaderList() { }
        public SalesOrderDlvyHeaderList(IEnumerable<SalesOrderDlvyHeader> items) : base(items) { }
        public SalesOrderDlvyHeaderList(DateTime startDate, DateTime endDate)
        {
            this.startDate = startDate;
            this.endDate = endDate;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            string sql = "SELECT * FROM sales_OrderDlvy_Header (NOLOCK) WHERE ReqDate BETWEEN @StartDate AND @EndDate ORDER BY ReqDate DESC ";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCom, "@StartDate", DbType.String, startDate.ToShortDateString());
            db.AddInParameter(dbCom, "@EndDate", DbType.String, endDate.ToShortDateString());
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new SalesOrderDlvyHeader
                    {
                        DnNo = (string)u["DnNo"],
                        SoType = (string)u["SoType"],
                        MoveType = (string)u["MoveType"],
                        ShipTo = (string)u["ShipTo"],
                        DocumentNo = u["DocumentNo"].ToString(),
                        PostFlag = (string)u["PostFlag"],
                        PackingFlag = (string)u["PackingFlag"],
                        ActualDate = u["ActualDate"] == DBNull.Value ? null : (DateTime?)u["ActualDate"],
                        ReqDate = (DateTime)u["ReqDate"],
                        Memo = u["Memo"].ToString(),
                        Remark1 = u["Remark1"].ToString(),
                        Remark2 = u["Remark2"].ToString(),
                        Remark3 = u["Remark3"].ToString(),
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"]
                    }
                )
            );
        }
    }

    public class SalesOrderDlvyHeaderTable
    {
        private string soType;
        private string itemCode;
        private string shipTo;
        private DateTime startDate;
        private DateTime endDate;
        public DataTable Collections { get; set; }

        public SalesOrderDlvyHeaderTable(DateTime startDate, DateTime endDate, string soType, string itemCode, string shipTo)
        {
            this.soType = soType;
            this.itemCode = itemCode;
            this.shipTo = shipTo;
            this.startDate = startDate;
            this.endDate = endDate;
            InitializeList();
        }

        public void InitializeList()
        {
            Database db = ProviderFactory.Instance;

            DbCommand dbCom = db.GetStoredProcCommand("usps_sales_OrderDlvy_Header_List");
            dbCom.CommandType = CommandType.StoredProcedure;
            db.AddInParameter(dbCom, "@ItemCode", DbType.String, itemCode);
            db.AddInParameter(dbCom, "@ShipTo", DbType.String, shipTo);
            db.AddInParameter(dbCom, "@SoType", DbType.String, soType);
            db.AddInParameter(dbCom, "@StartDate", DbType.String, startDate.ToShortDateString());
            db.AddInParameter(dbCom, "@EndDate", DbType.String, endDate.ToShortDateString());
            db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, DSUser.Instance.BizAreaCode);
            DataSet ds = db.ExecuteDataSet(dbCom);

            if (ds.Tables.Count > 0)
                Collections = ds.Tables[0];
        }
    }

    public class SalesOrderDlvyHeaderDetail: ObservableCollection<SalesOrderDlvyHeader>
    {
        private DateTime startDate;
        private DateTime endDate;
        private string itemCode;
        private string bizCode;

        public SalesOrderDlvyHeaderDetail(DateTime startDate, DateTime endDate, string itemCode, string bizCode)
        {
            this.startDate = startDate;
            this.endDate = endDate;
            this.itemCode = itemCode;
            this.bizCode = bizCode;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;

            DbCommand dbCom = db.GetStoredProcCommand("usp_sales_OrderDlvy_Header_List");
            db.AddInParameter(dbCom, "@StartDate", DbType.Date, startDate);
            db.AddInParameter(dbCom, "@EndDate", DbType.Date, endDate);
            db.AddInParameter(dbCom, "@ItemCode", DbType.String, itemCode);
            db.AddInParameter(dbCom, "@ShipTo", DbType.String, bizCode);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new SalesOrderDlvyHeader
                    {
                        DnNo = (string)u["DnNo"],
                        SoType = (string)u["SoType"],
                        MoveType = (string)u["MoveType"],
                        ShipTo = (string)u["ShipTo"],
                        DocumentNo = u["DocumentNo"].ToString(),
                        PostFlag = (string)u["PostFlag"],
                        PackingFlag = (string)u["PackingFlag"],
                        ActualDate = u["ActualDate"] == DBNull.Value ? null : (DateTime?)u["ActualDate"],
                        ReqDate = (DateTime)u["ReqDate"],
                        Memo = u["Memo"].ToString(),
                        Remark1 = u["Remark1"].ToString(),
                        Remark2 = u["Remark2"].ToString(),
                        Remark3 = u["Remark3"].ToString(),
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"],

                        BizName = u["BizName"].ToString(),
                        ItemCode = u["ItemCode"].ToString(),
                        ItemName = u["ItemName"].ToString(),
                        ItemSpec = u["ItemSpec"].ToString(),
                        Qty = (decimal)u["Qty"],
                        WhCode = u["WhCode"].ToString(),
                        ReqNo = u["ReqNo"].ToString(),
                        ReqSeq = (int)u["ReqSeq"],
                        BasicUnit = u["BasicUnit"].ToString(),
                    }
                )
            );
        }
    }

    public class SalesDlvyNoteHeader : ViewModelBase
    {
        public string DnNo
        {
            get { return GetProperty(() => DnNo); }
            set { SetProperty(() => DnNo, value); }
        }
        public string SoType
        {
            get { return GetProperty(() => SoType); }
            set { SetProperty(() => SoType, value); }
        }
        public string MoveType
        {
            get { return GetProperty(() => MoveType); }
            set { SetProperty(() => MoveType, value); }
        }
        public string ShipTo
        {
            get { return GetProperty(() => ShipTo); }
            set { SetProperty(() => ShipTo, value); }
        }
        public string DocumentNo
        {
            get { return GetProperty(() => DocumentNo); }
            set { SetProperty(() => DocumentNo, value); }
        }
        public string PostFlag
        {
            get { return GetProperty(() => PostFlag); }
            set { SetProperty(() => PostFlag, value); }
        }
        public string PackingFlag
        {
            get { return GetProperty(() => PackingFlag); }
            set { SetProperty(() => PackingFlag, value); }
        }
        public DateTime? ActualDate
        {
            get { return GetProperty(() => ActualDate); }
            set { SetProperty(() => ActualDate, value); }
        }
        public DateTime? ReqDate
        {
            get { return GetProperty(() => ReqDate); }
            set { SetProperty(() => ReqDate, value); }
        }
        public string Memo
        {
            get { return GetProperty(() => Memo); }
            set { SetProperty(() => Memo, value); }
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
        public string Remark1
        {
            get { return GetProperty(() => Remark1); }
            set { SetProperty(() => Remark1, value); }
        }
        public string Remark2
        {
            get { return GetProperty(() => Remark2); }
            set { SetProperty(() => Remark2, value); }
        }
        public string Remark3
        {
            get { return GetProperty(() => Remark3); }
            set { SetProperty(() => Remark3, value); }
        }
        public string BizName
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string ItemCode
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string ItemName
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string ItemSpec
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public decimal Qty
        {
            get { return GetValue<decimal>(); }
            set { SetValue(value); }
        }
        public string WhCode
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string ReqNo
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public int ReqSeq
        {
            get { return GetValue<int>(); }
            set { SetValue(value); }
        }
        public string BasicUnit
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }

        public SalesDlvyNoteHeader() { }
        public SalesDlvyNoteHeader(string dnNo)
        {
            Database db = ProviderFactory.Instance;
            string sql = "SELECT * FROM sales_DlvyNote_Header (NOLOCK) WHERE DnNo = @DnNo";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCom, "@DnNo", DbType.String, dnNo);
            DataSet ds = db.ExecuteDataSet(dbCom);
            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
            {
                DnNo = (string)u["DnNo"];
                SoType = (string)u["SoType"];
                MoveType = (string)u["MoveType"];
                ShipTo = (string)u["ShipTo"];
                DocumentNo = u["DocumentNo"].ToString();
                PostFlag = (string)u["PostFlag"];
                PackingFlag = (string)u["PackingFlag"];
                ActualDate = u["ActualDate"] == DBNull.Value ? null : (DateTime?)u["ActualDate"];
                ReqDate = (DateTime)u["ReqDate"];
                Memo = u["Memo"].ToString();
                UpdateId = (string)u["UpdateId"];
                UpdateDate = (DateTime)u["UpdateDate"];
            });
        }

        public void Delete()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                dbCom = db.GetSqlStringCommand(string.Format("DELETE sales_DlvyNote_Header WHERE DnNo = '{0}'", DnNo));
                db.ExecuteNonQuery(dbCom);
            }
        }

        public void Save()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    dbCom = db.GetStoredProcCommand("usp_sales_DlvyNote_Header");
                    dbCom.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(dbCom, "@SoType", DbType.String, SoType);
                    db.AddInParameter(dbCom, "@MoveType", DbType.String, MoveType);
                    db.AddInParameter(dbCom, "@ShipTo", DbType.String, ShipTo);
                    db.AddInParameter(dbCom, "@ReqDate", DbType.Date, ReqDate);
                    db.AddInParameter(dbCom, "@Memo", DbType.String, Memo);
                    db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                    db.AddOutParameter(dbCom, "@OutDnNo", DbType.String, 20);
                    db.ExecuteNonQuery(dbCom, trans);
                    DnNo = db.GetParameterValue(dbCom, "@OutDnNo").ToString();
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public void PostDelivery()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                string sql = "Update sales_DlvyNote_Header SET ActualDate = '{1}', PostFlag = 'Y', UpdateId = '{2}', UpdateDate = getdate()  WHERE DnNo = '{0}'";
                sql = string.Format(sql, DnNo, ActualDate.Value.ToShortDateString(), DSUser.Instance.UserID);
                dbCom = db.GetSqlStringCommand(sql);
                db.ExecuteNonQuery(dbCom);
            }
        }

        public void PostDeliveryCancel()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                string sql = "Update sales_DlvyNote_Header SET ActualDate = null, PostFlag = 'N', UpdateId = '{2}', UpdateDate = getdate()  WHERE DnNo = '{0}'";
                sql = string.Format(sql, DnNo, ActualDate.Value.ToShortDateString(), DSUser.Instance.UserID);
                dbCom = db.GetSqlStringCommand(sql);
                db.ExecuteNonQuery(dbCom);
            }
        }
    }

    public class SalesDlvyNoteHeaderList : ObservableCollection<SalesDlvyNoteHeader>
    {
        private string bizAreaCode;
        private DateTime startDate;
        private DateTime endDate;

        public SalesDlvyNoteHeaderList() { }
        public SalesDlvyNoteHeaderList(IEnumerable<SalesDlvyNoteHeader> items) : base(items) { }
        public SalesDlvyNoteHeaderList(string bizAreaCode, DateTime startDate, DateTime endDate)
        {
            this.bizAreaCode = bizAreaCode;
            this.startDate = startDate;
            this.endDate = endDate;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            string sql = "SELECT * FROM sales_DlvyNote_Header (NOLOCK) A "
                + "INNER JOIN (SELECT DnNo, BizAreaCode FROM sales_DlvyNote_Detail GROUP BY DnNo, BizAreaCode) B ON A.DnNo = B.DnNo "
                + "WHERE ReqDate BETWEEN @StartDate AND @EndDate AND BizAreaCode = @BizAreaCode ORDER BY ReqDate DESC ";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, bizAreaCode);
            db.AddInParameter(dbCom, "@StartDate", DbType.String, startDate.ToShortDateString());
            db.AddInParameter(dbCom, "@EndDate", DbType.String, endDate.ToShortDateString());
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new SalesDlvyNoteHeader
                    {
                        DnNo = (string)u["DnNo"],
                        SoType = (string)u["SoType"],
                        MoveType = (string)u["MoveType"],
                        ShipTo = (string)u["ShipTo"],
                        DocumentNo = u["DocumentNo"].ToString(),
                        PostFlag = (string)u["PostFlag"],
                        PackingFlag = (string)u["PackingFlag"],
                        ActualDate = u["ActualDate"] == DBNull.Value ? null : (DateTime?)u["ActualDate"],
                        ReqDate = (DateTime)u["ReqDate"],
                        Memo = u["Memo"].ToString(),
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"]
                    }
                )
            );
        }
    }

    public class SalesDlvyNoteHeaderTable
    {
        private string soType;
        private string itemCode;
        private string shipTo;
        private string bizAreaCode;
        private DateTime startDate;
        private DateTime endDate;
        public DataTable Collections { get; set; }

        public SalesDlvyNoteHeaderTable(DateTime startDate, DateTime endDate, string soType, string itemCode, string shipTo, string bizAreaCode)
        {
            this.soType = soType;
            this.itemCode = itemCode;
            this.shipTo = shipTo;
            this.bizAreaCode = bizAreaCode;
            this.startDate = startDate;
            this.endDate = endDate;
            InitializeList();
        }

        public void InitializeList()
        {
            Database db = ProviderFactory.Instance;

            DbCommand dbCom = db.GetStoredProcCommand("usp_sales_DlvyNote_Header_List");
            dbCom.CommandType = CommandType.StoredProcedure;
            db.AddInParameter(dbCom, "@ItemCode", DbType.String, itemCode);
            db.AddInParameter(dbCom, "@ShipTo", DbType.String, shipTo);
            db.AddInParameter(dbCom, "@SoType", DbType.String, soType);
            db.AddInParameter(dbCom, "@StartDate", DbType.String, startDate.ToShortDateString());
            db.AddInParameter(dbCom, "@EndDate", DbType.String, endDate.ToShortDateString());
            db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, bizAreaCode);
            DataSet ds = db.ExecuteDataSet(dbCom);

            if (ds.Tables.Count > 0)
                Collections = ds.Tables[0];
        }
    }
}