using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace datebasedW.ооп
{
    /// <summary>
    /// Логика взаимодействия для DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        public MainWindow mw;
        private MySqlConnection maincon;
        User user;
        public DashboardPage()
        {
            InitializeComponent();
        }
        Brush highlight = new SolidColorBrush(Color.FromRgb(102, 102, 102)),
            defaultt = new SolidColorBrush(Color.FromRgb(85, 85, 85));
        public void SetContent(Page content)
        {
            content_frame.Content = content;
            bt_home.Background = bt_search.Background = bt_settings.Background = defaultt;
            switch(content.Title)
            {
                case "HomePage":
                    bt_home.Background = highlight;
                    break;
                case "SearchPage":
                    bt_search.Background = highlight;
                    break;
                case "SettingsPage":
                    bt_settings.Background = highlight;
                    break;
            }
        }
        bool tbidmouseupprocess;
        public void ChangeID(int id)
        {
            user.ID = id;
            tb_id.Text = $"ID: {user.ID}";
        }
        public DashboardPage(MySqlConnection maincon, User user, MainWindow mw) : this()
        {
            this.mw = mw;
            this.maincon = maincon;
            this.user = user;
            tb_id.Text = $"ID: {this.user.ID}";
            tb_id.MouseUp += async (s, a) =>
            {
                if (tbidmouseupprocess) return;
                tbidmouseupprocess = true;
                Clipboard.SetText(user.ID.ToString());
                tb_id.Text = "Copied!";
                await Task.Delay(1000);
                tb_id.Text = $"ID: {this.user.ID}";
                tbidmouseupprocess = false;
            };
            HomePage aa = new HomePage(this.maincon,user,this);
            content_frame.Content = aa;
        }

        void bt_logout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure?", "Logout", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.No) return;
            user = null;
            maincon.Close();
            mw.RestoreContent();
        }

        void bt_search_Click(object sender, RoutedEventArgs e)
        {
            SetContent(new SearchPage(maincon, user, this));
        }

        private void bt_settings_Click(object sender, RoutedEventArgs e)
        {
            SetContent(new SettingsPage(maincon, user, this));
        }

        void bt_home_Click(object sender, RoutedEventArgs e)
        {
            SetContent(new HomePage(maincon, user, this));
        }
    }
}
