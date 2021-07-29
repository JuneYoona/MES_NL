using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using System.Linq;
using DevExpress.Mvvm;
using System.Data.SqlClient;

namespace MesAdmin.Models
{
    public class QualityPrecedenceLot : ViewModelBase
    {
        public string QrNo
        {
            get { return GetProperty(() => QrNo); }
            set { SetProperty(() => QrNo, value); }
        }
        public string BizCode
        {
            get { return GetProperty(() => BizCode); }
            set { SetProperty(() => BizCode, value); }
        }
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
        public string LotNoWE10
        {
            get { return GetProperty(() => LotNoWE10); }
            set { SetProperty(() => LotNoWE10, value); }
        }
        public string LotNo
        {
            get { return GetProperty(() => LotNo); }
            set { SetProperty(() => LotNo, value); }
        }
        public decimal? Qty
        {
            get { return GetProperty(() => Qty); }
            set { SetProperty(() => Qty, value); }
        }
        public string BasicUnit
        {
            get { return GetProperty(() => BasicUnit); }
            set { SetProperty(() => BasicUnit, value); }
        }
        public DateTime? ReqDate
        {
            get { return GetProperty(() => ReqDate); }
            set { SetProperty(() => ReqDate, value); }
        }
        public string BizName
        {
            get { return GetProperty(() => BizName); }
            set { SetProperty(() => BizName, value); }
        }
        public string RegId
        {
            get { return GetProperty(() => RegId); }
            set { SetProperty(() => RegId, value); }
        }
        public string RegName
        {
            get { return GetProperty(() => RegName); }
            set { SetProperty(() => RegName, value); }
        }
        public string Memo1
        {
            get { return GetProperty(() => Memo1); }
            set { SetProperty(() => Memo1, value); }
        }
        public string Result
        {
            get { return GetProperty(() => Result); }
            set { SetProperty(() => Result, value); }
        }
        public string InspectorId
        {
            get { return GetProperty(() => InspectorId); }
            set { SetProperty(() => InspectorId, value); }
        }
        public string InspectorName
        {
            get { return GetProperty(() => InspectorName); }
            set { SetProperty(() => InspectorName, value); }
        }
        public DateTime? InspectDate
        {
            get { return GetProperty(() => InspectDate); }
            set { SetProperty(() => InspectDate, value); }
        }
        public string Memo2
        {
            get { return GetProperty(() => Memo2); }
            set { SetProperty(() => Memo2, value); }
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

        public QualityPrecedenceLot() { }
        public QualityPrecedenceLot(string qrNo)
        {
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("usps_quality_PrecedenceLot");
            db.AddInParameter(dbCom, "@QrNo", DbType.String, qrNo);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u => {
                QrNo = (string)u["QrNo"];
                BizCode = u["BizCode"].ToString();
                ItemCode = (string)u["ItemCode"];
                ItemName = (string)u["ItemName"];
                ItemSpec = (string)u["ItemSpec"];
                LotNoWE10 = (string)u["LotNoWE10"];
                LotNo = (string)u["LotNo"];
                Qty = (decimal?)u["Qty"];
                BasicUnit = (string)u["BasicUnit"];
                ReqDate = (DateTime?)u["ReqDate"];
                BizName = u["BizName"].ToString();
                RegId = (string)u["RegId"];
                RegName = (string)u["RegName"];
                Memo1 = u["Memo1"].ToString();
                InspectorId = u["InspectorId"].ToString();
                InspectDate = u["InspectDate"] == DBNull.Value ? null : (DateTime?)u["InspectDate"];
                Result = u["Result"].ToString();
                Memo2 = u["Memo2"].ToString();
                UpdateId = (string)u["UpdateId"];
                UpdateDate = (DateTime)u["UpdateDate"];
            });
        }

        NetUsers users = null;
        protected string GetInspectorName(string inspectorId)
        {
            if (string.IsNullOrEmpty(inspectorId)) return string.Empty;

            if (users == null)
                users = NetUsers.Select();
            var user = users.Where(u => u.UserName == inspectorId);
            return user.FirstOrDefault().Profile.KorName;
        }

