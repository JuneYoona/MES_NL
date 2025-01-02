using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using MesAdmin.Common.Common;

namespace MesAdmin.Models
{
    public class ProductionWorkOrderNL : StateBusinessObject
    {
        public string BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value); }
        }
        public string OrderNo
        {
            get { return GetProperty(() => OrderNo); }
            set { SetProperty(() => OrderNo, value); }
        }
        public DateTime OrderDate
        {
            get { return GetProperty(() => OrderDate); }
            set { SetProperty(() => OrderDate, value); }
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
        public string ItemCode
        {
            get { return GetProperty(() => ItemCode); }
            set { SetProperty(() => ItemCode, value); }
        }
        public decimal OrderQty
        {
            get { return GetProperty(() => OrderQty); }
            set { SetProperty(() => OrderQty, value); }
        }
        public string Remark
        {
            get { return GetProperty(() => Remark); }
            set { SetProperty(() => Remark, value); }
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
        public string ItemName
        {
            get { return GetProperty(() => ItemName); }
            set { SetProperty(() => ItemName, value); }
        }
        public string BasicUnit
        {
            get { return GetProperty(() => BasicUnit); }
            set { SetProperty(() => BasicUnit, value); }
        }
        public char IsEnd
        {
            get { return GetProperty(() => IsEnd); }
            set { SetProperty(() => IsEnd, value); }
        }
        public decimal? TSC
        {
            get { return GetProperty(() => TSC); }
            set { SetProperty(() => TSC, value); }
        }
        public string LotNo
        {
            get { return GetProperty(() => LotNo); }
            set { SetProperty(() => LotNo, value); }
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

        public ProductionWorkOrderNL() { }
        public ProductionWorkOrderNL(string bizAreaCode, string orderNo)
        {
            Database db = ProviderFactory.Instance;

            DbCommand dbCom = db.GetSqlStringCommand("SELECT A.*, B.ItemName, B.BasicUnit FROM Production_WorkOrder A (NOLOCK) INNER JOIN common_Item B (NOLOCK) ON A.ItemCode = B.ItemCode WHERE BizAreaCode = @BizAreaCode AND OrderNo = @OrderNo");
            db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, bizAreaCode);
            db.AddInParameter(dbCom, "@OrderNo", DbType.String, orderNo);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
            {
                BizAreaCode = (string)u["BizAreaCode"];
                OrderNo = (string)u["OrderNo"];
                OrderDate = (DateTime)u["OrderDate"];
                WaCode = (string)u["WaCode"];
                EqpCode = (string)u["EqpCode"];
                ItemCode = (string)u["ItemCode"];
                ItemName = (string)u["ItemName"];
                BasicUnit = (string)u["BasicUnit"];
                OrderQty = (decimal)u["OrderQty"];
                Remark = u["Remark"].ToString();
                Remark2 = u["Remark2"].ToString();
                Remark3 = u["Remark3"].ToString();
                IsEnd = Convert.ToChar(u["IsEnd"]);
                TSC = u["TSC"] == DBNull.Value ? (decimal?)null : (decimal)u["TSC"];
                LotNo = u["LotNo"].ToString();
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
                try
                {
                    dbCom = db.GetStoredProcCommand("usp_Production_WorkOrder");
                    dbCom.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(dbCom, "@State", DbType.String, EntityState.Added);
                    db.AddInParameter(dbCom, "@OrderNo", DbType.String, OrderNo);
                    db.ExecuteNonQuery(dbCom);
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public void Save(string bizAreaCode)
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    dbCom = db.GetStoredProcCommand("usp_Production_WorkOrder");
                    dbCom.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(dbCom, "@State", DbType.String, EntityState.Added);
                    db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, bizAreaCode);
                    db.AddInParameter(dbCom, "@OrderDate", DbType.Date, OrderDate);
                    db.AddInParameter(dbCom, "@WaCode", DbType.String, WaCode);
                    db.AddInParameter(dbCom, "@EqpCode", DbType.String, EqpCode);
                    db.AddInParameter(dbCom, "@ItemCode", DbType.String, ItemCode);
                    db.AddInParameter(dbCom, "@OrderQty", DbType.Decimal, OrderQty);
                    db.AddInParameter(dbCom, "@Remark", DbType.String, Remark);
                    db.AddInParameter(dbCom, "@Remark2", DbType.String, Remark2 != null ? Remark2.Trim().ToUpper() : "");
                    db.AddInParameter(dbCom, "@Remark3", DbType.String, Remark3 != null ? Remark3.Trim() : "");
                    db.AddInParameter(dbCom, "@TSC", DbType.Decimal, TSC);
                    db.AddInParameter(dbCom, "@LotNo", DbType.String, LotNo);
                    db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                    db.AddOutParameter(dbCom, "@RetOrderNo", DbType.String, 20);
                    db.ExecuteNonQuery(dbCom, trans);
                    OrderNo = db.GetParameterValue(dbCom, "@RetOrderNo").ToString();
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public void CreateLotNo()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    dbCom = db.GetStoredProcCommand("usp_getProductionLotNoWE10");
                    dbCom.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(dbCom, "@OrderDate", DbType.Date, OrderDate);
                    db.AddInParameter(dbCom, "@EqpCode", DbType.String, EqpCode);
                    db.AddInParameter(dbCom, "@ItemCode", DbType.String, ItemCode);
                    db.AddOutParameter(dbCom, "@LotNo", DbType.String, 50);
                    db.ExecuteNonQuery(dbCom, trans);
                    Remark2 = db.GetParameterValue(dbCom, "@LotNo").ToString();
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public void CreateLotNoWB20(string itemCode)
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    dbCom = db.GetStoredProcCommand("usps_getProductionLotNoWB20");
                    dbCom.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(dbCom, "@OrderDate", DbType.Date, OrderDate);
                    db.AddInParameter(dbCom, "@ItemCode", DbType.String, itemCode);
                    db.AddOutParameter(dbCom, "@LotNo", DbType.String, 50);
                    db.ExecuteNonQuery(dbCom, trans);
                    LotNo = db.GetParameterValue(dbCom, "@LotNo").ToString();
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
