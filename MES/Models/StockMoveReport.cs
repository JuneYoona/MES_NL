using System;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;

namespace MesAdmin.Models
{
    public class StockMoveReport
    {
        public StockMoveReport() { }
        public DataTable GetCollections(DateTime basicDate, string itemAccount, string itemCode = "")
        {
            Database db = ProviderFactory.Instance;

            DbCommand dbCom = db.GetStoredProcCommand("usp_stock_Movement_Report");
            dbCom.CommandType = CommandType.StoredProcedure;
            db.AddInParameter(dbCom, "@YYYYMM", DbType.String, string.Format("{0:yyyyMM}", basicDate));
            db.AddInParameter(dbCom, "@ItemAccount", DbType.String, itemAccount);
            db.AddInParameter(dbCom, "@ItemCode", DbType.String, itemCode);
            DataSet ds = db.ExecuteDataSet(dbCom);

            return ds.Tables[0];
        }
    }
}
