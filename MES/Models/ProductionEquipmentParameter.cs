using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using MesAdmin.Common.Common;
using System.Xml.Linq;

namespace MesAdmin.Models
{
    public class ProductionEquipmentParameter : StateBusinessObject
    {
        public string ProductOrderNo
        {
            get { return GetProperty(() => ProductOrderNo); }
            set { SetProperty(() => ProductOrderNo, value); }
        }
        public string Seq
        {
            get { return GetProperty(() => Seq); }
            set { SetProperty(() => Seq, value); }
        }
        public string EqpCode
        {
            get { return GetProperty(() => EqpCode); }
            set { SetProperty(() => EqpCode, value); }
        }
        public string EqpName
        {
            get { return GetProperty(() => EqpName); }
            set { SetProperty(() => EqpName, value); }
        }
        public int? Order
        {
            get { return GetProperty(() => Order); }
            set { SetProperty(() => Order, value); }
        }
        public new string Parameter
        {
            get { return GetProperty(() => Parameter); }
            set { SetProperty(() => Parameter, value); }
        }
        public string ParameterValue
        {
            get { return GetProperty(() => ParameterValue); }
            set { SetProperty(() => ParameterValue, value); }
        }
        public string ParameterSpec
        {
            get { return GetProperty(() => ParameterSpec); }
            set { SetProperty(() => ParameterSpec, value); }
        }
        public string DownRate
        {
            get { return GetProperty(() => DownRate); }
            set { SetProperty(() => DownRate, value); }
        }
        public string UpRate
        {
            get { return GetProperty(() => UpRate); }
            set { SetProperty(() => UpRate, value); }
        }
        public string Remark
        {
            get { return GetProperty(() => Remark); }
            set { SetProperty(() => Remark, value); }
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
        public DateTime FinishDate
        {
            get { return GetProperty(() => FinishDate); }
            set { SetProperty(() => FinishDate, value); }
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

        public DataTable GetList(DateTime startDate, DateTime endDate, string waCode, string seq)
        {
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("usp_production_EquipmentParameter");
            dbCom.CommandType = CommandType.StoredProcedure;
            db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, DSUser.Instance.BizAreaCode);
            db.AddInParameter(dbCom, "@WaCode", DbType.String, waCode);
            db.AddInParameter(dbCom, "@StartDate", DbType.Date, startDate);
            db.AddInParameter(dbCom, "@EndDate", DbType.Date, endDate);
            db.AddInParameter(dbCom, "@Seq", DbType.String, seq);
            DataSet ds = db.ExecuteDataSet(dbCom);

            return ds.Tables.Count > 0 ? ds.Tables[0] : null;
        }
    }

    public class ProductionEquipmentParameterList : ObservableCollection<ProductionEquipmentParameter>
    {
        private string productOrderNo;
        private string seq;

        public ProductionEquipmentParameterList(string productOrderNo, string seq)
        {
            this.productOrderNo = productOrderNo;
            this.seq = seq;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;

            DbCommand dbCom = db.GetStoredProcCommand("usp_production_EquipmentParameterValue");
            db.AddInParameter(dbCom, "@ProductOrderNo", DbType.String, productOrderNo);
            db.AddInParameter(dbCom, "@Seq", DbType.String, seq);
            dbCom.CommandType = CommandType.StoredProcedure;

            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new ProductionEquipmentParameter
                    {
                        State = EntityState.Unchanged,
                        ProductOrderNo = (string)u["ProductOrderNo"],
                        Seq = (string)u["Seq"],
                        EqpCode = (string)u["EqpCode"],
                        EqpName = (string)u["EqpName"],
                        Parameter = u["Parameter"].ToString(),
                        ParameterSpec = u["ParameterSpec"].ToString(),
                        DownRate = u["DownRate"].ToString(),
                        UpRate = u["UpRate"].ToString(),
                        ParameterValue = u["ParameterValue"].ToString(),
                        Remark = u["Remark"].ToString(),
                        ItemCode = u["ItemCode"].ToString(),
                        ItemName = u["ItemName"].ToString(),
                        FinishDate = (DateTime)u["FinishDate"],
                        LotNo = u["LotNo"].ToString(),
                        Qty = (decimal)u["Qty"],
                        BasicUnit = u["BasicUnit"].ToString(),
                    }
                )
            );
        }

        public void Save()
        {
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    XDocument doc = new XDocument(new XElement("Root",
                                    new XElement("Parameters", from item in this.Items
                                                               select new XElement("Parameter"
                                                               , new XAttribute("ParameterName", item.Parameter)
                                                               , new XAttribute("ParameterValue", item.ParameterValue ?? "")
                                                               , new XAttribute("ParameterSpec", item.ParameterSpec)
                                                               , new XAttribute("DownRate", item.DownRate)
                                                               , new XAttribute("UpRate", item.UpRate)
                                                               , new XAttribute("Remark", item.Remark ?? "")
                                                               )
                                )
                            )
                        );
                    string sql = "UPDATE production_EquipmentParameter SET ParameterData = @ParameterData, UpdateId = @UpdateId, UpdateDate = getdate() WHERE ProductOrderNo = @ProductOrderNo AND Seq = @Seq";
                    dbCom = db.GetSqlStringCommand(sql);
                    var result = Items.FirstOrDefault();
                    db.AddInParameter(dbCom, "@ProductOrderNo", DbType.String, productOrderNo);
                    db.AddInParameter(dbCom, "@Seq", DbType.String, seq);
                    db.AddInParameter(dbCom, "@ParameterData", DbType.Xml, doc.ToString());
                    db.AddInParameter(dbCom, "@UpdateId", DbType.String, DSUser.Instance.UserID);
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