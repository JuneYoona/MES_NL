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
    public class CommonEquipment : StateBusinessObject
    {
        [Required(ErrorMessage = "필수입력값 입니다.")]
        public string BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value); }
        }
        [Required(ErrorMessage = "필수입력값 입니다.")]
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
        public string WaCode
        {
            get { return GetProperty(() => WaCode); }
            set { SetProperty(() => WaCode, value); }
        }
        public string PauseGroup
        {
            get { return GetProperty(() => PauseGroup); }
            set { SetProperty(() => PauseGroup, value); }
        }
        public string EqpState
        {
            get { return GetProperty(() => EqpState); }
            set { SetProperty(() => EqpState, value); }
        }
        public string LeadTime
        {
            get { return GetProperty(() => LeadTime); }
            set { SetProperty(() => LeadTime, value); }
        }
        public string PauseTime
        {
            get { return GetProperty(() => PauseTime); }
            set { SetProperty(() => PauseTime, value); }
        }
        public bool IsEnabled
        {
            get { return GetProperty(() => IsEnabled); }
            set { SetProperty(() => IsEnabled, value); }
        }
        public bool? IsMonitor
        {
            get { return GetProperty(() => IsMonitor); }
            set { SetProperty(() => IsMonitor, value); }
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

    public class CommonEquipmentList : ObservableCollection<CommonEquipment>
    {
        public CommonEquipmentList(IEnumerable<CommonEquipment> items) : base(items) { }
        public CommonEquipmentList()
        {
            InitializeList();
        }

        public void SyncErp()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbCommand dbCom = db.GetSqlStringCommand("EXEC USP_IF_ERP2MES_FACILITY_MST_KO656");
                db.ExecuteNonQuery(dbCom);
            }

            // Global 기준정보를 다시 가져오기 위해 Instance 초기화
            GlobalCommonEquipment.Instance = null;
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;

            DbCommand dbCom = db.GetSqlStringCommand("SELECT * FROM fn_CommonEquipment() ORDER BY WaCode");
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new CommonEquipment
                    {
                        State = MesAdmin.Common.Common.EntityState.Unchanged,
                        BizAreaCode = (string)u["BizAreaCode"],
                        EqpCode = (string)u["EqpCode"],
                        EqpName = (string)u["EqpName"],
                        WaCode = u["WaCode"].ToString(),
                        EqpState = (string)u["EqpState"],
                        PauseGroup = u["PauseGroup"].ToString(),
                        IsEnabled = (bool)u["IsEnabled"],
                        IsMonitor = DBNull.Value == u["IsMonitor"] ? false : (bool?)u["IsMonitor"],
                        LeadTime = (string)u["LeadTime"],
                        PauseTime = u["PauseTime"].ToString(),
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"],
                    }
                )
            );
        }

        public void Save()
        {
            IEnumerable<CommonEquipment> items = this.Items;
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = null;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                try
                {
                    Insert(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Added), db, trans, dbCom);
                    Update(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Modified), db, trans, dbCom);
                    Delete(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Deleted), db, trans, dbCom);
                    trans.Commit();

                    // Global 기준정보를 다시 가져오기 위해 Instance 초기화
                    GlobalCommonEquipment.Instance = null;
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public void Insert(IEnumerable<CommonEquipment> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            foreach (CommonEquipment item in items)
            {
                dbCom = db.GetStoredProcCommand("usp_common_Equipment");
                dbCom.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, item.BizAreaCode);
                db.AddInParameter(dbCom, "@EqpCode", DbType.String, item.EqpCode);
                db.AddInParameter(dbCom, "@EqpName", DbType.String, item.EqpName);
                db.AddInParameter(dbCom, "@WaCode", DbType.String, item.WaCode);
                db.AddInParameter(dbCom, "@EqpState", DbType.String, item.EqpState);
                db.AddInParameter(dbCom, "@PauseGroup", DbType.String, item.PauseGroup);
                db.AddInParameter(dbCom, "@IsEnabled", DbType.Boolean, item.IsEnabled);
                db.AddInParameter(dbCom, "@IsMonitor", DbType.Boolean, item.IsMonitor);
                db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                db.AddInParameter(dbCom, "@OpType", DbType.String, "I");
                db.ExecuteNonQuery(dbCom, trans);
            }
        }

        public void Delete(IEnumerable<CommonEquipment> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            foreach (CommonEquipment item in items)
            {
                dbCom = db.GetStoredProcCommand("usp_common_Equipment");
                dbCom.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, item.BizAreaCode);
                db.AddInParameter(dbCom, "@EqpCode", DbType.String, item.EqpCode);
                db.AddInParameter(dbCom, "@EqpName", DbType.String, item.EqpName);
                db.AddInParameter(dbCom, "@WaCode", DbType.String, item.WaCode);
                db.AddInParameter(dbCom, "@EqpState", DbType.String, item.EqpState);
                db.AddInParameter(dbCom, "@PauseGroup", DbType.String, item.PauseGroup);
                db.AddInParameter(dbCom, "@IsEnabled", DbType.Boolean, item.IsEnabled);
                db.AddInParameter(dbCom, "@IsMonitor", DbType.Boolean, item.IsMonitor);
                db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                db.AddInParameter(dbCom, "@OpType", DbType.String, "X");
                db.ExecuteNonQuery(dbCom, trans);
            }
        }

        public void Update(IEnumerable<CommonEquipment> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            foreach (CommonEquipment item in items)
            {
                dbCom = db.GetStoredProcCommand("usp_common_Equipment");
                dbCom.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, item.BizAreaCode);
                db.AddInParameter(dbCom, "@EqpCode", DbType.String, item.EqpCode);
                db.AddInParameter(dbCom, "@EqpName", DbType.String, item.EqpName);
                db.AddInParameter(dbCom, "@WaCode", DbType.String, item.WaCode);
                db.AddInParameter(dbCom, "@EqpState", DbType.String, item.EqpState);
                db.AddInParameter(dbCom, "@PauseGroup", DbType.String, item.PauseGroup);
                db.AddInParameter(dbCom, "@IsEnabled", DbType.Boolean, item.IsEnabled);
                db.AddInParameter(dbCom, "@IsMonitor", DbType.Boolean, item.IsMonitor);
                db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                db.AddInParameter(dbCom, "@OpType", DbType.String, "U");
                db.ExecuteNonQuery(dbCom, trans);
            }
        }
    }
}
