using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace docx2tex.UI
{
    /// <summary>
    /// Interaction logic for MainForm.xaml
    /// </summary>
    public partial class MainForm : Window, IContentClosable
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void mnuConfDocument_Click(object sender, RoutedEventArgs e)
        {
            using (var ofd = new System.Windows.Forms.OpenFileDialog())
            {
                ofd.Multiselect = false;
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;
                ofd.DereferenceLinks = true;
                ofd.Filter = "Word 2007 documents (*.docx;*.docm)|*.docx;*.docm";
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    PutConfigEditor(ConfigurationClassEnum.Document, ofd.FileName);
                }
            }
        }

        private void mnuConfUser_Click(object sender, RoutedEventArgs e)
        {
            PutConfigEditor(ConfigurationClassEnum.User);
        }

        private void mnuConfSystem_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you really want to edit the System level configuration?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                PutConfigEditor(ConfigurationClassEnum.System);
            }
        }

        private void mnuNewConversion_Click(object sender, RoutedEventArgs e)
        {
            PutConverter();
        }

        private void PutConfigEditor(ConfigurationClassEnum confClassLevel)
        {
            PutConfigEditor(confClassLevel, null);
        }

        private void PutConfigEditor(ConfigurationClassEnum confClassLevel, string documentFilePath)
        {
            grdCenter.Children.Clear();
            var configEditor = new ConfigEditor(confClassLevel, documentFilePath, this);
            configEditor.Opacity = 0.9;
            Grid.SetColumn(configEditor, 0);
            Grid.SetRow(configEditor, 0);
            grdCenter.Children.Add(configEditor);
        }

        private void PutConverter()
        {
            grdCenter.Children.Clear();
            var converter = new Converter(this);
            converter.Opacity = 0.9;
            Grid.SetColumn(converter, 0);
            Grid.SetRow(converter, 0);
            grdCenter.Children.Add(converter);
        }

        public void ContentClose()
        {
            grdCenter.Children.Clear();
        }
    }

    public interface IContentClosable
    {
        void ContentClose();
    }
}
