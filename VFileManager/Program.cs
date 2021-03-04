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
            { "dir", Commands.Dirs },
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



        #endregion

        #region ---- FIELDS & PROPERTIES ----

        /// <summary>Настройки приложения</summary>
        private static Settings settings = new Settings(settingsFile);
        /// <summary>Вывод на экран</summary>
        private static Output output = new Output(settings);
        /// <summary>Работа с файлами</summary>
        private static FilesHandler filesHandler = new FilesHandler(settings);

        /// <summary>Сюда будет помещаться список каталогов</summary>
        private static List<string> dirList = new List<string>();
        /// <summary>Сюда будет помещаться список файлов</summary>
        private static List<string> fileList = new List<string>();

        #endregion

        static void Main(string[] args)
        {
            Init();//Инициализация

            Console.Clear();
            output.PrintMainFrame();
            output.PrintMessage(Areas.Info, Messages.EnterCommand);

            //Основной цикл
            bool isExit = false;
            while (!isExit)
            {
                output.PrintMessage(Areas.Info, Messages.EnterCommand);
                List<string> inputWords = CommandInput();
                if(inputWords.Count != 0)
                    switch (checkCommand(inputWords[0]))
                    {
                        case Commands.Help://Вывод справки
                            output.PrintManual();
                            break;

                        case Commands.Dirs://Вывод списка каталогов
                            DirList(inputWords);
                            break;

                        case Commands.Files://Вывод списка файлов
                            FileList(inputWords);
                            break;

                        case Commands.Exit://Выход
                            isExit = true;
                            break;

                        case Commands.WrongCommand://Неправильная команда
                            output.PrintMessage(Areas.Info, Messages.WrongCommand);
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
            output.PrintMessage(Areas.CommanLine, Messages.CommandSymbol);
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
                if(IsPathValid(words[number]))//Проверяем на отсутвие запрещенных символов
                {
                    path = words[number];
                }
            }

            return path;
        }

        /// <summary>
        /// Проверяет на правильность заданный путь к файлу
        /// </summary>
        /// <param name="path">Путь к файлу</param>
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

        /// <summary>
        /// Проверяется является ли путь к файлу/каталогу полным или кратким (относительным)
        /// </summary>
        /// <param name="path">Проверяемый путь</param>
        /// <returns>true, если путь полный (содержит двоеточие)</returns>
        private static bool IsPathLong(string path)
        {
            bool isPathFull = false;
            if(path != null)
            {
                foreach (char symbol in path)
                {
                    if (symbol == ':') isPathFull = true;
                }
            }

            return isPathFull;
        }

        /// <summary>
        /// Собирает из относительного и полного пути (корневого каталога) один полный
        /// </summary>
        /// <param name="rootPath">Путь к корневому каталогу</param>
        /// <param name="path">Относительный путь к каталогу/файлу</param>
        /// <returns>Полный путь к файлу/каталогу</returns>
        private static string MakeFullPath(string rootPath, string path)
        {
            StringBuilder newString = new StringBuilder();
            if(!IsPathLong(path))
            {
                newString.Append(rootPath);
                char lastChar = rootPath[rootPath.Length - 1];
                if (lastChar != '/' && lastChar != '\\')
                    newString.Append("\\");
            }
            newString.Append(path);

            return newString.ToString();
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

            if(!IsPathExist(fullPath) && arguments.ContainsKey(path))//На тот случай если один из аргументов был принят за путь к каталогу
            {
                fullPath = settings.LastPath;//То устанавливаем предыдущий путь
            }

            if (IsPathExist(fullPath))//Если путь существует, то сканируем его и выводим на экран
            {
                dirList.Clear();
                filesHandler.SeekDirectoryRecursion(fullPath, dirList, maxLevel);
                output.PrintList(Areas.DirList, dirList, page);
                settings.LastPath = fullPath;
                settings.SaveSettings();
            }
            else
            {
                output.PrintMessage(Areas.Info, Messages.WrongPath);
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

            if (!IsPathExist(fullPath) && arguments.ContainsKey(path))//На тот случай если один из аргументов был принят за путь к каталогу
            {
                fullPath = settings.LastPath;//То устанавливаем предыдущий путь
            }

            if (IsPathExist(fullPath))
            {
                fileList.Clear();
                filesHandler.SeekDirectoryForFiles(fullPath, fileList);
                output.PrintList(Areas.FileList, fileList, page); ;
            }
            else
            {
                output.PrintMessage(Areas.Info, Messages.WrongPath);
                Console.ReadKey();
            }
        }

        #endregion
    }
}
