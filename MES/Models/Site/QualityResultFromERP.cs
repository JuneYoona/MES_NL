using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using MesAdmin.Common.Common;
using System.Linq;
using DevExpress.Mvvm;
using System.Xml.Linq;

namespace MesAdmin.Models
{
    public class QualityResultFromERPList : ObservableCollection<QualityResult>
    {
        private string qrNo;
        private int order;

        public QualityResultFromERPList() { }
        public QualityResultFromERPList(IEnumerable<QualityResult> items) : base(items) { }
        public QualityResultFromERPList(string qrNo, int order)
        {
            this.qrNo = qrNo;
            this.order = order;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("usps_QualityResultFromERPXml");
            dbCom.CommandType = CommandType.StoredProcedure;
            db.AddInParameter(dbCom, "@QrNo", DbType.String, qrNo);
            db.AddInParameter(dbCom, "@Order", DbType.Int16, order);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new QualityResult
                    {
                        State = MesAdmin.Common.Common.EntityState.Unchanged,
                        QrNo = (string)u["QrNo"],
                        Order = int.Parse(u["Order"].ToString()),
                        QrType = (string)u["QrType"],
                        ItemCode = (string)u["ItemCode"],
                        InspectName = (string)u["InspectName"],
                        InspectSpec = (string)u["InspectSpec"],
                        DownRate = (string)u["DownRate"],
                        UpRate = (string)u["UpRate"],
                        InspectValue = (string)u["InspectValue"],
                        Unit = u["Unit"].ToString(),
                        Equipment = u["Equipment"].ToString(),
                        Memo = (string)u["Memo"],
                        Memo1 = u["Memo1"].ToString(),
                        Memo2 = u["Memo2"].ToString(),
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"]
                    }
                )
            );
        }
    }
}
