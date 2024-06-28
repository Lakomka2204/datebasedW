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
    /// Логика взаимодействия для ChangePass.xaml
    /// </summary>
    public partial class ChangePass : Window
    {
        private User user;

        public ChangePass()
        {
            InitializeComponent();
            Closing += HClosing;
        }

        private void HClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult is null)
                DialogResult = false;
        }

        public ChangePass(User user) : this()
        {
            this.user = user;
        }
        public string OBJECT;
        private void bt_search_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult r = MessageBoxResult.No;
            if (tb_oldpass.Password != user.Password)
                r = MessageBox.Show("Incorrect old password");
            if (tb_newpass.Password.IsNullOrEmptyOrWhiteSpace() || tb_newpass_Copy.Password.IsNullOrEmptyOrWhiteSpace())
                r = MessageBox.Show("New password field is empty");
            if (tb_newpass.Password != tb_newpass_Copy.Password)
                r = MessageBox.Show("New passwords mismatching");
            if (r != MessageBoxResult.No) return;
            OBJECT = tb_newpass.Password;
            DialogResult = true;
            Close();
        }
    }
}
