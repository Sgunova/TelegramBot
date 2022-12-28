using Administration.Dialogs;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Administration.MenuItems
{
    /// <summary>
    /// Логика взаимодействия для Groups.xaml
    /// </summary>
    public partial class Groups : UserControl
    {
        public Groups()
        {
            InitializeComponent();
        }

        private void GroupsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GroupsList.SelectedItem == null)
            {
                Куратор.ItemsSource = Староста.ItemsSource = Students.ItemsSource = null;
                Куратор.Text = Староста.Text = string.Empty;
                return;
            }
               
            var groups = GroupsList.SelectedItem;
            GroupInfo.Header = "Группа: "+(groups as Group).Название;
            using (var DB = new SQLite())
            {
                var _students = DB.Users.Where(x=>x.Группа == (groups as Group).Название).ToList();

                var FullStudent = new List<string>();
                foreach(var student in _students)
                {
                    FullStudent.Add(student.Фамилия + " " + student.Имя + " " + student.Отчество);
                }
                Староста.ItemsSource = Students.ItemsSource = FullStudent;

                var _teachers = DB.Users.Where(x => x.Статус == "Преподаватель").ToList();

                var AllTeachers = new List<string>();
                foreach (var teacher in _teachers)
                {
                    AllTeachers.Add(teacher.Фамилия + " " + teacher.Имя + " " + teacher.Отчество);
                }
                Куратор.ItemsSource = AllTeachers;

                if((groups as Group).IdСтаросты != null)
                {
                    var user = DB.Users.Where(x => x.Id == (groups as Group).IdСтаросты).First();
                    Староста.Text = (user.Фамилия + " " + user.Имя + " " + user.Отчество);
                }
                else
                    Староста.Text = string.Empty;

                if ((groups as Group).IdКуратора != null)
                {
                    var user = DB.Users.Where(x => x.Id == (groups as Group).IdКуратора).First();
                    Куратор.Text = (user.Фамилия + " " + user.Имя + " " + user.Отчество);
                }
                else
                    Куратор.Text = string.Empty;

                if ((groups as Group).Расписание != null)
                {
                    var user = DB.Groups.Where(x => x.Id == (groups as Group).Id).First();
                    Расписание.Text = user.Расписание;
                }
                else
                    Расписание.Text = string.Empty;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            using(var DB = new SQLite())
            {
                GroupsList.ItemsSource = DB.Groups.ToList();
            }
        }

        private void Куратор_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var GroupName = GroupsList.SelectedItem;
            if (Куратор.SelectedItem != null && GroupName != null)
            {
                var FullName = Куратор.SelectedItem.ToString().Split(' ');

                using (var DB = new SQLite())
                {
                    var user = DB.Users.Where(x => x.Фамилия == FullName[0] && x.Имя == FullName[1] && x.Отчество == FullName[2] && x.Статус =="Преподаватель").FirstOrDefault();
                    if (user != null)
                    {
                        var group = DB.Groups.Where(x => x.Название == (GroupName as Group).Название).FirstOrDefault();
                        (GroupName as Group).IdКуратора = group.IdКуратора = user.Id;
                        DB.Update(user);
                        DB.SaveChanges();
                    }
                }
            }
        }

        private void Староста_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var GroupName = GroupsList.SelectedItem;
            if (Староста.SelectedItem != null&& GroupName!=null)
            {
                var FullName = Староста.SelectedItem.ToString().Split(' ');
                
                using (var DB = new SQLite())
                {
                    var user = DB.Users.Where(x => x.Фамилия == FullName[0] && x.Имя == FullName[1]&& x.Отчество == FullName[2]&&x.Группа== (GroupName as Group).Название).FirstOrDefault();
                    if (user != null)
                    {
                        var group = DB.Groups.Where(x => x.Название == (GroupName as Group).Название).FirstOrDefault();
                        (GroupName as Group).IdСтаросты = group.IdСтаросты = user.Id;
                        DB.Update(user);
                        DB.SaveChanges();
                    }
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            using (var DB = new SQLite())
            {
                string name = string.Empty;
                Group group;

                if ((sender as MenuItem).Name == "Edit")
                {
                    if (GroupsList.SelectedItem == null)
                        return;
                    
                    name = (GroupsList.SelectedItem as Group).Название;
                }

                var window = new AddGroup(name);
                window.ShowDialog();
                if(window.DialogResult == true)
                {
                    if ((sender as MenuItem).Name == "Add")
                        DB.Groups.Add(new Group() { Название = window.Name.Text });
                    else
                    {
                        group = GroupsList.SelectedItem as Group;
                        group.Название = window.Name.Text;

                        DB.Groups.Update(group);

                        var UsersInGroup = DB.Users.Where(x => x.Группа == name);
                        foreach (var user in UsersInGroup)
                        {
                            user.Группа = window.Name.Text;
                        }
                        DB.Users.UpdateRange(UsersInGroup);
                    }
                    DB.SaveChanges();
                }
            }
            UserControl_Loaded(null, null);
        }

        private void Расписание_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                if (GroupsList.SelectedItem == null)
                    return;

                using (var DB = new SQLite())
                {
                    var group = DB.Groups.Where(x => x.Id == (GroupsList.SelectedItem as Group).Id).Single();
                    group.Расписание = Расписание.Text.ToString();
                    if (Расписание.Text.ToString() == string.Empty)
                        group.Расписание = null;
                    DB.Update(group);
                    DB.SaveChanges();
                }
            }
        }
    }
}
