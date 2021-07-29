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
    public class SalesLabelPrintHistory : StateBusinessObject
    {
        public string LbNo
        {
            get { return GetProperty(() => LbNo); }
            set { SetProperty(() => LbNo, value); }
        }
        public int Seq
        {
            get { return GetProperty(() => Seq); }
            set { SetProperty(() => Seq, value); }
        }
        public string DnNo
        {
            get { return GetProperty(() => DnNo); }
            set { SetProperty(() => DnNo, value); }
        }
        public int DnSeq
        {
            get { return GetProperty(() => DnSeq); }
            set { SetProperty(() => DnSeq, value); }
        }
        public string ReqNo
        {
            get { return GetProperty(() => ReqNo); }
            set { SetProperty(() => ReqNo, value); }
        }
        public int ReqSeq
        {
            get { return GetProperty(() => ReqSeq); }
            set { SetProperty(() => ReqSeq, value); }
        }
        public string ItemCode
        {
            get { return GetProperty(() => ItemCode); }
            set { SetProperty(() => ItemCode, value); }
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
        public decimal TareWeight
        {
            get { return GetProperty(() => TareWeight); }
            set { SetProperty(() => TareWeight, value); }
        }
        public int PrintSeq
        {
            get { return GetProperty(() => PrintSeq); }
            set { SetProperty(() => PrintSeq, value); }
        }
        public string Cnt
        {
            get { return GetProperty(() => Cnt); }
            set { SetProperty(() => Cnt, value); }
        }
        public DateTime ProductDate
        {
            get { return GetProperty(() => ProductDate); }
            set { SetProperty(() => ProductDate, value); }
        }
        public DateTime ExDate
        {
            get { return GetProperty(() => ExDate); }
            set { SetProperty(() => ExDate, value); }
        }
        public string QRCode
        {
            get { return GetProperty(() => QRCode); }
            set { SetProperty(() => QRCode, value); }
        }
        public int QRLength
        {
            get { return GetProperty(() => QRLength); }
            set { SetProperty(() => QRLength, value); }
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
        public string PDAId
        {
            get { return GetProperty(() => PDAId); }
            set { SetProperty(() => PDAId, value); }
        }
        public DateTime? PDADate
        {
            get { return GetProperty(() => PDADate); }
            set { SetProperty(() => PDADate, value); }
        }
    }

    public class SalesLabelPrintHistoryList : ObservableCollection<SalesLabelPrintHistory>
    {
        private string reqNo;
        private int reqSeq;

        public SalesLabelPrintHistoryList() { }
        public SalesLabelPrintHistoryList(string reqNo, int reqSeq)
        {
            this.reqNo = reqNo;
            this.reqSeq = reqSeq;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            string sql = "SELECT *, PDAId = C.UpdateId, PDADate = C.UpdateDate FROM view_sales_LabelPrint_History A " +
                         "LEFT JOIN PDA_LabelInsp_History (NOLOCK) C ON A.DnNo = C.DnNo AND A.DnSeq = C.DnSeq AND A.QRCode = C.QRCode " +
                         "WHERE A.ReqNo = @ReqNo AND A.ReqSeq = @ReqSeq";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCom, "@ReqNo", DbType.String, reqNo);
            db.AddInParameter(dbCom, "@ReqSeq", DbType.Int16, reqSeq);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new SalesLabelPrintHistory
                    {
                        LbNo = (string)u["LbNo"],
                        Seq = (int)u["Seq"],
                        DnNo = (string)u["DnNo"],
                        DnSeq = (int)u["DnSeq"],
                        ItemCode = (string)u["ItemCode"],
                        LotNo = (string)u["LotNo"],
                        Qty = (decimal)u["Qty"],
                        TareWeight = (decimal)u["TareWeight"],
                        PrintSeq = (int)u["PrintSeq"],
                        Cnt = (string)u["Cnt"],
                        ProductDate = (DateTime)u["ProductDate"],
                        ExDate = (DateTime)u["ExDate"],
                        QRCode = (string)u["QRCode"],
                        QRLength = u["QRCode"].ToString().Length,
                        InsertId = (string)u["InsertId"],
                        InsertDate = (DateTime)u["InsertDate"],
                        PDAId = u["PDAId"].ToString(),
                        PDADate = u["PDADate"] == DBNull.Value ? null : (DateTime?)u["PDADate"]
                    }
                )
            );
        }

        public void Save()
        {
            IEnumerable<SalesLabelPrintHistory> items = this.Items;
            Database db = ProviderFactory.Instance;
            DbCommand dbCom = null;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                try
                {
                    Delete(items.Where(u => u.State == MesAdmin.Common.Common.EntityState.Deleted), db, trans, dbCom);
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public void Delete(IEnumerable<SalesLabelPrintHistory> items, Database db, DbTransaction trans, DbCommand dbCom)
        {
            foreach (SalesLabelPrintHistory item in items)
            {
                dbCom = db.GetSqlStringCommand("DELETE sales_LabelPrint_History WHERE LbNo = @LbNo AND Seq = @Seq");
                db.AddInParameter(dbCom, "@LbNo", DbType.String, item.LbNo);
                db.AddInParameter(dbCom, "@Seq", DbType.Int16, item.Seq);
                db.ExecuteNonQuery(dbCom, trans);
            }
        }
    }
}
