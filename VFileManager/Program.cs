using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VFileManager
{
    class Program
    {
        #region ---- STRING CONSTANTS ----
        /// <summary>Имя файла для хранения настроек приложения</summary>
        private static string settingsFile = "settings.json";

        /// <summary>Стандартный разделитель между словаме в строке ввода</summary>
        private const char delimiterShort = ' ';
        /// <summary>Разделитель для длинных имен файлов</summary>
        private const char delimiterLong = '"';

        /// <summary>Команды выполняемые приложением</summary>
        enum Commands
        {
            Help,//Вывод справки
            Dir,//Вывод списка каталогов
            Files,//Вывод списка каталогов
            Info,//Вывод информации о файле
            Exit,//Выход из программы
            WrongCommand,//Неправильная комманда
        }

        /// <summary>Словарь тектовых ключей комманд</summary>
        private static readonly Dictionary<string, Commands> commands = new Dictionary<string, Commands>
        {
            { "help", Commands.Help },
            { "dir", Commands.Dir },
            { "files", Commands.Files },
            { "info", Commands.Info },
            { "exit", Commands.Exit },
        };

        /// <summary>Аргументы для комманд</summary>
        enum Arguments
        {
            Page,//номер страницы
            Level,//количество уровней (для вывода дерева каталогов)
            Back,//номер страницы
        }

        /// <summary>Словарь тектовых ключей комманд</summary>
        private static readonly Dictionary<string, Arguments> arguments = new Dictionary<string, Arguments>
        {
            { "-p", Arguments.Page },
            { "-l", Arguments.Level },
            { "..", Arguments.Back },
        };



        #endregion

        #region ---- FIELDS & PROPERTIES ----

        /// <summary>Настройки приложения</summary>
        private static Settings settings = new Settings(settingsFile);
        /// <summary>База текстовых сообщений</summary>
        private static MessagesBase messages = new MessagesBase();
        /// <summary>Вывод на экран</summary>
        private static Output output = new Output(settings, messages);
        /// <summary>Работа с файлами</summary>
        private static FilesHandler filesHandler = new FilesHandler(settings, messages);

        /// <summary>Сюда будет помещаться список каталогов</summary>
        private static List<string> dirList = new List<string>();
        /// <summary>Сюда будет помещаться список файлов</summary>
        private static List<string> fileList = new List<string>();
        /// <summary>Сюда будет помещаться информация о файле/каталоге</summary>
        private static List<string> fileInfo = new List<string>();

        #endregion

        static void Main(string[] args)
        {
            Init();//Инициализация

            //Основной цикл
            bool isExit = false;
            while (!isExit)
            {
                output.PrintMessage(Areas.CommandInfoLine, Messages.EnterCommand);
                List<string> inputWords = CommandInput();
                if(inputWords.Count != 0)
                    switch (checkCommand(inputWords[0]))
                    {
                        case Commands.Help://Вывод справки
                            output.PrintManual();
                            break;

                        case Commands.Dir://Вывод списка каталогов
                            DirList(inputWords);
                            break;

                        case Commands.Files://Вывод списка файлов
                            FileList(inputWords);
                            break;

                        case Commands.Info://Вывод списка файлов
                            Info(inputWords);
                            break;

                        case Commands.Exit://Выход
                            isExit = true;
                            break;

                        case Commands.WrongCommand://Неправильная команда
                            output.PrintMessage(Areas.CommandInfoLine, Messages.WrongCommand);
                            Console.ReadKey();
                            break;
                    }
            }
        }

        #region ---- INIT ----

        /// <summary>
        /// Инициализация приложения
        /// </summary>
        /// <param name="filename">Имя файла с настройками</param>
        private static void Init()
        {
            settings.LoadSettings();

            //Устанавливаем размер консольного окна и буфера
            Console.SetWindowSize(settings.AppWidth + 1, settings.AppHeight + 1);
            Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);

            Console.Clear();
            output.PrintMainFrame();

            //Вывод дерева последнего просматриваемого каталога
            filesHandler.SeekDirectoryRecursion(settings.LastPath, dirList);
            output.PrintList(Areas.DirList, dirList, 1, false);

            //Вывод содержимого последнего просматриваемого каталога
            filesHandler.SeekDirectoryForFiles(settings.LastPath, fileList);
            output.PrintList(Areas.FileList, fileList, 1, false);

            //Вывод информации о последнем просматриваемом каталоге
            filesHandler.GetInfo(settings.LastPath, fileInfo);
            output.PrintList(Areas.Info, fileInfo, 1, false);
        }

        #endregion

        #region ---- WORK WITH INPUT ----

        /// <summary>Принимает ввод пользователя и разбивает его на части</summary>
        /// <returns>Список содержащий комманду пользователя и аргументы</returns>
        private static List<string> CommandInput()
        {
            output.PrintMessage(Areas.CommandLine, Messages.CommandSymbol);
            string input = Console.ReadLine();
            List<string> inputWords = new List<string>();//Список для комманд и аргументов
            StringBuilder word = new StringBuilder();//Буфер для символов комманд
            bool isQuoteOpened = false;//Флаг того, что были пройдены открывающие кавычки
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] != delimiterShort && input[i] != delimiterLong)//Если текущий символ не разделитель
                {
                    word.Append(input[i]);//Записываем его в буфер
                    if (i == input.Length - 1)//Если это был конец строки то записываем буфер в список
                        inputWords.Add(word.ToString());
                }
                else if (input[i] == delimiterLong)//Если текущий символ разделитель для пути с пробелами (кавычки)
                {
                    if(isQuoteOpened)//Если открывающие кавычки уже были пройдены
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
                    if(isQuoteOpened)//Есди сейчас анализируется слово в кавычках, то пробел тоже идет в буфер
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

        /// <summary>
        /// Проверяет какую команду содержит строка символов
        /// </summary>
        /// <param name="input">Строка символов для проверки</param>
        /// <returns>Комманда</returns>
        private static Commands checkCommand(string input)
        {
            Commands command = Commands.WrongCommand;
            if (commands.ContainsKey(input))
                command = commands[input];
            return command;
        }

        /// <summary>
        /// Проверяет список слов на наличие заданного аргумента и возвращает его значение
        /// </summary>
        /// <param name="inputWords">Список слов для анализа</param>
        /// <param name="argument">Искомый аргумент</param>
        /// <returns>Число следующее за аргументом или 0 если аргумент не найден</returns>
        private static int findArguments(List<string> inputWords, Arguments argument)
        {
            int parameter = 0;//Возвращаемый параметр
            int index = 1;//Индекс проверяемого слова
            bool isFound = false;
            while (!isFound && index < inputWords.Count - 1)
            {
                if(arguments.ContainsKey(inputWords[index]))//Если проверяемое слово присутствует в списке возможных аргументов...
                {
                    if(arguments[inputWords[index]] == argument)//Если это нужный нам аргумент...
                    {
                        int.TryParse(inputWords[index + 1], out parameter);//То проверяем следующее слово на числовое значение
                        isFound = true;//Завершаем поиск
                    }
                }
                index++;
            }

            return parameter;
        }

        /// <summary>
        /// Ищет путь к файлу или каталогу в списке слов
        /// </summary>
        /// <param name="words">Список слов</param>
        /// <param name="number">Порядковый номер слова в списке которое надо проанализировать на наличие пути</param>
        /// <returns>Строку с найденым путем или пустую строку если путь не найден</returns>
        private static string FindPath(List<string> words, int number)
        {
            string path = null;//Здесь будет найденный путь

            if(number < words.Count) //Есть ли в списке слово под нужным индексом
            {
                if(filesHandler.IsPathValid(words[number]))//Проверяем на отсутвие запрещенных символов
                {
                    path = words[number];
                }
            }

            return path;
        }

        /// <summary>
        /// Собирает из относительного и полного пути (корневого каталога) один полный
        /// </summary>
        /// <param name="rootPath">Путь к корневому каталогу</param>
        /// <param name="path">Относительный путь к каталогу/файлу</param>
        /// <returns>Полный путь к файлу/каталогу</returns>
        private static string MakeFullPath(string rootPath, string path)
        {
            if (path == null)
                return rootPath;
            else if (path == "..")
                return (Path.GetDirectoryName(rootPath));
            else
                return (Path.Combine(rootPath, path));
        }

        #endregion

        #region --- COMMANDS ----

        /// <summary>Вывод дерева каталогов</summary>
        /// <param name="inputWords">Список содержащий слова из ввода пользователя</param>
        private static void DirList(List<string> inputWords)
        {
            int page = findArguments(inputWords, Arguments.Page);//Номер страницы с которой начинается вывод
            int maxLevel = findArguments(inputWords, Arguments.Level);//Глубина сканирования каталогов
            string path = FindPath(inputWords, 1);//Каталог который сканируем
            string fullPath = MakeFullPath(settings.LastPath, path);//Преобразуем путь к нему в асолютный (если необходимо)

            if(!filesHandler.IsDirExist(fullPath) && arguments.ContainsKey(path))//На тот случай если один из аргументов был принят за путь к каталогу
            {
                fullPath = settings.LastPath;//То устанавливаем предыдущий путь
            }

            if (filesHandler.IsDirExist(fullPath))//Если путь существует, то сканируем его и выводим на экран
            {
                dirList.Clear();
                filesHandler.SeekDirectoryRecursion(fullPath, dirList, maxLevel);
                output.PrintList(Areas.DirList, dirList, page);
                settings.LastPath = fullPath;
                settings.SaveSettings();
            }
            else
            {
                output.PrintMessage(Areas.CommandInfoLine, Messages.WrongPath);
                Console.ReadKey();
            }
        }

        /// <summary>Вывод списка файлов</summary>
        /// <param name="inputWords">Список содержащий слова из ввода пользователя</param>
        private static void FileList(List<string> inputWords)
        {
            int page = findArguments(inputWords, Arguments.Page);//Номер страницы с которой начинается вывод
            string path = FindPath(inputWords, 1);//Каталог который сканируем
            string fullPath = MakeFullPath(settings.LastPath, path);//Преобразуем путь к нему в асолютный (если необходимо)

            if (!filesHandler.IsDirExist(fullPath) && arguments.ContainsKey(path))//На тот случай если один из аргументов был принят за путь к каталогу
            {
                fullPath = settings.LastPath;//То устанавливаем предыдущий путь
            }

            if (filesHandler.IsDirExist(fullPath))
            {
                fileList.Clear();
                filesHandler.SeekDirectoryForFiles(fullPath, fileList);
                output.PrintList(Areas.FileList, fileList, page);
            }
            else
            {
                output.PrintMessage(Areas.CommandInfoLine, Messages.WrongPath);
                Console.ReadKey();
            }
        }

        private static void Info(List<string> inputWords)
        {
            string path = FindPath(inputWords, 1);//Каталог который сканируем
            string fullPath = MakeFullPath(settings.LastPath, path);//Преобразуем путь к нему в асолютный (если необходимо)

            if (!filesHandler.IsDirExist(fullPath) && arguments.ContainsKey(path))//На тот случай если один из аргументов был принят за путь к каталогу
            {
                fullPath = settings.LastPath;//То устанавливаем предыдущий путь
            }

            fileInfo.Clear();
            filesHandler.GetInfo(fullPath, fileInfo);
            if(fileInfo.Count != 0)
            {
                output.PrintList(Areas.Info, fileInfo);
            }
            else
            {
                output.PrintMessage(Areas.CommandInfoLine, Messages.WrongPath);
                Console.ReadKey();
            }

        }



        #endregion
    }
}
