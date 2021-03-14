using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace VFileManager
{
    /// <summary>
    /// Класс, позволяющий логгировать ошибки и исключения в лог-файл
    /// </summary>
    class Logger
    {
        #region ---- FIELDS ----

        /// <summary>Имя файла хранящего сообщения лога</summary>
        private string logFileName;

        #endregion

        #region ---- CONSTRUCTOR ----

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="logFileName">Имя лог файла</param>
        public Logger(string logFileName)
        {
            this.logFileName = logFileName;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(logFileName));
                StreamWriter logFile = new StreamWriter(new FileStream(logFileName, FileMode.Create));
                logFile.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Не удалось создать log-файл.");
                Console.WriteLine(e.Message);
                Console.WriteLine("Решите проблему и перезапустите приложениею");
                Console.ReadKey();
                Environment.Exit(1);
            }
            LogWrite("[Application start: " + DateTime.Now.ToString("F", new System.Globalization.CultureInfo("ru-Ru")) + "]");

        }

        #endregion

        #region ---- METHODS ----

        /// <summary>
        /// Записывает сообщение в лог-файл
        /// </summary>
        /// <param name="message">Сообщение</param>
        public void LogWrite(string message)
        {
            try
            {
                using(StreamWriter logFile = new StreamWriter(logFileName, true))
                {
                    logFile.WriteLine(message);
                }
            }
            catch
            {
                //!TODO не удалось записать сообщение в логфайл.
            }


        }

        /// <summary>
        /// Записывает в лог-файл информацию о закрытии приложения
        /// </summary>
        public void AppClose()
        {
            LogWrite("[Application shutdown: " + DateTime.Now.ToString("F", new System.Globalization.CultureInfo("ru-Ru")) + "]");
        }

        /// <summary>
        /// Извлекает информацию из лог-файла в список, для вывода на экран
        /// </summary>
        /// <returns>Список содержащий информацию из лог-файла</returns>
        public List<string> GetErrorsList()
        {
            List<string> errorsList = new List<string>();
            errorsList.Add("Содержимое лог-файла:");
            try
            {
                using(StreamReader logReader = new StreamReader(logFileName))
                {
                    string line = logReader.ReadLine();
                    while (line != null)
                    {
                        errorsList.Add(line);
                        line = logReader.ReadLine();
                    }
                }

            }
            catch
            {
                errorsList.Add("Не удалось прочитать лог файл.");
            }

            return errorsList;
        }

        #endregion
    }
}
