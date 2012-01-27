using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace WP7SDKDemo.common
{
    public class MainListItem
    {
        public string Caption { get; set; }
        public string ViewLocation { get; set; }

        public MainListItem(string caption, string location=null)
        {
            this.Caption = caption;
            this.ViewLocation = location;
        }
    }
}
