using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using MesAdmin.Common.Common;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace MesAdmin.Models
{
    public class CommonItem : StateBusinessObject
    {
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public string ItemCode
        {
            get { return GetProperty(() => ItemCode); }
            set { SetProperty(() => ItemCode, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
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
        public string ItemAccount
        {
            get { return GetProperty(() => ItemAccount); }
            set { SetProperty(() => ItemAccount, value); }
        }
        public string ItemAccountName
        {
            get { return GetProperty(() => ItemAccountName); }
            set { SetProperty(() => ItemAccountName, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public string BasicUnit
        {
            get { return GetProperty(() => BasicUnit); }
            set { SetProperty(() => BasicUnit, value); }
        }
        public string BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value); }
        }
        public string ProcureType
        {
            get { return GetProperty(() => ProcureType); }
            set { SetProperty(() => ProcureType, value); }
        }
        public string InWhCode
        {
            get { return GetProperty(() => InWhCode); }
            set { SetProperty(() => InWhCode, value); }
        }
        public string OutWhCode
        {
            get { return GetProperty(() => OutWhCode); }
            set { SetProperty(() => OutWhCode, value); }
        }
        public string InWaCode
        {
            get { return GetProperty(() => InWaCode); }
            set { SetProperty(() => InWaCode, value); }
        }
        public decimal? BtlQty
        {
            get { return GetProperty(() => BtlQty); }
            set { SetProperty(() => BtlQty, value); }
        }
        public bool IQCFlag
        {
            get { return GetProperty(() => IQCFlag); }
            set { SetProperty(() => IQCFlag, value); }
        }
        public bool LQCFlag
        {
            get { return GetProperty(() => LQCFlag); }
            set { SetProperty(() => LQCFlag, value); }
        }
        public bool FQCFlag
        {
            get { return GetProperty(() => FQCFlag); }
            set { SetProperty(() => FQCFlag, value); }
        }
        public bool OQCFlag
        {
            get { return GetProperty(() => OQCFlag); }
            set { SetProperty(() => OQCFlag, value); }
        }
        public bool IsEnabled
        {
            get { return GetProperty(() => IsEnabled); }
            set { SetProperty(() => IsEnabled, value); }
        }
        public string Remark1
        {
            get { return GetProperty(() => Remark1); }
            set { SetProperty(() => Remark1, value); }
        }
        public string Remark2
        {
            get { return GetProperty(() => Remark2); }
            set { SetProperty(() => Remark2, value); }
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

        public CommonItem ShallowCopy()
        {
            return (CommonItem)this.MemberwiseClone();
        }

        public CommonItem DeepCloneReflection()
        {
            var newItem = new CommonItem();
            PropertyInfo[] properties = typeof(CommonItem).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var property in properties)
            {
                try
                {
                    property.SetValue(newItem, property.GetValue(this, null), null);
                }
                catch { }
            }
            return newItem;
        }

        public void Save()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                try
                {
                    Save(db, trans);
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public void Save(Database db, DbTransaction trans)
        {
            switch (State)
            {
                case EntityState.Added:
                    Insert(db, trans);
                    break;
                case EntityState.Deleted:
                    Delete(db, trans);
                    break;
                case EntityState.Modified:
                    Insert(db, trans);
                    break;
            }
        }

        public void Insert(Database db, DbTransaction trans)
        {
            DbCommand dbCom = db.GetStoredProcCommand("usp_common_Item");
            db.AddInParameter(dbCom, "@ItemCode", DbType.String, ItemCode);
            db.AddInParameter(dbCom, "@ItemName", DbType.String, ItemName);
            db.AddInParameter(dbCom, "@ItemSpec", DbType.String, ItemSpec);
            db.AddInParameter(dbCom, "@ItemAccount", DbType.String, ItemAccount);
            db.AddInParameter(dbCom, "@ProcureType", DbType.String, ProcureType);
            db.AddInParameter(dbCom, "@BasicUnit", DbType.String, BasicUnit);
            db.AddInParameter(dbCom, "@IsEnabled", DbType.Boolean, IsEnabled);
            db.AddInParameter(dbCom, "@IQCFlag", DbType.Boolean, IQCFlag);
            db.AddInParameter(dbCom, "@LQCFlag", DbType.Boolean, LQCFlag);
            db.AddInParameter(dbCom, "@FQCFlag", DbType.Boolean, FQCFlag);
            db.AddInParameter(dbCom, "@OQCFlag", DbType.Boolean, OQCFlag);
            db.AddInParameter(dbCom, "@InWhcode", DbType.String, InWhCode);
            db.AddInParameter(dbCom, "@OutWhcode", DbType.String, OutWhCode);
            db.AddInParameter(dbCom, "@InWaCode", DbType.String, InWaCode);
            db.AddInParameter(dbCom, "@BtlQty", DbType.Decimal, BtlQty == null ? null : BtlQty);
            db.AddInParameter(dbCom, "@Remark1", DbType.String, Remark1);
            db.AddInParameter(dbCom, "@Remark2", DbType.String, Remark2);
            db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
            db.AddInParameter(dbCom, "@State", DbType.String, State);
            db.ExecuteNonQuery(dbCom, trans);
        }

        public void Delete(Database db, DbTransaction trans)
        {
            string str = string.Format("DELETE common_Item WHERE ItemCode = '{0}' ", ItemCode);
            DbCommand dbCom = db.GetSqlStringCommand(str);
            db.ExecuteNonQuery(dbCom, trans);
        }
    }

    public class CommonItemList : ObservableCollection<CommonItem>
    {
        private string bizAreaCode;

        public CommonItemList(IEnumerable<CommonItem> items) : base(items) { }
        public CommonItemList() : this("") { }
        public CommonItemList(string bizAreaCode)
        {
            this.bizAreaCode = bizAreaCode;
            InitializeList();
        }

        public void Save()
        {
            IEnumerable<CommonItem> items = this.Items;
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                try
                {
                    Delete(items.Where(u => u.State == EntityState.Deleted), db, trans);
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }
        
        public void Delete(IEnumerable<CommonItem> items, Database db, DbTransaction trans)
        {
            string str;
            DbCommand dbCom = null;

            foreach (CommonItem item in items)
            {
                str = string.Format("DELETE common_Item WHERE ItemCode = '{0}' ", item.ItemCode);
                dbCom = db.GetSqlStringCommand(str);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }
        public void SyncErp()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbCommand dbCom = db.GetSqlStringCommand("EXEC USP_IF_ERP2MES_B_ITEM_KO656");
                db.ExecuteNonQuery(dbCom);
            }

            // Global 기준정보를 다시 가져오기 위해 Instance 초기화
            GlobalCommonItem.Instance = null;
        }
        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("usp_common_ItemList");
            db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, bizAreaCode);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u => 
                base.Add(
                    new CommonItem 
                    {
                        State = EntityState.Unchanged, 
                        ItemCode = (string)u["ItemCode"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = (string)u["ItemSpec"],
                        ItemAccount = (string)u["ItemAccount"],
                        ItemAccountName = (string)u["ItemAccountName"],
                        ProcureType = u["ProcureType"].ToString(),
                        BasicUnit = (string)u["BasicUnit"],
                        IQCFlag = (bool)u["IQCFlag"],
                        LQCFlag = (bool)u["LQCFlag"],
                        FQCFlag = (bool)u["FQCFlag"],
                        OQCFlag = (bool)u["OQCFlag"],
                        BtlQty = u["BtlQty"] == DBNull.Value ? null : (decimal?)u["BtlQty"],
                        IsEnabled = (bool)u["IsEnabled"],
                        InWhCode = u["InWhCode"].ToString(),
                        OutWhCode = u["OutWhCode"].ToString(),
                        Remark1 = u["Remark1"].ToString(),
                        Remark2 = u["Remark2"].ToString(),
                        InWaCode = u["InWaCode"].ToString(),
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"],
                        BizAreaCode = (string)u["Ref01"]
                    }
                )
            );
        }
    }
}
