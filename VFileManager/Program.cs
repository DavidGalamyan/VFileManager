using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        #endregion

        #region ---- FIELDS & PROPERTIES ----

        /// <summary>Настройки приложения</summary>
        private static Settings settings = new Settings(settingsFile);
        /// <summary>База текстовых сообщений</summary>
        private static MessagesBase messages = new MessagesBase();
        /// <summary>Вывод на экран</summary>
        private static Output output = new Output(settings, messages);
        /// <summary>Обработка ввода</summary>
        private static Input input = new Input(settings, messages, output);
        /// <summary>Обработчик комманд</summary>
        private static CommandsHandler commands = new CommandsHandler(settings, messages, output);


        #endregion

        static void Main(string[] args)
        {
            Init();//Инициализация
            MainCycle();//Основной цикл
        }

        #region ---- INIT ----

        /// <summary>
        /// Инициализация приложения
        /// </summary>
        private static void Init()
        {
            settings.LoadSettings();

            //Устанавливаем размер консольного окна и буфера
            Console.SetWindowSize(settings.AppWidth + 1, settings.AppHeight + 1);
            Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);

            Console.Clear();
            output.PrintMainFrame();

            commands.Refresh(settings.LastPath);
        }


        #endregion

        #region ---- MAIN CYCLE ----

        /// <summary>
        /// Основной рабочий цикл программы
        /// Обрабатывает ввод и вызывает выполнение необходимой комманды
        /// </summary>
        private static void MainCycle()
        {
            //Основной цикл
            bool isExit = false;//Флаг выхода
            while (!isExit)
            {
                output.PrintMessage(Areas.CommandInfoLine, Messages.EnterCommand);//Просим пользователя ввести комманду
                //Берем ввод пользователя и разбиваем его на отдельные слова, для дальнейшего анализа
                List<string> inputWords = input.CommandInput();
                if (inputWords.Count != 0)//Если пользователь ввел хотя бы одно слово
                    switch (commands.CheckCommand(inputWords[0]))//Первое слово в воде пользователя должно быть командой
                    {
                        case Commands.Help://Вывод справки
                            output.PrintManual();
                            break;

                        case Commands.Dir://Вывод списка каталогов
                            commands.DirList(inputWords);
                            break;

                        case Commands.Files://Вывод списка файлов
                            commands.FileList(inputWords);
                            break;

                        case Commands.Info://Вывод списка файлов
                            commands.Info(inputWords);
                            break;

                        case Commands.Copy://Копирование файла
                            commands.FileCopyMove(inputWords, Commands.Copy);
                            break;

                        case Commands.Move://Перемещение файла
                            commands.FileCopyMove(inputWords, Commands.Move);
                            break;

                        case Commands.Delete://Удаление файла
                            commands.FileDelete(inputWords);
                            break;

                        case Commands.DirCopy://Копирование файла
                            commands.DirCopyMove(inputWords, Commands.Copy);
                            break;

                        case Commands.DirMove://Перемещение файла
                            commands.DirCopyMove(inputWords, Commands.Move);
                            break;

                        case Commands.DirDelete://Удаление каталога
                            commands.DirDelete(inputWords);
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

        #endregion
    }
}
