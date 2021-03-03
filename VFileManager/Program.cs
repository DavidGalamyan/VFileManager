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
            Dirs,//Вывод списка каталогов
            Files,//Вывод списка каталогов
            Exit,//Выход из программы
            WrongCommand,//Неправильная комманда
        }

        /// <summary>Словарь тектовых ключей комманд</summary>
        private static readonly Dictionary<string, Commands> commands = new Dictionary<string, Commands>
        {
            { "help", Commands.Help },
            { "dirs", Commands.Dirs },
            { "files", Commands.Files },
            { "exit", Commands.Exit },
        };

        /// <summary>Аргументы для комманд</summary>
        enum Arguments
        {
            Page,//номер страницы
            Level,//количество уровней (для вывода дерева каталогов)
        }

        /// <summary>Словарь тектовых ключей комманд</summary>
        private static readonly Dictionary<string, Arguments> arguments = new Dictionary<string, Arguments>
        {
            { "-p", Arguments.Page },
            { "-l", Arguments.Level },
        };


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

        /// <summary>Словарь сообщений</summary>
        private static readonly Dictionary<Messages, string> messages = new Dictionary<Messages, string>
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
        private static readonly string[,] manual = new string[,]
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

        #region ---- FIELDS & PROPERTIES ----

        private static Settings settings = new Settings(settingsFile);

        /// <summary>Сюда будет помещаться список каталогов</summary>
        private static List<string> dirList = new List<string>();
        /// <summary>Сюда будет помещаться список файлов</summary>
        private static List<string> fileList = new List<string>();

        #endregion


        #region ---- NUMERIC CONSTANTS ----

        /// <summary>Цвета для различных элементов интерфейса</summary>
        enum Colors
        {
            Frame = (int)ConsoleColor.Green,
            Standart = (int)ConsoleColor.Gray,
            Command = (int)ConsoleColor.Yellow,
            Argument = (int)ConsoleColor.DarkYellow,
            Background = (int)ConsoleColor.Black,
        }

        /// <summary>Экранные области приложения</summary>
        enum Areas
        {
            DirList,
            FileList,
            Info,
            CommanLine,
        }

        #endregion

        static void Main(string[] args)
        {
            Init();//Инициализация

            Console.Clear();
            PrintMainFrame();
            PrintMessage(Areas.Info, Messages.EnterCommand);

            //Основной цикл
            bool isExit = false;
            while (!isExit)
            {
                PrintMessage(Areas.Info, Messages.EnterCommand);
                List<string> inputWords = CommandInput();
                if(inputWords.Count != 0)
                    switch (checkCommand(inputWords[0]))
                    {
                        case Commands.Help://Вывод справки
                            PrintManual();
                            break;

                        case Commands.Dirs://Вывод списка каталогов
                            int page = findArguments(inputWords, Arguments.Page);//Номер страницы с которой начинается вывод
                            int maxLevel = findArguments(inputWords, Arguments.Level);//Глубина сканирования каталогов
                            string path = FindPath(inputWords, 1);//Каталог который сканируем
                            if (path == string.Empty) path = settings.LastPath;//Если каталог не задан, то используем последний каталог
                            if(IsPathExist(path))
                            {
                                dirList.Clear();
                                SeekDirectoryRecursion(path, maxLevel) ;
                                PrintDirList(dirList, page);
                                settings.LastPath = path;
                                settings.SaveSettings();
                            }
                            else
                            {
                                PrintMessage(Areas.Info, Messages.WrongPath);
                                Console.ReadKey();
                            }
                            break;

                        case Commands.Files://Вывод списка файлов
                            page = findArguments(inputWords, Arguments.Page);//Номер страницы с которой начинается вывод
                            path = FindPath(inputWords, 1);//Каталог который сканируем
                            if (path == string.Empty)
                            {
                                if (dirList.Count != 0)
                                    path = dirList[0];
                                else
                                    path = settings.LastPath;
                            }

                            if (IsPathExist(path))
                            {
                                fileList.Clear();
                                SeekDirectoryForFiles(path);
                                PrintFileList(fileList, page);
                            }
                            else
                            {
                                PrintMessage(Areas.Info, Messages.WrongPath);
                                Console.ReadKey();
                            }

                            break;

                        case Commands.Exit://Выход
                            isExit = true;
                            break;

                        case Commands.WrongCommand://Неправильная команда
                            PrintMessage(Areas.Info, Messages.WrongCommand);
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
            //Console.SetBufferSize(settings[SettingsKeys.AppWidth] + 1, settings[SettingsKeys.AppHeight] + 1);

        }

        #endregion

        #region ---- WORK WITH INPUT ----

        /// <summary>Принимает ввод пользователя и разбивает его на части</summary>
        /// <returns>Список содержащий комманду пользователя и аргументы</returns>
        private static List<string> CommandInput()
        {
            PrintMessage(Areas.CommanLine, Messages.CommandSymbol);
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
        /// <param name="inputWords">Список слов</param>
        /// <param name="number">Порядковый номер пути который нужно вернуть</param>
        /// <returns>Строку с найденым путем или пустую строку если путь не найден</returns>
        private static string FindPath(List<string> inputWords, int number)
        {
            string path = string.Empty;//Здесь будет найденный путь
            int index = 1;//Индекс проверяемого слова
            int currentNumber = 0;//Текущий номер пути в списке аргументов
            bool isFound = false;
            while (!isFound && index < inputWords.Count)
            {
                if (!int.TryParse(inputWords[index], out _))//Если очередной аргумент не число...
                {
                    if(IsPathValid(inputWords[index]))//Если путь валидный...
                    {
                        currentNumber++;
                        if (currentNumber == number)//Если это нужный нам путь по порядку, то заканчиваем поиск
                        {
                            path = inputWords[index];
                            isFound = true;//Завершаем поиск
                        }
                    }
                }
                index++;
            }
            return path;
        }

        /// <summary>
        /// Проверяет на правильность заданный путь к файлу
        /// </summary>
        /// <param name="dirName">Путь к файлу</param>
        /// <returns>true, если путь правильный</returns>
        private static bool IsPathValid(string path)
        {
            return (path != null) && (path.IndexOfAny(Path.GetInvalidPathChars()) == -1);
        }

        /// <summary>
        /// Проверяет существует ли указанный файл
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <returns>true, если файл существует</returns>
        private static bool IsFileExist(string fileName)
        {
            bool isExist = false;
            //Если имя файла не пустое и не содержит недопустимых символов
            if (IsPathValid(fileName))
                try
                {
                    FileInfo tempFileInfo = new FileInfo(fileName);
                    isExist = true;
                }
                catch (NotSupportedException)
                {
                    isExist = false;
                }
            return isExist;
        }

        /// <summary>
        /// Проверяет существует ли указанный каталог
        /// </summary>
        /// <param name="dirName">Имя каталога</param>
        /// <returns>true, если каталог существует</returns>
        private static bool IsPathExist(string dirName)
        {
            bool isExist = false;
            //Если имя файла не пустое и не содержит недопустимых символов
            if (IsPathValid(dirName))
                if (Directory.Exists(dirName))
                    isExist = true;
            return isExist;
        }


        #endregion

        #region ---- PRINT METHODS ----

        /// <summary>Очищает заданную область интерфейса</summary>
        /// <param name="area">Очищаемая область</param>
        private static void ClearArea(Areas area)
        {
            int firstRow = 0;
            int lastRow = 0;
            switch(area)
            {
                case Areas.DirList:
                    firstRow = settings.DirListAreaLine;
                    lastRow = settings.FileListAreaLine - 1;
                    break;
                case Areas.FileList:
                    firstRow = settings.FileListAreaLine;
                    lastRow = settings.InfoAreaLine - 1;
                    break;
                case Areas.Info:
                    firstRow = settings.InfoAreaLine;
                    lastRow = settings.CommandAreaLine - 1;
                    break;
                case Areas.CommanLine:
                    firstRow = settings.CommandAreaLine;
                    lastRow = settings.AppHeight - 1;
                    break;
            }

            for (int i = firstRow; i < lastRow; i++)
                ClearLine(1, i, settings.AppWidth - 2);

            //очистка информации о странице
            Console.SetCursorPosition(0, lastRow);
            Console.ForegroundColor = (ConsoleColor)Colors.Frame;
            PrintFrameLine((lastRow == settings.AppHeight - 1) ? "╚═╝" : "╠═╣");
            Console.ForegroundColor = (ConsoleColor)Colors.Standart;

        }

        /// <summary>Очищает в консоли указанную строку</summary>
        /// <param name="column">Номер столбца с которого нужно очистить строку</param>
        /// <param name="row">Номер строки которую нужно очистить</param>
        /// <param name="length">Длина строки которую нужно очистить</param>
        private static void ClearLine(int column, int row, int length)
        {
            Console.SetCursorPosition(column, row);
            for (int i = 0; i < length; i++)
            {
                if(i<= settings.AppWidth)
                    Console.Write(" ");
            }
        }

        /// <summary>Выводит сообщение с заданной области</summary>
        /// <param name="area">ОБласть экрана в которой нужно вывести сообщение</param>
        /// <param name="message">Ключ в словаре сообщений</param>
        private static void PrintMessage(Areas area, Messages message)
        {
            if(messages.ContainsKey(message))
            {
                ClearArea(area);
                int row = 1;
                switch (area)
                {
                    case Areas.DirList:
                        row = settings.DirListAreaLine;
                        break;
                    case Areas.FileList:
                        row = settings.FileListAreaLine;
                        break;
                    case Areas.Info:
                        row = settings.InfoAreaLine;
                        break;
                    case Areas.CommanLine:
                        row = settings.CommandAreaLine;
                        break;
                }
                Console.SetCursorPosition(1, row);
                Console.Write(messages[message]);
            }
        }

        /// <summary>Выводит на экран справку по коммандам</summary>
        private static void PrintManual()
        {
            ClearArea(Areas.DirList);
            for (int i = 0; i < manual.GetLength(0); i++)
            {
                Console.SetCursorPosition(1, settings.DirListAreaLine + i);
                Console.ForegroundColor = (ConsoleColor)Colors.Command;
                Console.Write(manual[i, 0]);
                Console.ForegroundColor = (ConsoleColor)Colors.Argument;
                Console.Write(manual[i, 1]);
                Console.ForegroundColor = (ConsoleColor)Colors.Standart;
                Console.Write(manual[i, 2]);
            }
        }


        /// <summary>Выводит на экран рамку приложения</summary>
        private static void PrintMainFrame()
        {
            //╔ ═ ╗ ╚ ║ ╝ ╠ ╣ ╦ ╩ ╬ █ - символы для рамки

            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = (ConsoleColor)Colors.Frame;
            //верхняя строка            
            PrintFrameLine("╔═╗");
            //область вывода списка каталогов
            for (int r = settings.DirListAreaLine; r < settings.FileListAreaLine - 1; r++)
                PrintFrameLine("║ ║");
            //разделитель областей
            PrintFrameLine("╠═╣");
            //область вывода списка файлов
            for (int r = settings.FileListAreaLine; r < settings.InfoAreaLine - 1; r++)
                PrintFrameLine("║ ║");
            //разделитель областей
            PrintFrameLine("╠═╣");
            //область вывода информации
            for (int r = settings.InfoAreaLine; r < settings.CommandAreaLine - 1; r++)
                PrintFrameLine("║ ║");
            //разделитель областей
            PrintFrameLine("╠═╣");
            //Область ввода комманд                        
            for (int r = settings.CommandAreaLine; r < settings.AppHeight - 1; r++)
                PrintFrameLine("║ ║");
            //нижняя строка
            PrintFrameLine("╚═╝");
            //название приложения
            Console.SetCursorPosition(2, 0);
            Console.WriteLine(messages[Messages.AppName]);
            Console.ForegroundColor = (ConsoleColor)Colors.Standart;
        }

        /// <summary>Выводит на экран одну строку рамки приложения</summary>
        /// <param name="lineSymbols">Строка из трех символов которые составляют строку рамки приложения</param>
        private static void PrintFrameLine(string lineSymbols)
        {
            if(lineSymbols.Length == 3)
            {
                Console.Write(lineSymbols[0]);
                for (int c = 1; c < settings.AppWidth - 1; c++)
                {
                    Console.Write(lineSymbols[1]);
                }
                Console.Write(lineSymbols[2]);
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Выводит на экран список файлов
        /// </summary>
        /// <param name="dirList">Список файлов для вывода на экран</param>
        /// <param name="lines">Количество поизций одновременно выводимых на экран</param>
        /// <param name="page">Номер страницы которую необходимо вывести на экран</param>
        private static void PrintDirList(List<string> dirList, int page = 1 )
        {
            ClearArea(Areas.DirList);
            PrintMessage(Areas.Info, Messages.ListMessage);


            int lines = settings.FileListAreaLine - settings.DirListAreaLine - 2;//Количество линий списка выводимых на экран за один раз
            int pages = (dirList.Count - 1) / lines;//Количество страниц в списке
            if (pages * lines < (dirList.Count - 1)) pages++;

            bool isExit = false;
            while(!isExit)
            {
                Console.BackgroundColor = (ConsoleColor)Colors.Command;
                Console.ForegroundColor = (ConsoleColor)Colors.Background;
                Console.SetCursorPosition(1, settings.DirListAreaLine);//Вывод названия корневого каталога в первой строке
                Console.Write(string.Format(dirList[0]));
                Console.BackgroundColor = (ConsoleColor)Colors.Background;
                Console.ForegroundColor = (ConsoleColor)Colors.Standart;

                if (page < 1) page = 1;
                if (page > pages) page = pages;
                int number = (page - 1) * lines;//Номер элемента списка начиная с которого будет вывод

                if(pages>0)
                    for (int i = number; i < number + lines; i++)
                    {
                        if (i < (dirList.Count - 1))
                        {
                            Console.SetCursorPosition(1, i - number + settings.DirListAreaLine + 1);
                            Console.Write(string.Format(dirList[i + 1]));
                        }
                    }

                //Информация о номере выводимой страницы
                string pageInfo = $" page {number / lines + 1} from {pages} ";
                Console.SetCursorPosition(settings.AppWidth / 2 - pageInfo.Length / 2, settings.FileListAreaLine - 1);
                Console.WriteLine(pageInfo);

                //Обработка нажатий клавиатуры
                if(pages > 1)//Если страниц больше одной, то включаем листалку
                {
                    Console.CursorVisible = false;
                    Console.SetCursorPosition(1, settings.CommandAreaLine);
                    ConsoleKeyInfo key = Console.ReadKey();
                    switch (key.Key)
                    {
                        case ConsoleKey.UpArrow:
                        case ConsoleKey.PageUp:
                            page--;
                            ClearArea(Areas.DirList);
                            break;
                        case ConsoleKey.DownArrow:
                        case ConsoleKey.PageDown:
                            ClearArea(Areas.DirList);
                            page++;
                            break;
                        case ConsoleKey.Q:
                        case ConsoleKey.Escape:
                            isExit = true;
                            break;
                    }
                }
                else
                    isExit = true;

            }
            Console.CursorVisible = true;
        }

        private static void PrintFileList(List<string> fileList, int page = 1)
        {
            ClearArea(Areas.FileList);
            PrintMessage(Areas.Info, Messages.ListMessage);

            int lines = settings.InfoAreaLine - settings.FileListAreaLine - 2;//Количество линий списка выводимых на экран за один раз
            int pages = (fileList.Count - 1) / lines;//Количество страниц в списке
            if (pages * lines < (fileList.Count - 1)) pages++;

            bool isExit = false;
            while (!isExit)
            {
                Console.BackgroundColor = (ConsoleColor)Colors.Command;
                Console.ForegroundColor = (ConsoleColor)Colors.Background;
                Console.SetCursorPosition(1, settings.FileListAreaLine);//Вывод названия корневого каталога в первой строке
                Console.Write(string.Format(fileList[0]));
                Console.BackgroundColor = (ConsoleColor)Colors.Background;
                Console.ForegroundColor = (ConsoleColor)Colors.Standart;

                if (page < 1) page = 1;
                if (page > pages) page = pages;
                int number = (page - 1) * lines;//Номер элемента списка начиная с которого будет вывод

                if (pages > 0)
                    for (int i = number; i < number + lines; i++)
                    {
                        if (i < (fileList.Count - 1))
                        {
                            Console.SetCursorPosition(1, i - number + settings.FileListAreaLine + 1);
                            Console.Write(string.Format(fileList[i + 1]));
                        }
                    }

                //Информация о номере выводимой страницы
                string pageInfo = $" page {number / lines + 1} from {pages} ";
                Console.SetCursorPosition(settings.AppWidth / 2 - pageInfo.Length / 2, settings.InfoAreaLine - 1);
                Console.WriteLine(pageInfo);

                //Обработка нажатий клавиатуры
                if (pages > 1)//Если страниц больше одной, то включаем листалку
                {
                    Console.CursorVisible = false;
                    Console.SetCursorPosition(1, settings.CommandAreaLine);
                    ConsoleKeyInfo key = Console.ReadKey();
                    switch (key.Key)
                    {
                        case ConsoleKey.UpArrow:
                        case ConsoleKey.PageUp:
                            page--;
                            ClearArea(Areas.FileList);
                            break;
                        case ConsoleKey.DownArrow:
                        case ConsoleKey.PageDown:
                            ClearArea(Areas.FileList);
                            page++;
                            break;
                        case ConsoleKey.Q:
                        case ConsoleKey.Escape:
                            isExit = true;
                            break;
                    }
                }
                else
                    isExit = true;
            }
            Console.CursorVisible = true;

        }

        #endregion

        #region ---- WORK WITH FILES ----

        /// <summary>
        /// Формирует список каталогов для вывода на экран,
        /// рекурсивно просматривая содержимое заданного каталога
        /// </summary>
        /// <param name="path">путь к просматриваемому каталогу</param>
        /// <param name="path">максимальная глубина сканирования каталогов</param>
        /// <param name="graphLine">строка содержащая графическую структуру каталогов
        /// формируется во время работы метода</param>
        /// <param name="level">уровень глубины катлогов от изначального</param>
        public static void SeekDirectoryRecursion(string path, int maxLevel = 0, string graphLine = "", int level = 1)
        {
            if (maxLevel == 0) maxLevel = settings.MaxLevelDefault;//Если максимальная глубина не задана, то ограничиваем дефолтным значением

            //Превращаем путь вроде "d:" в "d:\", т.к. Directory.Exist() считает его валидным, а DirectoryInfo не совсем
            if (path[path.Length - 1] == ':') path = path + '\\';

            if (level == 1) dirList.Add(path);//Если мы на самом первом уровне, то добавляем главный каталог в список

            try
            {
                DirectoryInfo[] dirContent = new DirectoryInfo(path).GetDirectories();//Получаеv содержимое заданного каталога

                foreach (DirectoryInfo dir in dirContent) //Просматриваем все элементы в полученном списке
                {
                    if (dir.Attributes == FileAttributes.Directory) //Если текущий элемент каталог
                    {

                        //Записываем в список для вывода на экран очередной каталог
                        if (dir == dirContent[dirContent.Length - 1]) { dirList.Add(graphLine + "└──" + dir + "\n"); }
                        else { dirList.Add(graphLine + "├──" + dir + "\n"); }


                        //Просмотр содержимого каталога если мы не глубже максимального уровня
                        if (level < maxLevel)
                        {
                            try
                            {
                                //Получаем содержимое текущего просматриваемого каталога
                                DirectoryInfo[] currentDirContent = new DirectoryInfo(path + "\\" + dir).GetDirectories();
                                if (currentDirContent.Length > 0)
                                {
                                    if (dir != dirContent[dirContent.Length - 1])
                                        SeekDirectoryRecursion(path + "\\" + dir, maxLevel, graphLine + "│  ", level + 1);
                                        else
                                        SeekDirectoryRecursion(path + "\\" + dir, maxLevel, graphLine + "   ", level + 1);
                                }
                            }
                            catch
                            {
                                //если каталог недоступен, то ничего не делаем (пока)
                            }
                        }
                    }
                }
            }
            catch
            {
                //!TODO если каталог недоступен, то ничего не делаем (пока)
            }
        }


        /// <summary>
        /// Сканирует файлы в указанном каталоге и помещает их в список
        /// </summary>
        /// <param name="path">Путь к каталогу</param>
        public static void SeekDirectoryForFiles(string path)
        {
            //Превращаем путь вроде "d:" в "d:\", т.к. Directory.Exist() считает его валидным, а DirectoryInfo не совсем
            if (path[path.Length - 1] == ':') path = path + '\\';

            fileList.Add(path);//Если мы на самом первом уровне, то добавляем главный каталог в список

            try
            {
                FileInfo[] dirContent = new DirectoryInfo(path).GetFiles();//Получаеv содержимое заданного каталога
                foreach (FileInfo file in dirContent)
                {
                    fileList.Add(file.Name);
                }
            }
            catch
            {
                //!TODO если каталог недоступен, то ничего не делаем (пока)
            }
        }


        #endregion
    }



}
