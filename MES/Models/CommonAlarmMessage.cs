using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using MesAdmin.Common.Common;
using System.ComponentModel.DataAnnotations;

namespace MesAdmin.Models
{
    public class CommonAlarmMessage : StateBusinessObject
    {
        public string MsgId
        {
            get { return GetProperty(() => MsgId); }
            set { SetProperty(() => MsgId, value); }
        }
        public string UserId
        {
            get { return GetProperty(() => UserId); }
            set { SetProperty(() => UserId, value); }
        }
        public string Title
        {
            get { return GetProperty(() => Title); }
            set { SetProperty(() => Title, value); }
        }
        public string Content1
        {
            get { return GetProperty(() => Content1); }
            set { SetProperty(() => Content1, value); }
        }
        public string Content2
        {
            get { return GetProperty(() => Content2); }
            set { SetProperty(() => Content2, value); }
        }
        public string ReadFlag
        {
            get { return GetProperty(() => ReadFlag); }
            set { SetProperty(() => ReadFlag, value); }
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

        public void UpdateReadFlag()
        {
            Database db = ProviderFactory.Instance;

            string str = "UPDATE common_AlarmMessage SET ReadFlag = 'Y', UpdateId = @UpdateId, UpdateDate = getdate() WHERE MsgId = @MsgId";
            DbCommand dbCom = db.GetSqlStringCommand(str);
            db.AddInParameter(dbCom, "@MsgId", DbType.String, MsgId);
            db.AddInParameter(dbCom, "@UpdateId", DbType.String, DSUser.Instance.UserID);
            db.ExecuteNonQuery(dbCom);
        }
    }

    public class CommonAlarmMessageList : ObservableCollection<CommonAlarmMessage>
    {
        private string userId;

        public CommonAlarmMessageList(string userId)
        {
            this.userId = userId;
            InitializeList();
        }

        public void InitializeList()
        {
            base.Clear();
            Database db = ProviderFactory.Instance;

            string str = "SELECT * FROM common_AlarmMessage (NOLOCK) WHERE UserId = @UserId AND ReadFlag = 'N' ORDER BY InsertDate";
            DbCommand dbCom = db.GetSqlStringCommand(str);
            db.AddInParameter(dbCom, "@UserId", DbType.String, userId);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new CommonAlarmMessage
                    {
                        MsgId = (string)u["MsgId"],
                        UserId = (string)u["UserId"],
                        Title = (string)u["Title"],
                        Content1 = (string)u["Content1"],
                        Content2 = (string)u["Content2"],
                        UpdateId = (string)u["UpdateId"],
                        ReadFlag = (string)u["ReadFlag"],
                        UpdateDate = (DateTime)u["UpdateDate"],
                    }
                )
            );
        }
    }
}
