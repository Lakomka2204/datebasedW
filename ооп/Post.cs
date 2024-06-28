using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MySqlConnector;

namespace datebasedW.ооп
{
    public class Post
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int AuthorID { get; set; }
        public DateTime CreatedAt { get; set; }
        public Color PostColor { get; set; }
        public PostState State { get; set; }
        public enum PostState { Active,Archived,Removed,Deleted}
        public static Post Parse(MySqlDataReader reader)
        {
            try
            {
                return new Post
                {
                    ID = reader.GetFieldValue<int>(0),
                    Title = reader.GetFieldValue<string>(1),
                    Content = reader.GetFieldValue<string>(2),
                    AuthorID = reader.GetFieldValue<int>(3),
                    PostColor = Color.FromArgb(reader.GetFieldValue<int>(4)),
                    State = Enum.Parse<PostState>(reader.GetFieldValue<string>(5),true),
                    CreatedAt = reader.GetFieldValue<DateTime>(6),
                };
            } catch (Exception e) { MessageBox.Show(e.Message); return null; }
        }
        public override string ToString()
        {
            return $"Post({GetHashCode()})\n" +
                $"ID: {ID}\n" +
                $"Title: {Title}\n" +
                $"Content: {Content}\n" +
                $"Author ID: {AuthorID}\n" +
                $"Post Color: {PostColor}\n" +
                $"Post State: {State}\n" +
                $"Created At: {CreatedAt}";
        }
    }
}
