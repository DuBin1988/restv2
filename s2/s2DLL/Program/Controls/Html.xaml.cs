using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Com.Aote.Controls
{
    public partial class Html : UserControl
    {
        public Html()
        {
            InitializeComponent();
        }

        // Source属性，设置Source后，改变内嵌网页为给定的html页面
        private String source;
        public String Source
        {
            set
            {
                source = value;
                web.Source = new Uri(source);
            }
        }
    }
}
