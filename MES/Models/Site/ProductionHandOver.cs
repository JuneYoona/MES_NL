using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using MesAdmin.Common.Common;
using System.Linq;

namespace MesAdmin.Models
{
    public class ProductionHandOver : StateBusinessObject
    {
        public string HoNo
        {
            get { return GetProperty(() => HoNo); }
            set { SetProperty(() => HoNo, value); }
        }
        public int Seq
        {
            get { return GetProperty(() => Seq); }
            set { SetProperty(() => Seq, value); }
        }
        public string ProductOrderNo
        {
            get { return GetProperty(() => ProductOrderNo); }
            set { SetProperty(() => ProductOrderNo, value); }
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
        public string BasicUnit
        {
            get { return GetProperty(() => BasicUnit); }
            set { SetProperty(() => BasicUnit, value); }
        }
        public string QrState
        {
            get { return GetProperty(() => QrState); }
            set { SetProperty(() => QrState, value); }
        }
        public string BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value); }
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
        public DateTime OutDate
        {
            get { return GetProperty(() => OutDate); }
            set { SetProperty(() => OutDate, value); }
        }
        public string InsertId
        {
            get { return GetProperty(() => InsertId); }
            set { SetProperty(() => InsertId, value); }
        }
        public string InsertName
        {
            get { return GetProperty(() => InsertName); }
            set { SetProperty(() => InsertName, value); }
        }
        public string Memo
        {
            get { return GetProperty(() => Memo); }
            set { SetProperty(() => Memo, value); }
        }
        public DateTime InsertDate
        {
            get { return GetProperty(() => InsertDate); }
            set { SetProperty(() => InsertDate, value); }
        }
    }

    public class ProductionHandOverList : ObservableCollection<ProductionHandOver>
    {
        private string hoNo;
        private DateTime? startDate;
        private DateTime? endDate;

        public ProductionHandOverList() { }
        public ProductionHandOverList(IEnumerable<ProductionHandOver> items) : base(items) { }
        public ProductionHandOverList(string hoNo = "", DateTime? startDate = null, DateTime? endDate = null)
        {
            this.hoNo = hoNo;
            this.startDate = startDate;
            this.endDate = endDate;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            string sql;

            sql = "SELECT A.*, B.ItemName, B.ItemSpec, B.BasicUnit ";
            sql += ", QrState = CASE WHEN TransferFlag is null THEN '검사요청 누락' WHEN TransferFlag = 1 THEN '완료' ELSE '대기' END ";
            sql += "FROM production_HandOver A (NOLOCK) ";
            sql += "INNER JOIN common_Item B (NOLOCK) ON A.ItemCode=B.ItemCode ";
            sql += "LEFT JOIN quality_Request (NOLOCK) C ON A.ProductOrderNo = C.ProductOrderNo ";
            sql += "WHERE HoNo = HoNo ";
            if (!string.IsNullOrEmpty(hoNo))
                sql += "And HoNo = '" + hoNo + "' ";
            if (startDate != null && endDate != null)
                sql += "And OutDate BETWEEN '" + startDate.ToString().Substring(0, 10) + "' AND '" + endDate.ToString().Substring(0, 10) + "' ";
            sql += "ORDER BY OutDate DESC";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new ProductionHandOver
                    {
                        HoNo = (string)u["HoNo"],
                        Seq = (int)u["Seq"],
                        ProductOrderNo = (string)u["ProductOrderNo"],
                        ItemCode = (string)u["ItemCode"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = (string)u["ItemSpec"],
                        LotNo = (string)u["LotNo"],
                        Qty = (decimal)u["Qty"],
                        BasicUnit = (string)u["BasicUnit"],
                        OutDate = (DateTime)u["OutDate"],
                        InsertId = (string)u["InsertId"],
                        InsertName = (string)u["InsertName"],
                        InsertDate = (DateTime)u["InsertDate"],
                        Memo = u["Memo"].ToString(),
                        QrState = u["QrState"].ToString(),
                    }
                )
            );
        }

        public string Save()
        {
            string hoNo;

            IEnumerable<ProductionHandOver> items = this.Items;
            hoNo = Insert(items.Where(u => u.State == EntityState.Added));
            Delete(items.Where(u => u.State == EntityState.Deleted));

            return hoNo;
        }

        public string Insert(IEnumerable<ProductionHandOver> items)
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    string hoNo = string.Empty;
                    foreach (ProductionHandOver item in items)
                    {
                        dbCom = db.GetStoredProcCommand("usps_production_HandOver");
                        dbCom.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(dbCom, "@Op", DbType.String, "C");
                        db.AddInParameter(dbCom, "@HoNo", DbType.String, hoNo);
                        db.AddInParameter(dbCom, "@ProductOrderNo", DbType.String, item.ProductOrderNo);
                        db.AddInParameter(dbCom, "@ItemCode", DbType.String, item.ItemCode);
                        db.AddInParameter(dbCom, "@LotNo", DbType.String, item.LotNo);
                        db.AddInParameter(dbCom, "@Qty", DbType.Decimal, item.Qty);
                        db.AddInParameter(dbCom, "@OutDate", DbType.Date, item.OutDate);
                        db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                        db.AddInParameter(dbCom, "@InsertName", DbType.String, DSUser.Instance.UserName);
                        db.AddInParameter(dbCom, "@Memo", DbType.String, item.Memo);
                        db.AddOutParameter(dbCom, "@ReturnNo", DbType.String, 20);
                        db.ExecuteNonQuery(dbCom, trans);
                        hoNo = db.GetParameterValue(dbCom, "@ReturnNo").ToString();
                    }
                    trans.Commit();
                    return hoNo;
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public void Delete(IEnumerable<ProductionHandOver> items)
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    foreach (ProductionHandOver item in items)
                    {
                        dbCom = db.GetStoredProcCommand("usps_production_HandOver");
                        dbCom.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(dbCom, "@Op", DbType.String, "D");
                        db.AddInParameter(dbCom, "@ProductOrderNo", DbType.String, item.ProductOrderNo);
                        db.AddInParameter(dbCom, "@ItemCode", DbType.String, item.ItemCode);
                        db.AddInParameter(dbCom, "@LotNo", DbType.String, item.LotNo);
                        db.AddInParameter(dbCom, "@Qty", DbType.Decimal, item.Qty);
                        db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                        db.AddOutParameter(dbCom, "@ReturnNo", DbType.String, 20);
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
