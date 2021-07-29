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
    public class SalesOrderHeader : ViewModelBase
    {
        public string SoNo
        {
            get { return GetProperty(() => SoNo); }
            set { SetProperty(() => SoNo, value); }
        }
        public string SoType
        {
            get { return GetProperty(() => SoType); }
            set { SetProperty(() => SoType, value); }
        }
        public string ShipTo
        {
            get { return GetProperty(() => ShipTo); }
            set { SetProperty(() => ShipTo, value); }
        }
        public string BillTo
        {
            get { return GetProperty(() => BillTo); }
            set { SetProperty(() => BillTo, value); }
        }
        public DateTime? SoDate
        {
            get { return GetProperty(() => SoDate); }
            set { SetProperty(() => SoDate, value); }
        }
        public DateTime? ReqDlvyDate
        {
            get { return GetProperty(() => ReqDlvyDate); }
            set { SetProperty(() => ReqDlvyDate, value); }
        }
        public string Currency
        {
            get { return GetProperty(() => Currency); }
            set { SetProperty(() => Currency, value); }
        }
        public decimal? ExchangeRate
        {
            get { return GetProperty(() => ExchangeRate); }
            set { SetProperty(() => ExchangeRate, value); }
        }
        public decimal? NetAmt
        {
            get { return GetProperty(() => NetAmt); }
            set { SetProperty(() => NetAmt, value); }
        }
        public decimal? NetAmtLocal
        {
            get { return GetProperty(() => NetAmtLocal); }
            set { SetProperty(() => NetAmtLocal, value); }
        }
        public string VATFlag
        {
            get { return GetProperty(() => VATFlag); }
            set { SetProperty(() => VATFlag, value); }
        }
        public decimal VATRate
        {
            get { return GetProperty(() => VATRate); }
            set { SetProperty(() => VATRate, value); }
        }
        public decimal? VATAmt
        {
            get { return GetProperty(() => VATAmt); }
            set { SetProperty(() => VATAmt, value); }
        }
        public decimal? VATAmtLocal
        {
            get { return GetProperty(() => VATAmtLocal); }
            set { SetProperty(() => VATAmtLocal, value); }
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

        public SalesOrderHeader() { }
        public SalesOrderHeader(string soNo)
        {
            Database db = ProviderFactory.Instance;
            string sql = "SELECT * FROM sales_Order_Header (NOLOCK) WHERE SoNo = @SoNo";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCom, "@SoNo", DbType.String, soNo);
            DataSet ds = db.ExecuteDataSet(dbCom);
            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
            {
                SoNo = (string)u["SoNo"];
                SoType = (string)u["SoType"];
                ShipTo = (string)u["ShipTo"];
                BillTo = (string)u["BillTo"];
                SoDate = (DateTime)u["SoDate"];
                ReqDlvyDate = (DateTime)u["ReqDlvyDate"];
                Currency = (string)u["Currency"];
                ExchangeRate = (decimal)u["ExchangeRate"];
                NetAmt = (decimal)u["NetAmt"];
                NetAmtLocal = (decimal)u["NetAmtLocal"];
                VATFlag = (string)u["VATFlag"];
                VATRate = (decimal)u["VATRate"];
                VATAmt = (decimal)u["VATAmt"];
                VATAmtLocal = (decimal)u["VATAmtLocal"];
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
                dbCom = db.GetSqlStringCommand(string.Format("DELETE sales_Order_Header WHERE SoNo = '{0}'", SoNo));
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
                    dbCom = db.GetStoredProcCommand("usp_sales_Order_Header");
                    dbCom.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(dbCom, "@SoNo", DbType.String, SoNo);
                    db.AddInParameter(dbCom, "@SoType", DbType.String, SoType);
                    db.AddInParameter(dbCom, "@ShipTo", DbType.String, ShipTo);
                    db.AddInParameter(dbCom, "@BillTo", DbType.String, BillTo);
                    db.AddInParameter(dbCom, "@SoDate", DbType.Date, SoDate);
                    db.AddInParameter(dbCom, "@ReqDlvyDate", DbType.Date, ReqDlvyDate);
                    db.AddInParameter(dbCom, "@Currency", DbType.String, Currency);
                    db.AddInParameter(dbCom, "@ExchangeRate", DbType.Decimal, ExchangeRate);
                    db.AddInParameter(dbCom, "@NetAmt", DbType.Decimal, NetAmt);
                    db.AddInParameter(dbCom, "@NetAmtLocal", DbType.Decimal, NetAmtLocal);
                    db.AddInParameter(dbCom, "@VATFlag", DbType.String, VATFlag);
                    db.AddInParameter(dbCom, "@VATRate", DbType.Decimal, VATRate);
                    db.AddInParameter(dbCom, "@VATAmt", DbType.Decimal, VATAmt);
                    db.AddInParameter(dbCom, "@VATAmtLocal", DbType.Decimal, VATAmtLocal);
                    db.AddInParameter(dbCom, "@Memo", DbType.String, Memo);
                    db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                    db.AddOutParameter(dbCom, "@OutSoNo", DbType.String, 20);
                    db.ExecuteNonQuery(dbCom, trans);
                    SoNo = db.GetParameterValue(dbCom, "@OutSoNo").ToString();
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

    public class SalesOrderHeaderList : ObservableCollection<SalesOrderHeader>
    {
        private DateTime startDate;
        private DateTime endDate;

        public SalesOrderHeaderList() { }
        public SalesOrderHeaderList(IEnumerable<SalesOrderHeader> items) : base(items) { }
        public SalesOrderHeaderList(DateTime startDate, DateTime endDate)
        {
            this.startDate = startDate;
            this.endDate = endDate;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            string sql = "SELECT * FROM sales_Order_Header (NOLOCK) WHERE SoDate BETWEEN @StartDate AND @EndDate ORDER BY SoDate DESC ";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCom, "@StartDate", DbType.String, startDate.ToShortDateString());
            db.AddInParameter(dbCom, "@EndDate", DbType.String, endDate.ToShortDateString());
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new SalesOrderHeader
                    {
                        SoNo = (string)u["SoNo"],
                        SoType = (string)u["SoType"],
                        ShipTo = (string)u["ShipTo"],
                        BillTo = (string)u["BillTo"],
                        SoDate = (DateTime)u["SoDate"],
                        ReqDlvyDate = (DateTime)u["ReqDlvyDate"],
                        Currency = (string)u["Currency"],
                        ExchangeRate = (decimal)u["ExchangeRate"],
                        NetAmt = (decimal)u["NetAmt"],
                        NetAmtLocal = (decimal)u["NetAmtLocal"],
                        VATFlag = (string)u["VATFlag"],
                        VATRate = (decimal)u["VATRate"],
                        VATAmt = (decimal)u["VATAmt"],
                        VATAmtLocal = (decimal)u["VATAmtLocal"],
                        Memo = u["Memo"].ToString(),
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"]
                    }
                )
            );
        }
    }
}
