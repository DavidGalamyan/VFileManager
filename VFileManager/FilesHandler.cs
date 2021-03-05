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

        #endregion

        #region ---- CONSTRUCTORS ----

        public FilesHandler(Settings settings)
        {
            this.settings = settings;
        }

        #endregion

        #region ---- CHECKS ----

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


        public void GetInfo(string path, List<string> fileInfo)
        {
            fileInfo.Add("Информация о файле");
            if (IsFileExist(path))
            {
                FileInfo info = new FileInfo(path);
                fileInfo.Add(info.FullName);
            }
            else if (IsDirExist(path))
            {
                DirectoryInfo info = new DirectoryInfo(path);
                fileInfo.Add(info.FullName);
            }

        }


        #endregion
    }
}
