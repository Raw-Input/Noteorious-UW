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

namespace Noteorious.Rich_text_controls
{
	
	public partial class RichTextEditorSample : Window
	{
		public RichTextBox activeBox;
		ObservableCollection<MyTabItem> tabItems = new ObservableCollection<MyTabItem>();

		public RichTextEditorSample()
		{
			InitializeComponent();
			cmbFontFamily.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
			cmbFontSize.ItemsSource = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };

			// testing tab stuff
			addTab();

			TabControl1.ItemsSource = tabItems;

			// tree view init
			InitializeFileSystemObjects();
		}

		// adds a new blank tab
		private void addTab()
		{
			MyTabItem newTab = new MyTabItem();
			newTab.ContextMenuUpdate += HandleContextMenu;
			newTab.NoteLinkUpdate += HandleNoteLink;
			tabItems.Add(newTab);
		}

		// adds a new tab with a name of String s
		private void addTab(String s)
		{
			MyTabItem newTab = new MyTabItem(s);
			newTab.ContextMenuUpdate += HandleContextMenu;
			newTab.NoteLinkUpdate += HandleNoteLink;
			tabItems.Add(newTab);
		}

		private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var tc = sender as TabControl;

			if (tc != null)
			{
				MyTabItem item = (MyTabItem)tc.SelectedItem;
				activeBox = item.Content;
			}
		}

		// Handles making a new note + link via the context menu
		public void HandleContextMenu(object sender, MenuItem item)
		{
			String headerText = tabItems[TabControl1.SelectedIndex].Content.Selection.Text;
			addTab(headerText);
			tabItems[TabControl1.SelectedIndex].createHyperLink(tabItems[TabControl1.SelectedIndex].Content.Selection);
			

			// after creating the hyperlink for our new part of the note, we want to save the blank note that was created so it can be referenced by the hyperlink
			SaveXamlPackage(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + headerText + ".noto");

			// Update our tab position
			TabControl1.SelectedIndex = tabItems.Count - 1;

			tabItems[TabControl1.SelectedIndex].Header = headerText;


		}

		public void HandleNoteLink(object sender, Hyperlink h)
		{
			var files = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "*").Select(f => Path.GetFileName(f));
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
					addTab(new TextRange(h.ContentStart, h.ContentEnd).Text);
					TabControl1.SelectedIndex = tabItems.Count - 1;
					LoadXamlPackage(h.NavigateUri.LocalPath);
				}
				
			} else
			{
				addTab(new TextRange(h.ContentStart, h.ContentEnd).Text);
				TabControl1.SelectedIndex = tabItems.Count - 1;
				LoadXamlPackage(h.NavigateUri.LocalPath);
			}
			
			
		}

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

		private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Noteorious Note (*.noto)|*.noto|All files (*.*)|*.*";
			if (dlg.ShowDialog() == true)
			{
				LoadXamlPackage(dlg.FileName);
				tabItems[TabControl1.SelectedIndex].Header = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
			}
		}

		private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (tabItems[TabControl1.SelectedIndex].Header != "New Note")
			{
				SaveXamlPackage(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + tabItems[TabControl1.SelectedIndex].Header + ".noto");
			} else
			{
				SaveFileDialog dlg = new SaveFileDialog();
				dlg.Filter = "Noteorious Note (*.noto)|*.noto|All files (*.*)|*.*";
				if (dlg.ShowDialog() == true)
				{
					SaveXamlPackage(dlg.FileName);
				}
			}
			
		}


		private void SaveXamlPackage(string filePath)
		{
			var range = new TextRange(activeBox.Document.ContentStart,
				activeBox.Document.ContentEnd);
			var fStream = new FileStream(filePath, FileMode.Create);
			range.Save(fStream, DataFormats.XamlPackage);
			fStream.Close();
		}

		void LoadXamlPackage(string filePath)
		{
			if (File.Exists(filePath))
			{
				var range = new TextRange(activeBox.Document.ContentStart,
					activeBox.Document.ContentEnd);
				var fStream = new FileStream(filePath, FileMode.OpenOrCreate);
				range.Load(fStream, DataFormats.XamlPackage);
				fStream.Close();

				foreach (var paragraph in activeBox.Document.Blocks.OfType<Paragraph>())
				{
					foreach (var hyperlink in paragraph.Inlines.OfType<Hyperlink>())
					{
						tabItems[TabControl1.SelectedIndex].createHyperLink(hyperlink);
					}
				}
			}

			
		}


		private void cmbFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (cmbFontFamily.SelectedItem != null)
				activeBox.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, cmbFontFamily.SelectedItem);
		}

		private void cmbFontSize_TextChanged(object sender, TextChangedEventArgs e)
		{
			activeBox.Selection.ApplyPropertyValue(Inline.FontSizeProperty, cmbFontSize.Text);
		}

		private void img_MouseDown(object sender, MouseButtonEventArgs e)
		{

		}

		// File tree methods test

		private void InitializeFileSystemObjects()
		{
			var drives = DriveInfo.GetDrives();
			DriveInfo.GetDrives().ToList().ForEach(drive =>
			{
				var fileSystemObject = new FileSystemObjectInfo(drive);
				fileSystemObject.BeforeExplore += FileSystemObject_BeforeExplore;
				fileSystemObject.AfterExplore += FileSystemObject_AfterExplore;
				treeView.Items.Add(fileSystemObject);
			});
		}

		private void FileSystemObject_AfterExplore(object sender, System.EventArgs e)
		{
			Cursor = Cursors.Arrow;
		}

		private void FileSystemObject_BeforeExplore(object sender, System.EventArgs e)
		{
			Cursor = Cursors.Wait;
		}
	}
}