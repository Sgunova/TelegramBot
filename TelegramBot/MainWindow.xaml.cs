using Administration;
using Administration.MenuItems;
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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TelegramBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Canvas? Menu;
        List<object> Windows;
        SQLite DataBase;
        public MainWindow()
        {
            DataBase = new SQLite();
            InitializeComponent();
        }

        void SetDeafultSettings(string Name, Path Icon)
        {
            Icon.Fill = new SolidColorBrush(Colors.Black);
            switch (Name)
            {
                case "ChatIcon": { Icon.Data = Geometry.Parse(Administration.Properties.Resources.Chat); break; }
                case "UsersIcon": { Icon.Data = Geometry.Parse(Administration.Properties.Resources.Users); break; }
                case "GroupsIcon": { Icon.Data = Geometry.Parse(Administration.Properties.Resources.Groups); break; }
                case "DisciplinesIcon": { Icon.Data = Geometry.Parse(Administration.Properties.Resources.Disciplines); break; }
                case "TestsIcon": { Icon.Data = Geometry.Parse(Administration.Properties.Resources.Tests); break; }
                case "InfoIcon": { Icon.Data = Geometry.Parse(Administration.Properties.Resources.Info); break; }
                case "NewsIcon": { Icon.Data = Geometry.Parse(Administration.Properties.Resources.News); break; }
            }
        }

        [Obsolete]
        private void StartBot(object sender, RoutedEventArgs e)
        {
            
            Windows = new List<object>();
            Windows.Add(new Chats());
            Windows.Add(new Users(DataBase));
            Windows.Add(new Groups());
            Windows.Add(new Subjects());
            Windows.Add(new Tests());
            Windows.Add(new Administration.MenuItems.News());

            foreach (Canvas Button in MenuList.Children)
            {
                var IconPath = new Path();
                SetDeafultSettings(Button.Name, IconPath);
                Button.Children.Add(IconPath);
            }
            InfoIcon.Children.Add(new Path { Fill = new SolidColorBrush(Colors.Black), Data = Geometry.Parse(Administration.Properties.Resources.Info) });

            Menu = InfoIcon;
            Icon_MouseDown(ChatIcon, null);
        }

        [Obsolete]
        private void Icon_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Canvas).BitmapEffect = new DropShadowBitmapEffect() { ShadowDepth = 1, Opacity = 0.7, Softness = 0.15 };
        }
        private void Icon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var Button = sender as Canvas;
            if (Button == Menu)
                return;

            Path Icon = Button.Children[0] as Path;
            if(Button.Name != "InfoIcon")
                Icon.Fill = new SolidColorBrush(Colors.DeepSkyBlue);

            switch (Button.Name)
            {
                case "ChatIcon": { 
                        Icon.Data = Geometry.Parse(Administration.Properties.Resources.ChatFocus);
                        Window.Children.Clear();
                        Window.Children.Add((UIElement)Windows[0]);
                        break; }
                case "UsersIcon": { 
                        Icon.Data = Geometry.Parse(Administration.Properties.Resources.UsersFocus); 
                        Window.Children.Clear(); 
                        Window.Children.Add((UIElement)Windows[1]); 
                        break; }
                case "GroupsIcon": { 
                        Icon.Data = Geometry.Parse(Administration.Properties.Resources.GroupsFocus); 
                        Window.Children.Clear();
                        Window.Children.Add((UIElement)Windows[2]);
                        break; }
                case "DisciplinesIcon": { 
                        Icon.Data = Geometry.Parse(Administration.Properties.Resources.DisciplinesFocus); 
                        Window.Children.Clear();
                        Window.Children.Add((UIElement)Windows[3]);
                        break; }
                case "TestsIcon": { 
                        Icon.Data = Geometry.Parse(Administration.Properties.Resources.TestsFocus);
                        Window.Children.Clear();
                        Window.Children.Add((UIElement)Windows[4]);
                        break; }
                case "NewsIcon":{
                        Icon.Data = Geometry.Parse(Administration.Properties.Resources.NewsFocus);
                        Window.Children.Clear();
                        Window.Children.Add((UIElement)Windows[5]);
                        break;
                    }
                case "InfoIcon": { var Window = new About(); Window.Show(); break; }
            }
            if (Menu != null)
                SetDeafultSettings(Menu.Name, Menu.Children[0] as Path);
            Menu = Button;
        }
        [Obsolete]
        private void Icon_MouseLeave(object sender, MouseEventArgs? e)
        {
            Canvas Icon = sender as Canvas;
            Icon.BitmapEffect = null;
            if (Menu != Icon)
                SetDeafultSettings(Icon.Name, Icon.Children[0] as Path);
        }
    }
}
