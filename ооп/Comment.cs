using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;

namespace datebasedW.ооп
{
    public class Comment
    {
        public int ID { get; set; }
        public string Message { get; set; }
        public int AuthorID { get; set; }
        public int PostID { get; set; }
        public CommentState State { get; set; }
        public DateTime CreatedAt { get; set; }
        public enum CommentState { Active, Removed, Deleted }
        public static Comment Parse(MySqlDataReader reader)
        {
            try
            {
                return new Comment
                {
                    ID = reader.GetFieldValue<int>(0),
                    Message = reader.GetFieldValue<string>(1),
                    AuthorID = reader.GetFieldValue<int>(2),
                    PostID = reader.GetFieldValue<int>(3),
                    State = Enum.Parse<CommentState>(reader.GetFieldValue<string>(4), true),
                    CreatedAt = reader.GetFieldValue<DateTime>(5),
                };
            }
            catch { return null; }

        }
        public override string ToString()
        {
            return $"Comment({GetHashCode()})\n" +
                $"ID: {ID}\n" +
                $"Message: {Message}\n" +
                $"Author ID: {AuthorID}\n" +
                $"Post ID: {PostID}\n" +
                $"Comment State: {State}\n" +
                $"Created At: {CreatedAt}";
        }
    }
}
