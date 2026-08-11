using System.Windows;
using System.Windows.Threading;

namespace LoginRegisterApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherUnhandledException += (s, ex) =>
            {
                MessageBox.Show(ex.Exception.ToString(), "Erreur non gérée",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                ex.Handled = true; // empêche l'appli de se fermer brutalement
            };
        }
    }
}
