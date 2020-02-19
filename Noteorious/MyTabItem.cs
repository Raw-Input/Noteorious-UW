using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Noteorious
{
    public class MyTabItem : INotifyPropertyChanged
    {
        

        public event EventHandler<MenuItem> ContextMenuUpdate;

        public event EventHandler<Hyperlink> NoteLinkUpdate;

        public event PropertyChangedEventHandler PropertyChanged;

        private String _header;
        public string Header
        {
            get { return _header; }
            set
            {
                if (value == _header) return;
                _header = value;
                OnPropertyChanged();
            }
        }

        public RichTextBox Content { get; set; }

        // Default constructor takes in no variables and creates a new tab
        public MyTabItem()
        {
            // Create Context menu for the RichTextBox
            ContextMenu cm = new ContextMenu();  // make a context menu
            MenuItem item = new MenuItem(); // make a menuitem instance
            item.Header = "Make new note"; // give the item a header
            cm.Items.Add(item);
            item.Click += (sendingelement, eventargs) => cUpdate(item, eventargs); // give the item a click event handler
            cm.IsEnabled = true;

            //Create header of tab
            Header = "New Note";

            // Create a RichTextBox for editing text
            Content = new RichTextBox();
            Content.ContextMenu = cm;

            // allows hyperlinks to function
            Content.IsDocumentEnabled = true;

            
        }

        // This constructor takes in a string for the header and creates a new tab with the identified header
        public MyTabItem(String s)
        {
            // Create Context menu for the RichTextBox
            ContextMenu cm = new ContextMenu(); // make a context menu
            MenuItem item = new MenuItem(); // make a menuitem instance
            item.Header = "Make new note"; // give the item a header
            cm.Items.Add(item);
            item.Click += (sendingelement, eventargs) => cUpdate(item, eventargs); // give the item a click event handler
            cm.IsEnabled = true;

            // Create header of tab
            Header = s;

            // Create a RichTextBox for editing text
            Content = new RichTextBox();
            Content.ContextMenu = cm;

            // Allows hyperlinks to function
            Content.IsDocumentEnabled = true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



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
            // set the uri of the new hyperlink we created
            var uri = new System.Uri(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + s.Text + ".noto");
            Run r = new Run(s.Text);
            TextPointer tp = Content.CaretPosition.GetInsertionPosition(LogicalDirection.Forward);
            Hyperlink h = new Hyperlink(r, tp);
            h.NavigateUri = uri;
            tp.DeleteTextInRun(s.Text.Length);
            h.Click += (sendingelement, eventargs) => nUpdate(h, eventargs);


        }

        public void createHyperLink(Hyperlink h)
        {
            h.Click += (sendingelement, eventargs) => nUpdate(h, eventargs);
        }

    }
}
