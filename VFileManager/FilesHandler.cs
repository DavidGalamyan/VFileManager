using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace VFileManager
{
    class FilesHandler
    {
        #region ---- FIELDS ----

        Settings settings;
        MessagesBase messages;

        #endregion

        #region ---- CONSTRUCTORS ----

        public FilesHandler(Settings settings, MessagesBase messages)
        {
            this.settings = settings;
            this.messages = messages;
        }

        #endregion

        #region ---- CHECKS ----

        /// <summary>
        /// Ищет путь к файлу или каталогу в списке слов
        /// </summary>
        /// <param name="words">Список слов</param>
        /// <param name="number">Порядковый номер слова в списке которое надо проанализировать на наличие пути</param>
        /// <returns>Строку с найденым путем или пустую строку если путь не найден</returns>
        public string FindPath(List<string> words, int number)
        {
            string path = string.Empty;//Здесь будет найденный путь

            if (number < words.Count) //Есть ли в списке слово под нужным индексом
            {
                if (IsPathValid(words[number]))//Проверяем на отсутвие запрещенных символов
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
        public bool IsPathValid(string path)
        {
            return (path != null) && (path.IndexOfAny(Path.GetInvalidPathChars()) == -1);
        }

        /// <summary>
        /// Проверяет существует ли указанный файл
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <returns>true, если файл существует</returns>
        public bool IsFileExist(string fileName)
        {
            bool isExist = false;
            //Если имя файла не пустое и не содержит недопустимых символов
            if (IsPathValid(fileName))
                try
                {
                    FileInfo tempFileInfo = new FileInfo(fileName);
                    isExist = tempFileInfo.Exists;
                }
                catch
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
        public bool IsDirExist(string dirName)
        {
            bool isExist = false;
            //Если имя файла не пустое и не содержит недопустимых символов
            if (IsPathValid(dirName))
                if (Directory.Exists(dirName))
                    isExist = true;
            return isExist;
        }

        /// <summary>
        /// Проверяет чем является объект по указанному пути - файлом или каталогом
        /// Путь должен быть заранее проверен на валидность
        /// </summary>
        /// <param name="path">Путь для проверки</param>
        /// <returns>true, если объект является файлом
        /// false, если объект является каталогом</returns>
        public bool IsPathFile(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            return fileInfo.Exists;
        }

        /// <summary>
        /// Собирает из относительного и полного пути (корневого каталога) один полный
        /// </summary>
        /// <param name="rootPath">Путь к корневому каталогу</param>
        /// <param name="path">Относительный путь к каталогу/файлу</param>
        /// <returns>Полный путь к файлу/каталогу</returns>
        public string MakeFullPath(string rootPath, string path)
        {
            if (path == null)
                return rootPath;
            else if (path == "..")
                return (Path.GetDirectoryName(rootPath));
            else
                return (Path.Combine(rootPath, path));
        }

        #endregion

        #region ---- WORK WITH FILES ----

        /// <summary>
        /// Формирует список каталогов для вывода на экран,
        /// рекурсивно просматривая содержимое заданного каталога
        /// </summary>
        /// <param name="path">путь к просматриваемому каталогу</param>
        /// <param name="dirList">Список в который будет помещено дерево каталогов</param>
        /// <param name="maxLevel">максимальная глубина сканирования каталогов</param>
        /// <param name="graphLine">строка содержащая графическую структуру каталогов
        /// формируется во время работы метода</param>
        /// <param name="level">уровень глубины катлогов от изначального</param>
        public void SeekDirectoryRecursion(string path, List<string> dirList, int maxLevel = 0, string graphLine = "", int level = 1)
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

                        //Формируем представление в списке очередного каталога
                        StringBuilder name = new StringBuilder();
                        name.Append((char)Output.ColorSymbols.Argument);//Цвет структуры дерева каталогов
                        //Префикс
                        name.Append(graphLine);
                        if (dir == dirContent[dirContent.Length - 1])
                            name.Append("└──");//Если последний элемент в каталоге
                        else
                            name.Append("├──");//Если нет
                        name.Append((char)Output.ColorSymbols.Standart);//Цвет имени каталога
                        name.Append(dir);//Само имя каталога

                        //Заносим получившееся в список
                        dirList.Add(name.ToString());

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
                                        SeekDirectoryRecursion(path + "\\" + dir, dirList, maxLevel, graphLine + "│  ", level + 1);
                                    else
                                        SeekDirectoryRecursion(path + "\\" + dir, dirList, maxLevel, graphLine + "   ", level + 1);
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
        /// <param name="fileList">Список в который будет помещено содержимое каталога</param>
        public void SeekDirectoryForFiles(string path, List<string> fileList)
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


        /// <summary>
        /// Заносит информацию о файле/каталоге в список для вывода на экран
        /// </summary>
        /// <param name="path">Полный путь к файлу/каталогу</param>
        /// <param name="fileInfo">Список в который нужно поместить информацию о файле</param>
        public void GetInfo(string path, List<string> fileInfo)
        {
            if (IsFileExist(path))
            {
                fileInfo.Add(messages[Messages.FileInfo] + path);
                FileInfo info = new FileInfo(path);
                fileInfo.Add(messages[Messages.CreationDate] + info.CreationTime);
                fileInfo.Add(messages[Messages.Attributes] + info.Attributes);
                fileInfo.Add(messages[Messages.Size] + info.Length + messages[Messages.Bytes]);
            }
            else if (IsDirExist(path))
            {
                fileInfo.Add(messages[Messages.DirInfo] + path);
                DirectoryInfo info = new DirectoryInfo(path);
                fileInfo.Add(messages[Messages.CreationDate] + info.CreationTime);
                fileInfo.Add(messages[Messages.Attributes] + info.Attributes);
            }
            else
            {
                fileInfo.Add(messages[Messages.PathNotExist] + path);
            }


        }

        /// <summary>
        /// Копирует файл с отображением прогресс бара
        /// Заданнные пути файлов должны быть прловрены.
        /// </summary>
        /// <param name="sourceFileName">Имя копируемый файл</param>
        /// <param name="destFileName">Имя конечного файла</param>
        /// <returns>true, если процесс копирования прошел успешно.</returns>
        public bool FileCopy(string sourceFileName, string destFileName)
        {
            bool isSucces = true;

            //Потоки для файлов
            FileStream source = null;
            FileStream destination = null;

            int data;//Сюда будет заноситься очередной байт для копирования

            try
            {
                FileInfo destFileInfo = new FileInfo(destFileName);
                //Если каталога назначения нету, то создадим его
                Directory.CreateDirectory(destFileInfo.DirectoryName);

                source = new FileStream(sourceFileName, FileMode.Open);
                destination = new FileStream(destFileName, FileMode.Create);

                long length = source.Length;//Длина файла в байтах для прогресс бара.
                long current = 0;//Счетчик скопированных байтов.
                int percent = 0;
                int percentOld = 0;

                //Копирование файлов побайтно
                data = source.ReadByte();
                while (data != -1)
                {
                    //Копирование
                    destination.WriteByte((byte)data);
                    data = source.ReadByte();

                    //Прогресс бар !TODO переделать.
                    current++;
                    percentOld = percent;
                    percent = (int)(current * 100 / length);
                    if (percent>percentOld)
                    {
                        Console.SetCursorPosition(1, settings.CommandAreaLine);
                        Console.WriteLine($"Copied {percent}% from 100%                                    ");
                    }

                }
            }
            catch
            {
                //!TODO если ошибка при копировании
                isSucces = false;
            }
            finally
            {
                // Закрытие файлов
                if (source != null) source.Close();
                if (destination != null) destination.Close();
            }
            return isSucces;

        }

        /// <summary>
        /// Перемещает указанный файл
        /// Заданнные пути файлов должны быть прловрены.
        /// </summary>
        /// <param name="sourceFileName">Имя копируемый файл</param>
        /// <param name="destFileName">Имя конечного файла</param>
        /// <returns>true, если процесс перемещения прошел успешно.</returns>
        public bool FileMove(string sourceFileName, string destFileName)
        {
            bool isSucces = true;

            try
            {
                FileInfo destFileInfo = new FileInfo(destFileName);
                //Если каталога назначения нету, то создадим его
                Directory.CreateDirectory(destFileInfo.DirectoryName);

                File.Move(sourceFileName, destFileName);
            }
            catch
            {
                isSucces = false;
                //!TODO
            }

            return isSucces;
        }

        /// <summary>
        /// Удаляет указанный файл
        /// Заданнные пути к файлу должен быть прловрен.
        /// </summary>
        /// <param name="sourceFileName">Имя копируемый файл</param>
        /// <returns>true, если процесс перемещения прошел успешно.</returns>
        public bool FileDelete(string sourceFileName)
        {
            bool isSucces = true;

            try
            {
                File.Delete(sourceFileName);
            }
            catch
            {
                isSucces = false;
                //!TODO
            }

            return isSucces;
        }


        public bool DirCopyMove(string sourceDirName, string destDirName, bool isMove = false)
        {
            bool isSuccess = true;

            DirectoryInfo sourceDir = new DirectoryInfo(sourceDirName);

            DirectoryInfo[] dirContent = sourceDir.GetDirectories();

            Directory.CreateDirectory(destDirName);

            FileInfo[] dirFiles = sourceDir.GetFiles();
            foreach (FileInfo currentFile in dirFiles)
            {
                string tempDestPath = Path.Combine(destDirName, currentFile.Name);
                if (!isMove)
                    FileCopy(currentFile.FullName, tempDestPath);
                else
                    FileMove(currentFile.FullName, tempDestPath);
            }

            foreach(DirectoryInfo currentDir in dirContent)
            {
                string tempDestPath = Path.Combine(destDirName, currentDir.Name);
                DirCopyMove(currentDir.FullName, tempDestPath, isMove);
            }

            if (isMove)
                try
                {
                    Directory.Delete(sourceDir.FullName);
                }
                catch
                {
                    isSuccess = false;
                    //!TODO
                }

            return isSuccess;
        }

        /// <summary>
        /// Удаляет указанный каталог
        /// Заданнный путь к каталогу должен быть прловрен.
        /// </summary>
        /// <param name="sourceDirName">Путь к удаляемому каталогу</param>
        /// <returns>true, если процесс удаления прошел успешно.</returns>
        public bool DirDelete(string sourceDirName)
        {
            bool isSucces = true;

            //Информация о текущем каталоге
            DirectoryInfo sourceDir = new DirectoryInfo(sourceDirName);

            //Получаем все подкаталоги текущего каталога 
            DirectoryInfo[] dirContent = sourceDir.GetDirectories();
            //И если они есть, то вызываем их удаление рекурсивно
            foreach (DirectoryInfo currentDir in dirContent)
            {
                DirDelete(currentDir.FullName);
            }

            //Получаем список файлов в текущем каталоге
            FileInfo[] dirFiles = sourceDir.GetFiles();
            //И если они есть, то удаляем их
            foreach (FileInfo currentFile in dirFiles)
            {
                isSucces = FileDelete(currentFile.FullName);
            }

            try
            {
                Directory.Delete(sourceDirName);
            }
            catch
            {
                isSucces = false;
                //!TODO
            }

            return isSucces;
        }


        #endregion
    }
}
