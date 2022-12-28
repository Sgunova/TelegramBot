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
using System.Windows.Threading;

namespace Administration.MenuItems
{
    /// <summary>
    /// Логика взаимодействия для Chats.xaml
    /// </summary>
    public partial class Chats : UserControl
    {
        DispatcherTimer timer;
        public Chats()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += Update;
            timer.Start();
        }
        void Update(object sender, EventArgs e)
        {
            using (var DB = new SQLite())
            {
                if (ChatsList.SelectedItem != null)
                {
                    var chat = ChatsList.SelectedItem as Chat;
                    ChatsList.ItemsSource = DB.Chats.ToList();
                    ChatsList.SelectedItem = DB.Chats.Where(x=>x.ChatId == chat.ChatId).Single();
                }
                else
                    ChatsList.ItemsSource = DB.Chats.ToList();
            }
        }

        private void ChatsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ChatsList.SelectedItem == null)
            {
                ChatWithUser.ItemsSource = null;
                ChatName.Header = "";
                return;
            }
            using (var DB = new SQLite())
            {
                var ChatInfo = ChatsList.SelectedItem as Chat;

                if(ChatInfo.UserId == null)
                    ChatName.Header = "Неизвестный пользователь";
                else
                {
                    var user = DB.Users.Where(x=>x.Id == ChatInfo.UserId).FirstOrDefault();
                    ChatName.Header = user.Фамилия + " " + user.Имя + " " + user.Отчество + "("+((user.Статус=="Преподаватель")? user.Статус:user.Группа) +")";
                }

                var History = DB.Chats_History.Where(x=>x.ChatId == (ChatInfo).ChatId);
                var Sourse = new List<string>();
                foreach(Record chat in History)
                {
                    Sourse.Add(chat.DateTime+"\t"+chat.Message);
                }
                ChatWithUser.ItemsSource = Sourse;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) => Update(null, null);

        private void ChatWithUser_MouseEnter(object sender, MouseEventArgs e) => timer.Stop();

        private void ChatWithUser_MouseLeave(object sender, MouseEventArgs e) => timer.Start();

    }
}
