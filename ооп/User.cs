using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MySqlConnector;

namespace datebasedW.ооп
{
    public class User
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime CreatedAt { get; set; }
        public AccountState State { get; set; }
        public AccountRole Role { get; set; }
        public string DisableReason { get; set; }
        public enum AccountState { Active, Banned, Deleted }
        public enum AccountRole { Guest, User, Vip, Admin, Root }
        public static User Parse(MySqlDataReader reader)
        {
            try
            {
                    var ID = reader.GetFieldValue<int>(0);
                    var Username = reader.GetFieldValue<string>(1);
                    var Password = reader.GetFieldValue<string>(2);
                    var CreatedAt = reader.GetFieldValue<DateTime>(3);
                    var State = Enum.Parse<AccountState>(reader.GetFieldValue<string>(4),true);
                    var Role = Enum.Parse<AccountRole>(reader.GetFieldValue<string>(5),true);
                    var DisableReason = reader.GetFieldValue<string>(6);
                    return new User(){
                      ID=ID,Username=Username,
                      Password=Password,
                      CreatedAt=CreatedAt,
                      State=State,
                      Role=Role,
                      DisableReason=DisableReason
                    };
            } catch (Exception e)
             {
              MessageBox.Show(e.Message);
               return null; 
             }
        }
        public override string ToString()
        {
            string hpass = Password.Remove(Password.Length / 2, Password.Length - (Password.Length / 2));
            return $"User({GetHashCode()})\n" +
                $"ID: {ID}\n" +
                $"Username: {Username}\n" +
                $"Password: {hpass + new string('*', Password.Length - hpass.Length)}\n" +
                $"Created At: {CreatedAt}\n" +
                $"Account State: {State}\n" +
                $"Account Role: {Role}" +
                $"{(string.IsNullOrEmpty(DisableReason) ? "" : $"\nDisable Reason: {DisableReason}")}";
        }
    }
}
