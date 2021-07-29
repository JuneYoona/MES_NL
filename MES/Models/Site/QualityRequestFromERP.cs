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
    public class QualityRequestFromERP : ViewModelBase
    {
        string qrNo;
        int order;

        public QualityRequestFromERP(string qrNo, int order)
        {
            this.qrNo = qrNo;
            this.order = order;
        }

        public QualityRequest GetQualityRequestHeader()
        {
            Database db = ProviderFactory.Instance;

            string sql = "SELECT * FROM views_quality_Request WHERE QrNo = '" + qrNo + "' AND ResultOrder=" + order;

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            DataSet ds = db.ExecuteDataSet(dbCom);

            QualityRequest qualityRequest = null;
            DataRow dr = ds.Tables[0].Rows[0];

            qualityRequest = new QualityRequest
            {
                QrNo = (string)dr["QrNo"],
                QrType = (string)dr["QrType"],
                ItemAccount = (string)dr["ItemAccount"],
                QrRequestDate = (DateTime)dr["QrRequestDate"],
                BizCode = (string)dr["BizCode"],
                ItemCode = (string)dr["ItemCode"],
                ItemName = (string)dr["ItemName"],
                //PoNo = (string)u["PoNo"],
                //PoSeq = (int?)u["PoSeq"],
                LotNo = (string)dr["LotNo"],
                Qty = (decimal)dr["Qty"],
                InspectDate = dr["InspectDate"] == DBNull.Value ? null : (DateTime?)dr["InspectDate"],
                InspectorId = dr["InspectorId"].ToString(),
                Result = dr["Result"].ToString(),
                WhCode = (string)dr["WhCode"],
                GrNo = (string)dr["GrNo"],
                //GrSeq = (int?)u["GrSeq"],
                PmNo = (string)dr["PmNo"],
                //PmSeq = (int?)u["PmSeq"],
                ResultOrder = (int)dr["ResultOrder"],
                LastOrder = (int)dr["LastOrder"],
                Status = (bool)dr["Status"],
                TransferFlag = (bool)dr["TransferFlag"],
                Memo = dr["Memo"].ToString(),
                UpdateId = (string)dr["UpdateId"],
                UpdateDate = (DateTime)dr["UpdateDate"],
            };

            return qualityRequest;
        }
    }

    public class QualityRequestFromERPList : ObservableCollection<QualityRequest>
    {
        private DateTime startDate;
        private DateTime endDate;
        private string qrNo;
        private string qrType;
        private string bizCode;
        private string lotNo;
        private string bizAreaCode;

        public QualityRequestFromERPList() { }
        public QualityRequestFromERPList(IEnumerable<QualityRequest> items) : base(items) { }
        public QualityRequestFromERPList(DateTime startDate, DateTime endDate, string qrNo = "", string qrType = "", string bizCode = "", string lotNo = "", string bizAreaCode = "")
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
            DbCommand dbCom = db.GetStoredProcCommand("usps_QualityRequestFromERP");
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
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"]
                    }
                )
            );
        }

        public DataTable GetRequestDetail(string qrType, DateTime startDate, DateTime endDate, string bizCode, string bizAreaCode)
        {
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetSqlStringCommand("SELECT * FROM ufn_qualityRequestFromERP(@QrType, @StartDate, @EndDate, @BizCode, @BizAreaCode) ORDER BY QrNo DESC");
            db.AddInParameter(dbCom, "@QrType", DbType.String, qrType);
            db.AddInParameter(dbCom, "@StartDate", DbType.String, startDate.ToShortDateString());
            db.AddInParameter(dbCom, "@EndDate", DbType.String, endDate.ToShortDateString());
            db.AddInParameter(dbCom, "@BizCode", DbType.String, bizCode);
            db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, bizAreaCode);
            DataSet ds = db.ExecuteDataSet(dbCom);

            return ds.Tables[0];
        }

        public DataTable GetResultDetail(string qrType, string itemCode, DateTime startDate, DateTime endDate, string bizCode)
        {
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("usps_QualityResultFromERP");
            db.AddInParameter(dbCom, "@QrType", DbType.String, qrType);
            db.AddInParameter(dbCom, "@ItemCode", DbType.String, itemCode);
            db.AddInParameter(dbCom, "@StartDate", DbType.String, startDate.ToShortDateString());
            db.AddInParameter(dbCom, "@EndDate", DbType.String, endDate.ToShortDateString());
            db.AddInParameter(dbCom, "@BizCode", DbType.String, bizCode);
            DataSet ds = db.ExecuteDataSet(dbCom);

            return ds.Tables[0];
        }
    }
}
