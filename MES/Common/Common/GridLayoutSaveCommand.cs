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
    public class GridLayoutSaveCommand : Behavior<GridControl>
    {
        #region Public Properties
        public string ViewName { get; set; }
        public Stream LayoutStream { get; set; }
        #endregion

        #region Commands
        protected DelegateCommand saveLayoutCmd;
        public DelegateCommand SaveLayoutCmd
        {
            get
            {
                if (saveLayoutCmd == null)
                {
                    saveLayoutCmd = new DelegateCommand(OnSaveLayout);
                }

                return saveLayoutCmd;
            }
        }
        protected DelegateCommand restoreLayoutCmd;
        public DelegateCommand RestoreLayoutCmd
        {
            get
            {
                if (restoreLayoutCmd == null)
                {
                    restoreLayoutCmd = new DelegateCommand(OnRestoreLayout, CanRestoreLayout);
                }

                return restoreLayoutCmd;
            }
        }
        #endregion

        private const string layoutType = "grid";

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

            var rows = GlobalCommonLayout.Instance.AsEnumerable().Where(o => o.Field<string>("ViewName") == ViewName && o.Field<string>("LayoutType") == layoutType);
            if (rows.Count() == 0) return;

            using (MemoryStream str = new MemoryStream(Encoding.UTF8.GetBytes(rows.FirstOrDefault().Field<string>("Layout"))))
            {
                AssociatedObject.AutoExpandAllGroups = true;
                AssociatedObject.RestoreLayoutFromStream(str);
            }
        }

        protected void OnSaveLayout()
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
                        db.AddInParameter(dbCom, "@LayoutType", DbType.String, layoutType);
                        db.ExecuteNonQuery(dbCom, trans);
                        trans.Commit();
                        GlobalCommonLayout.Instance = null;
                    }
                }
            }
            catch (Exception ex)
            {
                DXMessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected bool CanRestoreLayout()
        {
            var rows = GlobalCommonLayout.Instance.AsEnumerable().Where(o => o.Field<string>("ViewName") == ViewName && o.Field<string>("LayoutType") == layoutType);
            return rows.Count() > 0;
        }
        protected void OnRestoreLayout()
        {
            try
            {
                Database db = ProviderFactory.Instance;
                string sql = "DELETE common_gridLayout WHERE UserId = @UserID AND ViewName = @ViewName AND LayoutType = @LayoutType";

                DbCommand dbCom = db.GetSqlStringCommand(sql);
                db.AddInParameter(dbCom, "@UserID", DbType.String, DSUser.Instance.UserID);
                db.AddInParameter(dbCom, "@ViewName", DbType.String, ViewName);
                db.AddInParameter(dbCom, "@LayoutType", DbType.String, layoutType);
                db.ExecuteNonQuery(dbCom);
                GlobalCommonLayout.Instance = null;

                LayoutStream.Seek(0, SeekOrigin.Begin);
                AssociatedObject.RestoreLayoutFromStream(LayoutStream);
            }
            catch (Exception ex)
            {
                DXMessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}