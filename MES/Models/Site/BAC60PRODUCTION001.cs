using MesAdmin.Common.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace MesAdmin.Models
{
    public class BAC60PRODUCTION001 : StateBusinessObject
    {
        public string ItemCode
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string Remark2
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public decimal Qty
        {
            get { return GetValue<decimal>(); }
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
        public decimal WE00
        {
            get { return GetValue<decimal>(); }
            set { SetValue(value); }
        }
        public decimal WE10
        {
            get { return GetValue<decimal>(); }
            set { SetValue(value); }
        }
        public decimal WE20
        {
            get { return GetValue<decimal>(); }
            set { SetValue(value); }
        }
        public decimal WE30
        {
            get { return GetValue<decimal>(); }
            set { SetValue(value); }
        }
        public decimal WE40
        {
            get { return GetValue<decimal>(); }
            set { SetValue(value); }
        }
        public decimal WE42
        {
            get { return GetValue<decimal>(); }
            set { SetValue(value); }
        }
        public decimal WE50
        {
            get { return GetValue<decimal>(); }
            set { SetValue(value); }
        }
        public decimal WETotal
        {
            get { return Qty + WE00 + WE10 + WE20 + WE30 + WE40 + WE42 + WE50; }
        }
        public decimal TTL
        {
            get { return Qty + WE00 + WE10 + WE20 + WE30 + WE40 + WE42 + WE50 + VendorQty; }
        }
        public decimal PrcTotal
        {
            get { return WE10 + WE20 + WE30 + WE40 + WE42 + WE50; }
        }
        public decimal GI_QTY
        {
            get { return GetValue<decimal>(); }
            set { SetValue(value); }
        }
        public decimal PLAN_QTY
        {
            get { return GetValue<decimal>(); }
            set { SetValue(value); }
        }
        public decimal REMAIN_QTY
        {
            get { return PLAN_QTY - GI_QTY; }
        }
        public DateTime? Date1
        {
            get { return GetValue<DateTime?>(); }
            set { SetValue(value); }
        }
        public DateTime? Date2
        {
            get { return GetValue<DateTime?>(); }
            set { SetValue(value); }
        }
        public DateTime? Date3
        {
            get { return GetValue<DateTime?>(); }
            set { SetValue(value); }
        }
        public decimal VendorQty
        {
            get { return GetValue<decimal>(); }
            set { SetValue(value); }
        }
    }

    public class BAC60PRODUCTION001LIST : ObservableCollection<BAC60PRODUCTION001>
    {
        private DateTime date;
        private string type;
        private string status;

        public BAC60PRODUCTION001LIST(DateTime date, string type, string status)
        {
            this.date = date;
            this.type = type;
            this.status = status;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;

            DbCommand dbCom = db.GetStoredProcCommand("BAC60PRODUCTION001R");
            db.AddInParameter(dbCom, "@Date", DbType.Date, date);
            db.AddInParameter(dbCom, "@Type", DbType.String, type);
            db.AddInParameter(dbCom, "@Status", DbType.String, status);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add
                (
                    new BAC60PRODUCTION001
                    {
                        State = EntityState.Unchanged,
                        ItemCode = (string)u["ItemCode"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = (string)u["ItemSpec"],
                        Remark2 = u["Remark2"].ToString(),
                        Qty = u["Qty"] == DBNull.Value ? 0 : (decimal)u["Qty"],
                        WE00 = u["WE00"] == DBNull.Value ? 0 : (decimal)u["WE00"],
                        WE10 = u["WE10"] == DBNull.Value ? 0 : (decimal)u["WE10"],
                        WE20 = u["WE20"] == DBNull.Value ? 0 : (decimal)u["WE20"],
                        WE30 = u["WE30"] == DBNull.Value ? 0 : (decimal)u["WE30"],
                        WE40 = u["WE40"] == DBNull.Value ? 0 : (decimal)u["WE40"],
                        WE42 = u["WE42"] == DBNull.Value ? 0 : (decimal)u["WE42"],
                        WE50 = u["WE50"] == DBNull.Value ? 0 : (decimal)u["WE50"],
                        GI_QTY = u["GI_QTY"] == DBNull.Value ? 0 : (decimal)u["GI_QTY"],
                        PLAN_QTY = u["PLAN_QTY"] == DBNull.Value ? 0 : (decimal)u["PLAN_QTY"],
                        Date1 = u["Date1"] == DBNull.Value ? (DateTime?)null : (DateTime)u["Date1"],
                        Date2 = u["Date2"] == DBNull.Value ? (DateTime?)null : (DateTime)u["Date2"],
                        Date3 = u["Date3"] == DBNull.Value ? (DateTime?)null : (DateTime)u["Date3"],
                        VendorQty = u["VendorQty"] == DBNull.Value ? 0 : (decimal)u["VendorQty"],
                    }
                )
            );
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
                    foreach (BAC60PRODUCTION001 detail in Items.Where(o => o.State == EntityState.Modified))
                    {
                        dbCom = db.GetStoredProcCommand("BAC60PRODUCTION001C");
                        dbCom.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(dbCom, "@ItemCode", DbType.String, detail.ItemCode);
                        db.AddInParameter(dbCom, "@LotCnt", DbType.String, detail.Remark2);
                        db.AddInParameter(dbCom, "@Date1", DbType.Date, detail.Date1);
                        db.AddInParameter(dbCom, "@Date2", DbType.Date, detail.Date2);
                        db.AddInParameter(dbCom, "@Date3", DbType.Date, detail.Date3);
                        db.AddInParameter(dbCom, "@VendorQty", DbType.Decimal, detail.VendorQty);
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
    }
}