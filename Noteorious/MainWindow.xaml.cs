using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Noteorious.ShellClasses;
using swf = System.Windows.Forms;
using System.Collections;
using System.Windows.Data;

namespace Noteorious.Rich_text_controls
{
	
	public partial class RichTextEditorSample : Window
	{
		public RichTextBox activeBox; // this is a copy of the current Rich Text Box that is currently selected, should only be used for reading from the box
		ObservableCollection<MyTabItem> tabItems = new ObservableCollection<MyTabItem>(); // stores a list of all the tabs currently loaded by the program
		public String defaultFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); //  where the app will save and load notes from 
		public String matchedText = "";
		public bool debugging = true;

		public int clickCount = 0; // controls the click count for file tree objects
		public object lastSender = null;
		public RichTextEditorSample()
		{
			// init editor
			InitializeComponent();

			// font sizes and families
			cmbFontFamily.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
			cmbFontSize.ItemsSource = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };

			// open a blank tab at the start
			addTab();

			// Our tabcontrol will get its tabs from the tabItems collection
			TabControl1.ItemsSource = tabItems;

			// tree view init
			InitializeFileSystemObjects();
		}

		// adds a new blank tab
		private void addTab()
		{
			MyTabItem newTab = new MyTabItem();
			newTab.ContextMenuUpdate += HandleContextMenu; // event handler
			newTab.NoteLinkUpdate += HandleNoteLink; // event handler
			tabItems.Add(newTab); // add to collection
		}

		// adds a new tab with a name of String s
		private void addTab(String s)
		{
			s = s.Trim();
			MyTabItem newTab = new MyTabItem(s); 
			newTab.ContextMenuUpdate += HandleContextMenu; // event handler
			newTab.NoteLinkUpdate += HandleNoteLink; // event handler
			tabItems.Add(newTab); // add to collection
		}

		// fires when a new tab is selected
		private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var tc = sender as TabControl;

			if (tc != null)
			{
				MyTabItem item = (MyTabItem)tc.SelectedItem;
				activeBox = item.Content; 
			}
		}

		// Handles all context menu events
		public void HandleContextMenu(object sender, MenuItem item)
		{
			if((String)item.Header == "Make new note")
			{
				String headerText = tabItems[TabControl1.SelectedIndex].Content.Selection.Text;
				addTab(headerText); // whatever was selected by the user is set as the header of the new tab
				tabItems[TabControl1.SelectedIndex].createHyperLink(); // make the selected text link to the new tab

				// Update our tab position
				TabControl1.SelectedIndex = tabItems.Count - 1;
				
				// after creating the hyperlink for our new part of the note, we want to save the blank note that was created so it can be referenced by the hyperlink
				SaveXamlPackage(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + headerText + ".noto");

				tabItems[TabControl1.SelectedIndex].Header = headerText;
			} else if ((String)item.Header == "Link to existing note") // making the sub menu for selecting a note
			{
				tabItems[TabControl1.SelectedIndex].addSubMenuItem(item, getNotes());
			} else // when selecting a note via the sub menu
			{
				String selectedText = tabItems[TabControl1.SelectedIndex].Content.Selection.Text;
				var files = Directory.GetFiles(defaultFolder, "*").Select(f => Path.GetFileNameWithoutExtension(f));
				var uri = new System.Uri(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + (String)item.Header + ".noto");
				tabItems[TabControl1.SelectedIndex].createHyperLink(uri); // make the selected text link to the new tab
				if (files.Contains(System.IO.Path.GetFileName((String)item.Header)))
				{
					for (int i = 0; i < tabItems.Count; i++)
					{
						if (tabItems[i].Header == System.IO.Path.GetFileNameWithoutExtension((String)item.Header))
						{
							TabControl1.SelectedIndex = i;
						}
					}
				} else
				{
					addTab((String)item.Header); // whatever was selected by the user is set as the header of the new tab

					tabItems[TabControl1.SelectedIndex].createHyperLink(uri); // make the selected text link to the new tab

					// after creating the hyperlink for our new part of the note, we want to save the blank note that was created so it can be referenced by the hyperlink
					// SaveXamlPackage(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + (String)item.Header + ".noto");

					// Update our tab position
					TabControl1.SelectedIndex = tabItems.Count - 1;

					// Load the linked to tab
					LoadXamlPackage(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + (String)item.Header + ".noto");

					// Change the header of the tab
					tabItems[TabControl1.SelectedIndex].Header = (String)item.Header;
				}
			}
		}

		// Handles clicking a hyperlink
		public void HandleNoteLink(object sender, Hyperlink h)
		{
			// get all the files in the user's documents and search for a match with the sent hyper link
			var files = Directory.GetFiles(defaultFolder, "*").Select(f => Path.GetFileName(f));
			if (files.Contains(System.IO.Path.GetFileName(h.NavigateUri.OriginalString))) {
				bool trigger = false;
				for (int i = 0; i < tabItems.Count; i++)
				{
					if (tabItems[i].Header == System.IO.Path.GetFileNameWithoutExtension(h.NavigateUri.OriginalString))
					{
						trigger = true;
						TabControl1.SelectedIndex = i;
					}
				}
				if (!trigger)
				{
					addTab(Path.GetFileNameWithoutExtension(h.NavigateUri.OriginalString));
					TabControl1.SelectedIndex = tabItems.Count - 1;
					activeBox = tabItems[TabControl1.SelectedIndex].Content;
					LoadXamlPackage(h.NavigateUri.LocalPath);
				}
				
			} else
			{
				addTab(new TextRange(h.ContentStart, h.ContentEnd).Text);
				TabControl1.SelectedIndex = tabItems.Count - 1;
				LoadXamlPackage(h.NavigateUri.LocalPath);
			}
			
			
		}

		// Fires whenever the X of a tab is clicked, closes the tab
		private void close_MouseUp(object sender, RoutedEventArgs e)
		{

			if (tabItems.Count > 1 && TabControl1.SelectedIndex == tabItems.Count-1)
			{
				TabControl1.SelectedIndex -= 1;
				tabItems.RemoveAt(TabControl1.SelectedIndex + 1);
			} else if (tabItems.Count > 1)
			{
				TabControl1.SelectedIndex += 1;
				tabItems.RemoveAt(TabControl1.SelectedIndex - 1);
			}
		}

		// Fires when a new note button is clicked
		private void btnNewNote_MouseUp(object sender, RoutedEventArgs e)
		{
			addTab();
			TabControl1.SelectedIndex += 1;
		}

		// Fires when save as button is clicked
		private void btnSaveAs_MouseUp(object sender, RoutedEventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.Filter = "Noteorious Note (*.noto)|*.noto|All files (*.*)|*.*";
			if (dlg.ShowDialog() == true)
			{
				SaveXamlPackage(dlg.FileName);
				tabItems[TabControl1.SelectedIndex].Header = Path.GetFileNameWithoutExtension(dlg.FileName);
			}
		}

		// Fires when a file tree object is clicked, will only trigger code if object is double clicked
		private void fileTreeNote_click(object sender, MouseButtonEventArgs e)
		{
			
			clickCount++;
			if (sender == lastSender)
			{
				if (clickCount % 2 == 0 && matchedText == "")
				{
					Trace.WriteLine(matchedText);
					StackPanel s = (StackPanel)sender;
					TextBlock t = (TextBlock)s.Children[1];
					addTab(t.Text.Substring(0, t.Text.Length - 5));
					TabControl1.SelectedIndex = tabItems.Count - 1;
					LoadXamlPackage(defaultFolder + "\\" + t.Text);
				} else if (clickCount % 2 == 0)
				{
					
					StackPanel s = (StackPanel)sender;
					TextBlock t = (TextBlock)s.Children[1];
					addTab(t.Text.Substring(0, t.Text.Length - 5));
					TabControl1.SelectedIndex = tabItems.Count - 1;
					LoadXamlPackage(defaultFolder + "\\" + t.Text);
					highlightText();
					Keyboard.ClearFocus();
					Keyboard.Focus(activeBox);
					
				}
			}
			lastSender = sender;
		}

		// Handles when the font changes via the selection box
		private void rtbEditor_SelectionChanged(object sender, RoutedEventArgs e)
		{
			
			object temp = activeBox.Selection.GetPropertyValue(Inline.FontWeightProperty);
			btnBold.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontWeights.Bold));
			temp = activeBox.Selection.GetPropertyValue(Inline.FontStyleProperty);
			btnItalic.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontStyles.Italic));
			temp = activeBox.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
			btnUnderline.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(TextDecorations.Underline));
			temp = activeBox.Selection.GetPropertyValue(Inline.FontFamilyProperty);
			cmbFontFamily.SelectedItem = temp;
			temp = activeBox.Selection.GetPropertyValue(Inline.FontSizeProperty);
			cmbFontSize.Text = temp.ToString();
		}

		// Fires when clicking the open button at the top of the app
		private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Noteorious Note (*.noto)|*.noto|All files (*.*)|*.*";
			dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			if (dlg.ShowDialog() == true)
			{
				LoadXamlPackage(dlg.FileName);
				tabItems[TabControl1.SelectedIndex].Header = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
			}
		}

		// Fires when clicking the save button at the top of the app
		private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (tabItems[TabControl1.SelectedIndex].Header != "New Note") // if the app has been saved / already exists, save the file dynamically
			{
				SaveXamlPackage(defaultFolder + "\\" + tabItems[TabControl1.SelectedIndex].Header + ".noto");
			} else // otherwise let the user choose what to save the file as
			{
				SaveFileDialog dlg = new SaveFileDialog();
				dlg.Filter = "Noteorious Note (*.noto)|*.noto|All files (*.*)|*.*";
				if (dlg.ShowDialog() == true)
				{
					SaveXamlPackage(dlg.FileName);
					tabItems[TabControl1.SelectedIndex].Header = Path.GetFileNameWithoutExtension(dlg.FileName);
				}

			}
			
		}

		private void highlightText()
		{
			TextRange textRange = new TextRange(activeBox.Document.ContentStart, activeBox.Document.ContentEnd);
			TextPointer text = textRange.Start.GetInsertionPosition(LogicalDirection.Forward);
			Trace.WriteLine(matchedText);
			while (text != null)
			{
				String temptext = text.GetTextInRun(LogicalDirection.Forward);
				if (!string.IsNullOrWhiteSpace(temptext) && matchedText != "")
				{
					int index = temptext.IndexOf(matchedText);
					
					if (index != -1)
					{
						Trace.WriteLine(index);
						Trace.WriteLine(matchedText.Length);
						Trace.WriteLine("Highlighted");
						TextPointer selectionStart = text.GetPositionAtOffset(index, LogicalDirection.Forward);
						TextPointer selectionEnd = selectionStart.GetPositionAtOffset(matchedText.Length, LogicalDirection.Forward);
						TextRange selection = new TextRange(selectionStart, selectionEnd);
						selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
						selection.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
						Trace.WriteLine(selection.Text);
						//activeBox.SelectionBrush = Brushes.Yellow;
						activeBox.Selection.Select(selection.Start, selection.End);
					}
				}
				text = text.GetNextContextPosition(LogicalDirection.Forward);
			}
			matchedText = "";
		}

		public List<String> getNotes ()
		{

			var notes = Directory.GetFiles(defaultFolder, "*.noto").Select(f => Path.GetFileNameWithoutExtension(f));
			List<String> noteList = notes.ToList();
			return noteList;
		}

		// saves the current tab's rich text box to a specified filepath
		private void SaveXamlPackage(string filePath)
		{
			var range = new TextRange(activeBox.Document.ContentStart, activeBox.Document.ContentEnd);
			var fStream = new FileStream(filePath, FileMode.Create);
			range.Save(fStream, DataFormats.XamlPackage);
			fStream.Close();
		}

		// loads the current tab's rich text box from a specified filepath
		void LoadXamlPackage(string filePath)
		{
			if (File.Exists(filePath))
			{
				var range = new TextRange(activeBox.Document.ContentStart,
					activeBox.Document.ContentEnd);
				var fStream = new FileStream(filePath, FileMode.OpenOrCreate);
				range.Load(fStream, DataFormats.XamlPackage);
				fStream.Close();

				// add back all the hyperlink event handlers as they are not automatically added 
				foreach (var paragraph in activeBox.Document.Blocks.OfType<Paragraph>()) 
				{
					foreach (var hyperlink in paragraph.Inlines.OfType<Hyperlink>())
					{
						tabItems[TabControl1.SelectedIndex].createHyperLink(hyperlink);
					}
				}
			}
		}

		private String LoadSearchFiles(string filePath)
		{
			if (File.Exists(filePath))
			{
				RichTextBox dummybox = new RichTextBox();
				var range = new TextRange(dummybox.Document.ContentStart, dummybox.Document.ContentEnd);
				var fStream = new FileStream(filePath, FileMode.OpenOrCreate);
				range.Load(fStream, DataFormats.XamlPackage);
				Debug.WriteLine("[" + filePath + "]: " + range.Text);

				fStream.Close();

				return range.Text;
			}
			else
			{
				Debug.WriteLine("[ERROR file not loaded fixme");
				return "";
			}
		}

		// Fires when the Font Selection box is changed
		private void cmbFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (cmbFontFamily.SelectedItem != null)
				activeBox.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, cmbFontFamily.SelectedItem);
		}

		// Fires when the Font Size box is changed
		private void cmbFontSize_TextChanged(object sender, TextChangedEventArgs e)
		{
			activeBox.Selection.ApplyPropertyValue(Inline.FontSizeProperty, cmbFontSize.Text);
		}

		


		// File tree methods (and cursor type methods)
		private void InitializeFileSystemObjects()
		{ 
			// Get path to default folder (set to documents by default)
			var docspath = defaultFolder;
			// Get folder info for default folder
			var docsfolderinfo = new DirectoryInfo(docspath);

			// Create file system object with default folder info (have to feed file system object into items.add)
			var fileSystemObject = new FileSystemObjectInfo(docsfolderinfo);


			// Changing cursor to indicate loading (Seen mostly when opening additional files)
			fileSystemObject.BeforeExplore += FileSystemObject_BeforeExplore;
			fileSystemObject.AfterExplore += FileSystemObject_AfterExplore;

			// After loading, add file system to tree view
			treeView.Items.Add(fileSystemObject);
		}

		private void FileSystemObject_AfterExplore(object sender, System.EventArgs e)
		{
			Cursor = Cursors.Arrow;
		}

		private void FileSystemObject_BeforeExplore(object sender, System.EventArgs e)
		{
			Cursor = Cursors.Wait;
		}

		// Triggers from xaml right click context menu on file tree -> when user selects "open new project folder"
		private void Pfolder_open(object sender, RoutedEventArgs e)
		{ 
			swf.FolderBrowserDialog dlg = new swf.FolderBrowserDialog();
			swf.DialogResult result = dlg.ShowDialog();
			dlg.RootFolder = Environment.SpecialFolder.MyDocuments;

			if (result == swf.DialogResult.OK) // Only try to add project folder if user hit ok and not cancel
			{
				var pfolderpath = dlg.SelectedPath; // Store file path for folder user selected

				// Take given file path and get folder info to be added to file tree
				var pfolderinfo = new DirectoryInfo(pfolderpath);

				// Create file system object with the directory info
				var fileSystemObject = new FileSystemObjectInfo(pfolderinfo);

				// Changing cursor to indicate loading (Seen mostly when opening additional files)
				fileSystemObject.BeforeExplore += FileSystemObject_BeforeExplore;
				fileSystemObject.AfterExplore += FileSystemObject_AfterExplore;

				// After loading, add file system to tree view
				treeView.Items.Add(fileSystemObject);
			}
		}

		// Triggers from xaml right click context menu on file tree -> when user selects "close docs folder"
		private void Pfolder_close(object sender, RoutedEventArgs e)
		{
			// NOT WORKING, WIP
			/*
			// Get path to system's documents folder
			var docspath = defaultFolder;
			// Get folder info for stored system's documents folder
			var docsfolderinfo = new DirectoryInfo(docspath);

			// Establish file tree only with established documents folder
			var fileSystemObject = new FileSystemObjectInfo(docsfolderinfo);

			// Changing cursor to indicate loading (Seen mostly when opening additional files)
			fileSystemObject.BeforeExplore += FileSystemObject_BeforeExplore;
			fileSystemObject.AfterExplore += FileSystemObject_AfterExplore;

			// After loading, add file system to tree view
			treeView.Items.Remove(fileSystemObject);
			*/
		}

		// Search box methods

		private void Pfolder_clear(object sender, RoutedEventArgs e)
		{
			treeView.Items.Clear();
		}

		private void Pfolder_hide(object sender, RoutedEventArgs e)
		{
			treeView.Visibility = Visibility.Hidden;
		}

		private void SearchResults_clear(object sender, RoutedEventArgs e)
		{
			// Purpose of having 2 methods where this one just calls the other is so EndSearch can be called without input objects sender/eventargs
			EndSearch(); // Handles text box, clearing search tree, tree view visibilities, etc
		}

		private void SearchBoxTextFocus(object sender, RoutedEventArgs e)
		{
			var tb1 = sender as TextBox;
			if (tb1.Text == " Search Notes...")
			{
				tb1.Text = "";
			}
			
		}

		private void SearchBoxLostFocus(object sender, RoutedEventArgs e)
		{
			var tb1 = sender as TextBox;
			if(tb1.Text == "") // Search box has been cleared (left blank)
			{
				tb1.Text = " Search Notes...";
				treeView.Visibility = Visibility.Visible;		// Show project folders
				searchView.Visibility = Visibility.Collapsed;   // Hide search files
			}
			else // Search box has text in it
			{
				// Do nothing; Should only be activated by user pressing enter, not just typing something then leaving the box
			}
		}

		// User pressed enter in the text box
		private void Search_KeyDownHandler(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return) // Enter/Return is pressed in the text box
			{
				var tb1 = sender as TextBox;
				if (tb1.Text == "") // Search box has been cleared (left blank)
				{
					EndSearch(); // Handles text box, clearing search tree, tree view visibilities, etc
				}
				else // Search box has text in it (activate search)
				{
					if (debugging) // If debugging property is enabled
					{ 
						switch (tb1.Text) // Input commands into search box for debugging
						{
							case "pi": // Print items:
								foreach (FileSystemObjectInfo Pdirectory in treeView.Items)
								{
									Debug.WriteLine(Pdirectory.FileSystemInfo); // Prints as file system info
									Debug.WriteLine(Pdirectory.FileSystemInfo.ToString()); // Prints (the same thing) as string
								}
								break;
							default: // No recognized command entered:
								ActivateSearch(tb1.Text); // Do regular search
								break;
						}
					}
					else // Not debugging
					{
						ActivateSearch(tb1.Text); // Do regular search
					}
				}
			}
		}

		

		private void ActivateSearch(string searchtxt)
		{
			Cursor = Cursors.Wait; // Set loading cursor before everything begins
			treeView.Visibility = Visibility.Hidden;        // Hide project folders
			searchView.Visibility = Visibility.Visible;     // Show search files

			foreach (FileSystemObjectInfo Pdirectory in treeView.Items) // For every project folder added to tree view
			{
				Debug.WriteLine(Pdirectory.FileSystemInfo.ToString()); // Console printing of directories (paths) in treeview, delete later

				
				string[] files = System.IO.Directory.GetFiles(Pdirectory.FileSystemInfo.ToString(), "*.noto");

				foreach (var notofile in files) // for each located noto file
				{
					string nototext = LoadSearchFiles(notofile); // Load the file (XamlPackage) to get its text
					if (nototext.Contains(searchtxt)) // If the text in a file contains the user searched text
					{
						
						matchedText = searchtxt;
						// Get file info
						var notofileinfo = new FileInfo(notofile);

						// Create file system object with file info (have to feed file system object into items.add)
						var fileSystemObject = new FileSystemObjectInfo(notofileinfo);

						// Add file system object to the search results tree view
						searchView.Items.Add(fileSystemObject);
					}
				}
			}

			/* // Seraching just through documents folder (left here for reference, delete later)
			 * 
			string[] files = System.IO.Directory.GetFiles(defaultFolder, "*.noto");

			foreach (var notofile in files) // for each located noto file
			{
				string nototext = LoadSearchFiles(notofile); // Load the file (XamlPackage) to get its text
				if (nototext.Contains(searchtxt)) // If the text in a file contains the user searched text
				{
					// Get file info
					var notofileinfo = new FileInfo(notofile);

					// Create file system object with file info (have to feed file system object into items.add)
					var fileSystemObject = new FileSystemObjectInfo(notofileinfo);

					// Add file system object to the search results tree view
					searchView.Items.Add(fileSystemObject);
				}
			}

			*/

			Cursor = Cursors.Arrow; // Set normal cursor once files loaded and finished
		}

		private void EndSearch()
		{
			txtSearchBox.Text = " Search Notes...";
			treeView.Visibility = Visibility.Visible;
			searchView.Visibility = Visibility.Collapsed;
			Keyboard.ClearFocus();
			treeView.Focus();
			searchView.Items.Clear();
		}

		private void Toolbar_MouseDownHandler(object sender, MouseButtonEventArgs e)
		{
			toolbar.Focus();
		}
	}
}