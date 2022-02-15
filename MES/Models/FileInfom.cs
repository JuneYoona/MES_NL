using System;
using System.Collections.ObjectModel;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Windows.Media;
using MesAdmin.Common.Common;

namespace MesAdmin.Models
{
    public class FileInform : StateBusinessObject
    {
        public Guid Id { get; set; }
        public string DocumentNo { get; set; }
        public int Seq { get; set; }
        public string FileName { get; set; }
        public Int32 FileSize { get; set; }
        public byte[] Contents { get; set; }
        public ImageSource ItemIcon { get; set; }
    }

    public class FileInformList : ObservableCollection<FileInform>
    {
        public FileInformList() { }
        public FileInformList(string documentNo, int seq)
        {
            InitializeList(documentNo, seq);
        }

        public void InitializeList(string documentNo, int seq)
        {
            base.Clear();
            Database db = ProviderFactory.Instance;
            string str = "SELECT * FROM common_AttachedFiles WHERE DocumentNo = '{0}' And Seq = {1} ";
            str = string.Format(str, documentNo, seq);
            DbCommand dbCom = db.GetSqlStringCommand(str);
            DataSet ds = db.ExecuteDataSet(dbCom);

            ds.Tables[0].AsEnumerable().ToList().ForEach(u =>
                base.Add(
                    new FileInform
                    {
                        Id = (Guid)u["Id"],
                        DocumentNo = (string)u["DocumentNo"],
                        Seq = (int)u["Seq"],
                        FileName = (string)u["FileName"],
                        FileSize = (Int32)u["FileSize"],
                        Contents = (byte[])u["Contents"],
                        ItemIcon = IconManager.FindIconForFilename((string)u["FileName"], false)
                    }
                )
            );
        }

        public void Save()
        {
            string documentNo = string.Empty;
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                string sql;
                try
                {
                    foreach (var item in this.Items.Where(u => u.State == EntityState.Added))
                    {
                        sql = "INSERT INTO common_AttachedFiles VALUES (@Id, @DocumentNo, @Seq, @FileName, @FileSize, @Contents, @InsertId, getdate())";
                        dbCom = db.GetSqlStringCommand(sql);
                        db.AddInParameter(dbCom, "@Id", DbType.Guid, item.Id);
                        db.AddInParameter(dbCom, "@DocumentNo", DbType.String, item.DocumentNo);
                        db.AddInParameter(dbCom, "@Seq", DbType.String, item.Seq);
                        db.AddInParameter(dbCom, "@FileName", DbType.String, item.FileName);
                        db.AddInParameter(dbCom, "@FileSize", DbType.Int32, item.FileSize);
                        db.AddInParameter(dbCom, "@Contents", DbType.Binary, item.Contents);
                        db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                        db.ExecuteNonQuery(dbCom, trans);
                    }
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public void Delete()
        {
            string documentNo = string.Empty;
            Database db = ProviderFactory.Instance;
            using (DbConnection conn = db.CreateConnection())
            {
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                DbCommand dbCom = null;
                string sql;
                try
                {
                    foreach (var item in this.Items)
                    {
                        sql = "DELETE common_AttachedFiles WHERE Id = '{0}'";
                        sql = string.Format(sql, item.Id);
                        dbCom = db.GetSqlStringCommand(sql);
                        db.ExecuteNonQuery(dbCom, trans);
                    }
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
