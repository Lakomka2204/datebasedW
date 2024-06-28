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
using datebasedW.ооп;
using MySqlConnector;

namespace datebasedW
{
    /// <summary>
    /// Логика взаимодействия для SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        private MySqlConnection maincon;
        private DashboardPage parent;
        User user;
        public SettingsPage(MySqlConnection maincon, User user, DashboardPage parent)
        {
            this.parent = parent;
            this.user = user;
            this.maincon = maincon;
            InitializeComponent();
            Loaded += HLoaded;
        }

        async void HLoaded(object sender, RoutedEventArgs e)
        {
            await UpdateInfo();
        }

        private void bt_showpass_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(user.Password, "Password", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        async void bt_changeusername_Click(object sender, RoutedEventArgs e)
        {
            ChangeName cn = new ChangeName(user);
            if ((bool)cn.ShowDialog())
            {
                using (var cmd = maincon.CreateCommand($"update users set username = '{cn.OBJECT}' where id = {user.ID};"))
                    if ((await cmd.ExecuteNonQueryAsync()) > 0) await UpdateInfo();
            }
        }

        async Task UpdateInfo()
        {
            using (var cmd = maincon.CreateCommand($"select * from users where id = {user.ID};"))
            using (var res = await cmd.ExecuteReaderAsync())
                if (await res.ReadAsync()) user = User.Parse(res);
            if (user is null)
            {
                MessageBox.Show("An error occured while fetching account data");
                await maincon.CloseAsync();
                parent.mw.RestoreContent();
            }
            tb_id.Text = $"ID: {user.ID}";
            tb_username.Text = $"Username: {user.Username}";
            tb_createdat.Text = $"Creation date: {user.CreatedAt}";
            tb_state.Text = $"Account state: {user.State}";
            tb_role.Text = $"Account role: {user.Role}";
            List<Post> posts = new List<Post>();
            List<Comment> comms = new List<Comment>();
            using (var cmd = maincon.CreateCommand($"select * from posts where authorid = {user.ID};"))
            using (var res = await cmd.ExecuteReaderAsync())
                while (await res.ReadAsync()) posts.Add(Post.Parse(res));
            using (var cmd = maincon.CreateCommand($"select * from comments where authorid = {user.ID};"))
            using (var res = await cmd.ExecuteReaderAsync())
                while (await res.ReadAsync()) comms.Add(Comment.Parse(res));
            tb_posts.Text = $"Made posts: {posts.Count(x => x.State == Post.PostState.Active)} (Total: {posts.Count})";
            tb_comments.Text = $"Made comments: {comms.Count(x => x.State == Comment.CommentState.Active)} (Total: {comms.Count})";
            bt_setid.Visibility
            = bt_truedelete.Visibility
            = bt_setdate.Visibility
            = user.Role > User.AccountRole.Vip
            ? Visibility.Visible
            : Visibility.Hidden;
            bt_setrole.Visibility = user.Role > User.AccountRole.Admin
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        async void bt_changepass_Click(object sender, RoutedEventArgs e)
        {
            ChangePass cn = new ChangePass(user);
            if ((bool)cn.ShowDialog())
            {
                using (var cmd = maincon.CreateCommand($"update users set password = '{cn.OBJECT}' where id = {user.ID};"))
                    if ((await cmd.ExecuteNonQueryAsync()) > 0) await UpdateInfo();
            }
        }

        async void bt_delaccount_Click(object sender, RoutedEventArgs e)
        {
            BanUnban bu = new BanUnban(user, true);
            if ((bool)bu.ShowDialog())
            {
              MessageBox.Show(bu.OBJ2);
                using (var cmd = maincon.CreateCommand($"update users set accountstate = '{bu.OBJ1.ToString().ToLower()}'" +
                    $"{(bu.OBJ1 == User.AccountState.Banned ? $", disablereason = '{bu.OBJ2}'" : "")} where id = {user.ID};"))
                    if ((await cmd.ExecuteNonQueryAsync()) > 0) await UpdateInfo();
            }
        }

        async void bt_delposts_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure?", "Delete all posts", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.No) return;
            using (var cmd = maincon.CreateCommand($"update posts set poststate = '{Post.PostState.Deleted.ToString().ToLower()}' where authorid = {user.ID};"))
                if ((await cmd.ExecuteNonQueryAsync()) > 0) await UpdateInfo();
        }

        async void bt_delcomms_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure?", "Delete all comments", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.No) return;
            using (var cmd = maincon.CreateCommand($"update comments set commentstate = '{Comment.CommentState.Deleted.ToString().ToLower()}' where authorid = {user.ID};"))
                if ((await cmd.ExecuteNonQueryAsync()) > 0) await UpdateInfo();
        }

        async void bt_setrole_Click(object sender, RoutedEventArgs e)
        {
            ChangeRole cr = new ChangeRole(user);
            if ((bool)cr.ShowDialog())
            {
                using (var cmd = maincon.CreateCommand($"update users set accountrole = '{cr.OBJECT.ToString().ToLower()}' where id = {user.ID};"))
                    if ((await cmd.ExecuteNonQueryAsync()) > 0) await UpdateInfo();
            }
        }

        async void bt_setdate_Click(object sender, RoutedEventArgs e)
        {
            ChangeDate cd = new ChangeDate(user);
            if ((bool)cd.ShowDialog())
            {
                using (var cmd = maincon.CreateCommand($"update users set createdat = '{cd.OBJECT:yyyy-MM-dd HH:mm:ss}' where id = {user.ID};"))
                    if ((await cmd.ExecuteNonQueryAsync()) > 0) await UpdateInfo();
            }
        }

        async void bt_setid_Click(object sender, RoutedEventArgs e)
        {
            ChangeName cd = new ChangeName(user, "ID");
            if ((bool)cd.ShowDialog())
            {
                if (!int.TryParse(cd.OBJECT, out int newid))
                {
                    MessageBox.Show("ID must be a number", cd.Title,
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                using (var cmd = maincon.CreateCommand($"select * from users where id = {cd.OBJECT};"))
                using (var res = await cmd.ExecuteReaderAsync())
                    if (await res.ReadAsync())
                    {
                        MessageBox.Show("ID is already occupied", cd.Title,
                            MessageBoxButton.OK,MessageBoxImage.Warning);
                        return;
                    }
                using (var cmd = maincon.CreateCommand($"update posts set authorid = {newid} where authorid = {user.ID};"))
                    await cmd.ExecuteNonQueryAsync();
                using(var cmd = maincon.CreateCommand($"update comments set authorid = {newid} where authorid = {user.ID};"))
                    await cmd.ExecuteNonQueryAsync();
                using (var cmd = maincon.CreateCommand($"update users set id = {newid} where id = {user.ID};"))
                    if ((await cmd.ExecuteNonQueryAsync()) > 0)
                    {
                        user.ID = newid;
                        parent.ChangeID(newid);
                        await UpdateInfo();
                    }
            }
        }

        async void bt_truedelete_Click(object sender, RoutedEventArgs e)
        {
            var x = MessageBox.Show("Are you sure?", "Truly delete account", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (x == MessageBoxResult.No) return;
            using (var cmd = maincon.CreateCommand($"delete from users where id = {user.ID};"))
                if ((await cmd.ExecuteNonQueryAsync()) > 0) parent.mw.RestoreContent();

        }
    }
}