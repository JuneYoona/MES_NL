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
    public class CommonPartCode : StateBusinessObject
    {
        public string BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public string PartCode
        {
            get { return GetProperty(() => PartCode); }
            set { SetProperty(() => PartCode, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public string PartName
        {
            get { return GetProperty(() => PartName); }
            set { SetProperty(() => PartName, value); }
        }
        public string WaCode
        {
            get { return GetProperty(() => WaCode); }
            set { SetProperty(() => WaCode, value); }
        }
        public string PartGroup
        {
            get { return GetProperty(() => PartGroup); }
            set { SetProperty(() => PartGroup, value); }
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
        public string Remark3
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string Remark4
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string Remark5
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string Remark6
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string Remark7
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }
        public string EqpCode
        {
            get { return GetProperty(() => EqpCode); }
            set { SetProperty(() => EqpCode, value); }
        }
        public string EqpName
        {
            get { return GetProperty(() => EqpName); }
            set { SetProperty(() => EqpName, value); }
        }
        public string LocationCode
        {
            get { return GetProperty(() => LocationCode); }
            set { SetProperty(() => LocationCode, value); }
        }
        public string Location
        {
            get { return GetProperty(() => Location); }
            set { SetProperty(() => Location, value); }
        }
        public string OriginPartCode
        {
            get { return GetProperty(() => OriginPartCode); }
            set { SetProperty(() => OriginPartCode, value); }
        }
        public bool IsEnabled
        {
            get { return GetProperty(() => IsEnabled); }
            set { SetProperty(() => IsEnabled, value); }
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

    public class CommonPartCodeList : ObservableCollection<CommonPartCode>
    {
        private string bizAreaCode;
        private string waCode;
        private string partGroup;

        public CommonPartCodeList(IEnumerable<CommonPartCode> items) : base(items) { }
        public CommonPartCodeList() : this("") { }
        public CommonPartCodeList(string bizAreaCode) : this(bizAreaCode, "") { }
        public CommonPartCodeList(string bizAreaCode, string waCode) : this(bizAreaCode, waCode, "") { }
        public CommonPartCodeList(string bizAreaCode, string waCode, string partGroup)
        {
            this.bizAreaCode = bizAreaCode;
            this.waCode = waCode;
            this.partGroup = partGroup;
            InitializeList();
        }

        public void InitializeList()
        {
            Clear();
            Database db = ProviderFactory.Instance;

            DbCommand dbCom = db.GetStoredProcCommand("usp_common_PartCodeList");
            db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, bizAreaCode);
            db.AddInParameter(dbCom, "@WaCode", DbType.String, waCode);
            db.AddInParameter(dbCom, "@PartGroup", DbType.String, partGroup);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                Add(
                    new CommonPartCode
                    {
                        State = EntityState.Unchanged,
                        BizAreaCode = (string)u["BizAreaCode"],
                        PartCode = (string)u["PartCode"],
                        PartName = u["PartName"].ToString(),
                        WaCode = u["WaCode"].ToString(),
                        PartGroup = u["PartGroup"].ToString(),
                        Remark1 = u["Remark1"].ToString(),
                        Remark2 = u["Remark2"].ToString(),
                        Remark3 = u["Remark3"].ToString(),
                        Remark4 = u["Remark4"].ToString(),
                        Remark5 = u["Remark5"].ToString(),
                        Remark6 = u["Remark6"].ToString(),
                        Remark7 = u["Remark7"].ToString(),
                        EqpCode = u["EqpCode"].ToString(),
                        EqpName = u["EqpName"].ToString(),
                        Location = u["Location"].ToString(),
                        IsEnabled = (bool)u["IsEnabled"],
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
                try
                {
                    Insert(Items.Where(u => u.State == EntityState.Added), db, trans);
                    Update(Items.Where(u => u.State == EntityState.Modified), db, trans);
                    Delete(Items.Where(u => u.State == EntityState.Deleted), db, trans);

                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public void Insert(IEnumerable<CommonPartCode> items, Database db, DbTransaction trans)
        {
            foreach (CommonPartCode item in items)
            {
                DbCommand dbCom = db.GetStoredProcCommand("usp_common_PartCode");
                dbCom.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(dbCom, "@State", DbType.String, item.State);
                db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, item.BizAreaCode);
                db.AddInParameter(dbCom, "@PartCode", DbType.String, item.PartCode);
                db.AddInParameter(dbCom, "@PartName", DbType.String, item.PartName);
                db.AddInParameter(dbCom, "@WaCode", DbType.String, item.WaCode);
                db.AddInParameter(dbCom, "@PartGroup", DbType.String, item.PartGroup);
                db.AddInParameter(dbCom, "@IsEnabled", DbType.Boolean, item.IsEnabled);
                db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }

        public void Update(IEnumerable<CommonPartCode> items, Database db, DbTransaction trans)
        {
            foreach (CommonPartCode item in items)
            {
                DbCommand dbCom = db.GetStoredProcCommand("usp_common_PartCode");
                dbCom.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(dbCom, "@State", DbType.String, item.State);
                db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, item.BizAreaCode);
                db.AddInParameter(dbCom, "@PartCode", DbType.String, item.PartCode);
                db.AddInParameter(dbCom, "@PartName", DbType.String, item.PartName);
                db.AddInParameter(dbCom, "@WaCode", DbType.String, item.WaCode);
                db.AddInParameter(dbCom, "@PartGroup", DbType.String, item.PartGroup);
                db.AddInParameter(dbCom, "@Remark1", DbType.String, item.Remark1);
                db.AddInParameter(dbCom, "@Remark2", DbType.String, item.Remark2);
                db.AddInParameter(dbCom, "@Remark3", DbType.String, item.Remark3);
                db.AddInParameter(dbCom, "@Remark4", DbType.String, item.Remark4);
                db.AddInParameter(dbCom, "@Remark5", DbType.String, item.Remark5);
                db.AddInParameter(dbCom, "@Remark6", DbType.String, item.Remark6);
                db.AddInParameter(dbCom, "@Remark7", DbType.String, item.Remark7);
                db.AddInParameter(dbCom, "@IsEnabled", DbType.Boolean, item.IsEnabled);
                db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }

        public void Delete(IEnumerable<CommonPartCode> items, Database db, DbTransaction trans)
        {
            foreach (CommonPartCode item in items)
            {
                DbCommand dbCom = db.GetSqlStringCommand("DELETE common_PartCode WHERE BizAreaCode = @BizAreaCode AND PartCode = @PartCode");
                db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, item.BizAreaCode);
                db.AddInParameter(dbCom, "@PartCode", DbType.String, item.PartCode);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }
    }

    public class ProductionPartCodeEquipList : ObservableCollection<CommonPartCode>
    {
        private string bizAreaCode;
        private string eqpCode;

        public ProductionPartCodeEquipList(string bizAreaCode, string eqpCode)
        {
            this.bizAreaCode = bizAreaCode;
            this.eqpCode = eqpCode;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("usps_production_PartCode");
            db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, bizAreaCode);
            db.AddInParameter(dbCom, "@EqpCode", DbType.String, eqpCode);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new CommonPartCode
                    {
                        State = EntityState.Unchanged,
                        BizAreaCode = (string)u["BizAreaCode"],
                        PartCode = u["PartCode"].ToString(),
                        OriginPartCode = u["PartCode"].ToString(),
                        LocationCode = u["LocationCode"].ToString(),
                        Location = u["Location"].ToString(),
                        PartGroup = u["PartGroup"].ToString(),
                        Remark3 = u["Remark3"].ToString(),
                        Remark4 = u["Remark4"].ToString(),
                        Remark6 = u["Remark6"].ToString(),
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
                try
                {
                    IEnumerable<CommonPartCode> items = this.Items.Where(o => o.State == EntityState.Added
                                                                                || o.State == EntityState.Deleted
                                                                                || o.State == EntityState.Modified);
                    foreach (CommonPartCode item in items)
                    {
                        DbCommand dbCom = db.GetStoredProcCommand("usps_production_PartCode");
                        db.AddInParameter(dbCom, "@State", DbType.String, item.State);
                        db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, item.BizAreaCode);
                        db.AddInParameter(dbCom, "@EqpCode", DbType.String, eqpCode);
                        db.AddInParameter(dbCom, "@Location", DbType.String, item.LocationCode);
                        db.AddInParameter(dbCom, "@PartCode", DbType.String, item.PartCode);
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