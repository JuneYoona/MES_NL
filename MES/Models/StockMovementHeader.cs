using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using MesAdmin.Common.Common;
using System.ComponentModel.DataAnnotations;
using DevExpress.Mvvm;
using System.Linq;

namespace MesAdmin.Models
{
    public class StockMovementHeader : ViewModelBase
    {
        public string DocumentNo
        { 
            get { return GetProperty(() => DocumentNo); }
            set { SetProperty(() => DocumentNo, value); }
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
        public DateTime DocumentDate
        {
            get { return GetProperty(() => DocumentDate); }
            set { SetProperty(() => DocumentDate, value); }
        }
        public string BizAreaCode
        {
            get { return GetProperty(() => BizAreaCode); }
            set { SetProperty(() => BizAreaCode, value); }
        }
        public string Memo
        {
            get { return GetProperty(() => Memo); }
            set { SetProperty(() => Memo, value); }

        }
        public string UpdateId
        {
            get { return GetProperty(() => UpdateId); }
            set { SetProperty(() => UpdateId, value); }
        }
        public DateTime UpdateDate
        {
            get { return GetProperty(() => UpdateDate); }
            set { SetProperty(() => UpdateDate, value); }
        }

        public StockMovementHeader() { }
        public StockMovementHeader(string documentNo)
        {
            Database db = ProviderFactory.Instance;
            string sql = "SELECT * FROM stock_Movement_Header A (NOLOCK) WHERE DocumentNo = @DocumentNo AND DelFlag='N' ";

            DbCommand dbCom = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCom, "@DocumentNo", DbType.String, documentNo);
            DataSet ds = db.ExecuteDataSet(dbCom);
            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
            {
                DocumentNo = (string)u["DocumentNo"];
                TransType = (string)u["TransType"];
                MoveType = (string)u["MoveType"];
                DocumentDate = (DateTime)u["DocumentDate"];
                BizAreaCode = u["BizAreaCode"].ToString();
                Memo = u["Memo"].ToString();
                UpdateId = (string)u["UpdateId"];
                UpdateDate = (DateTime)u["UpdateDate"];
            });
        }
    }

    public class StockMovementHeaderList : ObservableCollection<StockMovementHeader>
    {
        public StockMovementHeaderList() { }
        public StockMovementHeaderList(IEnumerable<StockMovementHeader> items) : base(items) { }
        public StockMovementHeaderList(DateTime startDate, DateTime endDate, string transType = "")
        {
            InitializeList(startDate, endDate, transType);
        }

        public void InitializeList(DateTime startDate, DateTime endDate, string transType = "")
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            string str = "SELECT A.* FROM stock_Movement_Header A (NOLOCK) INNER JOIN common_Minor B (NOLOCK) ON A.MoveType = B.MinorCode ";
                str += "WHERE A.DelFlag = 'N' AND B.IsEnabled = 1 AND DocumentDate BETWEEN '" + startDate.ToShortDateString() + "' AND '" + endDate.ToShortDateString() + "' ";
            if (!string.IsNullOrEmpty(transType))
                str += "And A.TransType = '" + transType + "' ";
            str += "ORDER BY DocumentDate DESC";
            DbCommand dbCom = db.GetSqlStringCommand(str);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new StockMovementHeader
                    {
                        DocumentNo = (string)u["DocumentNo"],
                        TransType = (string)u["TransType"],
                        MoveType = (string)u["MoveType"],
                        DocumentDate = (DateTime)u["DocumentDate"],
                        BizAreaCode = u["BizAreaCode"].ToString(),
                        Memo = u["Memo"].ToString(),
                        UpdateId = (string)u["UpdateId"],
                        UpdateDate = (DateTime)u["UpdateDate"]
                    }
                )
            );
        }
    }
}
