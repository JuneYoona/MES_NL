using System;
using System.Linq;
using DevExpress.Mvvm;
using System.Windows.Input;
using MesAdmin.Models;
using System.Web.Security;
using System.Collections.ObjectModel;
using MesAdmin.Common.Common;
using System.Threading.Tasks;

namespace MesAdmin.ViewModels
{
    public class StockCloseVM : ViewModelBase
    {
        #region Services
        IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        IDispatcherService DispatcherService { get { return GetService<IDispatcherService>(); } }
        #endregion

        #region Public Properties
        public StockClose StockClose
        {
            get { return GetProperty(() => StockClose); }
            set { SetProperty(() => StockClose, value); }
        }
        public bool? IsEnabled
        {
            get { return GetProperty(() => IsEnabled); }
            set { SetProperty(() => IsEnabled, value); }
        }
        #endregion

        #region Commands
        public AsyncCommand CloseCmd { get; set; }
        public AsyncCommand CancelCmd { get; set; }
        #endregion

        public StockCloseVM()
        {
            StockClose = new StockClose();

            CloseCmd = new AsyncCommand(OnClose);
            CancelCmd = new AsyncCommand(OnCancel, CanCancel);
        }

        public Task OnClose()
        {
            IsEnabled = false;
            return Task.Factory.StartNew(CloseCore);
        }
        private void CloseCore()
        {
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(0.5));
            DispatcherService.BeginInvoke(() =>
            {
                try
                {
                    StockClose.Close();
                    StockClose.Initialize();
                }
                catch (Exception ex)
                {
                    MessageBoxService.ShowMessage(ex.Message, "Information", MessageButton.OK, MessageIcon.Information);
                }
            });
            IsEnabled = true;
        }

        public bool CanCancel()
        {
            return StockClose.ClosedDate != null;
        }
        public Task OnCancel()
        {
            IsEnabled = false;
            return Task.Factory.StartNew(CancelCore);
        }
        private void CancelCore()
        {
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(0.5));
            DispatcherService.BeginInvoke(() =>
            {
                try
                {
                    StockClose.Cancel();
                    StockClose.Initialize();
                }
                catch (Exception ex)
                {
                    MessageBoxService.ShowMessage(ex.Message, "Information", MessageButton.OK, MessageIcon.Information);
                }
            });
            IsEnabled = true;
        }

        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            if (ViewModelBase.IsInDesignMode) return;

            DocumentParamter pm = parameter as DocumentParamter;
            ((MainViewModel)pm.ParentViewmodel).TabLoadingClose();
        }
    }
}
