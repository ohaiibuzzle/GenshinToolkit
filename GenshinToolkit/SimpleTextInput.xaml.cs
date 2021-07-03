using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GenshinToolkit
{
    /// <summary>
    /// Interaction logic for SimpleTextInput.xaml
    /// </summary>
    public partial class SimpleTextInput : Window
    {
        public string value;
        public SimpleTextInput(string InititialValue)
        {
            InitializeComponent();

            textBox.Text = InititialValue;
            value = InititialValue;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            value = textBox.Text;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
