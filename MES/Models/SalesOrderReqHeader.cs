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
    public class SalesOrderReqHeader : ViewModelBase
    {
        public bool IsDirty { get; set; }
        public string ReqNo
        {
            get { return GetProperty(() => ReqNo); }
            set { SetProperty(() => ReqNo, value); }
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
        public string Currency
        {
            get { return GetProperty(() => Currency); }
            set { SetProperty(() => Currency, value); }
        }
        public DateTime? DlvyDate
        {
            get { return GetProperty(() => DlvyDate); }
            set { SetProperty(() => DlvyDate, value); }
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
        public string FinalFlag
        {
            get { return GetProperty(() => FinalFlag); }
            set { SetProperty(() => FinalFlag, value); }
        }
        public string Remark1
        {
            get { return GetProperty(() => Remark1); }
            set { SetProperty(() => Remark1, value); }
        }
        public string UpdateId
        {
            get { return GetProperty(() => UpdateId); }
            set { SetProperty(() => UpdateId, value); }
        }
        public string Remark2
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string SoNo
        {
            get { return GetValue<string>(); }
            set { SetValue(value, () => IsDirty = true); }
        }
        public DateTime UpdateDate
        {
            get { return GetProperty(() => UpdateDate); }
            set { SetProperty(() => UpdateDate, value); }
        }

        public SalesOrderReqHeader() { }
        public SalesOrderReqHeader(string reqNo)
        {
            Database db = ProviderFactory.Instance;
            string sql = "SELECT * FROM sales_OrderReq_Header (NOLOCK) WHERE ReqNo = @ReqNo";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCom, "@ReqNo", DbType.String, reqNo);
            DataSet ds = db.ExecuteDataSet(dbCom);
            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
            {
                ReqNo = (string)u["ReqNo"];
                SoType = (string)u["SoType"];
                MoveType = (string)u["MoveType"];
                ShipTo = (string)u["ShipTo"];
               // DlvyDate = (DateTime)u["DlvyDate"];
                ReqDate = (DateTime)u["ReqDate"];
                Memo = u["Memo"].ToString();
                FinalFlag = u["FinalFlag"].ToString();
                Remark1 = u["Remark1"].ToString();
                Remark2 = u["Remark2"].ToString();
                SoNo = u["SoNo"].ToString();
                UpdateId = (string)u["UpdateId"];
                UpdateDate = (DateTime)u["UpdateDate"];
            });

            IsDirty = false;
        }

        public void Delete()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    dbCom = db.GetSqlStringCommand(string.Format("DELETE sales_OrderReq_Header WHERE ReqNo = '{0}'", ReqNo));
                    db.ExecuteNonQuery(dbCom, trans);
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public void Update()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    dbCom = db.GetSqlStringCommand(string.Format("UPDATE sales_OrderReq_Header SET SoNo = '{1}' WHERE ReqNo = '{0}'", ReqNo, SoNo));
                    db.ExecuteNonQuery(dbCom, trans);
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
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
                    dbCom = db.GetStoredProcCommand("usp_sales_OrderReq_Header");
                    dbCom.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(dbCom, "@SoType", DbType.String, SoType);
                    db.AddInParameter(dbCom, "@MoveType", DbType.String, MoveType);
                    db.AddInParameter(dbCom, "@ShipTo", DbType.String, ShipTo);
                    db.AddInParameter(dbCom, "@ReqDate", DbType.Date, ReqDate);
                    db.AddInParameter(dbCom, "@DlvyDate", DbType.Date, DlvyDate);
                    db.AddInParameter(dbCom, "@Currency", DbType.String, Currency);
                    db.AddInParameter(dbCom, "@Memo", DbType.String, Memo);
                    db.AddInParameter(dbCom, "@Remark1", DbType.String, Remark1);
                    db.AddInParameter(dbCom, "@Remark2", DbType.String, Remark2);
                    db.AddInParameter(dbCom, "@SoNo", DbType.String, SoNo);
                    db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                    db.AddOutParameter(dbCom, "@OutDrNo", DbType.String, 20);
                    db.ExecuteNonQuery(dbCom, trans);
                    ReqNo = db.GetParameterValue(dbCom, "@OutDrNo").ToString();
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public void Confirm()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    dbCom = db.GetSqlStringCommand(string.Format("UPDATE sales_OrderReq_Header SET FinalFlag = 'Y' WHERE ReqNo = '{0}'", ReqNo));
                    db.ExecuteNonQuery(dbCom, trans);

                    // 자동메일발송
                    dbCom = db.GetStoredProcCommand("usp_sales_OrderReq_MailService");
                    dbCom.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(dbCom, "@ReqNo", DbType.String, ReqNo);
                    db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                    db.ExecuteNonQuery(dbCom, trans);

                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }
    }

    public class SalesOrderReqHeaderList : ObservableCollection<SalesOrderReqHeader>
    {
        private DateTime startDate;
        private DateTime endDate;

        public SalesOrderReqHeaderList() { }
        public SalesOrderReqHeaderList(IEnumerable<SalesOrderReqHeader> items) : base(items) { }
        public SalesOrderReqHeaderList(DateTime startDate, DateTime endDate)
        {
            this.startDate = startDate;
            this.endDate = endDate;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            string sql = "SELECT * FROM sales_OrderReq_Header (NOLOCK) WHERE ReqDate BETWEEN @StartDate AND @EndDate ORDER BY ReqDate DESC ";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCom, "@StartDate", DbType.String, startDate.ToShortDateString());
            db.AddInParameter(dbCom, "@EndDate", DbType.String, endDate.ToShortDateString());
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new SalesOrderReqHeader
                    {
                        ReqNo = (string)u["ReqNo"],
                        SoType = (string)u["SoType"],
                        MoveType = (string)u["MoveType"],
                        ShipTo = (string)u["ShipTo"],
                       // DlvyDate = (DateTime)u["DlvyDate"],
                        ReqDate = (DateTime)u["ReqDate"],
                        Memo = u["Memo"].ToString(),
                        Remark1 = u["Remark1"].ToString(),
                        Remark2 = u["Remark2"].ToString(),
                        SoNo = u["SoNo"].ToString(),
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"]
                    }
                )
            );
        }
    }
}
