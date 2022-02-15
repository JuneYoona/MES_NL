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
    public class QualityElementIVL : StateBusinessObject
    {
        public string QrNo
        {
            get { return GetProperty(() => QrNo); }
            set { SetProperty(() => QrNo, value); }
        }
        public decimal LumRef
        {
            get { return GetProperty(() => LumRef); }
            set { SetProperty(() => LumRef, value); }
        }
        public decimal LumLot
        {
            get { return GetProperty(() => LumLot); }
            set { SetProperty(() => LumLot, value); }
        }
        public decimal? RefV
        {
            get { return GetProperty(() => RefV); }
            set { SetProperty(() => RefV, value); }
        }
        public decimal? RefCurrent
        {
            get { return GetProperty(() => RefCurrent); }
            set { SetProperty(() => RefCurrent, value); }
        }
        public decimal? RefJ
        {
            get { return GetProperty(() => RefJ); }
            set { SetProperty(() => RefJ, value); }
        }
        public decimal? RefCdA
        {
            get { return GetProperty(() => RefCdA); }
            set { SetProperty(() => RefCdA, value); }
        }
        public decimal? RefLmW
        {
            get { return GetProperty(() => RefLmW); }
            set { SetProperty(() => RefLmW, value); }
        }
        public decimal? RefEQE
        {
            get { return GetProperty(() => RefEQE); }
            set { SetProperty(() => RefEQE, value); }
        }
        public decimal? RefL
        {
            get { return GetProperty(() => RefL); }
            set { SetProperty(() => RefL, value); }
        }
        public decimal? RefCIEx
        {
            get { return GetProperty(() => RefCIEx); }
            set { SetProperty(() => RefCIEx, value); }
        }
        public decimal? RefCIEy
        {
            get { return GetProperty(() => RefCIEy); }
            set { SetProperty(() => RefCIEy, value); }
        }
        public decimal? V
        {
            get { return GetProperty(() => V); }
            set { SetProperty(() => V, value); }
        }
        public decimal? Current
        {
            get { return GetProperty(() => Current); }
            set { SetProperty(() => Current, value); }
        }
        public decimal? J
        {
            get { return GetProperty(() => J); }
            set { SetProperty(() => J, value); }
        }
        public decimal? CdA
        {
            get { return GetProperty(() => CdA); }
            set { SetProperty(() => CdA, value); }
        }
        public decimal? LmW
        {
            get { return GetProperty(() => LmW); }
            set { SetProperty(() => LmW, value); }
        }
        public decimal? EQE
        {
            get { return GetProperty(() => EQE); }
            set { SetProperty(() => EQE, value); }
        }
        public decimal? L
        {
            get { return GetProperty(() => L); }
            set { SetProperty(() => L, value); }
        }
        public decimal? CIEx
        {
            get { return GetProperty(() => CIEx); }
            set { SetProperty(() => CIEx, value); }
        }
        public decimal? CIEy
        {
            get { return GetProperty(() => CIEy); }
            set { SetProperty(() => CIEy, value); }
        }
    }

    public class QualityElementIVLList : ObservableCollection<QualityElementIVL>
    {
        private QualityElementIVL result;
        public QualityElementIVL Result
        {
            get { return result; }
        }

        private string qrNo;

        public QualityElementIVLList() { }
        public QualityElementIVLList(string qrNo)
        {
            this.qrNo = qrNo;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = db.GetStoredProcCommand("usps_QualityElementIVLRawData");
            dbCom.CommandType = CommandType.StoredProcedure;
            db.AddInParameter(dbCom, "@QrNo", DbType.String, qrNo);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new QualityElementIVL
                    {
                        State = EntityState.Unchanged,
                        QrNo = qrNo,
                        LumRef = (decimal)u["LumRef"],
                        LumLot = (decimal)u["LumLot"],
                        RefV = u["RefV"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(u["RefV"]),
                        RefCurrent = u["RefCurrent"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(u["RefCurrent"]),
                        RefJ = u["RefJ"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(u["RefJ"]),
                        RefCdA = u["RefCdA"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(u["RefCdA"]),
                        RefLmW = u["RefLmW"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(u["RefLmW"]),
                        RefEQE = u["RefEQE"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(u["RefEQE"]),
                        RefL = u["RefL"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(u["RefL"]),
                        RefCIEx = u["RefCIEx"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(u["RefCIEx"]),
                        RefCIEy = u["RefCIEy"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(u["RefCIEy"]),

                        V = u["V"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(u["V"]),
                        Current = u["Current"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(u["Current"]),
                        J = u["J"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(u["J"]),
                        CdA = u["CdA"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(u["CdA"]),
                        LmW = u["LmW"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(u["LmW"]),
                        EQE = u["EQE"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(u["EQE"]),
                        L = u["L"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(u["L"]),
                        CIEx = u["CIEx"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(u["CIEx"]),
                        CIEy = u["CIEy"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(u["CIEy"]),
                    }
                )
            );

            dbCom = db.GetSqlStringCommand("SELECT * FROM quality_Element_IVL (NOLOCK) WHERE QrNo = @QrNo");
            db.AddInParameter(dbCom, "@QrNo", DbType.String, qrNo);
            ds = db.ExecuteDataSet(dbCom);

            if (ds.Tables[0].Rows.Count != 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];

                result = new QualityElementIVL
                {
                    RefV = dr["Ref_V"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(dr["Ref_V"]),
                    RefCurrent = dr["Ref_Current"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(dr["Ref_Current"]),
                    RefJ = dr["Ref_J"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(dr["Ref_J"]),
                    RefCdA = dr["Ref_cd_A"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(dr["Ref_cd_A"]),
                    RefLmW = dr["Ref_lm_W"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(dr["Ref_lm_W"]),
                    RefEQE = dr["Ref_EQE"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(dr["Ref_EQE"]),
                    RefL = dr["Ref_L"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(dr["Ref_L"]),
                    RefCIEx = dr["Ref_CIEx"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(dr["Ref_CIEx"]),
                    RefCIEy = dr["Ref_CIEy"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(dr["Ref_CIEy"]),

                    V = dr["V"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(dr["V"]),
                    Current = dr["Current"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(dr["Current"]),
                    J = dr["J"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(dr["J"]),
                    CdA = dr["cd_A"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(dr["cd_A"]),
                    LmW = dr["lm_W"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(dr["lm_W"]),
                    EQE = dr["EQE"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(dr["EQE"]),
                    L = dr["L"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(dr["L"]),
                    CIEx = dr["CIEx"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(dr["CIEx"]),
                    CIEy = dr["CIEy"].ToString() == "" ? (decimal?)null : Convert.ToDecimal(dr["CIEy"]),
                };
            }
        }

        public void Save(ObservableCollection<RawResult> result)
        {
            var ivl = Items.FirstOrDefault();
            if (ivl == null || ivl.State == EntityState.Unchanged) return;

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
                                , new XAttribute("RefV", item.RefV.ToString())
                                , new XAttribute("RefCurrent", item.RefCurrent.ToString())
                                , new XAttribute("RefJ", item.RefJ.ToString())
                                , new XAttribute("RefCdA", item.RefCdA.ToString())
                                , new XAttribute("RefLmW", item.RefLmW.ToString())
                                , new XAttribute("RefEQE", item.RefEQE.ToString())
                                , new XAttribute("RefL", item.RefL.ToString())
                                , new XAttribute("RefCIEx", item.RefCIEx.ToString())
                                , new XAttribute("RefCIEy", item.RefCIEy.ToString())
                                , new XAttribute("V", item.V.ToString())
                                , new XAttribute("Current", item.Current.ToString())
                                , new XAttribute("J", item.J.ToString())
                                , new XAttribute("CdA", item.CdA.ToString())
                                , new XAttribute("LmW", item.LmW.ToString())
                                , new XAttribute("EQE", item.EQE.ToString())
                                , new XAttribute("L", item.L.ToString())
                                , new XAttribute("CIEx", item.CIEx.ToString())
                                , new XAttribute("CIEy", item.CIEy.ToString())
                            )
                        )
                    );
                    
                    dbCom = db.GetStoredProcCommand("usps_qualityElementIVL_Save");
                    db.AddInParameter(dbCom, "@Op", DbType.String, ivl.State);
                    db.AddInParameter(dbCom, "@QrNo", DbType.String, ivl.QrNo);
                    db.AddInParameter(dbCom, "@LumRef", DbType.Decimal, ivl.LumRef);
                    db.AddInParameter(dbCom, "@LumLot", DbType.Decimal, ivl.LumLot);
                    db.AddInParameter(dbCom, "@RawData", DbType.Xml, doc.ToString());
                    db.AddInParameter(dbCom, "@Ref_V", DbType.Decimal, result == null || result[0].Result == "" ? null : result[0].Result);
                    db.AddInParameter(dbCom, "@Ref_Current", DbType.Decimal, result == null || result[1].Result == "" ? null : result[1].Result);
                    db.AddInParameter(dbCom, "@Ref_J", DbType.Decimal, result == null || result[2].Result == "" ? null : result[2].Result);
                    db.AddInParameter(dbCom, "@Ref_cd_A", DbType.Decimal, result == null || result[3].Result == "" ? null : result[3].Result);
                    db.AddInParameter(dbCom, "@Ref_lm_W", DbType.Decimal, result == null || result[4].Result == "" ? null : result[4].Result);
                    db.AddInParameter(dbCom, "@Ref_EQE", DbType.Decimal, result == null || result[5].Result == "" ? null : result[5].Result);
                    db.AddInParameter(dbCom, "@Ref_L", DbType.Decimal, result == null || result[6].Result == "" ? null : result[6].Result);
                    db.AddInParameter(dbCom, "@Ref_CIEx", DbType.Decimal, result == null || result[7].Result == "" ? null : result[7].Result);
                    db.AddInParameter(dbCom, "@Ref_CIEy", DbType.Decimal, result == null || result[8].Result == "" ? null : result[8].Result);
                    db.AddInParameter(dbCom, "@V", DbType.Decimal, result == null || result[9].Result == "" ? null : result[9].Result);
                    db.AddInParameter(dbCom, "@Current", DbType.Decimal, result == null || result[10].Result == "" ? null : result[10].Result);
                    db.AddInParameter(dbCom, "@J", DbType.Decimal, result == null || result[11].Result == "" ? null : result[11].Result);
                    db.AddInParameter(dbCom, "@cd_A", DbType.Decimal, result == null || result[12].Result == "" ? null : result[12].Result);
                    db.AddInParameter(dbCom, "@lm_W", DbType.Decimal, result == null || result[13].Result == "" ? null : result[13].Result);
                    db.AddInParameter(dbCom, "@EQE", DbType.Decimal, result == null || result[14].Result == "" ? null : result[14].Result);
                    db.AddInParameter(dbCom, "@L", DbType.Decimal, result == null || result[15].Result == "" ? null : result[15].Result);
                    db.AddInParameter(dbCom, "@CIEx", DbType.Decimal, result == null || result[16].Result == "" ? null : result[16].Result);
                    db.AddInParameter(dbCom, "@CIEy", DbType.Decimal, result == null || result[17].Result == "" ? null : result[17].Result);
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
