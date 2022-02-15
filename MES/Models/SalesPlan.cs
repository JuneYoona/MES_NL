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
    public class SalesPlan : StateBusinessObject
    {
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public string BizCode
        {
            get { return GetProperty(() => BizCode); }
            set { SetProperty(() => BizCode, value); }
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
        public int PlanYear
        {
            get { return GetProperty(() => PlanYear); }
            set { SetProperty(() => PlanYear, value); }
        }
        public int PlanMonth
        {
            get { return GetProperty(() => PlanMonth); }
            set { SetProperty(() => PlanMonth, value); }
        }
        public DateTime ApplyDate
        {
            get { return GetProperty(() => ApplyDate); }
            set { SetProperty(() => ApplyDate, value); }
        }
        [Range(0.000001, double.MaxValue, ErrorMessage = "0 보다 커야합니다.")]
        public decimal Qty
        {
            get { return GetProperty(() => Qty); }
            set { SetProperty(() => Qty, value); }
        }
        [Range(0.000001, double.MaxValue, ErrorMessage = "0 보다 커야합니다.")]
        public decimal Account
        {
            get { return GetProperty(() => Account); }
            set { SetProperty(() => Account, value); }
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

    public class SalesPlanList : ObservableCollection<SalesPlan>
    {
        private string planYear;
        private string bizCode;
        private string itemCode;

        public SalesPlanList(string planYear, string bizCode, string itemCode)
        {
            this.planYear = planYear;
            this.bizCode = bizCode;
            this.itemCode = itemCode;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetSqlStringCommand("SELECT * FROM fn_salesPlan(@PlanYear, @BizCode, @ItemCode) ORDER BY BizCode, ItemCode, PlanYear, PlanMonth, ApplyDate");
            db.AddInParameter(dbCom, "@PlanYear", DbType.String, planYear);
            db.AddInParameter(dbCom, "@BizCode", DbType.String, bizCode);
            db.AddInParameter(dbCom, "@ItemCode", DbType.String, itemCode);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new SalesPlan
                    {
                        BizCode = (string)u["BizCode"],
                        ItemCode = (string)u["ItemCode"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = (string)u["ItemSpec"],
                        BasicUnit = (string)u["BasicUnit"],
                        PlanYear = (int)u["PlanYear"],
                        PlanMonth = (int)u["PlanMonth"],
                        ApplyDate = (DateTime)u["ApplyDate"],
                        Qty = (decimal)u["Qty"],
                        Account = (decimal)u["Account"],
                        Memo = u["Memo"].ToString(),
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"]
                    }
                )
            );
        }

        public void Save()
        {
            IEnumerable<SalesPlan> items = this.Items;
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

        public void Insert(IEnumerable<SalesPlan> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            foreach (SalesPlan item in items)
            {
                dbCom = db.GetStoredProcCommand("usp_sales_Plan");
                dbCom.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(dbCom, "@BizCode", DbType.String, item.BizCode);
                db.AddInParameter(dbCom, "@ItemCode", DbType.String, item.ItemCode);
                db.AddInParameter(dbCom, "@PlanYear", DbType.String, item.PlanYear);
                db.AddInParameter(dbCom, "@PlanMonth", DbType.String, item.PlanMonth);
                db.AddInParameter(dbCom, "@ApplyDate", DbType.Date, item.ApplyDate);
                db.AddInParameter(dbCom, "@Qty", DbType.Decimal, item.Qty);
                db.AddInParameter(dbCom, "@Account", DbType.Decimal, item.Account);
                db.AddInParameter(dbCom, "@Memo", DbType.String, item.Memo);
                db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }

        public void Delete(IEnumerable<SalesPlan> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            string str;
            foreach (SalesPlan item in items)
            {
                str = "DELETE sales_Plan WHERE BizCode = @BizCode AND ItemCode = @ItemCode AND PlanYear = @PlanYear AND PlanMonth = @PlanMonth AND ApplyDate = @ApplyDate";
                dbCom = db.GetSqlStringCommand(str);
                db.AddInParameter(dbCom, "@BizCode", DbType.String, item.BizCode);
                db.AddInParameter(dbCom, "@ItemCode", DbType.String, item.ItemCode);
                db.AddInParameter(dbCom, "@PlanYear", DbType.String, item.PlanYear);
                db.AddInParameter(dbCom, "@PlanMonth", DbType.String, item.PlanMonth);
                db.AddInParameter(dbCom, "@ApplyDate", DbType.Date, item.ApplyDate);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }
    }
}
