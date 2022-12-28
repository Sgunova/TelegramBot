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

namespace Administration.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для AddGroup.xaml
    /// </summary>
    public partial class AddGroup : Window
    {
        public AddGroup(string Text)
        {
            InitializeComponent();
            Name.Text = Text;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Name.Text.Length > 2)
                DialogResult = true;
        }
    }
}
