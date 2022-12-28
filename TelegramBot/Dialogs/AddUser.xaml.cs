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
    /// Логика взаимодействия для AddUser.xaml
    /// </summary>
    public partial class AddUser : Window
    {
        User _User;
        public AddUser(User user, List<string> Groups, List<string> Status)
        {
            _User = user;
            InitializeComponent();
            Фамилия.Text = _User.Фамилия;
            Имя.Text = _User.Фамилия;
            Отчество.Text = _User.Отчество;
            Email.Text = _User.Email;

            Статус.ItemsSource = Status;
            Группа.ItemsSource = Groups;

            Статус.Text = _User.Статус;
            Группа.Text = _User.Группа;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _User.Фамилия = Фамилия.Text;
            _User.Имя = Имя.Text;
            _User.Отчество = Отчество.Text;
            _User.Email = Email.Text;
            _User.Группа = Группа.Text.ToString();
            _User.Статус = Статус.Text.ToString();
            DialogResult = true;
        }

        private void Статус_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(Статус.SelectedItem.ToString() == "Преподаватель")
            {
                Группа.IsEnabled = false;
                Группа.Text = string.Empty;
            }
            else
                Группа.IsEnabled = true;
            
        }
    }
}
