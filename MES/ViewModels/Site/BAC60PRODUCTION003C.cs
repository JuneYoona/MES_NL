using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Windows;

namespace MesAdmin.ViewModels
{
    public class BAC60PRODUCTION003CVM : ViewModelBase
    {
        #region Services
        #endregion

        #region Public Properties
        public DateTime? SourceDate
        {
            get { return GetValue<DateTime?>(); }
            set { SetValue(value); }
        }
        public DateTime? TargetDate
        {
            get { return GetValue<DateTime?>(); }
            set { SetValue(value); }
        }
        #endregion

        #region Commands
        public List<UICommand> DialogCmds { get; private set; }
        protected UICommand ConfirmUICmd { get; private set; }
        protected UICommand CancelUICmd { get; private set; }
        #endregion

        public BAC60PRODUCTION003CVM()
        {
            // dialog command
            ConfirmUICmd = new UICommand()
            {
                Caption = "저장",
                IsDefault = true,
                IsCancel = false,
                Id = MessageBoxResult.OK,
                Command = new DelegateCommand(null, () => SourceDate != null && TargetDate != null)
            };

            CancelUICmd = new UICommand()
            {
                Caption = "닫기",
                IsDefault = false,
                IsCancel = true,
                Id = MessageBoxResult.Cancel,
            };

            DialogCmds = new List<UICommand>() { ConfirmUICmd, CancelUICmd };
        }
    }
}
