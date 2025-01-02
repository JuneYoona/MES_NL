using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using MesAdmin.Common.Common;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MesAdmin.Models
{
    public class CommonMajor : StateBusinessObject
    {
        [Required(ErrorMessage="필수입력값 입니다.")]
        public string MajorCode
        {
            get { return GetProperty(() => MajorCode); }
            set { SetProperty(() => MajorCode, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public string MajorName
        {
            get { return GetProperty(() => MajorName); }
            set { SetProperty(() => MajorName, value); }
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

    public class CommonMajorList : ObservableCollection<CommonMajor>
    { 
        public CommonMajorList(IEnumerable<CommonMajor> items) : base(items) { }
        public CommonMajorList()
        {
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;

            DbCommand dbCom = db.GetStoredProcCommand("usp_common_Major");
            db.AddInParameter(dbCom, "@RoleName", DbType.String, string.Join(",", DSUser.Instance.RoleName));
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new CommonMajor
                    {
                        State = EntityState.Unchanged,
                        MajorCode = (string)u["MajorCode"],
                        MajorName = (string)u["MajorName"],
                        IsEnabled = (bool)u["IsEnabled"],
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"]
                    }
                )
            );
        }

        public void Save()
        {
            IEnumerable<CommonMajor> items = this.Items;
            Insert(items.Where(u => u.State == EntityState.Added));
            Update(items.Where(u => u.State == EntityState.Modified));
            Delete(items.Where(u => u.State == EntityState.Deleted));
        }

        public void Insert(IEnumerable<CommonMajor> items)
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                string str;
                DbCommand dbCom = null;
                try
                {
                    foreach (CommonMajor item in items)
                    {
                        str = string.Format("INSERT INTO common_Major(MajorCode, MajorName, IsEnabled, InsertId, UpdateId, UpdateDate) VALUES ('{0}', '{1}', '{2}', '{3}', '{3}', getdate()) "
                            , item.MajorCode
                            , item.MajorName
                            , item.IsEnabled
                            , DSUser.Instance.UserID);
                        dbCom = db.GetSqlStringCommand(str);
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

        public void Update(IEnumerable<CommonMajor> items)
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                string str;
                DbCommand dbCom = null;
                try
                {
                    foreach (CommonMajor item in items)
                    {
                        str = string.Format("UPDATE common_Major SET MajorName = '{1}' WHERE MajorCode = '{0}' ", item.MajorCode, item.MajorName);
                        dbCom = db.GetSqlStringCommand(str);
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

        public void Delete(IEnumerable<CommonMajor> items)
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                string str;
                DbCommand dbCom = null;
                try
                {
                    foreach (CommonMajor item in items)
                    {
                        str = string.Format("DELETE common_Major WHERE MajorCode = '{0}' ", item.MajorCode);
                        dbCom = db.GetSqlStringCommand(str);
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
