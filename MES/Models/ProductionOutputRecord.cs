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
    public class ProductionOutputRecord : ViewModelBaseEx
    {
        public string BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value); }
        }
        public string OrderNo
        {
            get { return GetProperty(() => OrderNo); }
            set { SetProperty(() => OrderNo, value); }
        }
        public int Seq
        {
            get { return GetProperty(() => Seq); }
            set { SetProperty(() => Seq, value); }
        }
        public string WhCode
        {
            get { return GetProperty(() => WhCode); }
            set { SetProperty(() => WhCode, value); }
        }
        public string WaCode
        {
            get { return GetProperty(() => WaCode); }
            set { SetProperty(() => WaCode, value); }
        }
        public string EqpCode
        {
            get { return GetProperty(() => EqpCode); }
            set { SetProperty(() => EqpCode, value); }
        }
        public string LotNo
        {
            get { return GetProperty(() => LotNo); }
            set { SetProperty(() => LotNo, value); }
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
        public string BasicUnit
        {
            get { return GetProperty(() => BasicUnit); }
            set { SetProperty(() => BasicUnit, value); }
        }
        public decimal Qty
        {
            get { return GetProperty(() => Qty); }
            set { SetProperty(() => Qty, value); }
        }
        public string EqpIn_OrderNo
        {
            get { return GetProperty(() => EqpIn_OrderNo); }
            set { SetProperty(() => EqpIn_OrderNo, value); }
        }
        public int EqpIn_Seq
        {
            get { return GetProperty(() => EqpIn_Seq); }
            set { SetProperty(() => EqpIn_Seq, value); }
        }
        public string InsertId
        {
            get { return GetProperty(() => InsertId); }
            set { SetProperty(() => InsertId, value); }
        }
        public DateTime InsertDate
        {
            get { return GetProperty(() => InsertDate); }
            set { SetProperty(() => InsertDate, value); }
        }
    }

    public class ProductionOutputRecordList : ObservableCollection<ProductionOutputRecord>
    {
        private string orderNo;
        private int seq;

        public ProductionOutputRecordList(string orderNo, int seq)
        {
            this.orderNo = orderNo;
            this.seq = seq;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;

            string str = "SELECT A.*, B.ItemName, B.ItemSpec, B.BasicUnit FROM production_OutputRecord A (NOLOCK) ";
                str += "INNER JOIN common_Item B (NOLOCK) ON A.ItemCode = B.ItemCode ";
                str += "WHERE OrderNo = '" + orderNo + "' AND Seq = " + seq;
            DbCommand dbCom = db.GetSqlStringCommand(str);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new ProductionOutputRecord
                    {
                        BizAreaCode = (string)u["BizAreaCode"],
                        OrderNo = (string)u["OrderNo"],
                        Seq = (int)u["Seq"],
                        WhCode = (string)u["WhCode"],
                        WaCode = (string)u["WaCode"],
                        EqpCode = (string)u["EqpCode"],
                        LotNo = (string)u["LotNo"],
                        ItemCode = (string)u["ItemCode"],
                        ItemName = (string)u["ItemName"],
                        ItemSpec = (string)u["ItemSpec"],
                        BasicUnit = (string)u["OutUnit"],
                        Qty = (decimal)u["Qty"],
                        InsertId = (string)u["InsertId"],
                        InsertDate = (DateTime)u["InsertDate"],
                        EqpIn_OrderNo = (string)u["EqpIn_OrderNo"],
                        EqpIn_Seq = (int)u["EqpIn_Seq"]
                    }
                )
            );
        }
    }
}
