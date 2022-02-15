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
    public class CommonAltItem : StateBusinessObject
    {
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public string ItemCode
        {
            get { return GetProperty(() => ItemCode); }
            set { SetProperty(() => ItemCode, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public string AltItemCode
        {
            get { return GetProperty(() => AltItemCode); }
            set { SetProperty(() => AltItemCode, value); }
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
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public int Order
        {
            get { return GetProperty(() => Order); }
            set { SetProperty(() => Order, value); }
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

    public class CommonAltItemList : ObservableCollection<CommonAltItem>
    {
        private string itemCode;

        public CommonAltItemList(IEnumerable<CommonAltItem> items) : base(items) { }
        public CommonAltItemList(string itemCode)
        {
            this.itemCode = itemCode;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;

            string str = "SELECT A.ItemName, A.ItemSpec, B.* FROM common_Item (NOLOCK) A INNER JOIN common_AltItem (NOLOCK) B ON A.ItemCode = B.ItemCode WHERE B.ItemCode = @ItemCode";
            DbCommand dbCom = db.GetSqlStringCommand(str);
            db.AddInParameter(dbCom, "@ItemCode", DbType.String, itemCode);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new CommonAltItem
                    {
                        State = EntityState.Unchanged,
                        ItemCode = (string)u["ItemCode"],
                        AltItemCode = (string)u["AltItemCode"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = u["ItemSpec"].ToString(),
                        Order = (int)u["Order"],
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"],
                    }
                )
            );
        }

        public void Save()
        {
            IEnumerable<CommonAltItem> items = this.Items;
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

        public void Insert(IEnumerable<CommonAltItem> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            foreach (CommonAltItem item in items)
            {
                dbCom = db.GetSqlStringCommand("INSERT INTO common_AltItem VALUES (@ItemCode, @AltItemCode, @Order, @InsertId, getdate(), @InsertId, getdate())");
                db.AddInParameter(dbCom, "@ItemCode", DbType.String, item.ItemCode);
                db.AddInParameter(dbCom, "@AltItemCode", DbType.String, item.AltItemCode);
                db.AddInParameter(dbCom, "@Order", DbType.Int16, item.Order);
                db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }

        public void Delete(IEnumerable<CommonAltItem> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            foreach (CommonAltItem item in items)
            {
                dbCom = db.GetSqlStringCommand("DELETE common_AltItem WHERE ItemCode = @ItemCode AND AltItemCode = @AltItemCode");
                db.AddInParameter(dbCom, "@ItemCode", DbType.String, item.ItemCode);
                db.AddInParameter(dbCom, "@AltItemCode", DbType.String, item.AltItemCode);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }

        public void Update(IEnumerable<CommonAltItem> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            foreach (CommonAltItem item in items)
            {
                dbCom = db.GetSqlStringCommand("UPDATE common_AltItem SET [Order] = @Order, UpdateId = @UpdateId, UpdateDate = getdate() WHERE ItemCode = @ItemCode AND AltItemCode = @AltItemCode");
                db.AddInParameter(dbCom, "@ItemCode", DbType.String, item.ItemCode);
                db.AddInParameter(dbCom, "@AltItemCode", DbType.String, item.AltItemCode);
                db.AddInParameter(dbCom, "@Order", DbType.Int16, item.Order);
                db.AddInParameter(dbCom, "@UpdateId", DbType.String, DSUser.Instance.UserID);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }
    }
}
