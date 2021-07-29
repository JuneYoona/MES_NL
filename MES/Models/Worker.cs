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
    public class Worker : ViewModelBase
    {
        public bool IsChecked
        {
            get { return GetProperty(() => IsChecked); }
            set { SetProperty(() => IsChecked, value); }
        }
        public string WorkerId
        {
            get { return GetProperty(() => WorkerId); }
            set { SetProperty(() => WorkerId, value); }
        }
        public string WorkerName
        {
            get { return GetProperty(() => WorkerName); }
            set { SetProperty(() => WorkerName, value); }
        }
    }

    public class WorkerList : ObservableCollection<Worker>
    {
        private string waCode;
        private string bizAreaCode;

        public WorkerList(string waCode, string bizAreaCode = "")
        {
            this.waCode = waCode;
            this.bizAreaCode = bizAreaCode;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;

            DbCommand dbCom = db.GetStoredProcCommand("usp_GetWorker");
            dbCom.CommandType = CommandType.StoredProcedure;
            db.AddInParameter(dbCom, "@WaCode", DbType.String, waCode);
            db.AddInParameter(dbCom, "@BizAreaCode", DbType.String, bizAreaCode);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new Worker
                    {
                        WorkerId = (string)u["WorkerId"],
                        WorkerName = (string)u["WorkerName"]
                    }
                )
            );
        }
    }
}
