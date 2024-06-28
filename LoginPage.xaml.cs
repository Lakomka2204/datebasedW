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
using System.Windows.Media.Animation;
using datebasedW.ооп;

namespace datebasedW
{
    /// <summary>
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        MySqlConnection maincon;
        bool islogin = true;

        public LoginPage()
        {
            InitializeComponent();
        }

        public LoginPage(

            MySqlConnection maincon) : this()
        {
            this.maincon = maincon;
        }
        void bt_toggle_Click(object sender, RoutedEventArgs e)
        {
            islogin = !islogin;
            bt_login.Content = lb_login.Text = islogin ? "Login" : "Register";
            bt_toggle.Content = islogin ? "Register" : "Login";
        }
        bool inprocess = false;
        async void SetError(string text)
        {
            if (inprocess) return;
            inprocess = true;
            lb_err.Text = text;
            lb_err.Visibility = Visibility.Visible;
            await Task.Delay(3000);
            lb_err.Text = "";
            lb_err.Visibility = Visibility.Collapsed;
            inprocess = false;
        }
        async void bt_login_Click(object sender, RoutedEventArgs e)
        {
            string u = tb_username.Text.Trim(),
                    p = tb_password.Password.Trim();
            if (string.IsNullOrWhiteSpace(u) || string.IsNullOrWhiteSpace(p))
            {
                SetError("Please enter username and password");
                return;
            }
            if (maincon.State < System.Data.ConnectionState.Open)
            {
                try
                {
                    bt_login.IsEnabled = false;
                    await maincon.OpenAsync();

                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("offline"))
                        SetError("Server is offline");
                    bt_login.IsEnabled = true;
                    return;
                }
                finally
                {
                    if (maincon.State < System.Data.ConnectionState.Open)
                        SetError("Server is offline");
                    bt_login.IsEnabled = true;
                }
            }
            if (islogin == true)
            {
                using (var cmd = maincon.CreateCommand($"select * from users where '{u}' like username;"))
                {

                    using var res = await cmd.ExecuteReaderAsync();
                    if (!await res.ReadAsync())
                    {
                        SetError("Account does not exists");
                        return;
                    }
                }
                using (var cmd = maincon.CreateCommand($"select * from users where '{u}' = username and '{p}' = password;"))
                {
                    using (var res = await cmd.ExecuteReaderAsync())
                    {
                        if (!await res.ReadAsync())
                        {
                            SetError("Wrong password");
                            return;
                        }
                        
                        User user = User.Parse(res);
                        if (user.State != User.AccountState.Active)
                        {
                            SetError($"This account was {user.State.ToString().ToLowerInvariant()}. " +
                                $"Reason: {user.DisableReason}");
                            return;
                        }
                        //MessageBox.Show(user.ToString());
                        await maincon.CloseAsync();
                        MainWindow.SetContent(new DashboardPage(maincon, user, (MainWindow)Application.Current.MainWindow));
                    }
                }

            }
            else
            {
                using (var cmd = maincon.CreateCommand($"select count(*) from users where username = '{u}';"))
                using (var res = await cmd.ExecuteReaderAsync())
                    if (await res.ReadAsync() && (await res.GetFieldValueAsync<int>(0) > 10))
                    {
                        SetError("Username is overused, please choose another one");
                        return;
                    }
                using (var cmd = maincon.CreateCommand($"insert into users(id,username,password) value({Utils.GetID(u)},'{u}','{p}');"))
                    if ((await cmd.ExecuteNonQueryAsync()) > 0)
                    {
                        islogin = !islogin;
                        bt_login_Click(null, null);
                    }
            }
        }
    }
}
