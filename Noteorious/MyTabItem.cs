using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Noteorious
{
    public class MyTabItem : INotifyPropertyChanged
    {


        public event EventHandler<MenuItem> ContextMenuUpdate; // Event handler for right click context menu

        public event EventHandler<Hyperlink> NoteLinkUpdate; // Event handler for adding a hyperlink

        public event PropertyChangedEventHandler PropertyChanged; // Event handler for changing the tab header via opening a file

        private String _header; // the tab's header

        // sets or gets the tab's header
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

        // the tab's RichTextBox
        public RichTextBox Content { get; set; }

        // Default constructor takes in no variables and creates a new tab
        public MyTabItem()
        {
            // Build context menu for right click
            ContextMenu cm = createContextMenu();

            // Create header of tab
            Header = "New Note";

            // Create a RichTextBox for editing text
            Content = new RichTextBox();
            Content.ContextMenu = cm;
            //SolidColorBrush b = (SolidColorBrush)(new BrushConverter().ConvertFrom("#6da03c"));
            Content.BorderThickness = new Thickness(0);
            Content.Margin = new Thickness(10);
            //Content.BorderBrush = b;
            
            // allows hyperlinks to function
            Content.IsDocumentEnabled = true;


        }

        // This constructor takes in a string for the header and creates a new tab with the identified header
        public MyTabItem(String s)
        {
            // Build context menu for right click
            ContextMenu cm = createContextMenu();

            // Create header of tab
            Header = s;

            // Create a RichTextBox for editing text
            Content = new RichTextBox();
            Content.ContextMenu = cm;
            //SolidColorBrush b = (SolidColorBrush)(new BrushConverter().ConvertFrom("#6da03c"));
            Content.BorderThickness = new Thickness(0);
            Content.Margin = new Thickness(10);
            //Content.BorderBrush = b;

            // Allows hyperlinks to function
            Content.IsDocumentEnabled = true;
        }


        public ContextMenu createContextMenu()
        {
            // Create Context menu for the RichTextBox
            ContextMenu cm = new ContextMenu();  // make a context menu

            // Create make new note context menu button
            MenuItem mkNewnote = new MenuItem();
            mkNewnote.Header = "Make new note";
            cm.Items.Add(mkNewnote);
            mkNewnote.Click += (sendingelement, eventargs) => cUpdate(mkNewnote, eventargs); // give the item a click event handler

            // Create link note context menu button
            MenuItem lnkNote = new MenuItem();
            lnkNote.Header = "Link to existing note";
            lnkNote.Items.Add(new MenuItem());
            cm.Items.Add(lnkNote);
            lnkNote.SubmenuOpened += (sendingelement, eventargs) => cUpdate(lnkNote, eventargs);


            //foreach ()

            //lnkNote.Click += (sendingelement, eventargs) => cUpdate(lnkNote, eventargs); // give the item a click event handler

            cm.IsEnabled = true; // enable context menu

            return (cm);
        }

        // implementing header event handler
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void addSubMenuItem(MenuItem item, List<String> notes) 
        {
            item.Items.Clear();
            foreach(String s in notes)
            {
                MenuItem newItem = new MenuItem();
                newItem.Header = s;
                newItem.Click += (sendingelement, eventargs) => cUpdate(newItem, eventargs);
                item.Items.Add(newItem);
            }
            
        }

        // Handles context menu events
        public void cUpdate(MenuItem item, EventArgs e)
        {
            
            if (ContextMenuUpdate != null)
            {
                ContextMenuUpdate(this, item);
            }
                
        }

        // Handles note hyper link events
        public void nUpdate(Hyperlink h, EventArgs e)
        {

            if (NoteLinkUpdate != null)
            {
                NoteLinkUpdate(this, h);
            }

        }

        // handles mousing over a hyperlink
        public void hEnterUpdate(Hyperlink h, EventArgs e)
        {

            h.TextDecorations = TextDecorations.Underline;

        }

        // handles mousing over a hyperlink
        public void hLeaveUpdate(Hyperlink h, EventArgs e)
        {

            h.TextDecorations = null;

        }

        // creates new hyper link from a selection of text. S is the source, the destination is automatically set from the source.
        public void createHyperLink (TextSelection s)
        {
            // set the uri of the new hyperlink we created
            var uri = new System.Uri(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + s.Text + ".noto");
            Run r = new Run(s.Text);
            TextPointer tp = Content.CaretPosition.GetInsertionPosition(LogicalDirection.Forward);
            Run prev = (Run)Content.Selection.Start.Parent;
            Hyperlink h = new Hyperlink(r, tp);
            h.FontFamily = prev.FontFamily;
            h.FontSize = prev.FontSize;
            h.NavigateUri = uri;
            h.TextDecorations = null;
            h.Foreground = Brushes.LimeGreen;
            tp.DeleteTextInRun(s.Text.Length);
            h.MouseEnter += (sendingelement, eventargs) => hEnterUpdate(h, eventargs);
            h.MouseLeave += (sendingelement, eventargs) => hLeaveUpdate(h, eventargs);
            h.Click += (sendingelement, eventargs) => nUpdate(h, eventargs);


        }
        // altenative constructor for linking a notes with different names. S is the text you want to be the source, u is the destination.
        public void createHyperLink(TextSelection s, Uri u)
        {
            Run r = new Run(s.Text);
            TextPointer tp = Content.CaretPosition.GetInsertionPosition(LogicalDirection.Forward);
            Hyperlink h = new Hyperlink(r, tp);
            Run prev = (Run)Content.Selection.Start.Parent;
            h.FontFamily = prev.FontFamily;
            h.FontSize = prev.FontSize;
            h.NavigateUri = u;
            h.TextDecorations = null;
            h.Foreground = Brushes.LimeGreen;
            tp.DeleteTextInRun(s.Text.Length);
            h.MouseEnter += (sendingelement, eventargs) => hEnterUpdate(h, eventargs);
            h.MouseLeave += (sendingelement, eventargs) => hLeaveUpdate(h, eventargs);
            h.Click += (sendingelement, eventargs) => nUpdate(h, eventargs);
        }

        // alternative constructor to add event handlers back when opening an existing note
        public void createHyperLink(Hyperlink h)
        {
            h.TextDecorations = null;
            h.Foreground = Brushes.LimeGreen;
            h.MouseEnter += (sendingelement, eventargs) => hEnterUpdate(h, eventargs);
            h.MouseLeave += (sendingelement, eventargs) => hLeaveUpdate(h, eventargs);
            h.Click += (sendingelement, eventargs) => nUpdate(h, eventargs);

        }

    }
}
