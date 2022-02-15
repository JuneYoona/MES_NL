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
    public class ProductionWorkOrderDetail : StateBusinessObject
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
        public int Seq
        {
            get { return GetProperty(() => Seq); }
            set { SetProperty(() => Seq, value); }
        }
        public string WaCode
        {
            get { return GetProperty(() => WaCode); }
            set { SetProperty(() => WaCode, value); }
        }
        public string WhCode
        {
            get { return GetProperty(() => WhCode); }
            set { SetProperty(() => WhCode, value); }
        }
        public string ItemCode
        {
            get { return GetProperty(() => ItemCode); }
            set { SetProperty(() => ItemCode, value); }
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
        public string InsertId
        {
            get { return GetProperty(() => InsertId); }
            set { SetProperty(() => InsertId, value); }
        }
        public string TSC
        {
            get { return GetProperty(() => TSC); }
            set { SetProperty(() => TSC, value); }
        }
        public string PIG
        {
            get { return GetProperty(() => PIG); }
            set { SetProperty(() => PIG, value); }
        }
        public string AC
        {
            get { return GetProperty(() => AC); }
            set { SetProperty(() => AC, value); }
        }
        public string CAL
        {
            get { return GetProperty(() => CAL); }
            set { SetProperty(() => CAL, value); }
        }
        public string Solution
        {
            get { return GetProperty(() => Solution); }
            set { SetProperty(() => Solution, value); }
        }
        public DateTime? ExpDate
        {
            get { return GetProperty(() => ExpDate); }
            set { SetProperty(() => ExpDate, value); }
        }
        public DateTime InsertDate
        {
            get { return GetProperty(() => InsertDate); }
            set { SetProperty(() => InsertDate, value); }
        }
    }

    public class ProductionWorkOrderDetailList : ObservableCollection<ProductionWorkOrderDetail>
    {
        private string orderNo;

        public ProductionWorkOrderDetailList() { }
        public ProductionWorkOrderDetailList(string orderNo)
        {
            this.orderNo = orderNo;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            string sql = "SELECT A.*, B.ItemName, B.ItemSpec, B.BasicUnit FROM production_WorkOrder_Detail A (NOLOCK) INNER JOIN common_Item B (NOLOCK) ON A.ItemCode = B.ItemCode WHERE OrderNo = @OrderNo";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCom, "@OrderNo", DbType.String, orderNo);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new ProductionWorkOrderDetail
                    {
                        BizAreaCode = (string)u["BizAreaCode"],
                        OrderNo = (string)u["OrderNo"],
                        Seq = (int)u["Seq"],
                        ItemCode = (string)u["ItemCode"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = (string)u["ItemSpec"],
                        BasicUnit = (string)u["BasicUnit"],
                        WhCode = (string)u["WhCode"],
                        WaCode = (string)u["WaCode"],
                        LotNo = (string)u["LotNo"],
                        Qty = (decimal)u["Qty"],
                        TSC = u["TSC"].ToString(),
                        PIG = u["PIG"].ToString(),
                        AC = u["AC"].ToString(),
                        CAL = u["CAL"].ToString(),
                        Solution = u["Solution"].ToString(),
                        ExpDate = u["ExpDate"] == DBNull.Value ? (DateTime?)null : (DateTime)u["ExpDate"],
                        InsertId = (string)u["InsertId"],
                        InsertDate = (DateTime)u["InsertDate"]
                    }
                )
            );
        }

        public void Save()
        {
            IEnumerable<ProductionWorkOrderDetail> items = this.Items;
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = null;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                try
                {
                    Insert(items.Where(u => u.State == EntityState.Added), db, trans, dbCom);
                    Delete(items.Where(u => u.State == EntityState.Deleted), db, trans, dbCom);
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public void Insert(IEnumerable<ProductionWorkOrderDetail> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            foreach (ProductionWorkOrderDetail item in items)
            {
                dbCom = db.GetStoredProcCommand("usps_Production_WorkOrder_Detail");
                dbCom.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(dbCom, "@State", DbType.String, EntityState.Added);
                db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, item.BizAreaCode);
                db.AddInParameter(dbCom, "@OrderNo", DbType.String, item.OrderNo);
                db.AddInParameter(dbCom, "@Seq", DbType.Int16, item.Seq);
                db.AddInParameter(dbCom, "@WhCode", DbType.String, item.WhCode);
                db.AddInParameter(dbCom, "@WaCode", DbType.String, item.WaCode);
                db.AddInParameter(dbCom, "@LotNo", DbType.String, item.LotNo);
                db.AddInParameter(dbCom, "@ItemCode", DbType.String, item.ItemCode);
                db.AddInParameter(dbCom, "@Qty", DbType.Decimal, item.Qty);
                db.AddInParameter(dbCom, "@TSC", DbType.String, item.TSC);
                db.AddInParameter(dbCom, "@PIG", DbType.String, item.PIG);
                db.AddInParameter(dbCom, "@AC", DbType.String, item.AC);
                db.AddInParameter(dbCom, "@CAL", DbType.String, item.CAL);
                db.AddInParameter(dbCom, "@Solution", DbType.String, item.Solution);
                db.AddInParameter(dbCom, "@ExpDate", DbType.Date, item.ExpDate);
                db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }

        public void Delete(IEnumerable<ProductionWorkOrderDetail> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            foreach (ProductionWorkOrderDetail item in items)
            {
                dbCom = db.GetStoredProcCommand("usps_Production_WorkOrder_Detail");
                dbCom.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(dbCom, "@State", DbType.String, EntityState.Deleted);
                db.AddInParameter(dbCom, "@OrderNo", DbType.String, item.OrderNo);
                db.AddInParameter(dbCom, "@Seq", DbType.Int16, item.Seq);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }
    }
}
