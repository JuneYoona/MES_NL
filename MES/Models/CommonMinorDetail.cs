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
    public class CommonMinorDetail : StateBusinessObject
    {
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public string DetailCode
        {
            get { return GetProperty(() => DetailCode); }
            set { SetProperty(() => DetailCode, value); }
        }
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
        public string DetailName
        {
            get { return GetProperty(() => DetailName); }
            set { SetProperty(() => DetailName, value); }
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

    public class CommonMinorDetailList : ObservableCollection<CommonMinorDetail>
    {
        private string majorCode;
        private string minorCode;

        public CommonMinorDetailList(IEnumerable<CommonMinorDetail> items) : base(items) { }
        public CommonMinorDetailList(string majorCode, string minorCode)
        {
            this.majorCode = majorCode;
            this.minorCode = minorCode;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;

            string str = "SELECT * FROM common_Minor_Detail WHERE MajorCode = '" + majorCode + "' AND MinorCode = '" + minorCode + "'";
           
            DbCommand dbCom = db.GetSqlStringCommand(str);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new CommonMinorDetail
                    {
                        State = MesAdmin.Common.Common.EntityState.Unchanged,
                        DetailCode = (string)u["DetailCode"],
                        MinorCode = (string)u["MinorCode"],
                        MajorCode = (string)u["MajorCode"],
                        DetailName = (string)u["DetailName"],
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
            IEnumerable<CommonMinorDetail> items = this.Items;
            Insert(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Added));
            Update(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Modified));
            Delete(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Deleted));

            // Global 기준정보를 다시 가져오기 위해 Instance 초기화
            GlobalCommonMinor.Instance = null;
        }

        public void Insert(IEnumerable<CommonMinorDetail> items)
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
                    foreach (CommonMinorDetail item in items)
                    {
                        str = "INSERT INTO common_Minor_Detail(DetailCode, MinorCode, MajorCode, DetailName, Ref01, Ref02, Ref03, Ref04, Ref05, IsEnabled, InsertId, UpdateId, UpdateDate) VALUES "
                            + "(@DetailCode, @MinorCode, @MajorCode, @DetailName, @Ref01, @Ref02, @Ref03, @Ref04, @Ref05, @IsEnabled, @InsertId, @InsertId, getdate())";

                        dbCom = db.GetSqlStringCommand(str);
                        db.AddInParameter(dbCom, "@DetailCode", DbType.String, item.DetailCode);
                        db.AddInParameter(dbCom, "@MinorCode", DbType.String, item.MinorCode);
                        db.AddInParameter(dbCom, "@MajorCode", DbType.String, item.MajorCode);
                        db.AddInParameter(dbCom, "@DetailName", DbType.String, item.DetailName);
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
        public void Update(IEnumerable<CommonMinorDetail> items)
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
                    foreach (CommonMinorDetail item in items)
                    {
                        str = "UPDATE common_Minor_Detail SET DetailName = @DetailName, Ref01 = @Ref01, Ref02 = @Ref02, Ref03 = @Ref03, Ref04 = @Ref04, Ref05 = @Ref05, IsEnabled = @IsEnabled, UpdateId = @UpdateId, UpdateDate = getdate() "
                            + "WHERE DetailCode = @DetailCode AND MajorCode = @MajorCode AND MinorCode = @MinorCode";
                        dbCom = db.GetSqlStringCommand(str);
                        db.AddInParameter(dbCom, "@DetailCode", DbType.String, item.DetailCode);
                        db.AddInParameter(dbCom, "@MinorCode", DbType.String, item.MinorCode);
                        db.AddInParameter(dbCom, "@MajorCode", DbType.String, item.MajorCode);
                        db.AddInParameter(dbCom, "@DetailName", DbType.String, item.DetailName);
                        db.AddInParameter(dbCom, "@Ref01", DbType.String, item.Ref01);
                        db.AddInParameter(dbCom, "@Ref02", DbType.String, item.Ref02);
                        db.AddInParameter(dbCom, "@Ref03", DbType.String, item.Ref03);
                        db.AddInParameter(dbCom, "@Ref04", DbType.String, item.Ref04);
                        db.AddInParameter(dbCom, "@Ref05", DbType.String, item.Ref05);
                        db.AddInParameter(dbCom, "@IsEnabled", DbType.Boolean, item.IsEnabled);
                        db.AddInParameter(dbCom, "@UpdateId", DbType.String, DSUser.Instance.UserID);
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

        public void Delete(IEnumerable<CommonMinorDetail> items)
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
                    foreach (CommonMinorDetail item in items)
                    {
                        str = string.Format("DELETE common_Minor_Detail WHERE MajorCode = '{0}' AND MinorCode = '{1}' AND DetailCode = '{2}' ", item.MajorCode, item.MinorCode, item.DetailCode);
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
