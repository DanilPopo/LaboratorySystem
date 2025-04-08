using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Mail_LIB
{
    public class Validator
    {
        // проверка почты
        public (bool isValid, string message) CheckMail(string mail)
        {
            if (string.IsNullOrEmpty(mail))
                return (false, "Email не может быть пустым.");

            if (!Regex.IsMatch(mail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return (false, "Некорректный формат электронной почты.");

            return (true, "Email корректен.");
        }

        //проверка пароля
        public (bool isValid, string message) CheckPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return (false, "Пароль не может быть пустым.");

            if (password.Length < 8)
                return (false, "Пароль должен содержать минимум 8 символов.");

            if (!Regex.IsMatch(password, @"[A-Za-z]"))
                return (false, "Пароль должен содержать хотя бы одну букву.");

            if (!Regex.IsMatch(password, @"[0-9]"))
                return (false, "Пароль должен содержать хотя бы одну цифру.");

            if (!Regex.IsMatch(password, @"[\W_]"))
                return (false, "Пароль должен содержать хотя бы один специальный символ.");

            return (true, "Пароль корректен.");
        }

        // проверка логина
        public (bool isValid, string message) CheckLogin(string login)
        {
            if (string.IsNullOrEmpty(login))
                return (false, "Логин не может быть пустым.");

            if (login.Length < 6)
                return (false, "Логин должен содержать минимум 6 символов.");

            if (!Regex.IsMatch(login, @"^[a-zA-Z0-9]+$"))
                return (false, "Логин может содержать только латинские буквы и цифры.");

            return (true, "Логин корректен.");
        }
    }
}
