using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace Noteorious
{
    class MyTabItem
    {
        public MyTabItem() 
        {
            Header = "Test";
            Content = new RichTextBox();
        }
        public string Header { get; set; }
        public RichTextBox Content { get; set; }
    }
}
