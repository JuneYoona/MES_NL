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
    public class SalesOrderDetail : StateBusinessObject
    {
        public bool IsChecked
        {
            get { return GetProperty(() => IsChecked); }
            set { SetProperty(() => IsChecked, value); }
        }
        public string SoNo
        {
            get { return GetProperty(() => SoNo); }
            set { SetProperty(() => SoNo, value); }
        }
        public int Seq
        {
            get { return GetProperty(() => Seq); }
            set { SetProperty(() => Seq, value); }
        }
        public DateTime SoDate
        {
            get { return GetProperty(() => SoDate); }
            set { SetProperty(() => SoDate, value); }
        }
        public string SoType
        {
            get { return GetProperty(() => SoType); }
            set { SetProperty(() => SoType, value); }
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
        public decimal ExchangeRate
        {
            get { return GetProperty(() => ExchangeRate); }
            set { SetProperty(() => ExchangeRate, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public string WhCode
        {
            get { return GetProperty(() => WhCode); }
            set { SetProperty(() => WhCode, value); }
        }
        public string CloseFlag
        {
            get { return GetProperty(() => CloseFlag); }
            set { SetProperty(() => CloseFlag, value); }
        }
        public string ShipTo
        {
            get { return GetProperty(() => ShipTo); }
            set { SetProperty(() => ShipTo, value); }
        }
        public string CustItemCode
        {
            get { return GetProperty(() => CustItemCode); }
            set { SetProperty(() => CustItemCode, value); }
        }
        public DateTime? ReqDlvyDate
        {
            get { return GetProperty(() => ReqDlvyDate); }
            set { SetProperty(() => ReqDlvyDate, value); }
        }
        [Range(0.000001, double.MaxValue, ErrorMessage = "0 보다 커야합니다.")]
        public decimal Qty
        {
            get { return GetProperty(() => Qty); }
            set { SetProperty(() => Qty, value); }
        }
        public decimal ReqQty
        {
            get { return GetProperty(() => ReqQty); }
            set { SetProperty(() => ReqQty, value); }
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

    public class SalesOrderDetailList : ObservableCollection<SalesOrderDetail>
    {
        private string soNo;
        private DateTime? startDate;
        private DateTime? endDate;

        public SalesOrderDetailList() { }
        public SalesOrderDetailList(IEnumerable<SalesOrderDetail> items) : base(items) { }
        public SalesOrderDetailList(string soNo = "", DateTime? startDate = null, DateTime? endDate = null)
        {
            this.soNo = soNo;
            this.startDate = startDate;
            this.endDate = endDate;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            string sql = "";
            if (!string.IsNullOrEmpty(soNo))
                sql = "SELECT A.*, B.ItemName, B.ItemSpec, C.SoDate, C.Currency, C.SoType FROM sales_Order_Detail (NOLOCK) A "
                    + "INNER JOIN common_Item (NOLOCK) B ON A.ItemCode = B.ItemCode "
                    + "INNER JOIN sales_Order_Header (NOLOCK) C ON A.SoNo = C.SoNo "
                    + "WHERE A.SoNo = @SoNo";
            else
                sql = "SELECT A.*, B.ItemName, B.ItemSpec, C.SoDate, C.Currency, C.SoType FROM sales_Order_Detail (NOLOCK) A "
                    + "INNER JOIN common_Item (NOLOCK) B ON A.ItemCode = B.ItemCode "
                    + "INNER JOIN sales_Order_Header (NOLOCK) C ON A.SoNo = C.SoNo "
                    + "WHERE C.SoDate BETWEEN @StartDate AND @EndDate "
                    + "ORDER BY C.SoDate DESC";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCom, "@SoNo", DbType.String, soNo);
            db.AddInParameter(dbCom, "@StartDate", DbType.String, startDate == null ? "" : startDate.Value.ToShortDateString());
            db.AddInParameter(dbCom, "@EndDate", DbType.String, endDate == null ? "" : endDate.Value.ToShortDateString());
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new SalesOrderDetail
                    {
                        SoNo = (string)u["SoNo"],
                        Seq = (int)u["Seq"],
                        SoType = (string)u["SoType"],
                        SoDate = (DateTime)u["SoDate"],
                        ItemCode = (string)u["ItemCode"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = (string)u["ItemSpec"],
                        BasicUnit = (string)u["BasicUnit"],
                        ExchangeRate = (decimal)u["ExchangeRate"],
                        Currency = (string)u["Currency"],
                        WhCode = (string)u["WhCode"],
                        CloseFlag = (string)u["CloseFlag"],
                        ShipTo = (string)u["ShipTo"],
                        CustItemCode = (string)u["CustItemCode"],
                        ReqDlvyDate = (DateTime)u["ReqDlvyDate"],
                        Qty = (decimal)u["Qty"],
                        ReqQty = (decimal)u["ReqQty"],
                        UnitPrice = (decimal)u["UnitPrice"],
                        NetAmt = (decimal)u["NetAmt"],
                        NetAmtLocal = (decimal)u["NetAmtLocal"],
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

        public void Save()
        {
            IEnumerable<SalesOrderDetail> items = this.Items;
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

        public void Insert(IEnumerable<SalesOrderDetail> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            foreach (SalesOrderDetail item in items)
            {
                dbCom = db.GetStoredProcCommand("usp_sales_Order_Detail");
                dbCom.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(dbCom, "@SoNo", DbType.String, item.SoNo);
                db.AddInParameter(dbCom, "@ItemCode", DbType.String, item.ItemCode);
                db.AddInParameter(dbCom, "@BasicUnit", DbType.String, item.BasicUnit);
                db.AddInParameter(dbCom, "@ExchangeRate", DbType.Decimal, item.ExchangeRate);
                db.AddInParameter(dbCom, "@WhCode", DbType.String, item.WhCode);
                db.AddInParameter(dbCom, "@ShipTo", DbType.String, item.ShipTo);
                db.AddInParameter(dbCom, "@ReqDlvyDate", DbType.Date, item.ReqDlvyDate);
                db.AddInParameter(dbCom, "@Qty", DbType.Decimal, item.Qty);
                db.AddInParameter(dbCom, "@UnitPrice", DbType.Decimal, item.UnitPrice);
                db.AddInParameter(dbCom, "@VATRate", DbType.Decimal, item.VATRate);
                db.AddInParameter(dbCom, "@Memo", DbType.String, item.Memo);
                db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }

        public void Delete(IEnumerable<SalesOrderDetail> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            string str;
            foreach (SalesOrderDetail item in items)
            {
                str = string.Format("DELETE sales_Order_Detail WHERE SoNo = '{0}' AND Seq = {1}", item.SoNo, item.Seq);
                dbCom = db.GetSqlStringCommand(str);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }

        public void Close()
        {
            IEnumerable<SalesOrderDetail> items = this.Items.Where(u => u.IsChecked == true);
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                string str;
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    foreach (SalesOrderDetail detail in items)
                    {
                        str = string.Format("UPDATE sales_Order_Detail SET CloseFlag = 'Y' WHERE SoNo = '{0}' AND Seq = {1}", detail.SoNo, detail.Seq);
                        dbCom = db.GetSqlStringCommand(str);
                        db.ExecuteNonQuery(dbCom, trans);
                    }
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
}
