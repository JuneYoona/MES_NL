using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using DevExpress.Mvvm;

namespace MesAdmin.Models
{
    public class StockDetailBOMList : ObservableCollection<StockDetail>
    {
        private string prntIitemCode;
            
        public StockDetailBOMList(string prntIitemCode)
        {
            this.prntIitemCode = prntIitemCode;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("usp_GetStock_Detail");
            db.AddInParameter(dbCom, "@ItemCode", DbType.String, prntIitemCode);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new StockDetail
                    {
                        BizAreaCode = (string)u["BizAreaCode"],
                        ItemCode = (string)u["ItemCode"],
                        ItemAccount = (string)u["ItemAccount"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = (string)u["ItemSpec"],
                        WhCode = (string)u["WhCode"],
                        WaCode = (string)u["WaCode"],
                        LotNo = (string)u["LotNo"],
                        Qty = (decimal)u["Qty"],
                        EqpQty = (decimal)u["EqpQty"],
                        QrQty = (decimal)u["QrQty"],
                        BadQty = (decimal)u["BadQty"],
                        PickingQty = (decimal)u["PickingQty"],
                        BasicUnit = (string)u["BasicUnit"],
                        ProcureType = (string)u["ProcureType"],
                        Remark4 = u["TSC"].ToString(),
                        Remark5 = u["PIG"].ToString(),
                        Remark6 = u["ExpDate"].ToString(),
                    }
                )
            );
        }
    }
}
