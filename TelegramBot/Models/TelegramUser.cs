using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    public class TelegramUser : IEquatable<TelegramUser>
    {
        public string FirstName { get; private set; }

        public long ChatID { get; private set; }

        public List<string> Messages { get; private set; }

        public TelegramUser(string firstName, long chatID)
        {
            FirstName = firstName;
            ChatID = chatID;
            Messages = new List<string>();
        }

        /// <summary>
        /// Метод сравнения двух пользователей
        /// </summary>
        /// <param name="other">Сравниваемый пользователь</param>
        /// <returns></returns>
        public bool Equals(TelegramUser? other) => other?.ChatID == this.ChatID;

        /// <summary>
        /// Метод добавления сообщения
        /// </summary>
        /// <param name="text">Текст сообщения</param>
        public void AddMessage(string text) => Messages.Add(text);
    }
}
