using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VFileManager
{
    class Settings
    {
        #region ---- DEFAULT SETTINGS ----

        /// <summary>Дефолтное значение пути просматриваемого каталога</summary>
        private const string DEFAULT_PATH = "C:\\";


        /// <summary>Ширина окна приложения</summary>
        private const int APP_WIDTH = 120;
        /// <summary>Высота окна приложения</summary>
        private const int APP_HEIGHT = 40;

        /// <summary>Стандартная глубина для выводимого дерева каталогов</summary>
        private const int MAX_LEVEL_DEFAULT = 2;


        /// <summary>Номер строки в которой происходит вывод списка каталогов</summary>
        private const int DIRLIST_AREA_LINE = 1;
        /// <summary>Номер строки в которой происходит вывод списка файлов</summary>
        private const int FILELIST_AREA_LINE = 20;
        /// <summary>Номер строки в которой происходит вывод информации о файле/каталоге</summary>
        private const int INFO_AREA_LINE = 33;
        /// <summary>Номер строки в которой выводятся указания для пользователя</summary>
        private const int COMMAND_INFO_AREA_LINE = 36;
        /// <summary>Номер строки в которой происходит ввод комманд</summary>
        private const int COMMAND_AREA_LINE = 38;

        #endregion

        #region ---- FIELDS ----

        /// <summary>Имя файла с настройками</summary>
        private string settingsFileName;

        /// <summary>Ключи для словаря числовых настроек</summary>
        private enum SettingsKeys
        {
            AppWidth,
            AppHeight,
            MaxLevelDefault,
            DirListAreaLine,
            FileListAreaLine,
            InfoAreaLine,
            CommandInfoAreaLine,
            CommandAreaLine,
            LastPath,
        }

        /// <summary>Словарь содержащий текстовые настройки приложения</summary>
        private Dictionary<SettingsKeys, string> stringSettings = new Dictionary<SettingsKeys, string>
        {
            { SettingsKeys.LastPath, DEFAULT_PATH },
        };

        /// <summary>Словарь содержащий числовые настройки приложения</summary>
        private Dictionary<SettingsKeys, int> numericSettings = new Dictionary<SettingsKeys, int>
        {
            { SettingsKeys.AppWidth, APP_WIDTH },
            { SettingsKeys.AppHeight, APP_HEIGHT },
            { SettingsKeys.MaxLevelDefault, MAX_LEVEL_DEFAULT},
            { SettingsKeys.DirListAreaLine, DIRLIST_AREA_LINE},
            { SettingsKeys.FileListAreaLine, FILELIST_AREA_LINE},
            { SettingsKeys.InfoAreaLine, INFO_AREA_LINE},
            { SettingsKeys.CommandInfoAreaLine, COMMAND_INFO_AREA_LINE},
            { SettingsKeys.CommandAreaLine, COMMAND_AREA_LINE},
        };

        #endregion

        #region ---- PROPERTIES ----

        /// <summary>Имя параметра</summary>
        public string LastPath 
        {
            get 
            {
                return stringSettings[SettingsKeys.LastPath];
            }
            set 
            {
                stringSettings[SettingsKeys.LastPath] = value;
            } 
        }

        /// <summary>Ширина окна приложения</summary>
        public int AppWidth
        {
            get
            {
                return numericSettings[SettingsKeys.AppWidth];
            }
            set
            {
                numericSettings[SettingsKeys.AppWidth] = value;
            }
        }

        /// <summary>Высота окна приложения</summary>
        public int AppHeight
        {
            get
            {
                return numericSettings[SettingsKeys.AppHeight];
            }
            set
            {
                numericSettings[SettingsKeys.AppHeight] = value;
            }
        }

        /// <summary>Стандартная глубина для выводимого дерева каталогов</summary>
        public int MaxLevelDefault
        {
            get
            {
                return numericSettings[SettingsKeys.MaxLevelDefault];
            }
            set
            {
                numericSettings[SettingsKeys.MaxLevelDefault] = value;
            }
        }

        /// <summary>Номер строки в которой происходит вывод списка каталогов</summary>
        public int DirListAreaLine
        {
            get
            {
                return numericSettings[SettingsKeys.DirListAreaLine];
            }
            set
            {
                numericSettings[SettingsKeys.DirListAreaLine] = value;
            }
        }

        /// <summary>Номер строки в которой происходит вывод списка файлов</summary>
        public int FileListAreaLine
        {
            get
            {
                return numericSettings[SettingsKeys.FileListAreaLine];
            }
            set
            {
                numericSettings[SettingsKeys.FileListAreaLine] = value;
            }
        }

        /// <summary>Номер строки в которой происходит вывод информации для пользователя</summary>
        public int InfoAreaLine
        {
            get
            {
                return numericSettings[SettingsKeys.InfoAreaLine];
            }
            set
            {
                numericSettings[SettingsKeys.InfoAreaLine] = value;
            }
        }

        /// <summary>Номер строки в которой происходит ввод комманд</summary>
        public int CommandAreaLine
        {
            get
            {
                return numericSettings[SettingsKeys.CommandAreaLine];
            }
            set
            {
                numericSettings[SettingsKeys.CommandAreaLine] = value;
            }
        }

        /// <summary>Номер строки в которой выводятся указания для пользователя</summary>
        public int CommandInfoAreaLine
        {
            get
            {
                return numericSettings[SettingsKeys.CommandInfoAreaLine];
            }
            set
            {
                numericSettings[SettingsKeys.CommandInfoAreaLine] = value;
            }
        }

        #endregion

        #region ---- HELPER CLASSES ----

        /// <summary>Используется для сохранения/загрузки настроек приложения
        /// чтобы в файле настроек было более удобное текстовое представление параметров</summary>
        public class Parameter
        {
            /// <summary>Имя параметра</summary>
            public string Key { get; set; }
            /// <summary>Значение параметра</summary>
            public string Value { get; set; }

            public Parameter()
            {
                this.Key = string.Empty;
                this.Value = string.Empty;
            }

            public Parameter(string key, string value)
            {
                this.Key = key;
                this.Value = value;
            }
        }

        #endregion

        #region ---- CONSTRUCTOR ----

        public Settings(string settingsFileName)
        {
            this.settingsFileName = settingsFileName;
        }

        #endregion

        #region ---- METHODS ----


        /// <summary>Сохранение настроек в файл</summary>
        public void SaveSettings()
        {
            //Список для работы с настройками приложения
            List<Parameter> parameters = new List<Parameter>();

            //Перекидываем все параметры в промежуточный список
            foreach (SettingsKeys key in Enum.GetValues(typeof(SettingsKeys)))
            {
                if (numericSettings.ContainsKey(key))
                {
                    parameters.Add(new Parameter(key.ToString(), numericSettings[key].ToString()));
                }
                if (stringSettings.ContainsKey(key))
                {
                    parameters.Add(new Parameter(key.ToString(), stringSettings[key].ToString()));
                }
            }

            //Сериализуем его
            string line = JsonSerializer.Serialize(parameters, new JsonSerializerOptions { WriteIndented = true});
            //И сохраняем на диск
            try
            {
                File.WriteAllText(settingsFileName, line);
            }
            catch
            {
                //!TODO сделать обработку исключения
            }

        }


        /// <summary>Чтение настроек из файла</summary>
        public void LoadSettings()
        {
            //Список для работы с настройками приложения
            List<Parameter> parameters = new List<Parameter>();

            if (File.Exists(settingsFileName))
            {
                try
                {
                    parameters = JsonSerializer.Deserialize<List<Parameter>>(File.ReadAllText(settingsFileName));
                    ReadSettings(parameters);
                }
                catch
                {
                    //!TODO не удалось прочитать файл
                }
            }
            else
            {
                SaveSettings();
            }


        }

        /// <summary>Парсинг параметров из списка в словарь</summary>
        /// <param name="parameters">Список с текстовым представлением параметров и их значений</param>
        private void ReadSettings(List<Parameter> parameters)
        {

            foreach (Parameter param in parameters)
            {
                SettingsKeys key;//Сюда пойдет ключ
                int value;//Сюда пойдет значение
                bool isValueValid;//Проверка на то, что значение парсится
                try
                {
                    key = (SettingsKeys)Enum.Parse(typeof(SettingsKeys), param.Key);//Получили ключ (если не верный, то в обработку эксепшена)
                    //Если такая настройка есть среди текстовых переменных, то заносим ее в соответствующий словарь
                    if (stringSettings.ContainsKey(key))
                        stringSettings[key] = param.Value;
                    //Если такая настройка есть среди числовых переменных, то заносим ее в соответствующий словарь
                    if (numericSettings.ContainsKey(key))
                    {
                        isValueValid = int.TryParse(param.Value, out value);//Пытаемся получить числовое значение
                        if (isValueValid)//Если получилось, то заносим настройку в словарь
                            numericSettings[key] = value;
                    }

                }
                catch
                {
                    //!TODO если такого ключа нет среди настроек
                }
            }

            //Проверка некоторых параметров на валидность
            //!TODO добавить больше проверок
            if (numericSettings[SettingsKeys.AppWidth] < APP_WIDTH)//Ширина приложения
                numericSettings[SettingsKeys.AppWidth] = APP_WIDTH;
            if (numericSettings[SettingsKeys.AppHeight] < APP_HEIGHT)//Высота приложения
                numericSettings[SettingsKeys.AppHeight] = APP_HEIGHT;
            if (numericSettings[SettingsKeys.CommandAreaLine] > numericSettings[SettingsKeys.AppHeight] - 2)//Положение командной строки
                numericSettings[SettingsKeys.CommandAreaLine] = numericSettings[SettingsKeys.AppHeight] - 2;
            if (numericSettings[SettingsKeys.InfoAreaLine] > numericSettings[SettingsKeys.CommandAreaLine] - 4)//Положение информационной строки
                numericSettings[SettingsKeys.InfoAreaLine] = numericSettings[SettingsKeys.CommandAreaLine] - 4;
        }

        #endregion
    }
}
