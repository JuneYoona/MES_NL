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
    public class CommonYieldWE10PerItem : StateBusinessObject
    {
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
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public string ItemCodeCore
        {
            get { return GetProperty(() => ItemCodeCore); }
            set { SetProperty(() => ItemCodeCore, value); }
        }
        public string ItemNameCore
        {
            get { return GetProperty(() => ItemNameCore); }
            set { SetProperty(() => ItemNameCore, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public decimal? Molecule
        {
            get { return GetProperty(() => Molecule); }
            set { SetProperty(() => Molecule, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public decimal? Crude
        {
            get { return GetProperty(() => Crude); }
            set { SetProperty(() => Crude, value); }
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

    public class CommonYieldWE10PerItemList : ObservableCollection<CommonYieldWE10PerItem>
    {
        public CommonYieldWE10PerItemList()
        {
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;

            string sql = "SELECT A.*, B.ItemName, B.ItemSpec, ItemNameCore = C.ItemName FROM common_YieldWE10_PerItem (NOLOCK) A ";
            sql += "INNER JOIN common_Item (NOLOCK) B ON A.ItemCode = B.ItemCode ";
            sql += "INNER JOIN common_Item (NOLOCK) C ON A.ItemCodeCore = C.ItemCode";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new CommonYieldWE10PerItem
                    {
                        State = EntityState.Unchanged,
                        ItemCode = (string)u["ItemCode"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = (string)u["ItemSpec"],
                        ItemCodeCore = (string)u["ItemCodeCore"],
                        ItemNameCore = (string)u["ItemNameCore"],
                        Molecule = (decimal)u["Molecule"],
                        Crude = (decimal)u["Crude"],
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"],
                    }
                )
            );
        }

        public void Save()
        {
            IEnumerable<CommonYieldWE10PerItem> items = this.Items;
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

        public void Insert(IEnumerable<CommonYieldWE10PerItem> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            foreach (CommonYieldWE10PerItem item in items)
            {
                dbCom = db.GetStoredProcCommand("usps_common_YieldWE10_PerItem");
                dbCom.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(dbCom, "@State", DbType.String, item.State);
                db.AddInParameter(dbCom, "@ItemCode", DbType.String, item.ItemCode);
                db.AddInParameter(dbCom, "@ItemCodeCore", DbType.String, item.ItemCodeCore);
                db.AddInParameter(dbCom, "@Molecule", DbType.Decimal, item.Molecule);
                db.AddInParameter(dbCom, "@Crude", DbType.Decimal, item.Crude);
                db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }

        public void Update(IEnumerable<CommonYieldWE10PerItem> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            foreach (CommonYieldWE10PerItem item in items)
            {
                dbCom = db.GetStoredProcCommand("usps_common_YieldWE10_PerItem");
                dbCom.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(dbCom, "@State", DbType.String, item.State);
                db.AddInParameter(dbCom, "@ItemCode", DbType.String, item.ItemCode);
                db.AddInParameter(dbCom, "@ItemCodeCore", DbType.String, item.ItemCodeCore);
                db.AddInParameter(dbCom, "@Molecule", DbType.Decimal, item.Molecule);
                db.AddInParameter(dbCom, "@Crude", DbType.Decimal, item.Crude);
                db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }

        public void Delete(IEnumerable<CommonYieldWE10PerItem> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            foreach (CommonYieldWE10PerItem item in items)
            {
                dbCom = db.GetSqlStringCommand("DELETE common_YieldWE10_PerItem WHERE ItemCode = @ItemCode AND ItemCodeCore = @ItemCodeCore");
                db.AddInParameter(dbCom, "@ItemCode", DbType.String, item.ItemCode);
                db.AddInParameter(dbCom, "@ItemCodeCore", DbType.String, item.ItemCodeCore);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }
    }
}