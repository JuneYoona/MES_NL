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
    public class CommonEquipmentParameter : StateBusinessObject
    {
        public string BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value); }
        }
        public string EqpCode
        {
            get { return GetProperty(() => EqpCode); }
            set { SetProperty(() => EqpCode, value); }
        }
        public string Seq
        {
            get { return GetProperty(() => Seq); }
            set { SetProperty(() => Seq, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public int? Order
        {
            get { return GetProperty(() => Order); }
            set { SetProperty(() => Order, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public new string Parameter
        {
            get { return GetProperty(() => Parameter); }
            set { SetProperty(() => Parameter, value); }
        }
        public string ParameterSpec
        {
            get { return GetProperty(() => ParameterSpec); }
            set { SetProperty(() => ParameterSpec, value); }
        }
        public string DownRate
        {
            get { return GetProperty(() => DownRate); }
            set { SetProperty(() => DownRate, value); }
        }
        public string Remark
        {
            get { return GetProperty(() => Remark); }
            set { SetProperty(() => Remark, value); }
        }
        public string UpRate
        {
            get { return GetProperty(() => UpRate); }
            set { SetProperty(() => UpRate, value); }
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

    public class CommonEquipmentParameterList : ObservableCollection<CommonEquipmentParameter>
    {
        private string eqpCode;
        private string seq;

        public CommonEquipmentParameterList() { }
        public CommonEquipmentParameterList(IEnumerable<CommonEquipmentParameter> items) : base(items) { }
        public CommonEquipmentParameterList(string eqpCode, string seq)
        {
            this.eqpCode = eqpCode;
            this.seq = seq;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            string sql;
            sql = "SELECT * FROM common_EquipmentParameter (NOLOCK) WHERE EqpCode='" + eqpCode + "' AND Seq = '" + seq + "' ORDER BY [Order]";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new CommonEquipmentParameter
                    {
                        State = MesAdmin.Common.Common.EntityState.Unchanged,
                        BizAreaCode = (string)u["BizAreaCode"],
                        EqpCode = (string)u["EqpCode"],
                        Seq = (string)u["Seq"],
                        Parameter = (string)u["Parameter"],
                        ParameterSpec = u["ParameterSpec"].ToString(),
                        DownRate = u["DownRate"].ToString(),
                        UpRate = u["UpRate"].ToString(),
                        Order = (Int16)u["Order"],
                        Remark = u["Remark"].ToString(),
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"]
                    }
                )
            );
        }

        public void Save()
        {
            IEnumerable<CommonEquipmentParameter> items = this.Items;
            Insert(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Added));
            Update(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Modified));
            Delete(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Deleted));
        }

        public void Insert(IEnumerable<CommonEquipmentParameter> items)
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                string sql;
                try
                {
                    foreach (CommonEquipmentParameter item in items)
                    {
                        sql = "INSERT INTO common_EquipmentParameter(BizAreaCode, EqpCode, Seq, [Order], Parameter, ParameterSpec, UpRate, DownRate, Remark, InsertId, UpdateId) VALUES "
                            + "(@BizAreaCode, @EqpCode, @Seq, @Order, @Parameter, @ParameterSpec, @UpRate, @DownRate, @Remark, @InsertId, @InsertId)";
                        dbCom = db.GetSqlStringCommand(sql);
                        db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, item.BizAreaCode);
                        db.AddInParameter(dbCom, "@EqpCode", DbType.String, item.EqpCode);
                        db.AddInParameter(dbCom, "@Seq", DbType.String, item.Seq);
                        db.AddInParameter(dbCom, "@Order", DbType.Int16, item.Order);
                        db.AddInParameter(dbCom, "@Parameter", DbType.String, item.Parameter);
                        db.AddInParameter(dbCom, "@ParameterSpec", DbType.String, item.ParameterSpec);
                        db.AddInParameter(dbCom, "@UpRate", DbType.String, item.UpRate);
                        db.AddInParameter(dbCom, "@DownRate", DbType.String, item.DownRate);
                        db.AddInParameter(dbCom, "@Remark", DbType.String, item.Remark);
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

        public void Update(IEnumerable<CommonEquipmentParameter> items)
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                string sql;
                try
                {
                    foreach (CommonEquipmentParameter item in items)
                    {
                        sql = "UPDATE common_EquipmentParameter SET [Order] = @Order "
                            + ", ParameterSpec = @ParameterSpec "
                            + ", DownRate = @DownRate "
                            + ", UpRate = @UpRate "
                            + ", Remark = @Remark "
                            + ", UpdateId = @UpdateId "
                            + ", UpdateDate = getdate() "
                            + "WHERE EqpCode = @EqpCode AND Seq = @Seq AND Parameter = @Parameter";
                        dbCom = db.GetSqlStringCommand(sql);
                        db.AddInParameter(dbCom, "@EqpCode", DbType.String, item.EqpCode);
                        db.AddInParameter(dbCom, "@Seq", DbType.String, item.Seq);
                        db.AddInParameter(dbCom, "@Parameter", DbType.String, item.Parameter);
                        db.AddInParameter(dbCom, "@ParameterSpec", DbType.String, item.ParameterSpec);
                        db.AddInParameter(dbCom, "@Order", DbType.Int16, item.Order);
                        db.AddInParameter(dbCom, "@DownRate", DbType.String, item.DownRate);
                        db.AddInParameter(dbCom, "@UpRate", DbType.String, item.UpRate);
                        db.AddInParameter(dbCom, "@Remark", DbType.String, item.Remark);
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

        public void Delete(IEnumerable<CommonEquipmentParameter> items)
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                string sql;
                try
                {
                    foreach (CommonEquipmentParameter item in items)
                    {
                        sql = "DELETE common_EquipmentParameter WHERE EqpCode = @EqpCode AND Seq = @Seq AND Parameter = @Parameter";
                        dbCom = db.GetSqlStringCommand(sql);
                        db.AddInParameter(dbCom, "@EqpCode", DbType.String, item.EqpCode);
                        db.AddInParameter(dbCom, "@Seq", DbType.String, item.Seq);
                        db.AddInParameter(dbCom, "@Parameter", DbType.String, item.Parameter);
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