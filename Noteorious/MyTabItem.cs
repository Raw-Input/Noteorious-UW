using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Noteorious
{
    public class MyTabItem 
    {

        public event EventHandler ContextMenuUpdate;

        // Default constructor takes in no variables and creates a new tab
        public MyTabItem()
        {
            // Create Context menu for the RichTextBox
            ContextMenu cm = new ContextMenu();  // make a context menu
            MenuItem item = new MenuItem(); // make a menuitem instance
            item.Header = "Make new Note"; // give the item a header
            cm.Items.Add(item);
            item.Click += (sendingelement, eventargs) => cUpdate(item, eventargs); // give the item a click event handler
            cm.IsEnabled = true;

            //Create header of tab
            Header = "New Tab";

            // Create a RichTextBox for editing text
            Content = new RichTextBox();
            Content.ContextMenu = cm;
            


        }

        // This constructor takes in a string for the header and creates a new tab with the identified header
        public MyTabItem(String s)
        {
            // Create Context menu for the RichTextBox
            ContextMenu cm = new ContextMenu(); // make a context menu
            MenuItem item = new MenuItem(); // make a menuitem instance
            item.Header = "Make new Note"; // give the item a header
            cm.Items.Add(item);
            item.Click += (sendingelement, eventargs) => cUpdate(item, eventargs); // give the item a click event handler
            cm.IsEnabled = true;

            //Create header of tab
            Header = s;

            // Create a RichTextBox for editing text
            Content = new RichTextBox();
            Content.ContextMenu = cm;
        }

        public string Header { get; set; }
        public RichTextBox Content { get; set; }

        // Handles context menu events
        public void cUpdate(MenuItem item, EventArgs e)
        {
            
            if (ContextMenuUpdate != null)
            {
                ContextMenuUpdate(this, null);
            }
                
        }
    }

}
