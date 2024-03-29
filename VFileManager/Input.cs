﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFileManager
{
    /// <summary>
    /// Класс обрабатывает ввод пользователя
    /// </summary>
    class Input
    {
        #region ---- STRING CONSTANTS ----

        /// <summary>Стандартный разделитель между словаме в строке ввода</summary>
        private const char delimiterStandart = ' ';
        /// <summary>Разделитель для имен файлов/каталогов с пробелами</summary>
        private const char delimiterLong = '"';

        #endregion

        #region ---- FIELDS ----

        /// <summary>Настройки приложения</summary>
        Settings settings;
        /// <summary>База текстовых сообщений</summary>
        MessagesBase messages;
        /// <summary>Вывод на экран</summary>
        Output output;

        #endregion

        #region ---- CONSTRUCTOR ----

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="settings">Настройки приложения</param>
        /// <param name="output">Вывод на экран</param>
        public Input(Settings settings, Output output)
        {
            this.settings = settings;
            this.messages = settings.Messages;
            this.output = output;
        }


        #endregion

        #region ---- METHODS ----

        /// <summary>Принимает ввод пользователя и разбивает его на части</summary>
        /// <returns>Список содержащий комманду пользователя и аргументы</returns>
        public List<string> CommandInput()
        {
            output.PrintMessage(Areas.CommandLine, Messages.CommandSymbol);
            string input = Console.ReadLine();
            List<string> inputWords = new List<string>();//Список для комманд и аргументов
            StringBuilder word = new StringBuilder();//Буфер для символов комманд
            bool isQuoteOpened = false;//Флаг того, что были пройдены открывающие кавычки
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] != delimiterStandart && input[i] != delimiterLong)//Если текущий символ не разделитель
                {
                    word.Append(input[i]);//Записываем его в буфер
                    if (i == input.Length - 1)//Если это был конец строки то записываем буфер в список
                        inputWords.Add(word.ToString());
                }
                else if (input[i] == delimiterLong)//Если текущий символ разделитель для пути с пробелами (кавычки)
                {
                    if (isQuoteOpened)//Если открывающие кавычки уже были пройдены
                    {
                        //Записываем буфер в список и очищаем
                        inputWords.Add(word.ToString());
                        word.Clear();
                        //Сбрасываем флаг, что были пройдены окрывающие кавычки
                        isQuoteOpened = false;
                    }
                    else//Если открывающие кавычки не были пройдены
                    {
                        //Устанавливаем флаг, что были пройдены окрывающие кавычки
                        isQuoteOpened = true;
                    }
                }
                else//Если это обычный разделитель (пробел)
                {
                    if (isQuoteOpened)//Есди сейчас анализируется слово в кавычках, то пробел тоже идет в буфер
                        word.Append(input[i]);
                    else if (word.Length != 0)//Если нет, то записываем буфер в список (если он не пустой)
                    {
                        inputWords.Add(word.ToString());
                        word.Clear();
                    }
                }
            }
            return inputWords;
        }


        #endregion
    }
}
