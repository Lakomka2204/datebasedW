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
    /// Логика взаимодействия для ChangeDate.xaml
    /// </summary>
    public partial class ChangeDate : Window
    {
        User user;

        public ChangeDate()
        {
            InitializeComponent();
        }
        public ChangeDate(User user) : this()
        {
            this.user = user;
            Loaded += HLoaded;
            Closing += HClosing;
        }

        private void HClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult is null)
                DialogResult = false;
        }

        public DateTime OBJECT;
        void HLoaded(object sender, RoutedEventArgs e)
        {
            lb_cu.Text = $"Change {user.Username}'s username to";
            dt_date.SelectedDate = user.CreatedAt;
        }

        private void bt_search_Click(object sender, RoutedEventArgs e)
        {
            if (dt_date.SelectedDate == user.CreatedAt) return;
            else
            {
                OBJECT = (DateTime)dt_date.SelectedDate;
                OBJECT = OBJECT
                    .AddHours(user.CreatedAt.Hour)
                    .AddMinutes(user.CreatedAt.Minute)
                    .AddSeconds(user.CreatedAt.Second);
                DialogResult = true;
            }
            Close();
        }
    }
}
