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
    /// Логика взаимодействия для Tests.xaml
    /// </summary>
    public partial class Tests : UserControl
    {
        public Tests()
        {
            InitializeComponent();
        }
        class GroupWithAccess : Administration.Group
        {
            public bool IsChecked { get; set; }
            public GroupWithAccess(Group Value, bool Key)
            {
                Id = Value.Id;
                Название = Value.Название;
                IdКуратора = Value.IdКуратора;
                IsChecked = Key;
            }
        }
        private void Список_предметов_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var Select = Список_предметов.SelectedItem as Discipline;
            if(Select != null)
            {
                using var DB = new SQLite();
                GroupName.Text = Select.Название;
                var Teachers = DB.Users.Where(x => x.Статус == "Преподаватель").ToList();
                TeachersList.ItemsSource = Teachers;
                var Groups = DB.Groups.ToList();

                var Access = DB.Disciplines_Access.Where(x => x.DisciplineId == Select.Id).ToList();

                var List = new List<Group>();

                foreach (var Item in Groups)
                {
                    List.Add(new GroupWithAccess(Item, (Access.Find(x => x.GroupId == Item.Id) != null)));
                    var x = Access.FindAll(x => x.GroupId == Item.Id).Count;
                }
                this.Groups.ItemsSource = List;

                var _Tests = DB.Tests.Where(x => x.DisciplineId == Select.Id).ToList();
                TestsList.ItemsSource = _Tests;
            }
            else
            {
                GroupName.Text = string.Empty;
                TeachersList.ItemsSource = null;
                Groups.ItemsSource = null;
                TestsList.ItemsSource = null;
                TestLink.Text = string.Empty;
                TestName.Text = string.Empty;
                TestTime.Text = string.Empty;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            using var DB = new SQLite();

            var Items = DB.Disciplines.OrderBy(x => x.Название).ToList();
            Список_предметов.ItemsSource = Items;
        }

        private void TeachersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var Item = Groups.SelectedItem as Group;
            var Select = Список_предметов.SelectedItem as Discipline;
            var Name = TeachersList.SelectedItem as User;

            if (Item != null && Select != null && Name != null)
            {
                using var DB = new SQLite();
                var Access = DB.Disciplines_Access.Where(x => x.DisciplineId == Select.Id && x.GroupId == Item.Id).FirstOrDefault();
                if(Access == null)
                {
                    DB.Disciplines_Access.Add(new Access { DisciplineId = Select.Id, GroupId = Item.Id, TeacherId = Name.Id });
                }
                else if(Name != null)
                {
                    DB.Disciplines_Access.Remove(Access);
                    DB.Disciplines_Access.Add(new Access { DisciplineId = Select.Id, GroupId = Item.Id, TeacherId = Name.Id });
                }
            }
            else
                TeachersList.SelectedItem = null;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var Item = Groups.SelectedItem as Group;
            var Select = Список_предметов.SelectedItem as Discipline;
            var Name = TeachersList.SelectedItem as User;

            if (Item != null && Select != null && Name != null)
            {
                using var DB = new SQLite();
                var Value = DB.Disciplines_Access.Where(x=>x.DisciplineId == Select.Id && x.GroupId == Item.Id && x.TeacherId == Name.Id).FirstOrDefault();
                if (Value != null)
                    DB.Disciplines_Access.Remove(Value);
                DB.SaveChanges();

                var SelectedItem = Список_предметов.SelectedItem;
                Список_предметов.SelectedItem = null;
                Список_предметов.SelectedItem = SelectedItem;
            }

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var Item = Groups.SelectedItem as Group;
            var Select = Список_предметов.SelectedItem as Discipline;
            var Name = TeachersList.SelectedItem as User;

            if (Item != null && Select != null && Name != null)
            {
                using var DB = new SQLite();
                DB.Disciplines_Access.Add(new Access { DisciplineId = Select.Id, GroupId = Item.Id, TeacherId = Name.Id });
                DB.SaveChanges();

                var SelectedItem = Список_предметов.SelectedItem;
                Список_предметов.SelectedItem = null;
                Список_предметов.SelectedItem = SelectedItem;
            }
            else
                (sender as CheckBox).IsChecked = false;
        }

        private void Groups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var Item = Groups.SelectedItem as Group;
            var Select = Список_предметов.SelectedItem as Discipline;
            if (Groups.ItemsSource != null && Item != null && Item.IdКуратора!=null&& TeachersList.ItemsSource!=null)
            {
                using var DB = new SQLite();
                var Access = DB.Disciplines_Access.Where(x => x.DisciplineId == Select.Id && x.GroupId == Item.Id).FirstOrDefault();
                if(Access!=null)
                    TeachersList.SelectedItem = (TeachersList.ItemsSource as List<User>).Find(x=>x.Id == Access.TeacherId);
                else
                    TeachersList.SelectedItem = null;
            }
            else 
                TeachersList.SelectedItem = null;
        }

        private void Tests_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var SelectItemInTestList = TestsList.SelectedItem as Test;
            if(SelectItemInTestList == null)
            {
                TestLink.Text = 
                TestName.Text = 
                TestTime.Text = string.Empty;
            }
            else
            {
                TestLink.Text = SelectItemInTestList.Ссылка;
                TestName.Text = SelectItemInTestList.Название;
                TestTime.Text = SelectItemInTestList.Время;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            using var DB = new SQLite();
            var MenuItem = sender as MenuItem;
            var Select = Список_предметов.SelectedItem as Discipline;
            var SelectItemInTestList = TestsList.SelectedItem as Test;
            if (MenuItem.Header.ToString() == "Добавить"&& Select!=null)
            {
                DB.Tests.Add(new Test { DisciplineId = Select.Id, Название = "Новый тест", Время = $"12:00 01.01.2023"});
            }
            else if(MenuItem.Header.ToString() == "Удалить" && Select != null&& SelectItemInTestList!=null)
            {
                DB.Tests.Remove(SelectItemInTestList);
            }
            DB.SaveChanges();

            var SelectedItem = Список_предметов.SelectedItem;
            Список_предметов.SelectedItem = null;
            Список_предметов.SelectedItem = SelectedItem;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var SelectItemInTestList = TestsList.SelectedItem as Test;
            if (SelectItemInTestList != null)
            {
                using var DB = new SQLite();

                try
                {
                    if (TestName.Text != string.Empty && TestTime.Text != string.Empty)
                    {
                        DateTime.ParseExact(TestTime.Text, "HH:mm dd.MM.yyyy", null);
                        SelectItemInTestList.Ссылка = TestLink.Text;
                        SelectItemInTestList.Название = TestName.Text;
                        SelectItemInTestList.Время = TestTime.Text;

                        DB.Tests.Update(SelectItemInTestList);
                        DB.SaveChanges();
                    }
                }
                finally
                {
                    var SelectedItem = Список_предметов.SelectedItem;
                    Список_предметов.SelectedItem = null;
                    Список_предметов.SelectedItem = SelectedItem;
                }

            }
        }
    }
}
