using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VFileManager
{

    class Program
    {
        #region ---- STRING CONSTANTS ----

        /// <summary>Команды выполняемые приложением</summary>
        enum Commands
        {
            Help,//Вывод справки
            List,//Вывод списка файлов
            Exit,//Выход из программы
            WrongCommand,//Неправильная комманда
        }

        /// <summary>Словарь тектовых ключей комманд</summary>
        private static readonly Dictionary<string, Commands> commands = new Dictionary<string, Commands>
        {
            { "help", Commands.Help },
            { "list", Commands.List },
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
            Help,
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
            { Messages.Help, "Список комманд:\n" +
                "║help - вывод справки\n" +
                "║list <path> [-p <int>] [-l <int>]- вывод списка файлов и каталогов\n" +
                "║  path - путь к выводимому каталогу\n" +
                "║  -p <int> - номер страницы, default=1\n" +
                "║  -l <int> - количество уровней каталогов, default=2\n" +
                "║exit - выход из программы\n" },
            { Messages.AppName, " VFileManager " },
            { Messages.EnterCommand, "Введите комманду. (help - для списка комманд)" },
            { Messages.WrongCommand, "Неправильная команда. Повторите ввод." },
            { Messages.WrongPath, "Неправильный путь. Повторите ввод." },
            { Messages.CommandSymbol, ":>" },
            { Messages.ListMessage, "Up/Down arrows - change pages. Q - stop." },
        };

        #endregion

        #region ---- NUMERIC CONSTANTS ----

        /// <summary>Цвета для различных элементов интерфейса</summary>
        enum Colors
        {
            Frame = (int)ConsoleColor.Green,
            Standart = (int)ConsoleColor.Gray,
        }

        /// <summary>Ширина окна приложения</summary>
        private const int APP_WIDTH = 120;
        /// <summary>Высота окна приложения</summary>
        private const int APP_HEIGHT = 24;

        /// <summary>Экранные области приложения</summary>
        enum Areas
        {
            Main,
            Info,
            CommanLine,
        }

        /// <summary>Стандартная глубина для выводимого дерева каталогов</summary>
        private const int MAX_LEVEL_DEFAULT = 2;


        /// <summary>Номер строки в которой происходит вывод информации для пользователя</summary>
        private const int MAIN_AREA_LINE = 1;
        /// <summary>Номер строки в которой происходит вывод информации для пользователя</summary>
        private const int INFO_AREA_LINE = 18;
        /// <summary>Номер строки в которой происходит ввод координат</summary>
        private const int COMMAND_AREA_LINE = 22;

        #endregion

        #region ---- FIELDS & PROPERTIES ----

        /// <summary>Сюда будет помещаться список файлов и каталогов</summary>
        private static List<string> fileList = new List<string>();

        #endregion

        static void Main(string[] args)
        {
            //Устанавливаем размер консольного окна и буфера
            Console.SetWindowSize(APP_WIDTH+1, APP_HEIGHT+1);
            //Console.SetBufferSize(APP_WIDTH, APP_HEIGHT+1);

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
                        case Commands.Help:
                            PrintMessage(Areas.Main, Messages.Help);
                            break;
                        case Commands.List:
                            fileList.Clear();
                            int page = findArguments(inputWords, Arguments.Page);
                            int maxLevel = findArguments(inputWords, Arguments.Level);
                            string path = FindPath(inputWords, 1);
                            if(IsPathExist(path))
                            {
                                SeekDirectoryRecursion(path, maxLevel) ;
                                PrintFileList(fileList, INFO_AREA_LINE - MAIN_AREA_LINE - 1, page);
                            }
                            else
                            {
                                PrintMessage(Areas.Info, Messages.WrongPath);
                                Console.ReadKey();
                            }
                            break;
                        case Commands.Exit:
                            isExit = true;
                            break;
                        case Commands.WrongCommand:
                            PrintMessage(Areas.Info, Messages.WrongCommand);
                            Console.ReadKey();
                            break;

                    }

            }
            
        }

        #region ---- WORK WITH INPUT ----

        /// <summary>Принимает ввод пользователя и разбивает его на части</summary>
        /// <returns>Список содержащий комманду пользователя и аргументы</returns>
        private static List<string> CommandInput()
        {
            PrintMessage(Areas.CommanLine, Messages.CommandSymbol);
            string input = Console.ReadLine();
            List<string> inputWords = new List<string>();//Список для комманд и аргументов
            StringBuilder word = new StringBuilder();//Буфер для символов комманд
            for (int i = 0; i < input.Length; i++)
            {
                if(input[i] != ' ')//Если очередной символ не разделитель, то записываем его в буфер
                {
                    word.Append(input[i]);
                    if(i==input.Length-1)//Если это был конец строки то записываем буфер в список
                        inputWords.Add(word.ToString());
                }
                else if(word.Length!=0)//Если дошли до разделителя, то записываем буфер в список (если он не пустой)
                {
                    inputWords.Add(word.ToString());
                    word.Clear();
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
                case Areas.Main:
                    firstRow = MAIN_AREA_LINE;
                    lastRow = INFO_AREA_LINE - 1;
                    break;
                case Areas.Info:
                    firstRow = INFO_AREA_LINE;
                    lastRow = COMMAND_AREA_LINE - 1;
                    break;
                case Areas.CommanLine:
                    firstRow = COMMAND_AREA_LINE;
                    lastRow = APP_HEIGHT - 1;
                    break;
            }

            for (int i = firstRow; i < lastRow; i++)
                ClearLine(1, i, APP_WIDTH - 2);
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
                if(i<=APP_WIDTH)
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
                    case Areas.Main:
                        row = MAIN_AREA_LINE;
                        break;
                    case Areas.Info:
                        row = INFO_AREA_LINE;
                        break;
                    case Areas.CommanLine:
                        row = COMMAND_AREA_LINE;
                        break;
                }
                Console.SetCursorPosition(1, row);
                Console.Write(messages[message]);
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
            //область вывода списка файлов/каталогов
            for (int r = MAIN_AREA_LINE; r < INFO_AREA_LINE - 1; r++)
                PrintFrameLine("║ ║");
            //разделитель областей
            PrintFrameLine("╠═╣");
            //область вывода информации
            for (int r = INFO_AREA_LINE; r < COMMAND_AREA_LINE - 1; r++)
                PrintFrameLine("║ ║");
            //разделитель областей
            PrintFrameLine("╠═╣");
            //Область ввода комманд                        
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
                for (int c = 1; c < APP_WIDTH - 1; c++)
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
        /// <param name="fileList">Список файлов для вывода на экран</param>
        /// <param name="lines">Количество поизций одновременно выводимых на экран</param>
        /// <param name="page">Номер страницы которую необходимо вывести на экран</param>
        private static void PrintFileList(List<string> fileList, int lines, int page = 1 )
        {
            ClearArea(Areas.Main);
            PrintMessage(Areas.Info, Messages.ListMessage);

            int pages = fileList.Count / lines;//Количество страниц в списке
            if (pages * lines < fileList.Count) pages++;

            bool isExit = false;
            while(!isExit)
            {
                if (page < 1) page = 1;
                if (page > pages) page = pages;
                int number = (page - 1) * lines;//Номер элемента списка начиная с которого будет вывод

                for (int i = number; i < number + lines; i++)
                {
                    if (i < fileList.Count)
                    {
                        Console.SetCursorPosition(1, i - number + 1);
                        Console.Write(string.Format(fileList[i]));
                    }
                }

                //Информация о номере выводимой страницы
                string pageInfo = $" page {number / lines + 1} from {pages} ";
                Console.SetCursorPosition(APP_WIDTH / 2 - pageInfo.Length / 2, INFO_AREA_LINE - 1);
                Console.WriteLine(pageInfo);

                //Обработка нажатий клавиатуры
                Console.CursorVisible = false;
                Console.SetCursorPosition(1, COMMAND_AREA_LINE);
                ConsoleKeyInfo key = Console.ReadKey();
                switch(key.Key)
                {
                    case ConsoleKey.UpArrow:
                        page--;
                        ClearArea(Areas.Main);
                        break;
                    case ConsoleKey.DownArrow:
                        ClearArea(Areas.Main);
                        page++;
                        break;
                    case ConsoleKey.Q:
                        isExit = true;
                        break;
                }
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
            if (maxLevel == 0) maxLevel = MAX_LEVEL_DEFAULT;//Если максимальная глубина не задана, то ограничиваем дефолтным значением

            //Превращаем путь вроде "d:" в "d:\", т.к. Directory.Exist() считает его валидным, а DirectoryInfo не совсем
            if (path[path.Length - 1] == ':') path = path + '\\';

            if (level == 1) fileList.Add(path);//Если мы на самом первом уровне, то добавляем главный каталог в список

            try
            {
                DirectoryInfo[] dirContent = new DirectoryInfo(path).GetDirectories();//Получаеv содержимое заданного каталога

                foreach (DirectoryInfo dir in dirContent) //Просматриваем все элементы в полученном списке
                {
                    if (dir.Attributes == FileAttributes.Directory) //Если текущий элемент каталог
                    {

                        //Записываем в список для вывода на экран очередной каталог
                        if (dir == dirContent[dirContent.Length - 1]) { fileList.Add(graphLine + "└──" + dir + "\n"); }
                        else { fileList.Add(graphLine + "├──" + dir + "\n"); }


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
                //если каталог недоступен, то ничего не делаем (пока)
            }
        }

        #endregion
}



}
