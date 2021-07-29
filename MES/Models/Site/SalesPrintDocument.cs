using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using DevExpress.Mvvm;
using System.Linq;

namespace MesAdmin.Models
{
    public class SalesPrintDocument : ViewModelBase
    {
        private string soType;
        private string shipTo;
        private DateTime startDate;
        private DateTime endDate;
        public IEnumerable<object> SalesOrderReqHeader { get; set; }

        public SalesPrintDocument() { }
        public SalesPrintDocument(DateTime startDate, DateTime endDate, string soType, string shipTo)
        {
            this.soType = soType;
            this.shipTo = shipTo;
            this.startDate = startDate;
            this.endDate = endDate;
            InitializeList();
        }

        public void InitializeList()
        {
            Database db = ProviderFactory.Instance;

            DbCommand dbCom = db.GetStoredProcCommand("usps_sales_PrintDoc_Header");
            dbCom.CommandType = CommandType.StoredProcedure;
            db.AddInParameter(dbCom, "@ShipTo", DbType.String, shipTo);
            db.AddInParameter(dbCom, "@SoType", DbType.String, soType);
            db.AddInParameter(dbCom, "@StartDate", DbType.String, startDate.ToShortDateString());
            db.AddInParameter(dbCom, "@EndDate", DbType.String, endDate.ToShortDateString());
            DataSet ds = db.ExecuteDataSet(dbCom);

            SalesOrderReqHeader = ds.Tables[0].AsEnumerable().Select(r =>
            {
                return new
                {
                    ReqNo = (string)r["ReqNo"],
                    SoNo = (string)r["SoNo"],
                    BizName = (string)r["BizName"],
                    ReqDate = (DateTime)r["ReqDate"],
                    Qty = (decimal)r["Qty"],
                    BasicUnit = (string)r["BasicUnit"],
                    NetAmt = (decimal)r["NetAmt"],
                    Currency = (string)r["Currency"],
                    BoxCnt = (string)r["BoxCnt"],
                    SoType = (string)r["SoType"],
                    UpdateId = (string)r["UpdateId"],
                    UpdateDate = (DateTime)r["UpdateDate"],
                };
            });
        }

        public DataTable GetReqDatail(string reqNo)
        {
            Database db = ProviderFactory.Instance;
            string sql = "SELECT A.*, B.ItemName, B.ItemSpec FROM view_sales_OrderReq_Detail A INNER JOIN common_Item B (NOLOCK) ON A.ItemCode = B.ItemCode WHERE ReqNo = @ReqNo";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCom, "@ReqNo", DbType.String, reqNo);
            DataSet ds = db.ExecuteDataSet(dbCom);

            return ds.Tables[0];
        }

        public void SaveDocument(string reqNo, string boxCnt = "", string docDate = "", string remark = "", string grossWeight = "")
        {
            Database db = ProviderFactory.Instance;            
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    dbCom = db.GetStoredProcCommand("usps_report_Save");
                    dbCom.CommandType = CommandType.StoredProcedure;
                    db.AddInParameter(dbCom, "@ReqNo", DbType.String, reqNo);
                    db.AddInParameter(dbCom, "@BoxCnt", DbType.String, boxCnt);
                    db.AddInParameter(dbCom, "@DocDate", DbType.String, docDate);
                    db.AddInParameter(dbCom, "@Remark", DbType.String, remark);
                    db.AddInParameter(dbCom, "@GrossWeight", DbType.String, grossWeight);
                    db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                    db.ExecuteNonQuery(dbCom, trans);
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
