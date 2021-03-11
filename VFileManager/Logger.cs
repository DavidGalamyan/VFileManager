using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace VFileManager
{
    class Logger
    {
        #region ---- FIELDS ----

        /// <summary>Имя файла хранящего сообщения лога</summary>
        private string logFileName;

        private StreamWriter logFile;

        #endregion

        #region ---- CONSTRUCTOR ----
        public Logger(string logFileName)
        {
            this.logFileName = logFileName;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(logFileName));
                logFile = new StreamWriter(new FileStream(logFileName, FileMode.Create));
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

        public void AppClose()
        {
            LogWrite("[Application shutdown: " + DateTime.Now.ToString("F", new System.Globalization.CultureInfo("ru-Ru")) + "]");
        }

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
