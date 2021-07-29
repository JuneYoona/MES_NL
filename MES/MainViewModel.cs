using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using System;
using MesAdmin.Common.Utils;
using DevExpress.Xpf.NavBar;
using System.Windows;
using System.Collections.Generic;
using MesAdmin.Common.Common;
using System.Threading.Tasks;
using MesAdmin.Common.Services;
using DevExpress.Xpf.Core.Native;
using DevExpress.Xpf.Core;

namespace MesAdmin
{
    public class MainViewModel : ViewModelBase
    {
        protected IDocumentManagerService DocumentManagerService { get { return GetService<IDocumentManagerService>("MainViewService"); } }
        protected virtual IMessageBoxService MessageBoxService { get { return null; } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        protected ISplashScreenManagerService SplashScreenManagerService { get { return GetService<ISplashScreenManagerService>(); } }

        public IList<NetMenu> NaviItems
        {
            get { return GetProperty(() => NaviItems); }
            set { SetProperty(() => NaviItems, value); }
        }
        public NetMenu SelectedGroup
        {
            get { return GetValue<NetMenu>(); }
            set { SetValue(value, () => Properties.Settings.Default.ActiveGroup = value.MenuName); }
        }
        public string UpdaterText
        {
            get { return GetProperty(() => UpdaterText); }
            set { SetProperty(() => UpdaterText, value); }
        }
        public SilentUpdater UpdateService { get; }

        public static AsyncCommand<string> ShowDocumentCmd { get; set; }
        public ICommand CloseCmd { get; set; }
        public ICommand ExitCmd { get; set; }
        public ICommand LoadCmd { get; set; }
        public ICommand<GroupAddingEventArgs> GroupAddingCmd { get; set; }
        public ICommand HelpCmd { get; set; }
        public ICommand ReLoginCmd { get; set; }
        public ICommand ActiveDocumentChangedCmd { get; set; }
        public ICommand ReStartCmd { get; set; }

        #region Public Properties
        public string DBName { get { return "Database : " + DBInfo.Instance.Name; } }
        public string UserName { get { return "사용자 : " + DSUser.Instance.UserName; } }
        public string BizAreaCode { get { return "공장정보 : " + MinorCodeConverter.Convert("I0004", DSUser.Instance.BizAreaCode); } }
        public string Ver { get; set; }
        public string ViewName
        {
            get { return GetProperty(() => ViewName); }
            set { SetProperty(() => ViewName, value); }
        }
        public double? Opacity
        {
            get { return GetProperty(() => Opacity); }
            set { SetProperty(() => Opacity, value); }
        }
        #endregion

        public MainViewModel()
        {
            if (!IsInDesignMode)
            {
                UpdateService = SilentUpdater.Instance;
                UpdateService.ProgressChanged += SilentUpdaterOnProgressChanged;
                // Uncomment if app needs to be more disruptive
                //UpdateService.Completed += UpdateServiceCompleted;
            }

            LoadCmd = new DelegateCommand(OnLoad);
            ShowDocumentCmd = new AsyncCommand<string>(ShowDocument);
            CloseCmd = new DelegateCommand(OnClose);
            HelpCmd = new DelegateCommand(OnHelp);
            ExitCmd = new DelegateCommand(OnExit);
            ReLoginCmd = new DelegateCommand(OnReLogin);
            GroupAddingCmd = new DelegateCommand<GroupAddingEventArgs>(OnGroupAdding);
            ActiveDocumentChangedCmd = new DelegateCommand(OnActiveDocumentChanged);
            ReStartCmd = new DelegateCommand(() => Application.Current.ReStart());

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Version version = assembly.GetName().Version;

            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            {
                 Ver = "Application Version : " + System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
        }

        public void OnLoad()
        {
            NaviItems = (new NetMenus()).GetUserMenus(DSUser.Instance.UserID);
        }

        public void OnGroupAdding(GroupAddingEventArgs e)
        {
            NetMenu grp = e.SourceObject as NetMenu;

            e.Group.DisplaySource = DisplaySource.Content;
            e.Group.Name = grp.MenuName;
            e.Group.Header = grp.MenuName;
            e.Group.Content = grp.Submenu;
            e.Group.ContentTemplate = (DataTemplate)e.Group.FindResource("groupContentTemplate");
            e.Group.ImageSettings = new ImageSettings { Height = 16, Width = 16 };
            if (!string.IsNullOrEmpty(grp.ImageSourceUri))
                e.Group.ImageSource = WpfSvgRenderer.CreateImageSource(DXImageHelper.GetImageUri(grp.ImageSourceUri));

            if (grp.MenuName == Properties.Settings.Default.ActiveGroup) SelectedGroup = grp;
        }

        public Task ShowDocument(string p)
        {
            if (!string.IsNullOrEmpty(p)) TabLoadingOpen();
            return Task.Factory.StartNew(() => ShowDocumentCore(p));
        }

        public void ShowDocumentCore(string p)
        {
            if (string.IsNullOrEmpty(p)) return;

            string[] parameters = new string[5];
            string[] strSplit = p.Split(';');
            for (int i = 0; i < strSplit.Length; i++)
            {
                parameters[i] = strSplit[i];
            }

            DocumentParamter pm = new DocumentParamter(EntityMessageType.Added, parameters[2], parameters[3], this);

            DispatcherService.BeginInvoke(new Action(() =>
            {
                try
                {
                    IDocument document = DocumentManagerService.FindDocumentByIdOrCreate(parameters[0], x => CreateDocument(parameters[0], parameters[1], pm));
                    document.Id = new Guid(); // 무한탭열기
                    document.Show();
                }
                catch { }
            }));
        }

        IDocument CreateDocument(string viewName, string title, DocumentParamter parameter)
        {
            var document = DocumentManagerService.CreateDocument(viewName, parameter, this);
            document.Title = title;
            document.DestroyOnClose = true;
            return document;
        }

        public void OnClose()
        {
            IDocument document = DocumentManagerService.ActiveDocument;
            if (document != null)
                document.Close();

            TabLoadingClose();
        }

        public void OnExit()
        {
             System.Windows.Application.Current.MainWindow.Close();
        }

        public void OnHelp()
        {
            System.Windows.Forms.Help.ShowHelp(null, System.Windows.Forms.Application.StartupPath + @"\MES-NL.chm");
        }

        public void OnReLogin()
        {
            MainWindow mainWnd = Application.Current.MainWindow as MainWindow;
            Window loginWnd = new MesAdmin.Authentication.LoginView();
            Application.Current.MainWindow = loginWnd;
            loginWnd.Show();
            mainWnd.Close();
        }

        public void OnActiveDocumentChanged()
        {
            try
            {
                if (DocumentManagerService.ActiveDocument != null)
                {
                    var doc = DocumentManagerService.ActiveDocument as DevExpress.Xpf.Docking.Native.DockingDocumentUIServiceBase<DevExpress.Xpf.Docking.DocumentPanel, DevExpress.Xpf.Docking.DocumentGroup>.Document;
                    ViewName = doc.DocumentType;
                }
            }
            catch { TabLoadingClose(); }
        }

        public void SilentUpdaterOnProgressChanged(object sender, UpdateProgressChangedEventArgs e)
        {
            UpdaterText = e.StatusString;
        }

        public void TabLoadingOpen()
        {
            Opacity = 0.55;
            SplashScreenManagerService.Show();
        }

        public void TabLoadingClose()
        {
            Opacity = 1;
            SplashScreenManagerService.Close();
        }
    }
}