using System;
using System.Windows.Forms;

namespace WonderDog
{
    static class Program
    {
        public const string APP_ID = "2C68558F-86B8-4498-9785-3035E5ED64D3";
     
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

#if !DEBUG
            try
            {
                if(SelfUpdatingApp.Installer.IsUpdateAvailableAsync(APP_ID, true).Result)
                {
                    SelfUpdatingApp.Installer.Launch(APP_ID);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
#endif

            Application.Run(new frmMain());
        }
    }
}
