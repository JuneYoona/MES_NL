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
    public class SalesOrderDlvyDetail : StateBusinessObject
    {
        public string DnNo
        {
            get { return GetProperty(() => DnNo); }
            set { SetProperty(() => DnNo, value); }
        }
        public int Seq
        {
            get { return GetProperty(() => Seq); }
            set { SetProperty(() => Seq, value); }
        }
        public string BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value); }
        }
        public DateTime ReqDate
        {
            get { return GetProperty(() => ReqDate); }
            set { SetProperty(() => ReqDate, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
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
        public string WaCode
        {
            get { return GetProperty(() => WaCode); }
            set { SetProperty(() => WaCode, value); }
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
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public string DnLotNo
        {
            get { return GetProperty(() => DnLotNo); }
            set { SetProperty(() => DnLotNo, value); }
        }
        public string LotNo
        {
            get { return GetProperty(() => LotNo); }
            set { SetProperty(() => LotNo, value); }
        }
        public decimal Qty
        {
            get { return GetProperty(() => Qty); }
            set { SetProperty(() => Qty, value); }
        }
        public decimal AvailableQty
        {
            get { return GetProperty(() => AvailableQty); }
            set { SetProperty(() => AvailableQty, value); }
        }
        public decimal UnitPrice
        {
            get { return GetProperty(() => UnitPrice); }
            set { SetProperty(() => UnitPrice, value); }
        }
        public string ReqNo
        {
            get { return GetProperty(() => ReqNo); }
            set { SetProperty(() => ReqNo, value); }
        }
        public int ReqSeq
        {
            get { return GetProperty(() => ReqSeq); }
            set { SetProperty(() => ReqSeq, value); }
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
    }

    public class SalesOrderDlvyDetailList : ObservableCollection<SalesOrderDlvyDetail>
    {
        private string dnNo;
        private DateTime? startDate;
        private DateTime? endDate;

        public SalesOrderDlvyDetailList() { }
        public SalesOrderDlvyDetailList(IEnumerable<SalesOrderDlvyDetail> items) : base(items) { }
        public SalesOrderDlvyDetailList(string dnNo = "", DateTime? startDate = null, DateTime? endDate = null)
        {
            this.dnNo = dnNo;
            this.startDate = startDate;
            this.endDate = endDate;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            string sql = "";
            if (!string.IsNullOrEmpty(dnNo))
                sql = "SELECT A.*, B.ItemName, B.ItemSpec, C.ReqDate, C.ShipTo, C.SoType, C.MoveType FROM sales_OrderDlvy_Detail (NOLOCK) A "
                    + "INNER JOIN common_Item (NOLOCK) B ON A.ItemCode = B.ItemCode "
                    + "INNER JOIN sales_OrderDlvy_Header (NOLOCK) C ON A.DnNo = C.DnNo "
                    + "WHERE A.DnNo = @DnNo";
            else
                sql = "SELECT A.*, B.ItemName, B.ItemSpec, C.ReqDate, C.ShipTo, C.SoType, C.MoveType FROM sales_OrderDlvy_Detail (NOLOCK) A "
                    + "INNER JOIN common_Item (NOLOCK) B ON A.ItemCode = B.ItemCode "
                    + "INNER JOIN sales_OrderDlvy_Header (NOLOCK) C ON A.DnNo = C.DnNo "
                    + "WHERE C.ReqDate BETWEEN @StartDate AND @EndDate "
                    + "ORDER BY C.ReqDate DESC";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCom, "@DnNo", DbType.String, dnNo);
            db.AddInParameter(dbCom, "@StartDate", DbType.String, startDate == null ? "" : startDate.Value.ToShortDateString());
            db.AddInParameter(dbCom, "@EndDate", DbType.String, endDate == null ? "" : endDate.Value.ToShortDateString());
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new SalesOrderDlvyDetail
                    {
                        DnNo = (string)u["DnNo"],
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
                        DnLotNo = (string)u["DnLotNo"],
                        LotNo = (string)u["LotNo"],
                        Qty = (decimal)u["Qty"],
                        UnitPrice = (decimal)u["UnitPrice"],
                        ExchangeRate = (decimal)u["ExchangeRate"],
                        NetAmt = (decimal)u["NetAmt"],
                        NetAmtLocal = (decimal)u["NetAmtLocal"],
                        VATRate = (decimal)u["VATRate"],
                        VATAmt = (decimal)u["VATAmt"],
                        VATAmtLocal = (decimal)u["VATAmtLocal"],
                        ReqNo = (string)u["ReqNo"],
                        ReqSeq = (int)u["ReqSeq"],
                        SoNo = (string)u["SoNo"],
                        SoSeq = (int)u["SoSeq"],
                        Memo = u["Memo"].ToString(),
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"]
                    }
                )
            );
        }

        public void Save()
        {
            IEnumerable<SalesOrderDlvyDetail> items = this.Items;
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

        public void Insert(IEnumerable<SalesOrderDlvyDetail> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            foreach (SalesOrderDlvyDetail item in items)
            {
                dbCom = db.GetStoredProcCommand("usp_sales_OrderDlvy_Detail");
                dbCom.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(dbCom, "@DnNo", DbType.String, item.DnNo);
                db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, item.BizAreaCode);
                db.AddInParameter(dbCom, "@ItemCode", DbType.String, item.ItemCode);
                db.AddInParameter(dbCom, "@BasicUnit", DbType.String, item.BasicUnit);
                db.AddInParameter(dbCom, "@CustItemCode", DbType.String, item.CustItemCode);
                db.AddInParameter(dbCom, "@WhCode", DbType.String, item.WhCode);
                db.AddInParameter(dbCom, "@WaCode", DbType.String, item.WaCode);
                db.AddInParameter(dbCom, "@DnLotNo", DbType.String, item.DnLotNo);
                db.AddInParameter(dbCom, "@LotNo", DbType.String, item.LotNo);
                db.AddInParameter(dbCom, "@Qty", DbType.Decimal, item.Qty);
                db.AddInParameter(dbCom, "@UnitPrice", DbType.Decimal, item.UnitPrice);
                db.AddInParameter(dbCom, "@ExchangeRate", DbType.Decimal, item.ExchangeRate);
                db.AddInParameter(dbCom, "@VATRate", DbType.Decimal, item.VATRate);
                db.AddInParameter(dbCom, "@ReqNo", DbType.String, item.ReqNo);
                db.AddInParameter(dbCom, "@ReqSeq", DbType.Int16, item.ReqSeq);
                db.AddInParameter(dbCom, "@SoNo", DbType.String, item.SoNo);
                db.AddInParameter(dbCom, "@SoSeq", DbType.Int16, item.SoSeq);
                db.AddInParameter(dbCom, "@Memo", DbType.String, item.Memo);
                db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                db.ExecuteNonQuery(dbCom, trans);
            }

            // 선입선출 check
            if (DSUser.Instance.BizAreaCode == "BAC15")
            {
                if (items.Count() == 0) return;
                SalesOrderDlvyDetail item = items.First();
                FIFO(item.DnNo, item.ItemCode);
            }
        }

        public void Delete(IEnumerable<SalesOrderDlvyDetail> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            string str;
            foreach (SalesOrderDlvyDetail item in items)
            {
                str = string.Format("DELETE sales_OrderDlvy_Detail WHERE DnNo = '{0}' AND Seq = {1}", item.DnNo, item.Seq);
                dbCom = db.GetSqlStringCommand(str);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }

        public void FIFO(string DnNo, string itemCode)
        {
            IEnumerable<SalesOrderDlvyDetail> items = this.Items;
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = null;

            try
            {
                dbCom = db.GetStoredProcCommand("usp_sales_OrderDlvy_FIFO");
                dbCom.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, DSUser.Instance.BizAreaCode);
                db.AddInParameter(dbCom, "@DnNo", DbType.String, DnNo);
                db.AddInParameter(dbCom, "@ItemCode", DbType.String, itemCode);
                db.ExecuteDataSet(dbCom);
            }
            catch { throw; }
        }
    }

    public class SalesOrderDlvyTable
    {
        private string reqNo;
        private int seq;
        public DataTable Collections { get; set; }

        public SalesOrderDlvyTable(string reqNo, int seq)
        {
            this.reqNo = reqNo;
            this.seq = seq;
            InitializeList();
        }

        public void InitializeList()
        {
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = null;
            string sql;

            sql = "SELECT * FROM [ERPSERVER].[DSNL].dbo.S_DN_DTL WHERE DN_REQ_NO = @ReqNo AND DN_REQ_SEQ = @ReqSeq";
            dbCom = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCom, "@ReqNo", DbType.String, reqNo);
            db.AddInParameter(dbCom, "@ReqSeq", DbType.Int16, seq);
            DataSet ds = db.ExecuteDataSet(dbCom);

            if (ds.Tables.Count > 0)
                Collections = ds.Tables[0];
        }
    }
}
