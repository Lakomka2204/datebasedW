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
    /// Логика взаимодействия для PostPage.xaml
    /// </summary>
    public partial class PostPage : Page
    {
        private MySqlConnection maincon;
        private User user, author;
        private Post post;
        HomePage hp_parent;
        SearchPage spp_parent;
        private bool staticidcopy;

        public PostPage()
        {
            InitializeComponent();
            Loaded += PLoaded;
        }
        public PostPage(MySqlConnection maincon, User user, Post post) : this()
        {
            this.maincon = maincon;
            this.user = user;
            this.post = post;
        }
        public PostPage(MySqlConnection maincon, User user, Post post, HomePage parent) : this(maincon, user, post)
        {
            hp_parent = parent;
        }
        public PostPage(MySqlConnection maincon, User user, Post post, SearchPage parent) : this(maincon, user, post)
        {
            spp_parent = parent;
        }

        void bt_back_Click(object sender, RoutedEventArgs e)
        {
            if (hp_parent != null)
                hp_parent.SetContent(hp_parent);
            else spp_parent.SetContent(spp_parent);
        }

        async void bt_delete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox
                .Show("Are you sure?", $"Delete {post.ID}", MessageBoxButton.YesNo, MessageBoxImage.Question)
                != MessageBoxResult.Yes) return;
            int r;
            using (var cmd = maincon.CreateCommand($"update posts " +
                $"set poststate = '{Post.PostState.Deleted.ToString().ToLower()}' " +
                $"where id = {post.ID};"))
                r = await cmd.ExecuteNonQueryAsync();
            MessageBox.Show(r > 0 ? "Post deleted" : "Failed to delete post");
            if (r > 0) bt_back_Click(null, null);
        }

        async void PLoaded(object sender, RoutedEventArgs e)
        {
            tb_title.Text = post.Title;
            tb_content.Text = post.Content;
            using (var cmd = maincon.CreateCommand($"select * from users where id = {post.AuthorID};"))
            using (var res = await cmd.ExecuteReaderAsync())
                if (await res.ReadAsync()) author = User.Parse(res);
            sp_parent.Background = new SolidColorBrush(
                Color.FromRgb(post.PostColor.R, post.PostColor.G, post.PostColor.B));
            tb_info.Text = $"Posted by {(author is null ? post.AuthorID.ToString() : author.Username)} in {post.CreatedAt:dd.MM.yy-HH:mm:ss} ID: {post.ID}";
            bt_delete.Visibility = author != null && author.ID == user.ID || user.Role > User.AccountRole.Vip ? Visibility.Visible : Visibility.Collapsed;
            bt_archive.Visibility = user.Role > User.AccountRole.Vip ? Visibility.Visible : Visibility.Collapsed;
            gd_comment.Visibility = post.State == Post.PostState.Active ? Visibility.Visible : Visibility.Collapsed;
            bt_archive.Content = post.State == Post.PostState.Archived ? "Archived" : "Archive";
            bt_archive.IsEnabled = post.State == Post.PostState.Active;
            await UpdateComments();
        }
        async Task<Border> CreateComment(Comment c)
        {
            User author = null;
            using (var cmd = maincon.CreateCommand($"select * from users where id = {c.AuthorID};"))
            using (var res = await cmd.ExecuteReaderAsync())
                if (await res.ReadAsync()) author = User.Parse(res);
            Grid g = new Grid();
            TextBlock tb = new TextBlock()
            {
                Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204)),
                FontSize = 16,
                Text = author != null ? author.Username : c.AuthorID.ToString(),
            };
            Viewbox vb = new Viewbox()
            {
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                StretchDirection = StretchDirection.DownOnly,
                Child = tb,
            };
            g.Children.Add(vb);
            tb = new TextBlock()
            {
                Foreground = new SolidColorBrush(Color.FromRgb(238, 238, 238)),
                FontSize = 14,
                Text = c.Message,
            };
            vb = new Viewbox()
            {
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                StretchDirection = StretchDirection.DownOnly,
                Child = tb,
            };
            g.Children.Add(vb);
            tb = new TextBlock()
            {
                Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204)),
                FontSize = 14,
                Text = $"Commented in {c.CreatedAt:dd.MM.yy-HH:mm:ss} ID: {c.ID}",
            };
            tb.MouseUp += async (s, a) =>
            {
                if (staticidcopy) return;
                staticidcopy = true;
                TextBlock t = s as TextBlock;
                string prev = t.Text;
                Clipboard.SetText(c.ID.ToString());
                t.Text = "Copied!";
                await Task.Delay(1000);
                t.Text = prev;
                staticidcopy = false;
            };
            vb = new Viewbox()
            {
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                StretchDirection = StretchDirection.DownOnly,
                MaxWidth = 250,
                Child = tb,
            };
            g.Children.Add(vb);
            if (user.Role > User.AccountRole.Vip || user.ID == author.ID)
            {
                Button bb = new Button()
                {
                    FontSize = 12,
                    MinHeight = 20,
                    MinWidth = 70,
                    BorderBrush = new SolidColorBrush(Color.FromRgb(85, 17, 17)),
                    Background = new SolidColorBrush(Color.FromRgb(204, 34, 34)),
                    Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204)),
                    Content = user.Role > User.AccountRole.Vip ? "Remove" : user.ID == author.ID ? "Delete" : "No action",
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(5),
                };
                bb.Click += async (s, a) =>
                {
                    if (MessageBox.Show("Are you sure?",
                        $"Delete {c.ID}",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
                    int r;
                    using (var cmd = maincon.CreateCommand(
                        $"update comments " +
                        $"set commentstate = " +
                        $"'{(user.Role > User.AccountRole.Vip ? Comment.CommentState.Removed : user.ID == author.ID ? Comment.CommentState.Deleted : Comment.CommentState.Active)}' " +
                        $"where id = {c.ID};"))
                        r = await cmd.ExecuteNonQueryAsync();
                    MessageBox.Show(r > 0 ? "Deleted" : "Failed to delete");
                    if (r > 0) await UpdateComments();
                };
                Style s = new Style
                {
                    TargetType = typeof(Border),
                };
                s.Setters.Add(new Setter(Border.CornerRadiusProperty, new CornerRadius(5)));
                s.Seal();
                bb.Resources.Add(typeof(Border), s);
                g.Children.Add(bb);
            }
            Border b = new Border()
            {
                CornerRadius = new CornerRadius(10),
                BorderBrush = new SolidColorBrush(Color.FromRgb(17, 17, 17)),
                BorderThickness = new Thickness(3),
                Background = new SolidColorBrush(Color.FromArgb(75, 85, 85, 85)),
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinHeight = 60,
                Child = g,
            };
            return b;
        }

        async void sds_Click(object sender, RoutedEventArgs e)
        {
            if (tb_comment.Text.IsNullOrEmptyOrWhiteSpace()) return;
            string comment = tb_comment.Text;
            int r;
            using (var cmd = maincon.CreateCommand($"insert into comments(id,message,authorid,postid) " +
                $"value({Utils.GetID(comment)},'{comment}',{user.ID},{post.ID});"))
                r = await cmd.ExecuteNonQueryAsync();
            MessageBox.Show(r > 0 ? "Success" : "Failed to submit comment");
            tb_comment.Clear();
            if (r > 0) await UpdateComments();
        }

        async void bt_archive_Click(object sender, RoutedEventArgs e)
        {
            int r;
            using (var cmd = maincon.CreateCommand($"update posts " +
                $"set poststate = '{Post.PostState.Archived.ToString().ToLower()}' where id = {post.ID};"))
                r = await cmd.ExecuteNonQueryAsync();
            MessageBox.Show(r > 0 ? "Archived" : "Failed to archive");
            if (r > 0)
            {
                gd_comment.Visibility = Visibility.Collapsed;
                bt_archive.Content = "Archived";
                bt_archive.IsEnabled = false;
            }
        }

        async Task UpdateComments()
        {
            foreach (Border b in sp_parent.Children.OfType<Border>().ToArray())
                sp_parent.Children.Remove(b);
            foreach (TextBlock b in sp_parent.Children.OfType<TextBlock>().ToArray())
                sp_parent.Children.Remove(b);
            List<Comment> cs = new List<Comment>();
            using (var cmd = maincon.CreateCommand($"select * from comments where postid = {post.ID} and commentstate = '{Comment.CommentState.Active.ToString().ToLower()}' order by createdat desc;"))
            using (var res = await cmd.ExecuteReaderAsync())
            {
                while (await res.ReadAsync())
                {
                    Comment c = Comment.Parse(res);
                    cs.Add(c);
                }
                if (!res.HasRows)
                {
                    TextBlock tb = new TextBlock()
                    {
                        Text = "No comments",
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        TextAlignment = TextAlignment.Center,
                        Foreground = new SolidColorBrush(Color.FromRgb(170, 170, 170)),
                    };
                    sp_parent.Children.Add(tb);
                    return;
                }
            }
            foreach (Comment c in cs)
            {
                Border b = await CreateComment(c);
                sp_parent.Children.Add(b);
            }
        }
    }
}
