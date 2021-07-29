using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using MesAdmin.Common.Common;
using System.Linq;

namespace MesAdmin.Models
{
    public class MonitorERP
    {
        public DataTable GetInterface()
        {
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetSqlStringCommand("SELECT * FROM view_monitorERPInterface");
            DataSet ds = db.ExecuteDataSet(dbCom);

            return ds.Tables[0];
        }

        public DataTable GetInventoryVariances()
        {
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetSqlStringCommand("SELECT * FROM view_monitorERPVariances");
            DataSet ds = db.ExecuteDataSet(dbCom);

            return ds.Tables[0];
        }
    }

    public class SiteView
    {
        public DataTable GetStockFromBills(string itemCode)
        {
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("usps_GetStock_FromBills");
            dbCom.CommandType = CommandType.StoredProcedure;
            db.AddInParameter(dbCom, "@ItemCode", DbType.String, itemCode);
            DataSet ds = db.ExecuteDataSet(dbCom);

            return ds.Tables[0];
        }
    }

    public class StockDetailPE10
    {
        public DataTable GetStockDetail()
        {
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetSqlStringCommand("SELECT * FROM views_StockDetail_PE10");
            DataSet ds = db.ExecuteDataSet(dbCom);

            return ds.Tables[0];
        }
    }
}
