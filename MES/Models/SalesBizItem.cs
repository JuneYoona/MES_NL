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
    public class SalesBizItem : StateBusinessObject
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
        public string BizItemCode
        {
            get { return GetProperty(() => BizItemCode); }
            set { SetProperty(() => BizItemCode, value); }
        }
        public string BizItemName
        {
            get { return GetProperty(() => BizItemName); }
            set { SetProperty(() => BizItemName, value); }
        }
        public string BizItemSpec
        {
            get { return GetProperty(() => BizItemSpec); }
            set { SetProperty(() => BizItemSpec, value); }
        }
        public string BizUnit
        {
            get { return GetProperty(() => BizUnit); }
            set { SetProperty(() => BizUnit, value); }
        }
        public string UpdateId
        {
            get { return GetProperty(() => UpdateId); }
            set { SetProperty(() => UpdateId, value); }
        }
        public DateTime? UpdateDate
        {
            get { return GetProperty(() => UpdateDate); }
            set { SetProperty(() => UpdateDate, value); }
        }
    }

    public class SalesBizItemList : ObservableCollection<SalesBizItem>
    {
        private string bizCode;
        private string itemCode;
        private string itemName;

        public SalesBizItemList(string bizCode = "", string itemCode = "", string itemName = "")
        {
            this.bizCode = bizCode;
            this.itemCode = itemCode;
            this.itemName = itemName;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;

            DbCommand dbCom = db.GetStoredProcCommand("usp_sales_BizItem_List");
            db.AddInParameter(dbCom, "@BizCode", DbType.String, bizCode);
            db.AddInParameter(dbCom, "@ItemCode", DbType.String, itemCode);
            db.AddInParameter(dbCom, "@ItemName", DbType.String, itemName);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new SalesBizItem
                    {
                        State = EntityState.Unchanged,
                        BizCode = (string)u["BizCode"],
                        ItemCode = (string)u["ItemCode"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = (string)u["ItemSpec"],
                        BizItemCode = u["BizItemCode"].ToString(),
                        BizItemName = u["BizItemName"].ToString(),
                        BizItemSpec = u["BizItemSpec"].ToString(),
                        BizUnit = u["BizUnit"].ToString(),
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"]
                    }
                )
            );
        }

        public void Save()
        {
            IEnumerable<SalesBizItem> items = this.Items;
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = null;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                try
                {
                    Insert(items.Where(u => u.State == EntityState.Added), db, trans, dbCom);
                    Update(items.Where(u => u.State == EntityState.Modified), db, trans, dbCom);
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

        public void Insert(IEnumerable<SalesBizItem> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            foreach (SalesBizItem item in items)
            {
                dbCom = db.GetStoredProcCommand("usp_sales_BizItem");
                dbCom.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(dbCom, "@BizCode", DbType.String, item.BizCode);
                db.AddInParameter(dbCom, "@ItemCode", DbType.String, item.ItemCode);
                db.AddInParameter(dbCom, "@BizItemCode", DbType.String, item.BizItemCode);
                db.AddInParameter(dbCom, "@BizItemName", DbType.String, item.BizItemName);
                db.AddInParameter(dbCom, "@BizItemSpec", DbType.String, item.BizItemSpec);
                db.AddInParameter(dbCom, "@BizUnit", DbType.String, item.BizUnit);
                db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }

        public void Update(IEnumerable<SalesBizItem> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            string sql = "UPDATE sales_BizItem SET BizItemCode = @BizItemCode, BizItemName = @BizItemName, BizItemSpec = @BizItemSpec, BizUnit = @BizUnit, UpdateId = @UpdateId, UpdateDate = getdate() "
                + "WHERE BizCode = @BizCode AND ItemCode = @ItemCode";
            foreach (SalesBizItem item in items)
            {
                dbCom = dbCom = db.GetSqlStringCommand(sql);
                db.AddInParameter(dbCom, "@BizCode", DbType.String, item.BizCode);
                db.AddInParameter(dbCom, "@BizItemCode", DbType.String, item.BizItemCode);
                db.AddInParameter(dbCom, "@ItemCode", DbType.String, item.ItemCode);
                db.AddInParameter(dbCom, "@BizItemName", DbType.String, item.BizItemName);
                db.AddInParameter(dbCom, "@BizItemSpec", DbType.String, item.BizItemSpec);
                db.AddInParameter(dbCom, "@BizUnit", DbType.String, item.BizUnit);
                db.AddInParameter(dbCom, "@UpdateId", DbType.String, DSUser.Instance.UserID);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }

        public void Delete(IEnumerable<SalesBizItem> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            string str;
            foreach (SalesBizItem item in items)
            {
                str = string.Format("DELETE sales_BizItem WHERE BizCode = '{0}' AND ItemCode = '{1}'", item.BizCode, item.ItemCode);
                dbCom = db.GetSqlStringCommand(str);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }
    }
}
