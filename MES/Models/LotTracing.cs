using System;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;

namespace MesAdmin.Models
{
    public class LotTracing
    {
        public string ProductOrderNo { get; set; }
        public string ItemCode { get; set; }
        public string LotNo { get; set; }

    public DataTable Collections
        {
            get
            {
                Database db = ProviderFactory.Instance;

                DbCommand dbCom = db.GetSqlStringCommand("usp_LotTracing");
                dbCom.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(dbCom, "@LotNo", DbType.String, LotNo);
                DataSet ds = db.ExecuteDataSet(dbCom);

                return ds.Tables[0];
            }
        }

        public DataTable RCollections
        {
            get
            {
                Database db = ProviderFactory.Instance;

                DbCommand dbCom = db.GetSqlStringCommand("usp_LotTracingReverse");
                dbCom.CommandType = CommandType.StoredProcedure;
                db.AddInParameter(dbCom, "@LotNo", DbType.String, LotNo);
                DataSet ds = db.ExecuteDataSet(dbCom);

                return ds.Tables[0];
            }
        }

        public DataTable Details
        {
            get
            {
                Database db = ProviderFactory.Instance;
                DbCommand dbCom = db.GetSqlStringCommand("SELECT * FROM fn_lotTracingDetails(@ProductOrderNo, @ItemCode, @LotNo)");
                db.AddInParameter(dbCom, "@ProductOrderNo", DbType.String, ProductOrderNo);
                db.AddInParameter(dbCom, "@ItemCode", DbType.String, ItemCode);
                db.AddInParameter(dbCom, "@LotNo", DbType.String, LotNo);
                DataSet ds = db.ExecuteDataSet(dbCom);

                return ds.Tables[0];
            }
        }

        public LotTracing(string lotNo = "")
        {
            this.LotNo = lotNo;
        }
    }
}
