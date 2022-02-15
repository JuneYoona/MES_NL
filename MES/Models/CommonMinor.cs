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
    public class CommonMinor : StateBusinessObject
    {
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public string MinorCode
        {
            get { return GetProperty(() => MinorCode); }
            set { SetProperty(() => MinorCode, value); }
        }
        public string MajorCode
        {
            get { return GetProperty(() => MajorCode); }
            set { SetProperty(() => MajorCode, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public string MinorName
        {
            get { return GetProperty(() => MinorName); }
            set { SetProperty(() => MinorName, value); }
        }
        public string Ref01
        {
            get { return GetProperty(() => Ref01); }
            set { SetProperty(() => Ref01, value); }
        }
        public string Ref02
        {
            get { return GetProperty(() => Ref02); }
            set { SetProperty(() => Ref02, value); }
        }
        public string Ref03
        {
            get { return GetProperty(() => Ref03); }
            set { SetProperty(() => Ref03, value); }
        }
        public string Ref04
        {
            get { return GetProperty(() => Ref04); }
            set { SetProperty(() => Ref04, value); }
        }
        public string Ref05
        {
            get { return GetProperty(() => Ref05); }
            set { SetProperty(() => Ref05, value); }
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

    public class CommonMinorList : ObservableCollection<CommonMinor>
    {
        private string majorCode;
        private string minorCode;

        public CommonMinorList(IEnumerable<CommonMinor> items) : base(items) { }
        public CommonMinorList(string majorCode = "", string minorCode = "")
        {
            this.majorCode = majorCode;
            this.minorCode = minorCode;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;

            string str = "SELECT * FROM common_Minor WHERE MinorCode != '' ";
            if (majorCode != "")
                str += " And MajorCode = '" + majorCode + "'";
            if (minorCode != "")
                str += " And MinorCode = '" + minorCode + "'";

            DbCommand dbCom = db.GetSqlStringCommand(str);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new CommonMinor
                    {
                        State = EntityState.Unchanged,
                        MinorCode = (string)u["MinorCode"],
                        MajorCode = (string)u["MajorCode"],
                        MinorName = (string)u["MinorName"],
                        Ref01 = u["Ref01"].ToString(),
                        Ref02 = u["Ref02"].ToString(),
                        Ref03 = u["Ref03"].ToString(),
                        Ref04 = u["Ref04"].ToString(),
                        Ref05 = u["Ref05"].ToString(),
                        IsEnabled = (bool)u["IsEnabled"],
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"]
                    }
                )
            );
        }

        public void Save()
        {
            IEnumerable<CommonMinor> items = this.Items;
            Insert(items.Where(u => u.State == EntityState.Added));
            Update(items.Where(u => u.State == EntityState.Modified));
            Delete(items.Where(u => u.State == EntityState.Deleted));

            // Global 기준정보를 다시 가져오기 위해 Instance 초기화
            GlobalCommonMinor.Instance = null;
        }

        public void Insert(IEnumerable<CommonMinor> items)
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
                    foreach (CommonMinor item in items)
                    {
                        str = "INSERT INTO common_Minor(MinorCode, MajorCode, MinorName, Ref01, Ref02, Ref03, Ref04, Ref05, IsEnabled, InsertId, UpdateId, UpdateDate) VALUES "
                            + "(@MinorCode, @MajorCode, @MinorName, @Ref01, @Ref02, @Ref03, @Ref04, @Ref05, @IsEnabled, @InsertId, @InsertId, getdate())";
                       
                        dbCom = db.GetSqlStringCommand(str);
                        db.AddInParameter(dbCom, "@MinorCode", DbType.String, item.MinorCode);
                        db.AddInParameter(dbCom, "@MajorCode", DbType.String, item.MajorCode);
                        db.AddInParameter(dbCom, "@MinorName", DbType.String, item.MinorName);
                        db.AddInParameter(dbCom, "@Ref01", DbType.String, item.Ref01);
                        db.AddInParameter(dbCom, "@Ref02", DbType.String, item.Ref02);
                        db.AddInParameter(dbCom, "@Ref03", DbType.String, item.Ref03);
                        db.AddInParameter(dbCom, "@Ref04", DbType.String, item.Ref04);
                        db.AddInParameter(dbCom, "@Ref05", DbType.String, item.Ref05);
                        db.AddInParameter(dbCom, "@IsEnabled", DbType.Boolean, item.IsEnabled);
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
        public void Update(IEnumerable<CommonMinor> items)
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
                    foreach (CommonMinor item in items)
                    {
                        str = "UPDATE common_Minor SET MinorName = @MinorName, Ref01 = @Ref01, Ref02 = @Ref02, Ref03 = @Ref03, Ref04 = @Ref04, Ref05 = @Ref05, IsEnabled = @IsEnabled, UpdateId = @InsertId, UpdateDate = getdate() WHERE MajorCode = @MajorCode AND MinorCode = @MinorCode";
                        dbCom = db.GetSqlStringCommand(str);
                        db.AddInParameter(dbCom, "@MinorCode", DbType.String, item.MinorCode);
                        db.AddInParameter(dbCom, "@MajorCode", DbType.String, item.MajorCode);
                        db.AddInParameter(dbCom, "@MinorName", DbType.String, item.MinorName);
                        db.AddInParameter(dbCom, "@Ref01", DbType.String, item.Ref01);
                        db.AddInParameter(dbCom, "@Ref02", DbType.String, item.Ref02);
                        db.AddInParameter(dbCom, "@Ref03", DbType.String, item.Ref03);
                        db.AddInParameter(dbCom, "@Ref04", DbType.String, item.Ref04);
                        db.AddInParameter(dbCom, "@Ref05", DbType.String, item.Ref05);
                        db.AddInParameter(dbCom, "@IsEnabled", DbType.Boolean, item.IsEnabled);
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

        public void Delete(IEnumerable<CommonMinor> items)
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
                    foreach (CommonMinor item in items)
                    {
                        str = string.Format("DELETE common_Minor WHERE MajorCode = '{0}' AND MinorCode = '{1}' ", item.MajorCode, item.MinorCode);
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
