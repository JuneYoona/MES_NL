using System;
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Xpf.Grid;
using System.Data;
using System.IO;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data.Common;
using System.Windows;
using MesAdmin.Models;
using DevExpress.Xpf.Core;
using System.Linq;

namespace MesAdmin.Common.Common
{
    public class CommandToMethod : Behavior<GridControl>
    {
        public string ViewName { get; set; }
        public string DefaultLayout { get; set; }
        public Stream LayoutStream { get; set; }

        #region SaveCommand
        protected DelegateCommand saveCommand;
        public DelegateCommand SaveCommand
        {
            get
            {
                if (saveCommand == null)
                {
                    saveCommand = new DelegateCommand(SaveCommandExecute, SaveCommandCanExecute);
                }

                return this.saveCommand;
            }
        }

        protected bool SaveCommandCanExecute()
        {
            return true;
        }
        protected void SaveCommandExecute()
        {
            try
            {
                using (MemoryStream str = new MemoryStream())
                {
                    AssociatedObject.SaveLayoutToStream(str);
                    byte[] array = str.ToArray();
                    string layout = Encoding.UTF8.GetString(array, 0, array.Length);

                    Database db = ProviderFactory.Instance;
                    using (DbConnection conn = db.CreateConnection())
                    {
                        conn.Open();
                        DbTransaction trans = conn.BeginTransaction();
                        DbCommand dbCom = null;

                        dbCom = db.GetStoredProcCommand("usp_common_gridLayout");
                        db.AddInParameter(dbCom, "@ViewName", DbType.String, ViewName);
                        db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                        db.AddInParameter(dbCom, "@Layout", DbType.String, layout);
                        db.ExecuteNonQuery(dbCom, trans);
                        trans.Commit();
                        GlabalCommonLayout.Instance = null;
                    }
                }
            }
            catch (Exception ex)
            {
                DXMessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region RestoreCommand
        protected DelegateCommand restoreCommand;
        public DelegateCommand RestoreCommand
        {
            get
            {
                if (restoreCommand == null)
                {
                    restoreCommand = new DelegateCommand(RestoreCommandExecute, RestoreCommandCanExecute);
                }

                return restoreCommand;
            }
        }

        protected bool RestoreCommandCanExecute()
        {
            var rows = GlabalCommonLayout.Instance.AsEnumerable().Where(o => o.Field<string>("ViewName") == ViewName);
            return rows.Count() > 0;
        }
        protected void RestoreCommandExecute()
        {
            try
            {
                Database db = ProviderFactory.Instance;
                string sql = "DELETE common_gridLayout WHERE UserId = @UserID AND ViewName = @ViewName";

                DbCommand dbCom = db.GetSqlStringCommand(sql);
                db.AddInParameter(dbCom, "@UserID", DbType.String, DSUser.Instance.UserID);
                db.AddInParameter(dbCom, "@ViewName", DbType.String, ViewName);
                db.ExecuteNonQuery(dbCom);
                GlabalCommonLayout.Instance = null;

                LayoutStream.Seek(0, SeekOrigin.Begin);
                AssociatedObject.RestoreLayoutFromStream(LayoutStream);
            }
            catch (Exception ex)
            {
                DXMessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion RestoreCommand

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += OnAssociatedObjectLoaded;
        }

        protected void OnAssociatedObjectLoaded(object sender, RoutedEventArgs e)
        {
            // default layout 을 항상 갱신(최신상태 유지용)
            LayoutStream = new MemoryStream();
            AssociatedObject.SaveLayoutToStream(LayoutStream);

            var rows = GlabalCommonLayout.Instance.AsEnumerable().Where(o => o.Field<string>("ViewName") == ViewName);
            if (rows.Count() == 0) return;

            using (MemoryStream str = new MemoryStream(Encoding.UTF8.GetBytes(rows.FirstOrDefault().Field<string>("Layout"))))
            {
                ((GridControl)AssociatedObject).AutoExpandAllGroups = true;
                AssociatedObject.RestoreLayoutFromStream(str);
            }
        }
    }
}