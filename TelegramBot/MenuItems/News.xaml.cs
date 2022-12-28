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
    /// Логика взаимодействия для News.xaml
    /// </summary>
    public partial class News : UserControl
    {
        public News()
        {
            InitializeComponent();
        }
        void Updata()
        {
            var Item = NewsList.SelectedItem as Administration.News;

            using var DB = new SQLite();
            var News = DB.NewsList.ToArray().Reverse();
            NewsList.ItemsSource = News;

            if (Item != null && News.ToList().IndexOf(Item) == -1)
                Body.Text = Title.Text = string.Empty;
            else if (Item != null)
                NewsList.SelectedItem = News.Where(x=>x.Id == Item.Id).SingleOrDefault();

        }
        private void NewsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using var DB = new SQLite();
            if (NewsList.SelectedItem == null)
                Body.Text = Title.Text = string.Empty;
            else
            {
                var News = DB.NewsList.Where(x => x.Id == (NewsList.SelectedItem as Administration.News).Id).SingleOrDefault();
                Title.Text = News.Title;
                Body.Text = News.Body;
            }
        }

        private void Add_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            using var DB = new SQLite();
            DB.NewsList.Add(new Administration.News { Title = "Новая статья", Body = "" });
            DB.SaveChanges();
            Updata();
        }

        private void Delete_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            using var DB = new SQLite();
            if (NewsList.SelectedItem != null)
            {
                DB.NewsList.Remove(NewsList.SelectedItem as Administration.News);
                NewsList.SelectedItem = null;
                DB.SaveChanges();
                Updata();
            }
            else
                MessageBox.Show("Для удаления выберите нужную статью из списка.", "Ошибка");
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Updata();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using var DB = new SQLite();
            if (NewsList.SelectedItem != null)
            {
                var News = DB.NewsList.Where(x => x.Id == (NewsList.SelectedItem as Administration.News).Id).SingleOrDefault();
                News.Title = Title.Text;
                News.Body = Body.Text;
                DB.SaveChanges();
            }
            else
                MessageBox.Show("Для сохранения выберите нужную статью из списка.", "Ошибка");
        }
    }
}