        public void Save()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    dbCom = db.GetStoredProcCommand("usps_quality_PrecedenceLot_Save");
                    db.AddInParameter(dbCom, "@QrNo", DbType.String, QrNo);
                    db.AddInParameter(dbCom, "@BizCode", DbType.String, BizCode);
                    db.AddInParameter(dbCom, "@ItemCode", DbType.String, ItemCode);
                    db.AddInParameter(dbCom, "@LotNo", DbType.String, LotNo);
                    db.AddInParameter(dbCom, "@Qty", DbType.Decimal, Qty);
                    db.AddInParameter(dbCom, "@LotNoWE10", DbType.String, LotNoWE10);
                    db.AddInParameter(dbCom, "@ReqDate", DbType.Date, ReqDate);
                    db.AddInParameter(dbCom, "@RegId", DbType.String, RegId);
                    db.AddInParameter(dbCom, "@RegName", DbType.String, GetInspectorName(RegId));
                    db.AddInParameter(dbCom, "@Memo1", DbType.String, Memo1);
                    db.AddInParameter(dbCom, "@InspectorId", DbType.String, InspectorId);
                    db.AddInParameter(dbCom, "@InspectorName", DbType.String, GetInspectorName(InspectorId));
                    db.AddInParameter(dbCom, "@InspectDate", DbType.Date, InspectDate);
                    db.AddInParameter(dbCom, "@Result", DbType.String, Result);
                    db.AddInParameter(dbCom, "@Memo2", DbType.String, Memo2);
                    db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                    db.AddOutParameter(dbCom, "@QrNoOut", DbType.String, 20);
                    db.ExecuteNonQuery(dbCom, trans);
                    QrNo = db.GetParameterValue(dbCom, "@QrNoOut").ToString();
                    trans.Commit();
                }
                catch (SqlException ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Errors[0].Message);
                }
            }
        }

        public void Delete()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    dbCom = db.GetSqlStringCommand("DELETE quality_PrecedenceLot WHERE QrNo = @QrNo");
                    db.AddInParameter(dbCom, "@QrNo", DbType.String, QrNo);
                    db.ExecuteNonQuery(dbCom, trans);
                    trans.Commit();
                }
                catch (SqlException ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Errors[0].Message);
                }
            }
        }

        public void GetLotNoWE10()
        {
            Database db = ProviderFactory.Instance;
            string str = "SELECT Remark4 FROM production_InputRecord (NOLOCK) WHERE LotNo = @LotNo";
            DbCommand dbCom = db.GetSqlStringCommand(str);
            db.AddInParameter(dbCom, "@LotNo", DbType.String, LotNo);
            DataSet ds = db.ExecuteDataSet(dbCom);

            if(ds.Tables[0].Rows.Count > 0)
                LotNoWE10 = ds.Tables[0].Rows[0][0].ToString();
        }

        public DataTable GetQualityPrecedenceLotList(DateTime startDate, DateTime endDate, string itemCode, string bizCode, string state)
        {
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("usps_quality_PrecedenceLot_List");
            db.AddInParameter(dbCom, "@StartDate", DbType.Date, startDate);
            db.AddInParameter(dbCom, "@EndDate", DbType.Date, endDate);
            db.AddInParameter(dbCom, "@ItemCode", DbType.String, itemCode);
            db.AddInParameter(dbCom, "@BizCode", DbType.String, bizCode);
            DataSet ds = db.ExecuteDataSet(dbCom);
            DataTable dt = ds.Tables[0];

            if (state == "Complete")
            {
                var rows = dt.AsEnumerable().Where(o => !string.IsNullOrEmpty(o.Field<string>("Result")));
                dt = rows.Any() ? rows.CopyToDataTable() : null;
            }
            else if (state == "NotComplete")
            {
                var rows = dt.AsEnumerable().Where(o => string.IsNullOrEmpty(o.Field<string>("Result")));
                dt = rows.Any() ? rows.CopyToDataTable() : null;
            }

            return dt;
        }
    }
}