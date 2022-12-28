using Administration;
using System.Net;
using System.Net.Mail;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

int MaxMinuteWait = 30;
TelegramBotClient Bot;

async void Login(string[] Code, string Message, MessageEventArgs e)
{
    switch (Code[1])
    {
        case "1": {
                var Email = Message.ToLower().Split('@');
                if(Email.Length!=2)
                    await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Данный текст не является электронным адресом.");
                else if (Email[1] != "gmail.com"&& Email[1] != "mail.ru"&&Email[1] != "yandex.com"&& Email[1] != "ya.ru")
                    await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Для корректной работы необходимо использовать почтовую службу от компаний Google(@gmail.com), VK(@mail.ru) или Яндекс(@yandex.com, @ya.ru).");
                else if (Email[0].Length < 6)
                    await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Слишком короткий адрес почты.");
                else
                {
                    int Count = 0;
                    for(int i=0; i<Email[0].Length; i++)
                    {
                        if((Email[0][i] =='.'|| Email[0][i] == '_')&&(i==0||i== Email[0].Length - 1))
                        {
                            await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Неверный адрес почты.");
                            return;
                        }
                        if(!((Email[0][i]>='0'&& Email[0][i]<='9'|| Email[0][i] >= 'a' && Email[0][i] <= 'z'|| Email[0][i] == '.' || Email[0][i] == '_')))
                        {
                            await Bot.SendTextMessageAsync(e.Message.Chat.Id, "В адресе почты используются недопустимые символы.");
                            return;
                        }
                        if(Email[0][i] >= 'a' && Email[0][i] <= 'z')
                            Count++;
                    }
                    if(Count==0)
                    {
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "В адресе необходимо чтобы присудствовала хотя бы одна буква.");
                        break;
                    }
                    using var DB = new SQLite();
                    var user = DB.Users.Where(x => x.Email == Message).SingleOrDefault();
                    if(user == null)
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "В системе не заригестрирована данная почта.");
                    else
                    {
                        var chat = DB.Chats.Where(x=>x.ChatId==e.Message.Chat.Id).SingleOrDefault();
                        if (chat.DateTimeToSend == null || (DateTime.Now - DateTime.Parse(chat.DateTimeToSend)).TotalMinutes > MaxMinuteWait)
                        {
                            var rand = new Random();
                            chat.Access_code = rand.Next(100000, 1000000);
                            chat.Temporary_Email = Message;
                            chat.DateTimeToSend = DateTime.Now.ToString();
                            DB.Chats.Update(chat);
                            
                            SendEmail(Message, chat.Access_code.ToString());

                            var ikm = new ReplyKeyboardMarkup
                            {
                                Keyboard = new List<List<KeyboardButton>>
                                {
                                        new List<KeyboardButton>{ new KeyboardButton { Text = "Ввести код" }, new KeyboardButton { Text = "Изменить Email" } },
                                }
                            };
                            ikm.ResizeKeyboard = true;
                            await Bot.SendTextMessageAsync(e.Message.Chat.Id, $"На адрес {Message} выслан код подтверждения для доступа к функционалу чат бота.", replyMarkup: ikm);
                            chat.Access_Level = "0.2";
                            DB.SaveChanges();
                        }
                        else
                            await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Сообщение с кодом нельзя отправить раньше чем через 30 минут.");

                    }
                }
                break; }
        case "2": {
                switch (Message)
                {
                    case "Ввести код": {
                            var ReplyMarkup = new ReplyKeyboardRemove();
                            await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Введите код из письма:", replyMarkup: ReplyMarkup);
                            using var DB = new SQLite();
                            var user = DB.Chats.Where(x => x.ChatId == e.Message.Chat.Id).SingleOrDefault();
                            user.Access_Level = "0.3";
                            DB.Chats.Update(user);
                            DB.SaveChanges();
                            break; }
                    case "Изменить Email": {
                            var ReplyMarkup = new ReplyKeyboardRemove();
                            await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Введите новую почту:", replyMarkup: ReplyMarkup);
                            using var DB = new SQLite();
                            var user = DB.Chats.Where(x => x.ChatId == e.Message.Chat.Id).SingleOrDefault();
                            user.Access_Level = "0.1";
                            DB.Chats.Update(user);
                            DB.SaveChanges();
                            break; }
                    default: {
                            var ikm =new ReplyKeyboardMarkup
                            {
                                Keyboard = new List<List<KeyboardButton>>
                                {
                                        new List<KeyboardButton>{ new KeyboardButton { Text = "Ввести код" }, new KeyboardButton { Text = "Изменить Email" } },
                                }
                            };
                            ikm.ResizeKeyboard = true;
                            await Bot.SendTextMessageAsync(e.Message.Chat.Id, $"Введена неверная команда.", replyMarkup: ikm, replyToMessageId: e.Message.MessageId);
                            break; }
                }
                break; }
        case "3": {
                using var DB = new SQLite();
                var chat = DB.Chats.Where(x => x.ChatId == e.Message.Chat.Id).SingleOrDefault();
                if (chat.Access_code.ToString() == Message)
                {
                    var ReplyMarkup = new ReplyKeyboardRemove();
                    var user = DB.Users.Where(x => x.Email == chat.Temporary_Email).SingleOrDefault();
                    if(user == null)
                    {
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Произошла непредвиденная ошибка, введите Email еще раз.", replyMarkup: ReplyMarkup);
                        chat.Access_code = null;
                        chat.Temporary_Email =
                        chat.DateTimeToSend = null;
                        chat.Access_Level = "0.1";
                    }
                    else
                    {
                        chat.UserId = user.Id;
                        if(user.Статус=="Переподаватель")
                            chat.Access_Level = "1";
                        else
                            chat.Access_Level = "2";

                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Добро пожаловать в чат-бота для поддержки учебного процесса по специальности \"Прикладная информатика\"", replyMarkup: ReplyMarkup);
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, $"Вы авторизованы как {user.Фамилия} {user.Имя} {user.Отчество}");
                        if(user.Статус == "Преподаватель")
                            await Bot.SendTextMessageAsync(e.Message.Chat.Id, $"В данный чат будут отправлятся присланные от студентов работы.", replyMarkup: null);
                        else
                            await Bot.SendTextMessageAsync(e.Message.Chat.Id, $"Теперь вам доступны все пункты меню.", replyMarkup: null);
                    }
                }
                else
                {
                    var ikm = new ReplyKeyboardMarkup
                    {
                        Keyboard = new List<List<KeyboardButton>>
                                {
                                        new List<KeyboardButton>{ new KeyboardButton { Text = "Ввести код" }, new KeyboardButton { Text = "Изменить Email" } },
                                }
                    };
                    ikm.ResizeKeyboard = true;
                    await Bot.SendTextMessageAsync(e.Message.Chat.Id, $"Вы ввели неверный код.", replyMarkup: ikm);

                    chat.Access_Level = "0.2";
                }
                DB.Chats.Update(chat);
                DB.SaveChanges();
                break; }
    }
}
void SendEmail(string Email, string Code)
{
    MailAddress from = new MailAddress("donauigs.telegrambot@yandex.ru", "Телеграм бот ДонАУиГС");
    MailAddress to = new MailAddress(Email);
    MailMessage m = new MailMessage(from, to);
    m.Subject = "Код для авторизации";
    m.Body = $"<div style=\"border - style: solid; border - width: thin; border - color:#dadce0;border-radius: 8px;padding: 40px 20px;\" align=\"center\" class=\"mdv2rw_mr_css_attr\">" +
        $"<div style=\"font-family: 'Google Sans',Roboto,RobotoDraft,Helvetica,Arial,sans-serif;border-bottom: thin solid #dadce0;color: rgba(0,0,0,0.87);line-height: 32px;padding-bottom: 24px;text-align: center;word-break: break-word;\">" +
        $"<div style=\"font-size: 24px;\">" +
        $"Подтвердите свою почту для входа в чат-бот ДонАУиГС</div>" +
        $"</div>" +
        $"<div style=\"font-family: Roboto-Regular,Helvetica,Arial,sans-serif;font-size: 14px;color: rgba(0,0,0,0.87);line-height: 20px;padding-top: 20px;text-align: left;\">" +
        $"Используйте этот код, чтобы  подтвердить адрес электронной почты указанный в чат-боте:<br>" +
        $"<div style=\"text-align: center;font-size: 36px;margin-top: 20px;line-height: 44px;\">{Code}</div><br>" +
        $"<br>Данный код действует в течении {MaxMinuteWait} минут.<br>"+
        $"Если вы не совершали попытке регистрации в <a href=\"t.me/ItDonampaBot\">чат-боте ДонАУиГС</a>, просто проигнорируйте это письмо.</div></div>";
    m.IsBodyHtml = true;
    SmtpClient smtp = new SmtpClient("smtp.yandex.ru", 587);
    smtp.Credentials = new NetworkCredential("donauigs.telegrambot@yandex.ru", "dlzkbygpcmeilihb");
    smtp.EnableSsl = true;
    smtp.Send(m);
}
async void Bot_OnMessageAsync(object a, MessageEventArgs e)
{
    using var DB = new SQLite();
    Console.WriteLine($"Получено новое сообщение от {e.Message.Chat.FirstName} {e.Message.Chat.LastName}({e.Message.Chat.Username}): {e.Message.Text}");
    var History = new Record
    {
        ChatId = e.Message.Chat.Id,
        Message = e.Message.Text.ToString(),
        DateTime = DateTime.Now.ToString()
    };
    DB.Chats_History.Add(History);
    
    DB.SaveChanges();
    var user = DB.Chats.Where(x=>x.ChatId== e.Message.Chat.Id).SingleOrDefault();

    if(user == null)
    {
        DB.Chats.Add(new Administration.Chat() { ChatId = e.Message.Chat.Id,
            Access_Level = "0.1",
            FirstName = e.Message.Chat.FirstName,
            LastName = e.Message.Chat.LastName,
            UserName = e.Message.Chat.Username });
        DB.SaveChanges();
        var ReplyMarkup = new ReplyKeyboardRemove();
        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Добро пожаловать в чат-бот для поддержки учебного процесса по специальности \"Прикладная информатика\"", replyMarkup: ReplyMarkup);
        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Для использования полного функционала телеграмм бота необходимо авторизироваться в системе.");
        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Введите своей email адрес:");
    }
    else
    {
        var Code = user.Access_Level.Split('.');
        switch (Code[0])
        {
            case "0": { Login(Code, e.Message.Text, e); break;}
            case "1": { break;}
            case "2": { StudentsAsync(e); break;}
            case "-1": { await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Вы заблокированы в данном боте."); break;}
        }
    }
    DB.SaveChanges();
}
async Task StudentsAsync(MessageEventArgs e)
{
    using var DB = new SQLite();

    var Chat = DB.Chats.Where(x => x.ChatId == e.Message.Chat.Id).SingleOrDefault();

    switch (e.Message.Text)
    {
        case "/about":
            {
                var body = new StringBuilder();
                body.AppendLine(string.Format("<b style=\"text-align:center\">Чат-бот для поддержки учебного процесса по специальности \"Прикладная информатика\"</b>"));
                body.AppendLine(string.Format(""));
                body.AppendLine(string.Format("<b>Сайт ДонАУиГС: </b><a href=\"donampa.ru\">ГОУ ВПО «ДонАУиГС»</a>"));
                body.AppendLine(string.Format("<b>Группа ДонАУиГС в VK: </b><a href=\"vk.com/dsum_dn\">ГОУ ВПО «ДОНАУИГС в VK»</a>"));
                body.AppendLine(string.Format("<b>ДонАУиГС в Instagram: </b><a href=\"instagram.com/donauigs/\">ГОУ ВПО «ДОНАУИГС» в Instagram</a>"));
                body.AppendLine(string.Format(""));
                body.AppendLine(string.Format("<b>Форма для обратной связи: </b><a href=\"https://forms.gle/gEARK6dnam7phLQK9\">Google Форма</a>"));
                await Bot.SendTextMessageAsync(e.Message.Chat.Id, body.ToString(), parseMode: ParseMode.Html, replyMarkup: null);
                break;
            }
        case "/time_table":
            {
                if (Chat == null || Chat.UserId == null)
                {
                    await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Данная функция недоступна, поскольку вы не авторизированны.", parseMode: ParseMode.Html, replyMarkup: null);
                    break;
                }

                var chat = DB.Chats.Where(x => x.ChatId == e.Message.Chat.Id).SingleOrDefault();
                var user = DB.Users.Where(x => x.Id == chat.UserId).SingleOrDefault();
                if (user == null || user.Группа == null)
                {
                    await Bot.SendTextMessageAsync(e.Message.Chat.Id, "В базе данных у вас не указана группа.", parseMode: ParseMode.Html, replyMarkup: null);
                    break;
                }
                var group = DB.Groups.Where(x => x.Название == user.Группа).SingleOrDefault();
                if (group == null || group.Расписание == null)
                    await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Для вашей группы не указано расписание.", parseMode: ParseMode.Html, replyMarkup: null);
                else
                {
                    //var body = new StringBuilder();
                    //body.AppendLine(string.Format($"<b>Расписание для группы </b><a href=\"{group.Расписание}\">\"{group.Название}\"</a>"));
                    //await Bot.SendTextMessageAsync(e.Message.Chat.Id, body.ToString(), parseMode: ParseMode.Html, replyMarkup: null);
                    Bot.SendPhotoAsync(e.Message.Chat.Id, photo: group.Расписание, $"Расписание для группы \"{group.Название}\"");
                }
                break;
            }
        case "/news":
            {
                var News = DB.News_List.Skip(Math.Max(0, DB.News_List.Count() - 3)).ToList();

                for (int i = 2; i >= 0; i--)
                {
                    var body = new StringBuilder();
                    body.AppendLine(string.Format($"<b>{News[i].Title}</b>"));
                    body.AppendLine(string.Format($""));
                    body.AppendLine(string.Format($"{News[i].Body}"));
                    await Bot.SendTextMessageAsync(e.Message.Chat.Id, body.ToString(), parseMode: ParseMode.Html, replyMarkup: null);
                }

                break;
            }
        case "/disciplines":
            {
                var _Chat = DB.Chats.Where(x => x.ChatId == e.Message.Chat.Id).SingleOrDefault();
                var _User = DB.Users.Where(x => x.Id == _Chat.UserId).SingleOrDefault();
                var _GroupId = DB.Groups.Where(x => x.Название == _User.Группа).SingleOrDefault().Id;
                var _DisciplinesId = DB.DisciplinesId.Where(x => x.GroupId == _GroupId).Select(p => p.DisciplineId).ToArray();

                if (_DisciplinesId.Count() == 0)
                    await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Вам не открыт доступ к предметам.");
                else
                {
                    var _Disciplines = DB.Disciplines.Where(o => _DisciplinesId.Contains(o.Id));
                    var Buttons = new List<InlineKeyboardButton[]>();

                    foreach (var x in _Disciplines)
                        Buttons.Add(new[] { new InlineKeyboardButton() { Text = x.Название, CallbackData = "/disciplines|" + x.Id.ToString() } });

                    await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Выберите предмет из списка:", replyMarkup: new InlineKeyboardMarkup(Buttons));
                }
                break;
            }
        case "/tests":
            {
                var _Chat = DB.Chats.Where(x => x.ChatId == e.Message.Chat.Id).SingleOrDefault();
                var _User = DB.Users.Where(x => x.Id == _Chat.UserId).SingleOrDefault();
                var _GroupId = DB.Groups.Where(x => x.Название == _User.Группа).SingleOrDefault().Id;
                var _DisciplinesId = DB.DisciplinesId.Where(x => x.GroupId == _GroupId).Select(p => p.DisciplineId).ToArray();

                if (_DisciplinesId.Count() == 0)
                    await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Вам не открыт доступ к предметам.");
                else
                {
                    var _Disciplines = DB.Disciplines.Where(o => _DisciplinesId.Contains(o.Id));
                    var Buttons = new List<InlineKeyboardButton[]>();

                    foreach (var x in _Disciplines)
                        Buttons.Add(new[] { new InlineKeyboardButton() { Text = x.Название, CallbackData = "/tests|" + x.Id.ToString() } });

                    await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Выберите предмет из списка:", replyMarkup: new InlineKeyboardMarkup(Buttons));
                }
                    break;
            }

        case "/send_document":
            {

                break;
            }
        default:
            {
                await Bot.SendTextMessageAsync(e.Message.Chat.Id, "Неверная команда.");
                await Bot.SendTextMessageAsync(e.Message.Chat.Id, "При повторном возникновении ошибки обратитесь к администратору системы.");
                break;
            }
    

    }
}

