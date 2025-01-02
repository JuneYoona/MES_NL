using MesAdmin.Common.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace MesAdmin.Models
{
    public class BAC60PRODUCTION002 : StateBusinessObject
    {
        public string ItemCode
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public int PlanYear
        {
            get { return GetValue<int>(); }
            set { SetValue(value); }
        }
        public int PlanMonth
        {
            get { return GetValue<int>(); }
            set { SetValue(value); }
        }
        public int Revision
        {
            get { return GetValue<int>(); }
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
        public string BasicUnit
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string Memo
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string UpdateId
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public DateTime UpdateDate
        {
            get { return GetValue<DateTime>(); }
            set { SetValue(value); }
        }
    }

    public class BAC60PRODUCTION002LIST : ObservableCollection<BAC60PRODUCTION002>
    {
        private int planYear;
        private string itemCode;
        
        public BAC60PRODUCTION002LIST(int planYear, string itemCode)
        {
            this.planYear = planYear;
            this.itemCode = itemCode;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;

            DbCommand dbCom = db.GetStoredProcCommand("BAC60PRODUCTION002R");
            db.AddInParameter(dbCom, "@PlanYear", DbType.Int32, planYear);
            db.AddInParameter(dbCom, "@ItemCode", DbType.String, itemCode);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add
                (
                    new BAC60PRODUCTION002
                    {
                        State = EntityState.Unchanged,
                        ItemCode = (string)u["ItemCode"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = u["ItemSpec"].ToString(),
                        BasicUnit = u["BasicUnit"].ToString(),
                        Qty = u["Qty"] == DBNull.Value ? 0 : (decimal)u["Qty"],
                        PlanYear = (int)u["PlanYear"],
                        PlanMonth = (int)u["PlanMonth"],
                        Revision = (int)u["Revision"],
                        Memo = u["Memo"].ToString(),
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"],
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
                    foreach (BAC60PRODUCTION002 item in Items.Where(o => o.State != EntityState.Unchanged))
                    {
                        dbCom = db.GetStoredProcCommand("BAC60PRODUCTION002C");
                        dbCom.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(dbCom, "@ItemCode", DbType.String, item.ItemCode);
                        db.AddInParameter(dbCom, "@PlanYear", DbType.Int32, item.PlanYear);
                        db.AddInParameter(dbCom, "@PlanMonth", DbType.Int32, item.PlanMonth);
                        db.AddInParameter(dbCom, "@Revision", DbType.Int32, item.Revision);
                        db.AddInParameter(dbCom, "@Qty", DbType.Decimal, item.Qty);
                        db.AddInParameter(dbCom, "@Memo", DbType.String, item.Memo);
                        db.AddInParameter(dbCom, "@State", DbType.String, item.State);
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