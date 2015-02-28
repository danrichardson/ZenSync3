using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ZenSync
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region settings
        private void OnExit(object sender, ExitEventArgs e)
        {
            if (ZenSync.Properties.Settings.Default.SaveSettings)
                ZenSync.Properties.Settings.Default.Save();
        }


        #endregion
    }
}
