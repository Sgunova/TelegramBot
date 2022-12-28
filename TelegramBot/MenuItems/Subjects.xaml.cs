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
    /// Логика взаимодействия для Subjects.xaml
    /// </summary>
    public partial class Subjects : UserControl
    {
        public Subjects()
        {
            InitializeComponent();
        }

        private void Список_предметов_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(Список_предметов.SelectedItem != null)
            {
                using var DB = new SQLite();

                var Item = Список_предметов.SelectedItem as Discipline;
                Предмет.Header = Item.Название;
                var Items = DB.Links.Where(x => x.DisciplineId == Item.Id).ToList();
                Список_литературы.ItemsSource = Items;
                Название_предмета.Text = Item.Название;
            }
            else
            {
                Предмет.Header = "Выберите предмет";
                Список_литературы.ItemsSource = null;
                Название_предмета.Text = string.Empty;
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            using var DB = new SQLite();
            DB.Disciplines.Add(new Discipline { Название = "Новый предмет" });
            DB.SaveChanges();
            UserControl_Loaded(sender, e);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Тип.ItemsSource = new List<string> { "Литература", "Лабораторные работы", "Контрольные работы", "Расчетные работы", "Курсовые работы", "Методические материалы", "Рефераты" };
            using var DB = new SQLite();

            var Items = DB.Disciplines.OrderBy(x=>x.Название).ToList();
            Список_предметов.ItemsSource = Items;
        }

        private void Список_литературы_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Список_литературы.SelectedItem != null)
            {
                Save.IsEnabled = true;
                using var DB = new SQLite();
                var Item = DB.Links.Where(x => x.Id == (Список_литературы.SelectedItem as Literature).Id).SingleOrDefault();
                Название.Text = Item.Название;
                Ссылка.Text = Item.Ссылка;
                Описание.Text = Item.Описание;
                Тип.SelectedItem = Item.Тип;
            }
            else
            {
                Save.IsEnabled = false;
                Название.Text = Ссылка.Text = Описание.Text = string.Empty;
            }
        }

        private void Add_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Список_предметов.SelectedItem != null)
            {
                using var DB = new SQLite();
                DB.Links.Add(new Literature { Название = "Новая ссылка", Тип = "Литература", DisciplineId = (Список_предметов.SelectedItem as Discipline).Id});
                DB.SaveChanges();
                Список_предметов_SelectionChanged(null, null);
            }
            else
                MessageBox.Show("Выберите предмет чтобы добавить в него ссылку на литературу","Ошибка");
        }

        private void Delete_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Список_предметов.SelectedItem != null && Список_литературы.SelectedItem != null)
            {
                using var DB = new SQLite();
                DB.Links.Remove(Список_литературы.SelectedItem as Literature);
                DB.SaveChanges();
                Список_предметов_SelectionChanged(null, null);
            }
            else
                MessageBox.Show("Выберите литературу чтобы удалить ее из базы данных", "Ошибка");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Список_предметов.SelectedItem != null && Список_литературы.SelectedItem != null)
            {
                using var DB = new SQLite();
                var Item = DB.Links.Where(x=>x.Id == (Список_литературы.SelectedItem as Literature).Id).SingleOrDefault();
                Item.Название = Название.Text;
                Item.Ссылка = Ссылка.Text;
                Item.Описание= Описание.Text;
                Item.Тип = Тип.SelectedItem.ToString();
                DB.SaveChanges();
                Список_предметов_SelectionChanged(null, null);
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Список_предметов.SelectedItem != null)
            {
                using var DB = new SQLite();
                Discipline Item = DB.Disciplines.Where(x => x.Id == (Список_предметов.SelectedItem as Discipline).Id).SingleOrDefault();
                Item.Название = Название_предмета.Text;
                DB.SaveChanges();

                var Items = DB.Disciplines.OrderBy(x => x.Название).ToList();
                Список_предметов.ItemsSource = Items;

                Список_предметов.SelectedItem = Item;
            }
        }
    }
}
