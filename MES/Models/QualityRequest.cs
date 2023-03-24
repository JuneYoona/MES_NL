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
    public class QualityRequest : ViewModelBase
    {
        public string QrNo
        {
            get { return GetProperty(() => QrNo); }
            set { SetProperty(() => QrNo, value); }
        }
        public string QrType
        {
            get { return GetProperty(() => QrType); }
            set { SetProperty(() => QrType, value); }
        }
        public DateTime QrRequestDate
        {
            get { return GetProperty(() => QrRequestDate); }
            set { SetProperty(() => QrRequestDate, value); }
        }
        public string BizCode
        {
            get { return GetProperty(() => BizCode); }
            set { SetProperty(() => BizCode, value); }
        }
        public string ItemAccount
        {
            get { return GetProperty(() => ItemAccount); }
            set { SetProperty(() => ItemAccount, value); }
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
        public string PoNo
        {
            get { return GetProperty(() => PoNo); }
            set { SetProperty(() => PoNo, value); }
        }
        public int? PoSeq
        {
            get { return GetProperty(() => PoSeq); }
            set { SetProperty(() => PoSeq, value); }
        }
        public string LotNo
        {
            get { return GetProperty(() => LotNo); }
            set { SetProperty(() => LotNo, value); }
        }
        public decimal Qty
        {
            get { return GetProperty(() => Qty); }
            set { SetProperty(() => Qty, value); }
        }
        public string BasicUnit
        {
            get { return GetProperty(() => BasicUnit); }
            set { SetProperty(() => BasicUnit, value); }
        }
        public DateTime? InspectDate
        {
            get { return GetProperty(() => InspectDate); }
            set { SetProperty(() => InspectDate, value); }
        }
        public string InspectorId
        {
            get { return GetProperty(() => InspectorId); }
            set { SetProperty(() => InspectorId, value); }
        }
        public string Result
        {
            get { return GetProperty(() => Result); }
            set { SetProperty(() => Result, value); }
        }
        public string WhCode
        {
            get { return GetProperty(() => WhCode); }
            set { SetProperty(() => WhCode, value); }
        }
        public string GrNo
        {
            get { return GetProperty(() => GrNo); }
            set { SetProperty(() => GrNo, value); }
        }
        public int? GrSeq
        {
            get { return GetProperty(() => GrSeq); }
            set { SetProperty(() => GrSeq, value); }
        }
        public string PmNo
        {
            get { return GetProperty(() => PmNo); }
            set { SetProperty(() => PmNo, value); }
        }
        public int? PmSeq
        {
            get { return GetProperty(() => PmSeq); }
            set { SetProperty(() => PmSeq, value); }
        }
        public int ResultOrder
        {
            get { return GetProperty(() => ResultOrder); }
            set { SetProperty(() => ResultOrder, value); }
        }
        public int LastOrder
        {
            get { return GetProperty(() => LastOrder); }
            set { SetProperty(() => LastOrder, value); }
        }
        public bool Status
        {
            get { return GetProperty(() => Status); }
            set { SetProperty(() => Status, value); }
        }
        public bool LossFlag
        {
            get { return GetProperty(() => LossFlag); }
            set { SetProperty(() => LossFlag, value); }
        }
        public bool TransferFlag
        {
            get { return GetProperty(() => TransferFlag); }
            set { SetProperty(() => TransferFlag, value); }
        }
        public string BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value); }
        }
        public string Memo
        {
            get { return GetProperty(() => Memo); }
            set { SetProperty(() => Memo, value); }
        }
        public string DnNo
        {
            get { return GetProperty(() => DnNo); }
            set { SetProperty(() => DnNo, value); }
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
        public string SoNo
        {
            get { return GetProperty(() => SoNo); }
            set { SetProperty(() => SoNo, value); }
        }
        public string ReqNo
        {
            get { return GetProperty(() => ReqNo); }
            set { SetProperty(() => ReqNo, value); }
        }
        public int? ReqSeq
        {
            get { return GetProperty(() => ReqSeq); }
            set { SetProperty(() => ReqSeq, value); }
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

        public QualityRequest() { }
        public QualityRequest(string qrNo, int order)
        {
            Database db = ProviderFactory.Instance;
            string sql;
            sql = "SELECT * FROM view_quality_Request WHERE QrNo = '" + qrNo + "' AND ResultOrder=" + order;

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u => {
                BizAreaCode = (string)u["BizAreaCode"];
                QrNo = (string)u["QrNo"];
                QrType = (string)u["QrType"];
                ItemAccount = (string)u["ItemAccount"];
                QrRequestDate = (DateTime)u["QrRequestDate"];
                BizCode = (string)u["BizCode"];
                ItemCode = (string)u["ItemCode"];
                ItemName = (string)u["ItemName"];
                PoNo = (string)u["PoNo"];
                PoSeq = (int?)u["PoSeq"];
                LotNo = (string)u["LotNo"];
                Qty = (decimal)u["Qty"];
                InspectDate = u["InspectDate"] == DBNull.Value ? null : (DateTime?)u["InspectDate"];
                InspectorId = u["InspectorId"].ToString();
                Result = u["Result"].ToString();
                WhCode = (string)u["WhCode"];
                GrNo = (string)u["GrNo"];
                GrSeq = (int?)u["GrSeq"];
                PmNo = (string)u["PmNo"];
                PmSeq = (int?)u["PmSeq"];
                ResultOrder = (int)u["ResultOrder"];
                LastOrder = (int)u["LastOrder"];
                Status = (bool)u["Status"];
                TransferFlag = (bool)u["TransferFlag"];
                LossFlag = (bool)u["LossFlag"];
                Memo = u["Memo"].ToString();
                DnNo = u["DnNo"].ToString();
                ReqDate = u["ReqDate"] == DBNull.Value ? null : (DateTime?)u["ReqDate"];
                BizName = u["BizName"].ToString();
                SoNo = u["SoNo"].ToString();
                ReqNo = u["ReqNo"].ToString();
                ReqSeq = u["ReqSeq"] == DBNull.Value ? null : (int?)u["ReqSeq"];
                UpdateId = (string)u["UpdateId"];
                UpdateDate = (DateTime)u["UpdateDate"];
            });
        }

        public void TransferStock()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    if (QrType == "IQC")
                    {
                        dbCom = db.GetStoredProcCommand("usp_purcharse_Input");
                        dbCom.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(dbCom, "@PmNo", DbType.String, PmNo);
                        db.AddInParameter(dbCom, "@PmSeq", DbType.String, PmSeq);
                        db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                    }
                    else if (QrType == "LQC" || QrType == "FQC" || QrType == "OQC")
                    {
                        dbCom = db.GetStoredProcCommand("usp_Quality_Input");
                        dbCom.CommandType = CommandType.StoredProcedure;
                        db.AddInParameter(dbCom, "@QrNo", DbType.String, QrNo);
                    }
                    else { }

                    db.ExecuteNonQuery(dbCom, trans);

                    string str = string.Format("UPDATE quality_Request SET TransferFlag = 1 WHERE QrNo='{0}'", QrNo);
                    dbCom = db.GetSqlStringCommand(str);
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

        public void TransferStockCancel()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    string str = string.Format("UPDATE stock_Movement_Detail SET DelFlag = 'Y', UpdateId = '{2}', UpdateDate = getdate() WHERE DocumentNo = '{0}' AND Seq = {1}"
                        , PmNo
                        , PmSeq
                        , DSUser.Instance.UserID);
                    dbCom = db.GetSqlStringCommand(str);
                    db.ExecuteNonQuery(dbCom, trans);

                    str = string.Format("UPDATE quality_Request SET TransferFlag = 0 WHERE QrNo='{0}'", QrNo);
                    dbCom = db.GetSqlStringCommand(str);
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

        public void LossStock()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    dbCom = db.GetStoredProcCommand("usp_Quality_Stock_Movement");
                    dbCom.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(dbCom, "@QrNo", DbType.String, QrNo);
                    db.AddInParameter(dbCom, "@TransType", DbType.String, "OI");
                    db.AddInParameter(dbCom, "@MoveType", DbType.String, "I98");
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

        public void LossStockCancel()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    string sql = "UPDATE stock_Movement_Detail SET DelFlag = 'Y' WHERE DocumentNo = (SELECT PmNo FROM quality_Request (NOLOCK) WHERE QrNo = @QrNo) ";
                    sql += "UPDATE quality_Request SET LossFlag = 0 WHERE QrNo = @QrNo";

                    dbCom = db.GetSqlStringCommand(sql);
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
    }

    public class QualityRequestList : ObservableCollection<QualityRequest>
    {
        private DateTime startDate;
        private DateTime endDate;
        private string qrNo;
        private string qrType;
        private string bizCode;
        private string lotNo;
        private string bizAreaCode;

        public QualityRequestList() { }
        public QualityRequestList(IEnumerable<QualityRequest> items) : base(items) { }
        public QualityRequestList(DateTime startDate, DateTime endDate, string qrNo = "", string qrType = "", string bizCode = "", string lotNo = "", string bizAreaCode = "")
        {
            this.startDate = startDate;
            this.endDate = endDate;
            this.qrNo = qrNo;
            this.qrType = qrType;
            this.bizCode = bizCode;
            this.lotNo = lotNo;
            this.bizAreaCode = bizAreaCode;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("usp_QualityRequest");
            dbCom.CommandType = CommandType.StoredProcedure;
            db.AddInParameter(dbCom, "@QrNo", DbType.String, qrNo);
            db.AddInParameter(dbCom, "@QrType", DbType.String, qrType);
            db.AddInParameter(dbCom, "@LotNo", DbType.String, lotNo);
            db.AddInParameter(dbCom, "@BizCode", DbType.String, bizCode);
            db.AddInParameter(dbCom, "@StartDate", DbType.String, startDate.ToShortDateString());
            db.AddInParameter(dbCom, "@EndDate", DbType.String, endDate.ToShortDateString());
            db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, bizAreaCode);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new QualityRequest
                    {
                        QrNo = (string)u["QrNo"],
                        QrType = (string)u["QrType"],
                        QrRequestDate = (DateTime)u["QrRequestDate"],
                        BizCode = (string)u["BizCode"],
                        ItemAccount = (string)u["ItemAccount"],
                        ItemCode = (string)u["ItemCode"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = (string)u["ItemSpec"],
                        PoNo = (string)u["PoNo"],
                        PoSeq = (int?)u["PoSeq"],
                        LotNo = (string)u["LotNo"],
                        Qty = (decimal)u["Qty"],
                        BasicUnit = (string)u["BasicUnit"],
                        WhCode = (string)u["WhCode"],
                        GrNo = (string)u["GrNo"],
                        GrSeq = (int?)u["GrSeq"],
                        PmNo = (string)u["PmNo"],
                        PmSeq = (int?)u["PmSeq"],
                        ResultOrder = (int)u["ResultOrder"],
                        Status = (bool)u["Status"],
                        TransferFlag = (bool)u["TransferFlag"],
                        BizAreaCode = (string)u["BizAreaCode"],
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"]
                    }
                )
            );
        }

        public DataTable GetRequestDetail(string qrType, DateTime startDate, DateTime endDate, string bizCode, string bizAreaCode)
        {
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetSqlStringCommand("SELECT * FROM fn_qualityRequest(@QrType, @StartDate, @EndDate, @BizCode, @BizAreaCode) ORDER BY QrNo DESC");
            db.AddInParameter(dbCom, "@QrType", DbType.String, qrType);
            db.AddInParameter(dbCom, "@StartDate", DbType.String, startDate.ToShortDateString());
            db.AddInParameter(dbCom, "@EndDate", DbType.String, endDate.ToShortDateString());
            db.AddInParameter(dbCom, "@BizCode", DbType.String, bizCode);
            db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, bizAreaCode);
            DataSet ds = db.ExecuteDataSet(dbCom);

            return ds.Tables[0];
        }
    }
}
