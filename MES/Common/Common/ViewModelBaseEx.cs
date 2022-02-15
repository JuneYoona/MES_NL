using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections;
using System.ComponentModel;
using DevExpress.Mvvm;
using System.ComponentModel.DataAnnotations;

namespace MesAdmin.Common.Common
{
    public class ViewModelBaseEx : ViewModelBase
    {
        public string ModuleName { get; set; }
        public EntityState State
        {
            get { return GetProperty(() => State); }
            set { SetProperty(() => State, value); }
        }
    }
}