using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using MesAdmin.Common.Common;

namespace MesAdmin.Models
{
    public class SalesOrderTypeConfig : StateBusinessObject
    {
        public string SoType
        {
            get { return GetProperty(() => SoType); }
            set { SetProperty(() => SoType, value); }
        }
        public string SoTypeName
        {
            get { return GetProperty(() => SoTypeName); }
            set { SetProperty(() => SoTypeName, value); }
        }
        public string TransType
        {
            get { return GetProperty(() => TransType); }
            set { SetProperty(() => TransType, value); }
        }
        public string MoveType
        {
            get { return GetProperty(() => MoveType); }
            set { SetProperty(() => MoveType, value); }
        }
        public bool IsEnabled
        {
            get { return GetProperty(() => IsEnabled); }
            set { SetProperty(() => IsEnabled, value); }
        }
        public string UpdateId { get; set; }
        public DateTime UpdateDate { get; set; }
    }

    public class SalesOrderTypeConfigList : ObservableCollection<SalesOrderTypeConfig>
    {
        public SalesOrderTypeConfigList(IEnumerable<SalesOrderTypeConfig> items) : base(items) { }
        public SalesOrderTypeConfigList()
        {
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;

            string str = "SELECT * FROM sales_OrderTypeConfig ";

            DbCommand dbCom = db.GetSqlStringCommand(str);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new SalesOrderTypeConfig
                    {
                        SoType = (string)u["SoType"],
                        SoTypeName = (string)u["SoTypeName"],
                        TransType = u["TransType"].ToString(),
                        MoveType = u["MoveType"].ToString(),
                        IsEnabled = (bool)u["IsEnabled"],
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"]
                    }
                )
            );
        }
    }
}
