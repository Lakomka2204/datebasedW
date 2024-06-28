using System.Windows;
using MySqlConnector;
using System.Drawing;
using System.Windows.Controls;
using System;

namespace datebasedW
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MySqlConnection maincon;
        public static void SetContent(Page page)
        {
            Application.Current.MainWindow.Content = page;
        }
        int errcount = 0;
        public MainWindow()
        {
            InitializeComponent();
            Dispatcher.UnhandledException += (s, a) =>
            {
                if (a.Exception is MySqlException && errcount < 10)
                {
                    if (maincon.State < System.Data.ConnectionState.Open)
                        maincon.Open();
                    a.Handled = true;
                    MessageBox.Show("DB exception occured, please try again\n"+a.Exception.ToString());
                    if (!maincon.Ping())
                        errcount++;
                    return;
                }
                a.Handled = true;
                if (MessageBox.Show(a.Exception.ToString() + "\n\nQuit?", a.Exception.GetType().FullName,
                    MessageBoxButton.YesNo, MessageBoxImage.Error,
                    MessageBoxResult.Yes) == MessageBoxResult.Yes) Application.Current.Shutdown(a.Exception.HResult);
            };
            InitDB();
            Content = new LoginPage(maincon);
        }
        void InitDB()
        {
            if (maincon == null) // 37.54.7.14
                maincon = new MySqlConnection("server=localhost;port=3306;uid=user;pwd=justadefaultpassword;database=datebasedw");
            else if (maincon.State < System.Data.ConnectionState.Open)
                maincon.Open();
        }
        public void RestoreContent()
        {
            InitDB();
            Content = new LoginPage(maincon);
        }
    }
}
