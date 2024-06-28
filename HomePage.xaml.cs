using datebasedW.ооп;
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

namespace datebasedW
{
    /// <summary>
    /// Логика взаимодействия для HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private User user;
        private MySqlConnection maincon;
        private bool staticpostcopy;
        DashboardPage parent;
        public HomePage()
        {
            InitializeComponent();
            Loaded += HPLoaded;
        }

        public void SetContent(Page content)
        {
            parent.SetContent(content);
        }

        public HomePage(MySqlConnection maincon, User user, DashboardPage parent) : this()
        {
            this.parent = parent;
            this.user = user;
            this.maincon = maincon;
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
        async void HPLoaded(object sender, RoutedEventArgs e)
        {
            if (user.Role == User.AccountRole.Guest)
            {
                submit_grid.Visibility = Visibility.Collapsed;
                scv.Margin = new Thickness(0);
            }
            else if (user.Role > User.AccountRole.User)
            {
                cb_choosecolor.Visibility = Visibility.Visible;
                cb_choosecolor.ItemsSource = new SysColors().SSColors();
                //foreach(var x in new SysColors().SSColors())
                //{
                //    bt_choosecolor.Items.Add(x);
                //}
            }
            cb_choosecolor.SelectedItem = System.Drawing.Color.Black;
            cb_choosecolor.SelectedIndex = 0;
            await UpdatePosts();

        }
        async Task UpdatePosts()
        {
            parentpanel.Children.Clear();
            if (maincon.State != System.Data.ConnectionState.Open)
                await maincon.OpenAsync();
            List<Post> recent = new List<Post>(30);
            using (var cmd = maincon.CreateCommand(
                $"select * from posts " +
                $"where poststate in ('{Post.PostState.Active.ToString().ToLower()}', '{Post.PostState.Archived.ToString().ToLower()}') order by createdat desc limit 30;"))
            using (var res = await cmd.ExecuteReaderAsync())
                while (await res.ReadAsync())
                    recent.Add(Post.Parse(res));
            foreach (Post post in recent)
            {
                Border b = await CreatePost(post);
                parentpanel.Children.Add(b);
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
            Style s = new Style
            {
                TargetType = typeof(Border),
            };
            s.Setters.Add(new Setter(Border.CornerRadiusProperty, new CornerRadius(3)));
            s.Seal();
            bt.Resources.Add(typeof(Border), s);
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
        async void bt_submit_Click(object sender, RoutedEventArgs e)
        {
            if (tb_title.Text.IsNullOrEmptyOrWhiteSpace()) return;
            if (maincon.State != System.Data.ConnectionState.Open)
                await maincon.OpenAsync();
            int x;
            using (var cmd = maincon.CreateCommand($"insert into posts(id,title,content,authorid,color) " +
                $"value({Utils.GetID(tb_title.Text)},'{tb_title.Text}','{tb_content.Text}',{user.ID},{((System.Drawing.Color)cb_choosecolor?.SelectedItem).ToArgb()});"))
                x = await cmd.ExecuteNonQueryAsync();
            if (x > 0)
            {
                MessageBox.Show("Success!");
                tb_content.Clear();
                tb_title.Clear();
                cb_choosecolor.SelectedItem = System.Drawing.Color.Black;
                await UpdatePosts();
            }
            else
            {
                MessageBox.Show("Post wasn't submitted");
            }
        }
    }
}
