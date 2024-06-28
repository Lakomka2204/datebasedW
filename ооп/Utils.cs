using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;

namespace datebasedW.ооп
{
    public static class Utils
    {
        public static bool IsNullOrEmptyOrWhiteSpace(this string s)
            => string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s);
        public static MySqlCommand CreateCommand(this MySqlConnection con, string command)
        {
            //if (con.State < System.Data.ConnectionState.Open) con.Open();
            var c = con.CreateCommand();
            c.CommandText = command;
            return c;
        }
        public static void ApplyRoundButtonStyle(this Button b, double uniformradius)
        {
            Style s = new Style
            {
                TargetType = typeof(Border),
            };
            s.Setters.Add(new Setter(Border.CornerRadiusProperty, new CornerRadius(uniformradius)));
            s.Seal();
            b.Resources.Add(typeof(Border), s);
        }
        public static ulong GetID(string text)
        {
            ulong time = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds();
            int a = 0;
            foreach (int x in text.ToCharArray().Select(x => (int)x))
                a += x;
            a *= text.Length;
            time += (ulong)a;
            return time;
        }
    }
}
