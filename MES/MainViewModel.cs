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
using System.Deployment.Application;
using DevExpress.Xpf.Docking;
using System.Windows.Controls;
using DevExpress.Xpf.Docking.Native;

namespace MesAdmin
{
    public class MainViewModel : ViewModelBase
    {
        #region Services
        protected IDocumentManagerService DocumentManagerService { get { return GetService<IDocumentManagerService>("MainViewService"); } }
        protected virtual IMessageBoxService MessageBoxService { get { return null; } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        protected ISplashScreenManagerService SplashScreenManagerService { get { return GetService<ISplashScreenManagerService>(); } }
        #endregion

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
        #endregion

        #region Commands
        public static AsyncCommand<string> ShowDocumentCmd { get; set; }
        public ICommand CloseCmd { get; set; }
        public ICommand ExitCmd { get; set; }
        public ICommand LoadCmd { get; set; }
        public ICommand<GroupAddingEventArgs> GroupAddingCmd { get; set; }
        public ICommand HelpCmd { get; set; }
        public ICommand ReLoginCmd { get; set; }
        public ICommand ActiveDocumentChangedCmd { get; set; }
        public ICommand ReStartCmd { get; set; }
        public ICommand ConfigUserCmd { get; set; }
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
            ExitCmd = new DelegateCommand(() => Application.Current.MainWindow.Close());
            ReLoginCmd = new DelegateCommand(OnReLogin);
            GroupAddingCmd = new DelegateCommand<GroupAddingEventArgs>(OnGroupAdding);
            ActiveDocumentChangedCmd = new DelegateCommand<ActiveDocumentChangedEventArgs>(OnActiveDocumentChanged);
            ReStartCmd = new DelegateCommand(() => Application.Current.ReStart());
            ConfigUserCmd = new DelegateCommand(OnConfigUser);

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                Ver = "Application Version : ";
                Ver += ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
            else
            {
                Ver = "Application Version : Demo";
            }
        }

        public void OnLoad()
        {
            NaviItems = new NetMenus().GetUserMenus(DSUser.Instance.UserID);
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

        public void OnConfigUser()
        {
            IDocument document = DocumentManagerService.CreateDocument("NetUserNewView", new DocumentParamter(DSUser.Instance.UserID), this);
            document.DestroyOnClose = true;
            document.Title = "사용자 설정";
            document.Show();
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

        public IDocument CreateDocument(string viewName, string title, DocumentParamter parameter)
        {
            var document = DocumentManagerService.CreateDocument(viewName, parameter, this);
            document.Title = title;
            document.DestroyOnClose = true;

            // 사용자 컨트롤 로드가 완료되면 Loading Panel 닫기위해 ucLoaded = true 처리
            var doc = document as DockingDocumentUIServiceBase<DocumentPanel, DocumentGroup>.Document;
            ((UserControl)doc.DocumentPanel.Content).Loaded += (s, e) => ucLoaded = true;

            return document;
        }

        public IDocument FindDocument(string documentId)
        {
            foreach (var doc in DocumentManagerService.Documents)
                if (documentId.Equals(doc.Id))
                    return doc;
            return null;
        }

        public void OnClose()
        {
            IDocument document = DocumentManagerService.ActiveDocument;
            if (document != null) document.Close();

            Opacity = 1;
            SplashScreenManagerService.Close();
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

        public void OnActiveDocumentChanged(ActiveDocumentChangedEventArgs e)
        {
            if (DocumentManagerService.ActiveDocument != null)
            {
                var doc = e.NewDocument as DockingDocumentUIServiceBase<DocumentPanel, DocumentGroup>.Document;
                ViewName = doc.DocumentType;
            }
            else ViewName = string.Empty;
        }

        public void SilentUpdaterOnProgressChanged(object sender, UpdateProgressChangedEventArgs e)
        {
            UpdaterText = e.StatusString;
        }

        private bool ucLoaded;
        public void TabLoadingOpen()
        {
            ucLoaded = false;
            Opacity = 0.55;
            SplashScreenManagerService.Show();
        }

        public async void TabLoadingClose()
        {
            await Task.Run(() => { while (!ucLoaded) { } }).ContinueWith(t => System.Threading.Thread.Sleep(200));

            Opacity = 1;
            SplashScreenManagerService.Close();
        }
    }
}