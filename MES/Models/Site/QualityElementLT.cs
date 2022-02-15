using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using System.Linq;
using DevExpress.Mvvm;
using System.Data.SqlClient;
using MesAdmin.Common.Common;
using MesAdmin.ViewModels;
using System.Xml.Linq;

namespace MesAdmin.Models
{
    public class QualityElementLT : StateBusinessObject
    {
        public string QrNo
        {
            get { return GetProperty(() => QrNo); }
            set { SetProperty(() => QrNo, value); }
        }
        public decimal Const
        {
            get { return GetProperty(() => Const); }
            set { SetProperty(() => Const, value); }
        }
        public decimal? RefHP
        {
            get { return GetProperty(() => RefHP); }
            set { SetProperty(() => RefHP, value); }
        }
        public decimal? RefRef
        {
            get { return GetProperty(() => RefRef); }
            set { SetProperty(() => RefRef, value); }
        }
        public decimal? HP
        {
            get { return GetProperty(() => HP); }
            set { SetProperty(() => HP, value); }
        }
        public decimal? Ref
        {
            get { return GetProperty(() => Ref); }
            set { SetProperty(() => Ref, value); }
        }
    }

    public class QualityElementLTList : ObservableCollection<QualityElementLT>
    {
        private string qrNo;

        public QualityElementLTList() { }
        public QualityElementLTList(string qrNo)
        {
            this.qrNo = qrNo;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("usps_QualityElementLTRawData");
            dbCom.CommandType = CommandType.StoredProcedure;
            db.AddInParameter(dbCom, "@QrNo", DbType.String, qrNo);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new QualityElementLT
                    {
                        State = EntityState.Unchanged,
                        QrNo = qrNo,
                        Const = (decimal)u["Const"],
                        RefHP = u["RefHP"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(u["RefHP"]),
                        RefRef = u["RefRef"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(u["RefRef"]),
                        HP = u["HP"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(u["HP"]),
                        Ref = u["Ref"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(u["Ref"]),
                    }
                )
            );
        }

        public void Save(ObservableCollection<RawResult> result)
        {
            var lt = Items.FirstOrDefault();
            if (lt == null || lt.State == EntityState.Unchanged) return;

            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                try
                {
                    XDocument doc = new XDocument(new XElement("Root", from item in this.Items
                            select new XElement("InspectValue"
                                , new XAttribute("RefHP", item.RefHP.ToString())
                                , new XAttribute("RefRef", item.RefRef.ToString())
                                , new XAttribute("HP", item.HP.ToString())
                                , new XAttribute("Ref", item.Ref.ToString())
                            )
                        )
                    );

                    dbCom = db.GetStoredProcCommand("usps_qualityElementLT_Save");
                    db.AddInParameter(dbCom, "@Op", DbType.String, lt.State);
                    db.AddInParameter(dbCom, "@QrNo", DbType.String, lt.QrNo);
                    db.AddInParameter(dbCom, "@Const", DbType.Decimal, lt.Const);
                    db.AddInParameter(dbCom, "@RawData", DbType.Xml, doc.ToString());
                    db.AddInParameter(dbCom, "@Ref_HP", DbType.Decimal, result[0].Result == "" ? null : result[0].Result);
                    db.AddInParameter(dbCom, "@Ref_Ref", DbType.Decimal, result[1].Result == "" ? null : result[1].Result);
                    db.AddInParameter(dbCom, "@HP", DbType.Decimal, result[2].Result == "" ? null : result[2].Result);
                    db.AddInParameter(dbCom, "@Ref", DbType.Decimal, result[3].Result == "" ? null : result[3].Result);
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
