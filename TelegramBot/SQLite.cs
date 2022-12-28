using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Administration
{
    public class User
    {
        public int Id { get; set; }
        public string Фамилия { get; set; }
        public string Имя { get; set; }
        public string Отчество { get; set; }
        public string Статус { get; set; }
        public string Email { get; set; }
        public string? Группа { get; set; }
        public User()
        {
            Фамилия = Имя = Отчество = Статус = Email = Группа = string.Empty;
        }
    }
    public class Group
    {
        public int Id { get; set; }
        public string Название { get; set; }
        public int? IdСтаросты { get; set; }
        public int? IdКуратора { get; set; }
        public string? Расписание { get; set; }
    }
    public class Chat
    {
        public long ChatId { get; set; }
        public int? UserId { get; set; }
        public string Access_Level { get; set; }
        public string? UserName { get; set; }
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Temporary_Email { get; set; }
        public int? Access_code { get; set; }
        public string? DateTimeToSend { get; set; }
    }
    public class Record
    {
        public long ChatId { get; set; }
        public string DateTime { get; set; }
        public string Message { get; set; }
    }
    public class Discipline
    {
        public int Id { get; set; }
        public string Название { get; set; }

    }
    public class Access
    {
        public int Id { get; set; }
        public int DisciplineId { get; set; }
        public int GroupId { get; set; }
        public int TeacherId { get; set; }
    }
    public class Literature
    {
        public int Id { get; set; }
        public int DisciplineId { get; set; }
        public string Название { get; set; }
        public string? Описание { get; set; }
        public string? Ссылка { get; set; }
        public string Тип { get; set; }

    }
    public class Test
    {
        public int Id { get; set; }
        public string Название { get; set; }
        public int DisciplineId { get; set; }
        public string Время { get; set; }
        public string? Ссылка { get; set; }
    }
    public class News
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
    public class SQLite : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Record> Chats_History { get; set; }
        public DbSet<Discipline> Disciplines { get; set; }
        public DbSet<Access> Disciplines_Access { get; set; }
        public DbSet<Literature> Links { get; set; }
        public DbSet<News> NewsList { get; set; }
        public DbSet<Test> Tests { get; set; }

        public SQLite()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            /*C:\Users\Леонид\YandexDisk\Проекты\C#\Дипломы\TelegramBot\TelegramBot\bin\Debug\net6.0-windows\*/
            optionsBuilder.UseSqlite(@"Data Source=Information.db;"); 
            optionsBuilder.LogTo(message => System.Diagnostics.Debug.WriteLine(message));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Group>().ToTable("Groups");
            modelBuilder.Entity<Chat>().ToTable("Chats");
            modelBuilder.Entity<Record>().HasNoKey();
            modelBuilder.Entity<Discipline>().ToTable("Disciplines");
            modelBuilder.Entity<Access>().ToTable("Disciplines_Access");
            modelBuilder.Entity<Literature>().ToTable("Literature");
            modelBuilder.Entity<News>().ToTable("News");
            modelBuilder.Entity<Test>().ToTable("Tests");
        }
    }
}