async void Bot_OnCallbackQueryAsync(object? sender, CallbackQueryEventArgs e)
{
    var Code = e.CallbackQuery.Data.Split('|');
    using var DB = new SQLite();
    switch (Code[0])
    {
        case "/disciplines":
            {
                var Array = DB.Links.Where(x => x.DisciplineId == int.Parse(Code[1])).ToArray();
                var _DisciplineName = DB.Disciplines.Where(x => x.Id == int.Parse(Code[1])).Single();

                if (Array.Length == 0)
                {
                    Bot.EditMessageTextAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId, $"Для предмета \"{_DisciplineName.Название}\" не добавлены ссылки.", parseMode: ParseMode.Html, replyMarkup: null);
                    break;
                }

                var _Literature = Array.GroupBy(x => x.Тип);
                var body = new StringBuilder();
                body.AppendLine(string.Format($"Предмет: <b>{_DisciplineName.Название}</b>")); 
                body.AppendLine(string.Format($""));

                foreach (var item in _Literature)
                {
                    body.AppendLine(string.Format($"<b>{item.Key}</b>"));
                    foreach (Literature _link in item)
                    {
                        body.AppendLine(string.Format($"<b><a href=\"{_link.Ссылка}\">{_link.Название}</a></b>"));
                        if (_link.Описание != null && _link.Описание != String.Empty)
                            body.AppendLine(string.Format($"{_link.Описание}"));
                        body.AppendLine(string.Format($""));
                    }
                }
                Bot.EditMessageTextAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId, body.ToString(), parseMode: ParseMode.Html, replyMarkup: null);
                break;
            }
        case "/tests":
            {
                var _Tests = DB.Tests.Where(x => x.DisciplineId == int.Parse(Code[1])).ToList();
                var _DisciplineName = DB.Disciplines.Where(x => x.Id == int.Parse(Code[1])).Single();

                if (_Tests.Count==0)
                {
                    Bot.EditMessageTextAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId, $"Для предмета \"{_DisciplineName.Название}\" не добавлены ссылки.", parseMode: ParseMode.Html, replyMarkup: null);
                    break;
                }

                var body = new StringBuilder();
                body.AppendLine(string.Format($"Тесты для предмета: <b>{_DisciplineName.Название}</b>"));
                body.AppendLine(string.Format($""));
                foreach (var test in _Tests)
                {
                    DateTime TestTime = DateTime.ParseExact(test.Время, "HH:mm dd.MM.yyyy", null);
                    if(TestTime<DateTime.Now)
                        body.AppendLine(string.Format($"<a href=\"{test.Ссылка}\">{test.Название}</a>"));
                    else
                        body.AppendLine(string.Format($"{test.Название}, доступ откроется в {test.Время}"));
                }
                Bot.EditMessageTextAsync(e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Message.MessageId, body.ToString(), parseMode: ParseMode.Html, replyMarkup: null);
                break;
            }
        case "/send_document":
            {

                break;
            }
        default:
            {
                await Bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "Неверная команда.");
                await Bot.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "При повторном возникновении ошибки обратитесь к администратору системы.");
                break;
            }
    }
}
string Token = "";

Bot = new TelegramBotClient(Token);
Bot.OnMessage += Bot_OnMessageAsync;
Bot.OnCallbackQuery += Bot_OnCallbackQueryAsync;
Bot.StartReceiving();
Console.WriteLine("Запуск бота...");

while(true)
    Console.ReadLine();

Bot.StopReceiving();
