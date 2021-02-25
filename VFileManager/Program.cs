﻿using System;
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

        /// <summary>Ключи словаря сообщений</summary>
        enum Messages
        {
            Help,
            AppName,
            EnterCommand,
            WrongCommand,
            CommandSymbol,
            ListMessage,
        }

        /// <summary>Словарь сообщений</summary>
        private static readonly Dictionary<Messages, string> messages = new Dictionary<Messages, string>
        {
            { Messages.Help, "Список комманд:\n" +
                "║help - вывод справки\n" +
                "║list - вывод списка файлов и каталогов\n" +
                "║exit - выход из программы\n" },
            { Messages.AppName, " VFileManager " },
            { Messages.EnterCommand, "Введите комманду" },
            { Messages.WrongCommand, "Неправильная команда. Повторите ввод." },
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
        private const int APP_WIDTH = 80;
        /// <summary>Высота окна приложения</summary>
        private const int APP_HEIGHT = 24;

        /// <summary>Экранные области приложения</summary>
        enum Areas
        {
            Main,
            Info,
            CommanLine,
        }

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
                switch (CommandInput())
                {
                    case Commands.Help:
                        PrintMessage(Areas.Main, Messages.Help);
                        break;
                    case Commands.List:
                        fileList.Clear();
                        SeekDirectoryRecursion("D:\\Work");
                        PrintFileList(fileList, INFO_AREA_LINE - MAIN_AREA_LINE - 1, 1);
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

        #region ---- INPUT ----

        /// <summary>Принимает комманду от пользователя</summary>
        /// <returns>Возвращает строку введеную пользователем</returns>
        private static Commands CommandInput()
        {
            PrintMessage(Areas.CommanLine, Messages.CommandSymbol);
            string input = Console.ReadLine();
            Commands command = Commands.WrongCommand;
            if (commands.ContainsKey(input))
                command = commands[input];
            return command;
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
        /// <param name="graphLine">строка содержащая графическую структуру каталогов
        /// формируется во время работы метода</param>
        /// <param name="level">уровень глубины катлогов от изначального</param>
        public static void SeekDirectoryRecursion(string path, string graphLine = "", int level = 1)
        {
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
                        if (level < 2)
                        {
                            try
                            {
                                //Получаем содержимое текущего просматриваемого каталога
                                DirectoryInfo[] currentDirContent = new DirectoryInfo(path + "\\" + dir).GetDirectories();
                                if (currentDirContent.Length > 0)
                                {
                                    if (dir != dirContent[dirContent.Length - 1])
                                        SeekDirectoryRecursion(path + "\\" + dir, graphLine + "│  ", level + 1);
                                    else
                                        SeekDirectoryRecursion(path + "\\" + dir, graphLine + "   ", level + 1);
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
