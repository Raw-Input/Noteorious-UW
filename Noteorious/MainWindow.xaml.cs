using System;
using System.Windows;
using System.Windows.Controls;

namespace Noteorious
{
    public partial class MainWindow : Window
    {
        Microsoft.Win32.OpenFileDialog mDlgOpen = new Microsoft.Win32.OpenFileDialog();
        Microsoft.Win32.SaveFileDialog mDlgSave = new Microsoft.Win32.SaveFileDialog();

        public MainWindow()
        {
            InitializeComponent();
            ResetDlgs();
        }

        private void UpdateStatBar(string message)
        {
            mStatText.Text = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + message;
        }

        private void ResetDlgs()
        {
            mDlgOpen.FileName = "";
            mDlgSave.FileName = "";
            UpdateStatBar("Ready");
        }

        private void mBtnClear_Click(object sender, RoutedEventArgs e)
        {
            ResetDlgs();
            mTB.Text = "";
        }

        private void mBtnOpen_Click(object sender, RoutedEventArgs e)
        {
            if (mDlgOpen.ShowDialog() == true)
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(mDlgOpen.FileName);
                mTB.Text = reader.ReadToEnd();
                reader.Close();
                UpdateStatBar("Read " + mDlgOpen.FileName);
            }
        }

        private void mCBSpellCheck_Toggle(object sender, RoutedEventArgs e)
        {
            if (mTB != null)
            {
                mTB.SpellCheck.IsEnabled = (mCBSpellCheck.IsChecked == true);
                UpdateStatBar((mTB.SpellCheck.IsEnabled ? "En" : "Dis") + "abled spell check");
            }
        }

        private void mCBReadOnly_Toggle(object sender, RoutedEventArgs e)
        {
            if (mTB != null)
            {
                mTB.IsReadOnly = (mCBReadOnly.IsChecked == true);
                UpdateStatBar((mTB.IsReadOnly ? "En" : "Dis") + "abled read only");
            }
        }

        private void mBtnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileAs();
        }

        private void mBtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.Compare(mDlgSave.FileName, "") == 0)
                SaveFileAs();
            else
                SaveFile();
        }

        private void SaveFileAs()
        {
            if (mDlgSave.ShowDialog() == true)
                SaveFile();
            else
                UpdateStatBar("Text not saved to file.");
        }

        private void SaveFile()
        {
            System.IO.StreamWriter writer = new System.IO.StreamWriter(mDlgSave.FileName);
            writer.Write(mTB.Text);
            writer.Close();
            UpdateStatBar("Wrote " + mTB.Text.Length.ToString() + " chars in " + mDlgSave.FileName);
        }

        private void mTB_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
