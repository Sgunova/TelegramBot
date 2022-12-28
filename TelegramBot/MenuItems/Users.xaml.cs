using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
    public partial class Users : UserControl
    {
        SQLite DataBase;
        public Users(SQLite DataBase)
        {
            this.DataBase = DataBase;
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var emp = DataBase.Users.ToList();
            Data.ItemsSource = emp;
            SetDeafultSettings("ClearIcon", ClearIcon.Children[0] as Path);
            SetDeafultSettings("AddUserIcon", AddUserIcon.Children[0] as Path);
            SetDeafultSettings("DeleteUserIcon", DeleteUserIcon.Children[0] as Path);
            SetDeafultSettings("RefreshIcon", RefreshIcon.Children[0] as Path);
        }

        private void Text_TextChanged(object sender, TextChangedEventArgs e)
        {
            using(var DB = new SQLite())
            {
                if (Text.Text == string.Empty)
                {
                    var emp = DB.Users.ToList();
                    Data.ItemsSource = emp;
                }
                else
                {
                    var emp = DB.Users.ToList();
                    var SearchList = new List<User>();
                    foreach (User user in emp)
                    {
                        var FullName = (user.Фамилия + " " + user.Имя + " " + user.Отчество).ToLower();
                        if (FullName.Contains(Text.Text.ToLower()))
                        {
                            SearchList.Add(user);
                        }
                    }
                    Data.ItemsSource = SearchList;
                }
            }
        }
        private void Icon_MouseEnter(object sender, MouseEventArgs e)
        {
            var Button = sender as Canvas;
            Path Icon = Button.Children[0] as Path;
            Icon.Fill = new SolidColorBrush(Colors.Red);
            switch (Button.Name)
            {
                case "ClearIcon": { Icon.Data = Geometry.Parse(Administration.Properties.Resources.ClearFocus); break; }
                case "AddUserIcon": { Icon.Data = Geometry.Parse(Administration.Properties.Resources.AddUserFocus); break; }
                case "DeleteUserIcon": { Icon.Data = Geometry.Parse(Administration.Properties.Resources.DeleteUserFocus); break; }
                case "RefreshIcon": { Icon.Data = Geometry.Parse(Administration.Properties.Resources.RefreshDBFocus); break; }
            }
        }
        private void Icon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var Button = sender as Canvas;
            switch (Button.Name)
            {
                case "ClearIcon": { Text.Text = string.Empty; break; }
                case "AddUserIcon": {
                        var Status = new List<string>() { "Бакалавр", "Магистр", "Аспирант", "Выпускник", "Преподаватель"};
                        var Groups = DataBase.Groups.Select(x=>x.Название).ToList();
                        if (Data.SelectedItem != null)
                        {
                            User user = DataBase.Users.Where(x=>x.Id == (Data.SelectedItem as User).Id).Single();
                            var window = new Dialogs.AddUser(user, Groups, Status);
                            window.ShowDialog();
                            if (window.DialogResult == true)
                            {
                                DataBase.Users.Update(user);
                                DataBase.SaveChanges();
                            }
                        }
                        else
                        {
                            User user = new();
                            var window = new Dialogs.AddUser(user, Groups, Status);
                            window.ShowDialog();
                            if (window.DialogResult == true)
                            {
                                DataBase.Users.Add(user);
                                DataBase.SaveChanges();
                            }
                        }
                        Data.ItemsSource = DataBase.Users.ToList();
                        break; }
                case "DeleteUserIcon": {
                        if (Data.SelectedItem != null)
                        {
                            var Range = DataBase.Groups.Where(x=>x.IdСтаросты == (Data.SelectedItem as User).Id);
                            foreach(var item in Range)
                            {
                                item.IdСтаросты = null;
                            }
                            DataBase.Groups.UpdateRange(Range);
                            Range = DataBase.Groups.Where(x => x.IdКуратора == (Data.SelectedItem as User).Id);
                            foreach (var item in Range)
                            {
                                item.IdКуратора = null;
                            }
                            DataBase.Groups.UpdateRange(Range);

                            DataBase.Remove(DataBase.Users.Where(x => x.Id == (Data.SelectedItem as User).Id).Single());
                            DataBase.SaveChanges();
                            var emp = DataBase.Users.ToList();
                            Data.ItemsSource = emp;
                        }
                        break; }
                case "RefreshIcon": {
                        var emp = DataBase.Users.ToList();
                        Data.ItemsSource = emp;
                        Text_TextChanged(null, null);
                        break; }
            }
        }
        [Obsolete]
        private void Icon_MouseLeave(object sender, MouseEventArgs? e)
        {
            var Button = sender as Canvas;
            Path Icon = Button.Children[0] as Path;
            SetDeafultSettings(Button.Name, Icon);
        }
        void SetDeafultSettings(string Name, Path Icon)
        {
            Icon.Fill = new SolidColorBrush(Colors.Black);
            switch (Name)
            {
                case "ClearIcon": { Icon.Data = Geometry.Parse(Administration.Properties.Resources.Clear); break; }
                case "AddUserIcon": { Icon.Data = Geometry.Parse(Administration.Properties.Resources.AddUser); break; }
                case "DeleteUserIcon": { Icon.Data = Geometry.Parse(Administration.Properties.Resources.DeleteUser); break; }
                case "RefreshIcon": { Icon.Data = Geometry.Parse(Administration.Properties.Resources.RefreshDB); break; }
            }
        }
    }
}
