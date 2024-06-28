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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace datebasedW
{
    /// <summary>
    /// Логика взаимодействия для ChangeRole.xaml
    /// </summary>
    public partial class ChangeRole : Window
    {
        public ChangeRole()
        {
            InitializeComponent();
        }
        User user;
        public ChangeRole(User user) : this()
        {
            this.user = user;
            Loaded += HLoaded;
            Closing += HClosing;
        }
        public User.AccountRole OBJECT;
        private void HClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult is null)
                DialogResult = false;
        }

        private void HLoaded(object sender, RoutedEventArgs e)
        {
            lb_cu.Text = $"Change {user.Username}'s {user.Role} role to";
            cb_newrole.ItemsSource = Enum.GetValues(typeof(User.AccountRole));
            cb_newrole.SelectedItem = user.Role;
        }

        private void bt_search_Click(object sender, RoutedEventArgs e)
        {
            OBJECT = Enum.Parse<User.AccountRole>(cb_newrole.Text);
            DialogResult = OBJECT != user.Role;
            Close();
        }
    }
}
