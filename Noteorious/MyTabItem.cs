using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Noteorious
{
    class MyTabItem
    {
        public MyTabItem()
        {
            Uri uri = new Uri("close.png", UriKind.Relative);
            ImageSource imgSource = new BitmapImage(uri);
            Close = new Image { Source = imgSource, Height = 20, Width = 20 };

            Header = "Test";
            Content = new RichTextBox();
            
        }
        public string Header { get; set; }
        public RichTextBox Content { get; set; }
        public Image Close { get; set; }
    }
}
