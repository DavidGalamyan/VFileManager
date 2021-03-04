using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFileManager
{
    /// <summary>Ключи словаря сообщений</summary>
    enum Messages
    {
        AppName,
        EnterCommand,
        WrongCommand,
        WrongPath,
        CommandSymbol,
        ListMessage,
    }

    class MessagesBase
    {
        #region ---- STRING CONSTANTS ----

        /// <summary>Словарь сообщений</summary>
        private readonly Dictionary<Messages, string> messages = new Dictionary<Messages, string>
        {
            { Messages.AppName, " VFileManager " },
            { Messages.EnterCommand, "Введите комманду. (help - для списка комманд)" },
            { Messages.WrongCommand, "Неправильная команда. Повторите ввод." },
            { Messages.WrongPath, "Неправильный путь. Повторите ввод." },
            { Messages.CommandSymbol, ":>" },
            { Messages.ListMessage, "pageUp/pageDown (or arrows) - change pages. Q/Esc - stop." },
        };

        /// <summary>Справка по коммандам
        /// 0 элемент - команда
        /// 1 элемент - параметр/параметры
        /// 2 элемент - описание</summary>
        private readonly string[,] manual = new string[,]
        {
            { "", "", "Список комманд:" },
            { "help ", "", "- вывод справки" },
            { "dirs ", "[<path>] [-p <int>] [-l <int>] ", "- вывод списка каталогов" },
            { "", "     path ", "- путь к выводимому каталогу" },
            { "", "     -p <int> ", "- номер страницы, default=1" },
            { "", "     -l <int> ", "- количество уровней каталогов, default=2" },
            { "files ", "[<path>] [-p <int>]", "- вывод списка файлов" },
            { "", "      path ", "- путь к каталогу из файлов, если не указан то будет использован корень из списка каталогов" },
            { "", "      -p <int> ", "- номер страницы, default=1" },
            { "exit ", "", "- выход из программы" },
        };

        #endregion

        #region ---- INDEXER ----
        public string this[Messages key]
        {
            get
            {
                return messages[key];
            }
        }

        #endregion

        #region ---- METHODS ----

        /// <summary>Проверяет есть ли заданное сообщение в базе</summary>
        /// <param name="key">Идентификатор сообщения</param>
        /// <returns>true, если сообщение есть в базе</returns>
        public bool ContainsKey(Messages key)
        {
            return messages.ContainsKey(key) ? true : false;
        }

        #endregion
    }
}
