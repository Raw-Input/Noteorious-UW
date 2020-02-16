using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Noteorious
{
    public class MyTabItem 
    {

        public event EventHandler<MenuItem> ContextMenuUpdate;

        public event EventHandler<Hyperlink> NoteLinkUpdate;

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
            Content.IsDocumentEnabled = true;
            TextRange r = new TextRange(Content.Selection.Start, Content.Selection.End);
            r.Text = "Here is a hyperlink";
            Hyperlink h = new Hyperlink(r.Start, r.End);
            // h.NavigateUri = new Uri("");
            Paragraph p = new Paragraph(h);
            Content.Document.Blocks.InsertBefore(Content.Document.Blocks.FirstBlock, p);
            h.Click += (sendingelement, eventargs) => nUpdate(h, eventargs);
            




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
                ContextMenuUpdate(this, item);
            }
                
        }

        public void nUpdate(Hyperlink h, EventArgs e)
        {

            if (NoteLinkUpdate != null)
            {
                NoteLinkUpdate(this, h);
            }

        }

        public void createHyperLink (TextSelection s)
        {
            Hyperlink h = new Hyperlink(s.Start, s.End);
            Paragraph p = new Paragraph(h);
            TextPointer curPosition = s.Start;
            Content.Document.Blocks.InsertBefore(curPosition.Paragraph, p);
            // need to add on click event handler when we decide how to link notes together via files.
        }
    }

}
