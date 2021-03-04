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



        #endregion

        #region ---- FIELDS & PROPERTIES ----

        /// <summary>Настройки приложения</summary>
        private static Settings settings = new Settings(settingsFile);
        /// <summary>Вывод на экран</summary>
        private static Output output = new Output(settings);

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
                        if (dir == dirContent[dirContent.Length - 1]) { dirList.Add(graphLine + "└──" + dir); }
                        else { dirList.Add(graphLine + "├──" + dir); }


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

        #region --- COMMANDS ----

        /// <summary>Вывод дерева каталогов</summary>
        /// <param name="inputWords">Список содержащий слова из ввода пользователя</param>
        private static void DirList(List<string> inputWords)
        {
            int page = findArguments(inputWords, Arguments.Page);//Номер страницы с которой начинается вывод
            int maxLevel = findArguments(inputWords, Arguments.Level);//Глубина сканирования каталогов
            string path = FindPath(inputWords, 1);//Каталог который сканируем
            if (path == string.Empty) path = settings.LastPath;//Если каталог не задан, то используем последний каталог
            if (IsPathExist(path))
            {
                dirList.Clear();
                SeekDirectoryRecursion(path, maxLevel);
                output.PrintList(Areas.DirList, dirList, page);
                settings.LastPath = path;
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
