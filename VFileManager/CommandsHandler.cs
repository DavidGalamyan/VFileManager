using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace VFileManager
{
    #region ---- PUBLIC ENUMS ----

    /// <summary>Команды выполняемые приложением</summary>
    public enum Commands
    {
        Help,//Вывод справки
        Dir,//Вывод списка каталогов
        Files,//Вывод списка каталогов
        Info,//Вывод информации о файле
        Copy,//Копирование файла
        DirCopy,//Копирование каталога
        Move,//Перемещение файла
        DirMove,//Перемещение каталога
        Delete,//Удаление файла
        DirDelete,//Удаление каталога
        Errors,//Вывод списка ошибок
        Exit,//Выход из программы
        Version,//Показать версию приложения
        WrongCommand,//Неправильная комманда
    }

    /// <summary>Аргументы для комманд</summary>
    public enum Arguments
    {
        Page,//Номер страницы
        Level,//Количество уровней (для вывода дерева каталогов)
        Back,//Назаж
    }

    #endregion

    class CommandsHandler
    {
        #region ---- DICTIONARIES ----

        /// <summary>Словарь тектовых ключей комманд</summary>
        private readonly Dictionary<string, Commands> commands = new Dictionary<string, Commands>
        {
            { "help", Commands.Help },
            { "dir", Commands.Dir },
            { "files", Commands.Files },
            { "info", Commands.Info },
            { "copy", Commands.Copy },
            { "dcopy", Commands.DirCopy },
            { "move", Commands.Move },
            { "dmove", Commands.DirMove },
            { "del", Commands.Delete },
            { "ddel", Commands.DirDelete },
            { "log", Commands.Errors },
            { "exit", Commands.Exit },
            { "version", Commands.Version },
        };

        /// <summary>Словарь тектовых ключей комманд</summary>
        private readonly Dictionary<string, Arguments> arguments = new Dictionary<string, Arguments>
        {
            { "-p", Arguments.Page },
            { "-l", Arguments.Level },
            { "..", Arguments.Back },
        };

        #endregion

        #region ---- FIELDS ----

        /// <summary>Настройки приложения</summary>
        private Settings settings;
        /// <summary>Вывод на экран</summary>
        private Output output;
        /// <summary>Работа с файлами</summary>
        FilesHandler filesHandler;
        /// <summary>Логгер</summary>
        Logger logger;

        /// <summary>Сюда будет помещаться список каталогов</summary>
        private List<string> dirList = new List<string>();
        /// <summary>Сюда будет помещаться список файлов</summary>
        private List<string> fileList = new List<string>();
        /// <summary>Сюда будет помещаться информация о файле/каталоге</summary>
        private List<string> fileInfo = new List<string>();

        #endregion

        #region ---- CONSTRUCTORS ----

        public CommandsHandler(Settings settings, Output output)
        {
            this.settings = settings;
            this.output = output;
            this.filesHandler = new FilesHandler(settings);
            this.logger = settings.Logger;
        }

        #endregion

        #region ----- WORK WITH DICTIONARIES ----

        /// <summary>
        /// Проверяет какую команду содержит строка символов
        /// </summary>
        /// <param name="input">Строка символов для проверки</param>
        /// <returns>Комманда</returns>
        public Commands CheckCommand(string key)
        {
            Commands command = Commands.WrongCommand;
            if (commands.ContainsKey(key.ToLower()))
                command = commands[key.ToLower()];
            return command;
        }

        /// <summary>
        /// Проверяет список слов на наличие заданного аргумента и возвращает его значение
        /// </summary>
        /// <param name="inputWords">Список слов для анализа</param>
        /// <param name="argument">Искомый аргумент</param>
        /// <returns>Число следующее за аргументом или 0 если аргумент не найден</returns>
        private int FindArguments(List<string> inputWords, Arguments argument)
        {
            int parameter = 0;//Возвращаемый параметр
            int index = 1;//Индекс проверяемого слова
            bool isFound = false;
            while (!isFound && index < inputWords.Count - 1)
            {
                if (arguments.ContainsKey(inputWords[index]))//Если проверяемое слово присутствует в списке возможных аргументов...
                {
                    if (arguments[inputWords[index]] == argument)//Если это нужный нам аргумент...
                    {
                        int.TryParse(inputWords[index + 1], out parameter);//То проверяем следующее слово на числовое значение
                        isFound = true;//Завершаем поиск
                    }
                }
                index++;
            }

            return parameter;
        }

        #endregion

        #region ---- APPLICATION COMMANDS ----


        /// <summary>
        /// Обновляет информацию во всех трех окнах на экране
        /// </summary>
        /// <param name="path">Путь по которому происходит вывод информации</param>
        public void Refresh(string path)
        {
            //Вывод дерева последнего просматриваемого каталога
            dirList.Clear();
            filesHandler.SeekDirectoryRecursion(path, dirList);
            output.PrintList(Areas.DirList, dirList, 1, false);

            //Вывод содержимого последнего просматриваемого каталога
            fileList.Clear();
            filesHandler.SeekDirectoryForFiles(path, fileList);
            output.PrintList(Areas.FileList, fileList, 1, false);

            //Вывод информации о последнем просматриваемом каталоге
            fileInfo.Clear();
            filesHandler.GetInfo(path, fileInfo);
            output.PrintList(Areas.Info, fileInfo, 1, false);

            Console.SetCursorPosition(1, settings.CommandAreaLine);
        }


        /// <summary>Вывод дерева каталогов</summary>
        /// <param name="inputWords">Список содержащий слова из ввода пользователя</param>
        public void DirList(List<string> inputWords)
        {
            int page = FindArguments(inputWords, Arguments.Page);//Номер страницы с которой начинается вывод
            int maxLevel = FindArguments(inputWords, Arguments.Level);//Глубина сканирования каталогов
            string path = filesHandler.FindPath(inputWords, 1);//Каталог который сканируем
            string fullPath = filesHandler.MakeFullPath(settings.LastPath, path);//Преобразуем путь к нему в асолютный (если необходимо)

            if (!filesHandler.IsDirExist(fullPath) && arguments.ContainsKey(path))//На тот случай если один из аргументов был принят за путь к каталогу
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
        public void FileList(List<string> inputWords)
        {
            int page = FindArguments(inputWords, Arguments.Page);//Номер страницы с которой начинается вывод
            string path = filesHandler.FindPath(inputWords, 1);//Каталог который сканируем
            string fullPath = filesHandler.MakeFullPath(settings.LastPath, path);//Преобразуем путь к нему в асолютный (если необходимо)

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

        /// <summary>Вывод информации о файле/каталоге</summary>
        /// <param name="inputWords">Список содержащий слова из ввода пользователя</param>
        public void Info(List<string> inputWords)
        {
            string path = filesHandler.FindPath(inputWords, 1);//Каталог который сканируем
            string fullPath = filesHandler.MakeFullPath(settings.LastPath, path);//Преобразуем путь к нему в асолютный (если необходимо)

            if (!filesHandler.IsDirExist(fullPath) && arguments.ContainsKey(path))//На тот случай если один из аргументов был принят за путь к каталогу
            {
                fullPath = settings.LastPath;//То устанавливаем предыдущий путь
            }

            fileInfo.Clear();
            filesHandler.GetInfo(fullPath, fileInfo);
            if (fileInfo.Count != 0)
            {
                output.PrintList(Areas.Info, fileInfo);
            }
            else
            {
                output.PrintMessage(Areas.CommandInfoLine, Messages.WrongPath);
                Console.ReadKey();
            }

        }

        /// <summary>
        /// Копирование/перемещение файла по указанному пути
        /// </summary>
        /// <param name="inputWords">Список слов введенных пользователем</param>
        /// <param name="command">Комманда Copy или Move</param>
        /// <returns>true, в случае успешного завершения операции</returns>
        public bool FileCopyMove(List<string> inputWords, Commands command)
        {
            bool isSucces = false;

            //Извлекаем имя файла который копируем
            //Преобразуем путь к нему в асолютный (если необходимо)
            string sourceFileName = filesHandler.MakeFullPath(settings.LastPath, filesHandler.FindPath(inputWords, 1));

            //Извлекаем путь к каталогу куда копируем файл
            //Преобразуем путь к нему в асолютный (если необходимо)
            string destFullPath = filesHandler.MakeFullPath(settings.LastPath, filesHandler.FindPath(inputWords, 2));
            //Добавляем имя файла к имени пути назначения
            string destFileName = filesHandler.MakeFullPath(destFullPath, Path.GetFileName(sourceFileName));

            try
            {
                //Информация о файлах
                FileInfo sourceFileInfo = new FileInfo(sourceFileName);
                FileInfo destFileInfo = new FileInfo(destFileName);

                if (sourceFileInfo.Exists && !destFileInfo.Exists)//Проверка на то, что файл уже существует
                {
                    //Выбор нужной команды
                    switch (command)
                    {
                        case Commands.Copy:
                            isSucces = filesHandler.FileCopy(sourceFileName, destFileName);
                            break;
                        case Commands.Move:
                            isSucces = filesHandler.FileMove(sourceFileName, destFileName);
                            break;
                    }
                }
                else
                {
                    output.PrintMessage(Areas.CommandInfoLine, Messages.FileExist);
                }
            }
            catch (Exception e)
            {
                output.PrintMessage(Areas.CommandInfoLine, e.Message);
                logger.LogWrite($"Method: |FileCopyMove: *{e.Message}");
            }

            if (isSucces)
                output.PrintMessage(Areas.CommandInfoLine, Messages.Success);
            Console.ReadKey();
            Refresh(settings.LastPath);

            return isSucces;
        }

        public bool DirCopyMove(List<string> inputWords, Commands command)
        {
            bool isSucces = false;

            //Проверяем количество введенных аргументов
            if (inputWords.Count < 3)
            {
                output.PrintMessage(Areas.CommandInfoLine, Messages.WrongArguments);
                Console.ReadKey();
                return isSucces;
            }

            //Проверяем на то, что заданный путь не состоит только из символов '.','/','\'
            //Чтобы избежать конфликтных ситуаций с удалением каталогов выше текущего уровня
            bool isPathGood = false;
            foreach (char symbol in inputWords[1])
            {
                if (symbol != '.' && symbol != '/' && symbol != '\\')
                    isPathGood = true;
            }
            if (!isPathGood)
            {
                output.PrintMessage(Areas.CommandInfoLine, Messages.WrongPath);
                Console.ReadKey();
                return isSucces;
            }

            isPathGood = false;
            foreach (char symbol in inputWords[2])
            {
                if (symbol != '.' && symbol != '/' && symbol != '\\')
                    isPathGood = true;
            }
            if (!isPathGood)
            {
                output.PrintMessage(Areas.CommandInfoLine, Messages.WrongPath);
                Console.ReadKey();
                return isSucces;
            }



            //Извлекаем имя каталога который копируем
            //Преобразуем путь к нему в асолютный (если необходимо)
            string sourceDirName = filesHandler.MakeFullPath(settings.LastPath, filesHandler.FindPath(inputWords, 1));

            //Извлекаем путь к каталогу куда копируем каталог
            //Преобразуем путь к нему в асолютный (если необходимо)
            string destDirName = filesHandler.MakeFullPath(settings.LastPath, filesHandler.FindPath(inputWords, 2));



            try
            {
                //Информация о каталогах
                DirectoryInfo sourceDirInfo = new DirectoryInfo(sourceDirName);
                //Прибавляем к пути назначение название каталога который копируем/переносим
                destDirName = Path.Combine(destDirName, sourceDirInfo.Name);
                DirectoryInfo destDirInfo = new DirectoryInfo(destDirName);

                if (sourceDirInfo.Exists && sourceDirName != destDirName)//Проверка на то, что файл уже существует
                {
                    //Выбор нужной команды
                    bool isMove = false;
                    switch (command)
                    {
                        case Commands.Copy:
                            isMove = false;
                            break;
                        case Commands.Move:
                            isMove = true;
                            break;
                    }
                    isSucces = filesHandler.DirCopyMove(sourceDirName, destDirName, isMove);
                }
                else
                {
                    output.PrintMessage(Areas.CommandInfoLine, Messages.DirExist);
                }
            }
            catch (Exception e)
            {
                output.PrintMessage(Areas.CommandInfoLine, e.Message);
                logger.LogWrite($"Method: |DirCopyMove: *{e.Message}");
            }

            if (isSucces)
                output.PrintMessage(Areas.CommandInfoLine, Messages.Success);
            Console.ReadKey();
            Refresh(settings.LastPath);

            return isSucces;
        }



        /// <summary>Удаление файла в указанном расположении</summary>
        /// <param name="inputWords">Список содержащий слова из ввода пользователя</param>
        public bool FileDelete(List<string> inputWords)
        {
            bool isSucces = false;

            //Проверяем количество введенных аргументов
            if (inputWords.Count < 2)
            {
                output.PrintMessage(Areas.CommandInfoLine, Messages.WrongArguments);
                Console.ReadKey();
                return isSucces;
            }

            //Извлекаем имя файла который удаляем
            //Преобразуем путь к нему в асолютный (если необходимо)
            string sourceFileName = filesHandler.MakeFullPath(settings.LastPath, filesHandler.FindPath(inputWords, 1));

            //Проверить существует ли объект по пути источника
            if (!filesHandler.IsFileExist(sourceFileName))
            {
                output.PrintMessage(Areas.CommandInfoLine, Messages.WrongSourcePath);
                Console.ReadKey();
                return isSucces;
            }

            //Удаляем файл
            isSucces = filesHandler.FileDelete(sourceFileName);

            if (isSucces)
            {
                output.PrintMessage(Areas.CommandInfoLine, Messages.Success);
                Refresh(settings.LastPath);
            }
            else
                output.PrintMessage(Areas.CommandInfoLine, Messages.UnSuccess);
            Console.ReadKey();
            Refresh(settings.LastPath);

            return isSucces;
        }


        /// <summary>Удаление каталога в указанном расположении</summary>
        /// <param name="inputWords">Список содержащий слова из ввода пользователя</param>
        public bool DirDelete(List<string> inputWords)
        {
            bool isSucces = false;

            //Проверяем количество введенных аргументов
            if (inputWords.Count < 2)
            {
                output.PrintMessage(Areas.CommandInfoLine, Messages.WrongArguments);
                Console.ReadKey();
                return isSucces;
            }

            //Проверяем на то, что заданный путь не состоит только из символов '.','/','\'
            //Чтобы избежать конфликтных ситуаций с удалением каталогов выше текущего уровня
            bool isPathGood = false;
            foreach(char symbol in inputWords[1])
            {
                if (symbol != '.' && symbol != '/' && symbol != '\\')
                    isPathGood = true;
            }
            if (!isPathGood)
            {
                output.PrintMessage(Areas.CommandInfoLine, Messages.WrongSourcePath);
                Console.ReadKey();
                return isSucces;
            }


            //Извлекаем имя файла каталога удаляем
            //Преобразуем путь к нему в асолютный (если необходимо)
            string sourceDirName = filesHandler.MakeFullPath(settings.LastPath, filesHandler.FindPath(inputWords, 1));

            //Проверить существует ли объект по пути источника
            if (!filesHandler.IsDirExist(sourceDirName))
            {
                output.PrintMessage(Areas.CommandInfoLine, Messages.WrongSourcePath);
                Console.ReadKey();
                return isSucces;
            }

            //Удаляем файл
            isSucces = filesHandler.DirDelete(sourceDirName);

            if (isSucces)
            {
                output.PrintMessage(Areas.CommandInfoLine, Messages.Success);
                Refresh(settings.LastPath);
            }
            else
                output.PrintMessage(Areas.CommandInfoLine, Messages.UnSuccess);
            Console.ReadKey();
            Refresh(settings.LastPath);

            return isSucces;
        }

        public void Version()
        {
            output.PrintMessage(Areas.CommandInfoLine, Messages.Version, settings.Version);
            Console.ReadKey();

        }

        public void AppExit()
        {
            logger.AppClose();
            settings.SaveSettings();
        }

        public void Errors()
        {
            output.PrintList(Areas.DirList, logger.GetErrorsList());
        }



        #endregion
    }
}
