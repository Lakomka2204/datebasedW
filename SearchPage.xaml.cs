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
    /// Логика взаимодействия для SearchPage.xaml
    /// </summary>
    public partial class SearchPage : Page
    {
        MySqlConnection maincon;
        DashboardPage parent;
        User user;
        bool se_post, staticpostcopy;
        public SearchPage(MySqlConnection maincon, User user, DashboardPage parent)
        {
            this.user = user;
            this.maincon = maincon;
            this.parent = parent;
            InitializeComponent();
        }
        public void SetContent(Page content)
        {
            parent.SetContent(content);
        }
        async void bt_submit_Click(object sender, RoutedEventArgs e)
        {
            if (tb_searchbar.Text.IsNullOrEmptyOrWhiteSpace()) return;
            using (var cmd = maincon.CreateCommand($"select * from users where id = {user.ID};"))
            using (var res = await cmd.ExecuteReaderAsync())
                if (await res.ReadAsync()) user = User.Parse(res);
            List<Post> posts = new List<Post>();
            parent_panel.Children.Clear();
            bool is_id = int.TryParse(tb_searchbar.Text, out int id);
            string text = tb_searchbar.Text;
            using (var cmd = maincon.CreateCommand(
                $"select * from {(se_post ? "posts" : "users")} where " +
                $"{(is_id ? $"id = {id};" : $"{(se_post ? $"lower(title) like lower('%{text}%') {(user.Role < User.AccountRole.Admin ? "and (poststate = 'active' or poststate = 'archived')" : "")};" : $"lower(username) like lower('%{text}%') {(user.Role < User.AccountRole.Admin ? "and accountstate = 'active'" : "")};")}")}"))
            {
                using (var res = await cmd.ExecuteReaderAsync())
                {
                    while (await res.ReadAsync())
                    {
                        if (se_post)
                        {
                            Post p = Post.Parse(res);
                            posts.Add(p);
                        }
                        else
                        {
                            User u = User.Parse(res);
                            parent_panel.Children.Add(CreateUser(u));
                        }
                    }
                    if (!res.HasRows)
                    {
                        TextBlock tb = new TextBlock()
                        {
                            Text = "No results",
                            VerticalAlignment = VerticalAlignment.Top,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            TextAlignment = TextAlignment.Center,
                            Foreground = new SolidColorBrush(Color.FromRgb(170, 170, 170)),
                        };
                        parent_panel.Children.Add(tb);
                        return;
                    }
                }
                if (se_post)
                {
                    foreach (Post p in posts)
                        parent_panel.Children.Add(await CreatePost(p));
                }
            }
        }
        async Task<Border> CreatePost(Post post)
        {
            Grid g = new Grid();
            Viewbox vb = new Viewbox
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = 30,
                Margin = new Thickness(10, 5, 10, 5),
            };
            TextBlock tb = new TextBlock
            {
                Text = post.Title,
                Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204)),
            };
            vb.Child = tb;
            g.Children.Add(vb);
            vb = new Viewbox
            {
                Height = 70,
                Margin = new Thickness(5, 0, 5, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            };
            tb = new TextBlock
            {
                Text = post.Content,
                Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204)),
            };
            vb.Child = tb;
            g.Children.Add(vb);
            vb = new Viewbox
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(5, 0, 100, 5),
                MaxWidth = 350,
                MaxHeight = 30,
            };
            User author = null;
            using (var cmd = maincon.CreateCommand($"select * from users where id = {post.AuthorID}"))
            using (var res = await cmd.ExecuteReaderAsync())
                if (await res.ReadAsync()) author = User.Parse(res);

            tb = new TextBlock
            {
                Text = $"Posted by {(author == null ? post.AuthorID.ToString() : author.Username)} in {post.CreatedAt:dd.MM.yy-HH:mm:ss} ID: {post.ID}",
                Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153)),
            };
            tb.MouseUp += async (ss, aa) =>
            {
                if (staticpostcopy) return;
                staticpostcopy = true;
                Clipboard.SetText(post.ID.ToString());
                TextBlock ttt = (TextBlock)ss;
                string prev = ttt.Text;
                ttt.Text = "Copied!";
                await Task.Delay(1000);
                ttt.Text = prev;
                staticpostcopy = false;
            };
            vb.Child = tb;
            g.Children.Add(vb);
            Button bt = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                MinHeight = 20,
                MinWidth = 70,
                Margin = new Thickness(5),
                BorderBrush = new SolidColorBrush(Color.FromRgb(34, 34, 34)),
                Background = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204)),
                Content = "Details",
                FontSize = 12,
            };
            bt.Click += (ss, aa) =>
            {
                SetContent(new PostPage(maincon, user, post, this));
            };
            bt.ApplyRoundButtonStyle(3);
            g.Children.Add(bt);
            Border b = new Border()
            {
                CornerRadius = new CornerRadius(10),
                BorderBrush = new SolidColorBrush(Color.FromRgb(17, 17, 17)),
                BorderThickness = new Thickness(3),
                Background = new SolidColorBrush(Color.FromRgb(post.PostColor.R, post.PostColor.G, post.PostColor.B)),
                Margin = new Thickness(10, 5, 10, 5),
                Height = 150,
                Child = g,

            };
            return b;
        }
        Border CreateUser(User user)
        {
            Thickness tenfive = new Thickness(10, 5, 10, 5);
            Brush ccc = new SolidColorBrush(Color.FromRgb(204, 204, 204));
            Grid g1 = new Grid(),
                g2 = new Grid() { Margin = new Thickness(0, 0, 0, 30) };
            TextBlock tb = new TextBlock()
            {
                Text = $"Username: {user.Username}",
                Foreground = ccc,
            };
            Viewbox vb = new Viewbox()
            {
                StretchDirection = StretchDirection.DownOnly,
                Child = tb,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = 30,
                MaxWidth = 390,
                Margin = tenfive
            };
            g2.Children.Add(vb);
            if (this.user.Role > User.AccountRole.Vip && this.user.ID != user.ID)
            {
                Button bb = new Button()
                {
                    Content = "Change",
                    HorizontalAlignment = HorizontalAlignment.Right,
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Top,
                    MinHeight = 20,
                    MinWidth = 50,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(34, 34, 34)),
                    Background = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                    Foreground = ccc,
                };
                bb.ApplyRoundButtonStyle(5);
                bb.Click += async (s, a) =>
                {
                    ChangeName cn = new ChangeName(user);
                    if ((bool)cn.ShowDialog())
                    {
                        using (var cmd = maincon.CreateCommand($"update users set username = '{cn.OBJECT}' where id = {user.ID};"))
                            if ((await cmd.ExecuteNonQueryAsync()) > 0) bt_submit_Click(null, null);
                    }
                };
                g2.Children.Add(bb);
            }
            tb = new TextBlock()
            {
                Text = $"Role: {user.Role}",
                Foreground = ccc,
            };
            vb = new Viewbox()
            {
                Child = tb,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                StretchDirection = StretchDirection.DownOnly,
                Height = 30,
                MaxWidth = 390,
                Margin = new Thickness(10, 5, 10, 5),
            };
            g2.Children.Add(vb);
            if (this.user.Role == User.AccountRole.Root && this.user.ID != user.ID)
            {
                Button bb = new Button()
                {
                    Content = "Change",
                    HorizontalAlignment = HorizontalAlignment.Right,
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Center,
                    MinHeight = 20,
                    MinWidth = 50,
                    Margin = new Thickness(10),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(34, 34, 34)),
                    Background = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                    Foreground = ccc,
                };
                bb.ApplyRoundButtonStyle(5);
                bb.Click += async (s, a) =>
                {
                    ChangeRole cr = new ChangeRole(user);
                    if ((bool)cr.ShowDialog())
                    {
                        using (var cmd = maincon.CreateCommand($"update users set accountrole = '{cr.OBJECT.ToString().ToLower()}' where id = {user.ID};"))
                            if ((await cmd.ExecuteNonQueryAsync()) > 0) bt_submit_Click(null, null);
                    }
                };
                g2.Children.Add(bb);
            }
            tb = new TextBlock()
            {
                Text = $"Account state: {user.State} {(user.State != User.AccountState.Active ? $"Reason: {user.DisableReason}" : "")}",
                Foreground = ccc,
            };
            vb = new Viewbox()
            {
                Child = tb,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                StretchDirection = StretchDirection.DownOnly,
                Height = 30,
                MaxWidth = 390,
                Margin = tenfive
            };
            g2.Children.Add(vb);
            if (this.user.Role > User.AccountRole.Vip && this.user.ID != user.ID)
            {
                Button bb = new Button()
                {
                    Content = user.State == User.AccountState.Active ? "Ban" : "Unban",
                    HorizontalAlignment = HorizontalAlignment.Right,
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    MinHeight = 20,
                    MinWidth = 50,
                    Margin = new Thickness(10),
                    BorderBrush = new SolidColorBrush
                    (user.State == User.AccountState.Active ? Color.FromRgb(85, 17, 17) : Color.FromRgb(17, 85, 17)),
                    Background = new SolidColorBrush
                    (user.State == User.AccountState.Active ? Color.FromRgb(204, 34, 34) : Color.FromRgb(34, 204, 34)),
                    Foreground = ccc,
                };
                bb.ApplyRoundButtonStyle(5);
                bb.Click += async (s, a) =>
                {
                    BanUnban bu = new BanUnban(user);
                    if ((bool)bu.ShowDialog())
                    {
                        using (var cmd = maincon.CreateCommand($"update users set accountstate = '{bu.OBJ1.ToString().ToLower()}'" +
                            $"{(bu.OBJ1 == User.AccountState.Banned ? $", disablereason = '{bu.OBJ2}'" : "")} where id = {user.ID};"))
                            if ((await cmd.ExecuteNonQueryAsync()) > 0) bt_submit_Click(null, null);
                    }
                };
                g2.Children.Add(bb);
            }
            g1.Children.Add(g2);
            tb = new TextBlock()
            {
                Text = $"Created at {user.CreatedAt:dd.MM.yy-HH:mm:ss}",
                Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153)),
            };
            vb = new Viewbox()
            {
                Child = tb,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(5, 0, 0, 5),
                MaxWidth = 300,
                StretchDirection = StretchDirection.DownOnly,
            };
            g1.Children.Add(vb);
            tb = new TextBlock()
            {
                Text = $"ID: {user.ID}",
                Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153)),
            };
            tb.MouseUp += async (s, a) =>
            {
                if (staticpostcopy) return;
                staticpostcopy = true;
                Clipboard.SetText(user.ID.ToString());
                TextBlock ttt = (TextBlock)s;
                string prev = ttt.Text;
                ttt.Text = "Copied!";
                await Task.Delay(1000);
                ttt.Text = prev;
                staticpostcopy = false;
            };
            vb = new Viewbox()
            {
                Child = tb,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(5),
                Height = 20,
                StretchDirection = StretchDirection.DownOnly,
            };
            g1.Children.Add(vb);
            Border b = new Border()
            {
                Child = g1,
                CornerRadius = new CornerRadius(10),
                BorderBrush = new SolidColorBrush(Color.FromRgb(17, 17, 17)),
                BorderThickness = new Thickness(3),
                Background = new SolidColorBrush(Color.FromRgb(68, 68, 68)),
                Margin = tenfive,
                Height = 150,
            };
            return b;
        }

        void bt_togglesearch_Click(object sender, RoutedEventArgs e)
        {
            se_post = !se_post;
            bt_togglesearch.Content = se_post ? "Post" : "User";
        }
    }
}
