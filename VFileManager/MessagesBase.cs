﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFileManager
{
    #region ------ PUBLIC ENUMS ------

    /// <summary>Ключи словаря сообщений</summary>
    public enum Messages
    {
        AppName,
        EnterCommand,
        WrongCommand,
        WrongArguments,
        WrongPath,
        WrongSourcePath,
        WrongDestPath,
        FileExist,
        DirExist,
        PathNotExist,
        CommandSymbol,
        ListMessage,
        FileInfo,
        DirInfo,
        CreationDate,
        Attributes,
        Size,
        Bytes,
        Success,
        UnSuccess,
        Version
    }

    #endregion

    /// <summary>
    /// Класс содержит список текстовых сообщений применяемых в программе
    /// </summary>
    class MessagesBase
    {
        #region ---- TEXTS ----

        /// <summary>Словарь сообщений</summary>
        private Dictionary<Messages, string> messages = new Dictionary<Messages, string>
        {
            { Messages.AppName, " VFileManager " },
            { Messages.EnterCommand, "Введите комманду. (help - для списка комманд)" },
            { Messages.WrongCommand, "Неправильная команда. Повторите ввод." },
            { Messages.WrongArguments, "Неправильные аргументы. Повторите ввод." },
            { Messages.WrongPath, "Неправильный путь. Повторите ввод." },
            { Messages.WrongSourcePath, "Неправильный путь источника. Повторите ввод." },
            { Messages.WrongDestPath, "Неправильный путь к цели. Повторите ввод." },
            { Messages.FileExist, "Файл уже существует." },
            { Messages.DirExist, "Каталог уже существует." },
            { Messages.PathNotExist, "Путь не существует." },
            { Messages.CommandSymbol, ":>" },
            { Messages.ListMessage, "pageUp/pageDown (or arrows) - change pages. Q/Esc - stop." },
            { Messages.FileInfo, "Информация о файле: " },
            { Messages.DirInfo, "Информация о каталоге: " },
            { Messages.CreationDate, "?Дата создания: *" },
            { Messages.Attributes, "?Аттрибуты: *" },
            { Messages.Size, "?Размер: *" },
            { Messages.Bytes, " ?Bytes" },
            { Messages.Success, "Успешно." },
            { Messages.UnSuccess, "Что-то пошло не так." },
            { Messages.Version, "Версия приложения: " },
        };

        /// <summary>Справка по коммандам
        /// Подсвечивающие строку символы:
        /// '?' - цвет комманды
        /// '|' - цвет аргумента
        /// '*' - стандартный цвет текста</summary>
        private List<string> manual = new List<string>
        {
            { "Список комманд:" },
            { string.Empty },
            { "?help    |*- вывод справки" },
            { "?exit    |*- выход из программы" },
            { string.Empty },
            { "?dir     |[<path>] [-p <int>] [-l <int>] *- вывод списка каталогов" },
            { "?        |path *- путь к выводимому каталогу; |.. *- возврат на уровень назад" },
            { "?        |-p <int> *- номер страницы, default=1" },
            { "?        |-l <int> *- количество уровней каталогов, default=2" },
            { "?files   |[<path>] [-p <int>] *- вывод списка файлов" },
            { "?        |path *- путь к каталогу из файлов, если не указан то будет использован корень из списка каталогов" },
            { "?        |-p <int> *- номер страницы, default=1" },
            { "?info    |[<path>] *- вывод информации о каталоге/файле" },
            { "?        |path *- путь к каталогу/файлу информацию о котором необходимо вывести на экран" },
            { string.Empty },
            { "?copy    |<file name>, <destination dir> *- копирование файла" },
            { "?move    |<file name>, <destination dir> *- перемещение файла" },
            { "?del     |<file name>*- удалить файл" },
            { "?        |file name *- путь к файлу который нужно скопировать/переместить или удалить" },
            { "?        |destination dir *- путь к каталогу куда нужно скопировать или переместить указанный файл" },
            { string.Empty },
            { "?dcopy   |<dir name>, <destination dir> *- копирование файла" },
            { "?dmove   |<dir name>, <destination dir> *- перемещение файла" },
            { "?ddel    |<dir name> *- удалить файл" },
            { "?        |dir name *- путь к каталогу который нужно скопировать/переместить или удалить" },
            { "?        |destination dir *- путь к каталогу куда нужно скопировать или переместить указанный объект" },
            { string.Empty },
            { "?        *Имена файлов/каталогов могут задаваться как абсолютно, так и относительно текущего каталога." },
            { "?        *Имена файлов/каталогов содержащие пробелы, должны заключаться в двойные кавычки (\")" },
            { string.Empty },
            { "?version |*- вывод информации о версии приложения" },
        };

        #endregion

        #region ---- INDEXER ----

        /// <summary>
        /// Индексатор для доступа к значениям словаря по ключу
        /// </summary>
        /// <param name="key">Ключ словаря, по которому надо извлеч значение</param>
        /// <returns>Значение из словаря соответствующее ключу</returns>
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

        /// <summary>Возвращает Список содержащий справку</summary>
        /// <returns>Список содержащий справку по командам</returns>
        public List<string> GetManual()
        {
            return manual;
        }

        #endregion
    }
}
