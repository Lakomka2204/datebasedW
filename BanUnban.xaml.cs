using datebasedW.ооп;
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
using System.Windows.Shapes;

namespace datebasedW
{
    /// <summary>
    /// Логика взаимодействия для BanUnban.xaml
    /// </summary>
    public partial class BanUnban : Window
    {
        private User user;
        bool delete;
        public BanUnban()
        {
            InitializeComponent();
        }
        public BanUnban(User user, bool delete = false) : this()
        {
            this.delete = delete;
            this.user = user;
            Loaded += HLoaded;
            Closing += HClosing;
        }

        private void HClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult is null)
                DialogResult = false;
        }
        bool active;
        public User.AccountState OBJ1;
        public string OBJ2;
        private void HLoaded(object sender, RoutedEventArgs e)
        {
            active = user.State == User.AccountState.Active;
            string bu = delete ? "Delete" : active ? "Ban" : "Unban";
            lb_cu.Text = $"{bu} {(delete ? "account" : user.Username)}";
            Title = $"{bu} user";
            bt_search.Content = bu;
            bt_search.BorderBrush = active
                ? new SolidColorBrush(Color.FromRgb(85, 17, 17))
                : new SolidColorBrush(Color.FromRgb(17, 85, 17));
            bt_search.Background = active
                ? new SolidColorBrush(Color.FromRgb(204, 34, 34))
                : new SolidColorBrush(Color.FromRgb(34, 204, 34));
            lb_reason.Visibility = tb_newname.Visibility = active ? Visibility.Visible : Visibility.Collapsed;
        }

        private void bt_search_Click(object sender, RoutedEventArgs e)
        {
            if (active && tb_newname.Text.IsNullOrEmptyOrWhiteSpace())
            {
                Close();
                return;
            }
            OBJ1 = delete ? User.AccountState.Deleted : active ? User.AccountState.Banned : User.AccountState.Active;
            OBJ2 = tb_newname.Text;
            DialogResult = true;
            Close();
        }
    }
}
