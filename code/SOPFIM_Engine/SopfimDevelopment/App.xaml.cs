﻿using System.Windows.Threading;
using Esri.CommonUtils;
using Sopfim.CustomControls;
using log4net.Config;


namespace SopfimDevelopment
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : EsriWpfApp
    {
        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            XmlConfigurator.Configure();
            base.OnStartup(e);
        }

        private void EsriWpfApp_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Error("unhandled error caught in the application", e.Exception);
            Xceed.Wpf.Toolkit.MessageBox.Show("Error caught: " + e.Exception.Message);
        }
    }
}
