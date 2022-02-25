using DevExpress.Mvvm;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Docking;
using MesAdmin.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace MesAdmin.Common.Common
{
    public class DockLayoutSaveCommand : Behavior<DockLayoutManager>
    {
        #region Public Properties
        public string ViewName { get; set; }
        public string DefaultLayout { get; set; }
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

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += OnAssociatedObjectLoaded;
        }

        protected void OnAssociatedObjectLoaded(object sender, RoutedEventArgs e)
        {
            IWorkspaceManager workspaceManager = WorkspaceManager.GetWorkspaceManager(AssociatedObject);

            // default layout 을 항상 갱신(최신상태 유지용)
            LayoutStream = new MemoryStream();
            workspaceManager.CaptureWorkspace("Layout");
            workspaceManager.SaveWorkspace("Layout", LayoutStream);

            var rows = GlobalCommonDockLayout.Instance.AsEnumerable().Where(o => o.Field<string>("ViewName") == ViewName);
            if (rows.Count() == 0) return;

            using (MemoryStream str = new MemoryStream(Encoding.UTF8.GetBytes(rows.FirstOrDefault().Field<string>("Layout"))))
            {
                workspaceManager.LoadWorkspace("Layout", str);
                workspaceManager.ApplyWorkspace("Layout");
            }
        }

        protected void OnSaveLayout()
        {
            try
            {
                IWorkspaceManager workspaceManager = WorkspaceManager.GetWorkspaceManager(AssociatedObject);

                using (MemoryStream stream = new MemoryStream())
                {
                    workspaceManager.CaptureWorkspace("Layout");
                    workspaceManager.SaveWorkspace("Layout", stream);

                    byte[] array = stream.ToArray();
                    string layout = Encoding.UTF8.GetString(array, 0, array.Length);

                    Database db = ProviderFactory.Instance;
                    using (DbConnection conn = db.CreateConnection())
                    {
                        conn.Open();
                        DbTransaction trans = conn.BeginTransaction();
                        DbCommand dbCom = null;

                        dbCom = db.GetStoredProcCommand("usp_common_dockLayout");
                        db.AddInParameter(dbCom, "@ViewName", DbType.String, ViewName);
                        db.AddInParameter(dbCom, "@InsertId", DbType.String, DSUser.Instance.UserID);
                        db.AddInParameter(dbCom, "@Layout", DbType.String, layout);
                        db.ExecuteNonQuery(dbCom, trans);
                        trans.Commit();
                        GlobalCommonDockLayout.Instance = null;
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
            var rows = GlobalCommonDockLayout.Instance.AsEnumerable().Where(o => o.Field<string>("ViewName") == ViewName);
            return rows.Count() > 0;
        }
        protected void OnRestoreLayout()
        {
            try
            {
                Database db = ProviderFactory.Instance;
                string sql = "DELETE common_dockLayout WHERE UserId = @UserID AND ViewName = @ViewName";

                DbCommand dbCom = db.GetSqlStringCommand(sql);
                db.AddInParameter(dbCom, "@UserID", DbType.String, DSUser.Instance.UserID);
                db.AddInParameter(dbCom, "@ViewName", DbType.String, ViewName);
                db.ExecuteNonQuery(dbCom);
                GlobalCommonDockLayout.Instance = null;

                LayoutStream.Seek(0, SeekOrigin.Begin);
                IWorkspaceManager workspaceManager = WorkspaceManager.GetWorkspaceManager(AssociatedObject);
                workspaceManager.LoadWorkspace("Layout", LayoutStream);
                workspaceManager.ApplyWorkspace("Layout");
            }
            catch (Exception ex)
            {
                DXMessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}