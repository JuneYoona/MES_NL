using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using MesAdmin.Common.Common;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace MesAdmin.Models
{
    public class SalesOrderReqDetail : StateBusinessObject
    {
        public string ReqNo
        {
            get { return GetProperty(() => ReqNo); }
            set { SetProperty(() => ReqNo, value); }
        }
        public int Seq
        {
            get { return GetProperty(() => Seq); }
            set { SetProperty(() => Seq, value); }
        }
        public DateTime ReqDate
        {
            get { return GetProperty(() => ReqDate); }
            set { SetProperty(() => ReqDate, value); }
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
        public string WhCode
        {
            get { return GetProperty(() => WhCode); }
            set { SetProperty(() => WhCode, value); }
        }
        public string ShipTo
        {
            get { return GetProperty(() => ShipTo); }
            set { SetProperty(() => ShipTo, value); }
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
        public string CustItemCode
        {
            get { return GetProperty(() => CustItemCode); }
            set { SetProperty(() => CustItemCode, value); }
        }
        public DateTime? DlvyDate
        {
            get { return GetProperty(() => DlvyDate); }
            set { SetProperty(() => DlvyDate, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public decimal Qty
        {
            get { return GetProperty(() => Qty); }
            set { SetProperty(() => Qty, value); }
        }
        public decimal DlvyQty
        {
            get { return GetProperty(() => DlvyQty); }
            set { SetProperty(() => DlvyQty, value); }
        }
        public decimal UnitPrice
        {
            get { return GetProperty(() => UnitPrice); }
            set { SetProperty(() => UnitPrice, value); }
        }
        public string Currency
        {
            get { return GetProperty(() => Currency); }
            set { SetProperty(() => Currency, value); }
        }
        public string SoNo
        {
            get { return GetProperty(() => SoNo); }
            set { SetProperty(() => SoNo, value); }
        }
        public int SoSeq
        {
            get { return GetProperty(() => SoSeq); }
            set { SetProperty(() => SoSeq, value); }
        }
        public decimal ExchangeRate
        {
            get { return GetProperty(() => ExchangeRate); }
            set { SetProperty(() => ExchangeRate, value); }
        }
        public decimal NetAmt
        {
            get { return GetProperty(() => NetAmt); }
            set { SetProperty(() => NetAmt, value); }
        }
        public decimal NetAmtLocal
        {
            get { return GetProperty(() => NetAmtLocal); }
            set { SetProperty(() => NetAmtLocal, value); }
        }
        public decimal VATRate
        {
            get { return GetProperty(() => VATRate); }
            set { SetProperty(() => VATRate, value); }
        }
        public decimal VATAmt
        {
            get { return GetProperty(() => VATAmt); }
            set { SetProperty(() => VATAmt, value); }
        }
        public decimal VATAmtLocal
        {
            get { return GetProperty(() => VATAmtLocal); }
            set { SetProperty(() => VATAmtLocal, value); }
        }
        public string Memo
        {
            get { return GetProperty(() => Memo); }
            set { SetProperty(() => Memo, value); }
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
        public DateTime UpdateDate
        {
            get { return GetProperty(() => UpdateDate); }
            set { SetProperty(() => UpdateDate, value); }
        }

        public SalesOrderReqDetail() { }
        public SalesOrderReqDetail(string reqNo, int seq)
        {
            Database db = ProviderFactory.Instance;
            string sql = "";
            sql = "SELECT A.*, B.ItemName, B.ItemSpec, C.ReqDate, C.ShipTo, C.SoType, C.MoveType FROM sales_OrderReq_Detail (NOLOCK) A "
                    + "INNER JOIN common_Item (NOLOCK) B ON A.ItemCode = B.ItemCode "
                    + "INNER JOIN sales_OrderReq_Header (NOLOCK) C ON A.ReqNo = C.ReqNo "
                    + "WHERE A.ReqNo = @ReqNo AND A.Seq = @Seq";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCom, "@ReqNo", DbType.String, reqNo);
            db.AddInParameter(dbCom, "@Seq", DbType.Int16, seq);
            DataSet ds = db.ExecuteDataSet(dbCom);
            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
            {
                ReqNo = (string)u["ReqNo"];
                Seq = (int)u["Seq"];
                ReqDate = (DateTime)u["ReqDate"];
                ItemCode = (string)u["ItemCode"];
                ItemName = (string)u["ItemName"];
                ItemSpec = (string)u["ItemSpec"];
                BasicUnit = (string)u["BasicUnit"];
                SoType = (string)u["SoType"];
                MoveType = (string)u["MoveType"];
                WhCode = (string)u["WhCode"];
                ShipTo = (string)u["ShipTo"];
                CustItemCode = (string)u["CustItemCode"];
                Qty = (decimal)u["Qty"];
                DlvyQty = (decimal)u["DlvyQty"];
                UnitPrice = (decimal)u["UnitPrice"];
                ExchangeRate = (decimal)u["ExchangeRate"];
                NetAmt = (decimal)u["NetAmt"];
                NetAmtLocal = (decimal)u["NetAmtLocal"];
                VATRate = (decimal)u["VATRate"];
                VATAmt = (decimal)u["VATAmt"];
                VATAmtLocal = (decimal)u["VATAmtLocal"];
                SoNo = (string)u["SoNo"];
                SoSeq = (int)u["SoSeq"];
                Memo = u["Memo"].ToString();
                UpdateId = (string)u["UpdateId"];
                UpdateDate = (DateTime)u["UpdateDate"];
            });
        }
    }

    public class SalesOrderReqDetailList : ObservableCollection<SalesOrderReqDetail>
    {
        private string reqNo;
        private DateTime? startDate;
        private DateTime? endDate;

        public SalesOrderReqDetailList() { }
        public SalesOrderReqDetailList(IEnumerable<SalesOrderReqDetail> items) : base(items) { }
        public SalesOrderReqDetailList(string reqNo = "", DateTime? startDate = null, DateTime? endDate = null)
        {
            this.reqNo = reqNo;
            this.startDate = startDate;
            this.endDate = endDate;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            string sql = "";
            if (!string.IsNullOrEmpty(reqNo))
                sql = "SELECT A.*, B.ItemName, B.ItemSpec, C.ReqDate, C.ShipTo, C.SoType, C.MoveType, C.Remark1 FROM sales_OrderReq_Detail (NOLOCK) A "
                    + "INNER JOIN common_Item (NOLOCK) B ON A.ItemCode = B.ItemCode "
                    + "INNER JOIN sales_OrderReq_Header (NOLOCK) C ON A.ReqNo = C.ReqNo "
                    + "WHERE A.ReqNo = @ReqNo";
            else
                sql = "SELECT A.*, B.ItemName, B.ItemSpec, C.ReqDate, C.ShipTo, C.SoType, C.MoveType, C.Remark1 FROM sales_OrderReq_Detail (NOLOCK) A "
                    + "INNER JOIN common_Item (NOLOCK) B ON A.ItemCode = B.ItemCode "
                    + "INNER JOIN sales_OrderReq_Header (NOLOCK) C ON A.ReqNo = C.ReqNo "
                    + "WHERE C.ReqDate BETWEEN @StartDate AND @EndDate "
                    + "ORDER BY C.ReqDate DESC";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCom, "@ReqNo", DbType.String, reqNo);
            db.AddInParameter(dbCom, "@StartDate", DbType.String, startDate == null ? "" : startDate.Value.ToShortDateString());
            db.AddInParameter(dbCom, "@EndDate", DbType.String, endDate == null ? "" : endDate.Value.ToShortDateString());
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new SalesOrderReqDetail
                    {
                        ReqNo = (string)u["ReqNo"],
                        Seq = (int)u["Seq"],
                        ReqDate = (DateTime)u["ReqDate"],
                        ItemCode = (string)u["ItemCode"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = (string)u["ItemSpec"],
                        BasicUnit = (string)u["BasicUnit"],
                        SoType = (string)u["SoType"],
                        MoveType = (string)u["MoveType"],
                        WhCode = (string)u["WhCode"],
                        ShipTo = (string)u["ShipTo"],
                        CustItemCode = (string)u["CustItemCode"],
                        Qty = (decimal)u["Qty"],
                        DlvyQty = (decimal)u["DlvyQty"],
                        UnitPrice = (decimal)u["UnitPrice"],
                        ExchangeRate = (decimal)u["ExchangeRate"],
                        NetAmt = (decimal)u["NetAmt"],
                        NetAmtLocal = (decimal)u["NetAmtLocal"],
                        VATRate = (decimal)u["VATRate"],
                        VATAmt = (decimal)u["VATAmt"],
                        VATAmtLocal = (decimal)u["VATAmtLocal"],
                        SoNo = (string)u["SoNo"],
                        SoSeq = (int)u["SoSeq"],
                        Memo = u["Memo"].ToString(),
                        Remark1 = u["Remark1"].ToString(),
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"]
                    }
                )
            );
        }

        public void Save()
        {
            IEnumerable<SalesOrderReqDetail> items = this.Items;
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = null;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                try
                {
                    Insert(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Added), db, trans, dbCom);
                    Delete(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Deleted), db, trans, dbCom);
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public void Insert(IEnumerable<SalesOrderReqDetail> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            foreach (SalesOrderReqDetail item in items)
            {
                dbCom = db.GetStoredProcCommand("usp_sales_OrderReq_Detail");
                dbCom.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(dbCom, "@ReqNo", DbType.String, item.ReqNo);
                db.AddInParameter(dbCom, "@ItemCode", DbType.String, item.ItemCode);
                db.AddInParameter(dbCom, "@BasicUnit", DbType.String, item.BasicUnit);
                db.AddInParameter(dbCom, "@CustItemCode", DbType.String, item.CustItemCode);
                db.AddInParameter(dbCom, "@WhCode", DbType.String, item.WhCode);
                db.AddInParameter(dbCom, "@Qty", DbType.Decimal, item.Qty);
                db.AddInParameter(dbCom, "@UnitPrice", DbType.Decimal, item.UnitPrice);
                db.AddInParameter(dbCom, "@ExchangeRate", DbType.Decimal, item.ExchangeRate);
                db.AddInParameter(dbCom, "@VATRate", DbType.Decimal, item.VATRate);
                db.AddInParameter(dbCom, "@SoNo", DbType.String, item.SoNo);
                db.AddInParameter(dbCom, "@SoSeq", DbType.Int16, item.SoSeq);
                db.AddInParameter(dbCom, "@Memo", DbType.String, item.Memo);
                db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }

        public void Delete(IEnumerable<SalesOrderReqDetail> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            string str;
            foreach (SalesOrderReqDetail item in items)
            {
                str = string.Format("DELETE sales_OrderReq_Detail WHERE ReqNo = '{0}' AND Seq = {1}", item.ReqNo, item.Seq);
                dbCom = db.GetSqlStringCommand(str);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }
    }
}
