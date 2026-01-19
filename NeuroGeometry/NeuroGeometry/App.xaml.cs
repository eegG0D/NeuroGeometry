using System;
using System.Windows;

namespace NeuroGeometry
{
    /// <summary>
    /// Runtime initialization begins in a main method.
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // In a complex BCI app, we initialize the abstraction layer here
            // before the UI loads to ensure hardware connectivity.
        }
    }
}