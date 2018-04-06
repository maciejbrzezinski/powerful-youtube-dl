using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace powerful_youtube_dl
{
    public class ListViewItemMy
    {
        public bool check { get; set; }
        public string title { get; set; }
        public string duration { get; set; }
        public string status { get; set; }
        public Video parent { get; set; }
    }
}
