using System;
using System.Collections.Generic;

namespace VFileManager
{

    class Program
    {
        #region ---- STRING CONSTANTS ----

        enum Commands
        {
            Help,
            Exit,
            WrongCommand,
        }

        private static readonly Dictionary<string, Commands> commands = new Dictionary<string, Commands>
        {
            { "help", Commands.Help },
            { "exit", Commands.Exit },
        };

        enum Messages
        {
            Help,
            AppName,
            EnterCommand,
            WrongCommand,
            CommandSymbol,
        }

        private static readonly Dictionary<Messages, string> messages = new Dictionary<Messages, string>
        {
            { Messages.Help, "Список комманд:\n" +
                " help - вывод справки\n" +
                " exit - выход из программы\n" },
            { Messages.AppName, " VFileManager " },
            { Messages.EnterCommand, "Введите комманду" },
            { Messages.WrongCommand, "Неправильная команда. Повторите ввод." },
            { Messages.CommandSymbol, ":>" },
        };

        #endregion

        #region ---- NUMERIC CONSTANTS ----

        /// <summary>Ширина окна приложения</summary>
        private const int APP_WIDTH = 80;
        /// <summary>Высота окна приложения</summary>
        private const int APP_HEIGHT = 24;

        /// <summary>Номер строки в которой происходит вывод информации для пользователя</summary>
        private const int MAIN_AREA_LINE = 1;
        /// <summary>Номер строки в которой происходит вывод информации для пользователя</summary>
        private const int INFO_AREA_LINE = 18;
        /// <summary>Номер строки в которой происходит ввод координат</summary>
        private const int COMMAND_AREA_LINE = 22;

        #endregion

        static void Main(string[] args)
        {
            //Устанавливаем размер консольного окна и буфера
            //Console.SetWindowSize(APP_WIDTH, APP_HEIGHT+1);
            //Console.SetBufferSize(APP_WIDTH, APP_HEIGHT+1);

            PrintMainFrame();
            PrintMessage(2, 0, Messages.AppName);
            PrintMessage(1, INFO_AREA_LINE, Messages.EnterCommand);

            //Основной цикл
            bool isExit = false;
            while (!isExit)
            {
                switch (CommandInput())
                {
                    case Commands.Help:
                        PrintMessage(1, MAIN_AREA_LINE, Messages.Help);
                        break;
                    case Commands.Exit:
                        isExit = true;
                        break;
                    case Commands.WrongCommand:
                        PrintMessage(1, INFO_AREA_LINE, Messages.WrongCommand);
                        break;

                }

            }
            
        }


        /// <summary>Принимает комманду от пользователя</summary>
        /// <returns>Возвращает строку введеную пользователем</returns>
        private static Commands CommandInput()
        {
            ClearLine(1, COMMAND_AREA_LINE, APP_WIDTH-2);
            PrintMessage(1, COMMAND_AREA_LINE, Messages.CommandSymbol);
            string input = Console.ReadLine();
            Commands command = Commands.WrongCommand;
            if (commands.ContainsKey(input))
                command = commands[input];
            return command;
        }

        /// <summary>Очищает в консоли указанную строку</summary>
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

        /// <summary>Выводит сообщение с заданной строки</summary>
        /// <param name="row">Строка с которой нужно вывести сообщение</param>
        /// <param name="message">Ключ в словаре сообщений</param>
        private static void PrintMessage(int column, int row, Messages message)
        {
            if(messages.ContainsKey(message))
            {
                Console.SetCursorPosition(column, row);
                Console.Write(messages[message]);
            }
        }

        private static void PrintMainFrame()
        {
            //╔ ═ ╗ ╚ ║ ╝ ╠ ╣ ╦ ╩ ╬ █ - символы для рамки

            Console.SetCursorPosition(0, 0);
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
        }

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

    }
}
