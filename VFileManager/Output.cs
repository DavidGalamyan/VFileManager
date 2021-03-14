using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFileManager
{
    #region ------ PUBLIC ENUMS ------

    /// <summary>Экранные области приложения</summary>
    public enum Areas
    {
        DirList,
        FileList,
        Info,
        CommandLine,
        CommandInfoLine,
    }

    #endregion

    /// <summary>
    /// Класс выводит интерфейс и различную информацию на экран
    /// </summary>
    class Output
    {
        #region ---- CONSTANTS ----

        /// <summary>Цвета для различных элементов интерфейса</summary>
        enum Colors
        {
            Frame = (int)ConsoleColor.Green,
            Standart = (int)ConsoleColor.Gray,
            Command = (int)ConsoleColor.Yellow,
            Argument = (int)ConsoleColor.DarkYellow,
            Background = (int)ConsoleColor.Black,
        }

        /// <summary>
        /// Символы для расцветки выводимых сообщений
        /// Выбраны такие символы, которые не могут содержаться в именах файлов/каталогов
        /// </summary>
        public enum ColorSymbols
        {
            Standart = (int)'*',
            Command = (int)'?',
            Argument = (int)'|',
        }

        /// <summary>
        /// Цвета для расцветки выводимого текста, согласно спецсимволам
        /// </summary>
        private readonly Dictionary<char, Colors> colorCodes = new Dictionary<char, Colors>()
        {
            {(char)ColorSymbols.Standart, Colors.Standart},
            {(char)ColorSymbols.Command, Colors.Command},
            {(char)ColorSymbols.Argument, Colors.Argument},
        };

        #endregion

        #region ---- FIELDS ----

        /// <summary>Настройки приложения</summary>
        Settings settings;
        /// <summary>База текстовых сообщений</summary>
        MessagesBase messages;

        #endregion

        #region ---- CONSTRUCTORS ----

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="settings">Настройки приложения</param>
        public Output(Settings settings)
        {
            this.settings = settings;
            this.messages = settings.Messages;
        }

        #endregion

        #region ---- AREA PARAMETERS CALCULATING ----

        /// <summary>Возвращает номера первой и последней строки для заданной области окна приложения</summary>
        /// <param name="area">Область приложения (окно)</param>
        /// <returns>
        /// первый ряд области;
        /// последний ряд области.</returns>
        private (int, int) GetAreaRows(Areas area)
        {
            int firstRow = 0;
            int lastRow = 0;
            switch (area)
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
                    lastRow = settings.CommandInfoAreaLine - 1;
                    break;
                case Areas.CommandInfoLine:
                    firstRow = settings.CommandInfoAreaLine;
                    lastRow = settings.CommandAreaLine - 1;
                    break;
                case Areas.CommandLine:
                    firstRow = settings.CommandAreaLine;
                    lastRow = settings.AppHeight - 1;
                    break;
            }
            return (firstRow, lastRow);
        }

        #endregion

        #region ---- PRINT SINGLE MESSAGES ----

        /// <summary>Выводит сообщение с заданной области</summary>
        /// <param name="area">ОБласть экрана в которой нужно вывести сообщение</param>
        /// <param name="message">Ключ в словаре сообщений</param>
        public void PrintMessage(Areas area, Messages message)
        {
            if (messages.ContainsKey(message))
            {
                ClearArea(area);
                (int row, _) = GetAreaRows(area);
                Console.SetCursorPosition(1, row);
                Console.Write(messages[message]);
            }
        }

        /// <summary>Выводит сообщение с заданной области</summary>
        /// <param name="area">ОБласть экрана в которой нужно вывести сообщение</param>
        /// <param name="message">Ключ в словаре сообщений</param>
        public void PrintMessage(Areas area, string message)
        {
            ClearArea(area);
            (int row, _) = GetAreaRows(area);
            Console.SetCursorPosition(1, row);
            Console.Write(message);
        }

        /// <summary>Выводит сообщение с заданной области</summary>
        /// <param name="area">ОБласть экрана в которой нужно вывести сообщение</param>
        /// <param name="message">Ключ в словаре сообщений</param>
        public void PrintMessage(Areas area, Messages message, string message_tail)
        {
            if (messages.ContainsKey(message))
            {
                ClearArea(area);
                (int row, _) = GetAreaRows(area);
                Console.SetCursorPosition(1, row);
                Console.Write(messages[message] + message_tail);
            }
        }

        /// <summary>Выводит на экран строку символов, раскрашивая ее с помощью спец-символоов</summary>
        /// <param name="line">Строка для вывода на экран</param>
        private void PrintColorMessage(string line)
        {
            Console.ForegroundColor = (ConsoleColor)Colors.Standart;
            foreach (char chr in line)
            {
                if (colorCodes.ContainsKey(chr))
                    Console.ForegroundColor = (ConsoleColor)colorCodes[chr];
                else
                    Console.Write(chr);
            }
            Console.ForegroundColor = (ConsoleColor)Colors.Standart;
        }

        /// <summary>Выводит на экран справку по коммандам</summary>
        public void PrintManual()
        {
            PrintList(Areas.DirList, messages.GetManual());
        }

        #endregion

        #region ---- MAIN WINDOW OUTPUT METHODS ----

        /// <summary>Выводит на экран рамку приложения</summary>
        public void PrintMainFrame()
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
            for (int r = settings.InfoAreaLine; r < settings.CommandInfoAreaLine - 1; r++)
                PrintFrameLine("║ ║");
            //разделитель областей
            PrintFrameLine("╠═╣");
            //Область иформации о комманде
            for (int r = settings.CommandInfoAreaLine; r < settings.CommandAreaLine - 1; r++)
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
        private void PrintFrameLine(string lineSymbols)
        {
            if (lineSymbols.Length == 3)
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

        /// <summary>Очищает заданную область интерфейса</summary>
        /// <param name="area">Очищаемая область</param>
        private void ClearArea(Areas area)
        {
            //int firstRow = 0;
            //int lastRow = 0;
            (int firstRow, int lastRow) = GetAreaRows(area);

            for (int i = firstRow; i < lastRow; i++)
                ClearLine(1, i, settings.AppWidth - 2);

            //очистка информации о странице
            Console.SetCursorPosition(0, lastRow);
            Console.ForegroundColor = (ConsoleColor)Colors.Frame;
            switch (area)
            {
                case Areas.DirList:
                case Areas.FileList:
                case Areas.Info:
                case Areas.CommandInfoLine:
                    PrintFrameLine("╠═╣");
                    break;
                case Areas.CommandLine:
                    PrintFrameLine("╚═╝");
                    break;
            }
            //PrintFrameLine((lastRow == settings.AppHeight - 1) ? "╚═╝" : "╠═╣");
            Console.ForegroundColor = (ConsoleColor)Colors.Standart;

        }

        /// <summary>Очищает в консоли указанную строку</summary>
        /// <param name="column">Номер столбца с которого нужно очистить строку</param>
        /// <param name="row">Номер строки которую нужно очистить</param>
        /// <param name="length">Длина строки которую нужно очистить</param>
        private void ClearLine(int column, int row, int length)
        {
            Console.SetCursorPosition(column, row);
            for (int i = 0; i < length; i++)
            {
                if (i <= settings.AppWidth)
                    Console.Write(" ");
            }
        }

        #endregion

        #region ---- PRINT LIST WITH PAGING ----

        /// <summary>
        /// Выводит на экран список текстовых строк в заданном окне, с пэйджингом
        /// </summary>
        /// <param name="area">Область (окно) приложения в которое нужно вывести список</param>
        /// <param name="list">Список для вывода</param>
        /// <param name="page">Номер страницы с которой нужно начать вывод</param>
        /// <param name="isTurnPages">Включать ли литсалку страниц</param>
        public void PrintList(Areas area, List<string> list, int page = 1, bool isTurnPages = true)
        {
            ClearArea(area);

            (int firstRow, int lastRow) = GetAreaRows(area);
            int lines = lastRow - firstRow - 1;//Количество линий списка выводимых на экран за один раз
            int pages = (list.Count - 1) / lines;//Количество страниц в списке
            if (pages * lines < (list.Count - 1)) pages++;

            bool isExit = false;
            while (!isExit)
            {
                Console.BackgroundColor = (ConsoleColor)Colors.Command;
                Console.ForegroundColor = (ConsoleColor)Colors.Background;
                Console.SetCursorPosition(1, firstRow);//Вывод названия корневого каталога в первой строке
                Console.Write(string.Format(list[0]));
                Console.BackgroundColor = (ConsoleColor)Colors.Background;
                Console.ForegroundColor = (ConsoleColor)Colors.Standart;

                if (page < 1) page = 1;
                if (page > pages) page = pages;
                int number = (page - 1) * lines;//Номер элемента списка начиная с которого будет вывод

                if (pages > 0)
                    for (int i = number; i < number + lines; i++)
                    {
                        if (i < (list.Count - 1))
                        {
                            Console.SetCursorPosition(1, i - number + firstRow + 1);
                            //Console.Write(list[i + 1].PadRight(settings.AppWidth - 2));
                            PrintColorMessage(list[i + 1].PadRight(settings.AppWidth - 2));
                        }
                        else//Если индек за пределами списка, то просто очищаем следующие строки
                        {
                            ClearLine(1, i - number + firstRow + 1, settings.AppWidth - 2);
                        }
                    }

                //Информация о номере выводимой страницы
                string pageInfo = $" page {number / lines + 1} from {pages} ";
                Console.SetCursorPosition(settings.AppWidth / 2 - pageInfo.Length / 2, lastRow);
                Console.WriteLine(pageInfo);


                //Обработка нажатий клавиатуры
                if (pages > 1 && isTurnPages)//Если страниц больше одной, то включаем листалку
                {
                    PrintMessage(Areas.CommandInfoLine, Messages.ListMessage);
                    Console.CursorVisible = false;
                    Console.SetCursorPosition(settings.AppWidth-2, settings.CommandAreaLine);
                    ConsoleKeyInfo key = Console.ReadKey();
                    switch (key.Key)
                    {
                        case ConsoleKey.UpArrow:
                        case ConsoleKey.PageUp:
                            page--;
                            //ClearArea(area);
                            break;
                        case ConsoleKey.DownArrow:
                        case ConsoleKey.PageDown:
                            //ClearArea(area);
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
    }
}
