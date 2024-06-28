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
    /// Логика взаимодействия для ChangeName.xaml
    /// </summary>
    public partial class ChangeName : Window
    {
        private User user;

        public ChangeName()
        {
            InitializeComponent();
        }
        string cs_str = "parametr";
        public ChangeName(User user) : this()
        {
            cs_str = "username";
            this.user = user;
            Loaded += HLoaded;
            Closing += HClosing;
        }
        public ChangeName (User user, string cs_str) : this(user)
        {
            this.cs_str = cs_str;
        }

        private void HClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult is null)
                DialogResult = false;
        }
        public string OBJECT;
        private void HLoaded(object sender, RoutedEventArgs e)
        {
            Title = $"Change {cs_str}";
            lb_cu.Text = $"Change {user.Username}'s {cs_str} to";
        }

        private void bt_search_Click(object sender, RoutedEventArgs e)
        {
            if (tb_newname.Text.IsNullOrEmptyOrWhiteSpace()) return;
            else
            {
                OBJECT = tb_newname.Text;
                DialogResult = true;
            }
            Close();
        }
    }
}
