using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using MesAdmin.Common.Common;
using System.ComponentModel.DataAnnotations;

namespace MesAdmin.Models
{
    public class ProductionWorkOrder : StateBusinessObject
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
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public string WaCode
        {
            get { return GetProperty(() => WaCode); }
            set { SetProperty(() => WaCode, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public string EqpCode
        {
            get { return GetProperty(() => EqpCode); }
            set { SetProperty(() => EqpCode, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
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
        public char IsEnd
        {
            get { return GetProperty(() => IsEnd); }
            set { SetProperty(() => IsEnd, value); }
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

    public class ProductionWorkOrderList : ObservableCollection<ProductionWorkOrder>
    {
        private string bizAreaCode;
        private DateTime? startDate;
        private DateTime? endDate;

        public ProductionWorkOrderList() { }
        public ProductionWorkOrderList(IEnumerable<ProductionWorkOrder> items) : base(items) { }
        public ProductionWorkOrderList(string bizAreaCode, DateTime? stratDate = null, DateTime? endDate = null)
        {
            this.bizAreaCode = bizAreaCode;
            this.startDate = stratDate;
            this.endDate = endDate;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;

            DbCommand dbCom = db.GetSqlStringCommand("SELECT * FROM fn_Production_WorkOrder(@BizAreaCode, @StartDate, @EndDate) ORDER BY OrderDate DESC");
            db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, bizAreaCode);
            db.AddInParameter(dbCom, "@StartDate", DbType.String, startDate.Value.ToShortDateString());
            db.AddInParameter(dbCom, "@EndDate", DbType.String, endDate.Value.ToShortDateString());
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new ProductionWorkOrder
                    {
                        State = MesAdmin.Common.Common.EntityState.Unchanged,
                        BizAreaCode = (string)u["BizAreaCode"],
                        OrderNo = (string)u["OrderNo"],
                        OrderDate = (DateTime)u["OrderDate"],
                        WaCode = (string)u["WaCode"],
                        EqpCode = (string)u["EqpCode"],
                        ItemCode = (string)u["ItemCode"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = (string)u["ItemSpec"],
                        BasicUnit = (string)u["BasicUnit"],
                        OrderQty = (decimal)u["OrderQty"],
                        Remark = u["Remark"].ToString(),
                        Remark2 = u["Remark2"].ToString(),
                        IsEnd = Convert.ToChar(u["IsEnd"]),
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"]
                    }
                )
            );
        }

        public void Save()
        {
            IEnumerable<ProductionWorkOrder> items = this.Items;
            Insert(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Added));
            Update(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Modified));
            Delete(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Deleted));
        }

        public void Insert(IEnumerable<ProductionWorkOrder> items)
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    foreach (ProductionWorkOrder order in items)
                    {
                        dbCom = db.GetStoredProcCommand("usp_Production_WorkOrder");
                        dbCom.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(dbCom, "@State", DbType.String, order.State);
                        db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, order.BizAreaCode);
                        db.AddInParameter(dbCom, "@OrderDate", DbType.Date, order.OrderDate);
                        db.AddInParameter(dbCom, "@WaCode", DbType.String, order.WaCode);
                        db.AddInParameter(dbCom, "@EqpCode", DbType.String, order.EqpCode);
                        db.AddInParameter(dbCom, "@ItemCode", DbType.String, order.ItemCode);
                        db.AddInParameter(dbCom, "@OrderQty", DbType.Decimal, order.OrderQty);
                        db.AddInParameter(dbCom, "@Remark", DbType.String, order.Remark);
                        db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
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

        public void Update(IEnumerable<ProductionWorkOrder> items)
        {
        }

        public void Delete(IEnumerable<ProductionWorkOrder> items)
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    foreach (ProductionWorkOrder order in items)
                    {
                        dbCom = db.GetStoredProcCommand("usp_Production_WorkOrder");
                        dbCom.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(dbCom, "@State", DbType.String, order.State);
                        db.AddInParameter(dbCom, "@OrderNo", DbType.String, order.OrderNo);
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
