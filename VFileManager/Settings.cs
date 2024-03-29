﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VFileManager
{
    /// <summary>
    /// Класс хранит и оперирует настройки приложения.
    /// </summary>
    class Settings
    {
        #region ---- FIELDS ----

        /// <summary>Имя файла с настройками</summary>
        private string settingsFileName;

        /// <summary>Логгер</summary>
        private Logger logger;
        /// <summary>База текстовых сообщений</summary>
        private MessagesBase messages = new MessagesBase();

        /// <summary>Ключи для словаря числовых настроек</summary>
        private enum SettingsKeys
        {
            Version,
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
        private Dictionary<SettingsKeys, string> stringSettings;

        /// <summary>Словарь содержащий числовые настройки приложения</summary>
        private Dictionary<SettingsKeys, int> numericSettings;

        #endregion

        #region ---- PROPERTIES ----

        /// <summary>Ссылка на логгер</summary>
        public Logger Logger
        {
            get
            {
                return logger;
            }
        }

        /// <summary>Ссылка на базу сообщений</summary>
        public MessagesBase Messages
        {
            get
            {
                return messages;
            }
        }


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

        public string Version
        {
            get
            {
                return stringSettings[SettingsKeys.Version];
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

        /// <summary>
        /// Конструктор 
        /// </summary>
        /// <param name="settingsFileName">Имя файла хранящего настройки</param>
        public Settings(string settingsFileName)
        {
            this.settingsFileName = settingsFileName;
            InitSettings();
            string logFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "errors", $"{AppDomain.CurrentDomain.FriendlyName}.log");
            logger = new Logger(logFileName);
        }

        #endregion

        #region ---- METHODS ----

        /// <summary>
        /// Инициализация дефолтных значений настроек приложения
        /// </summary>
        private void InitSettings()
        {
            /// <summary>Словарь содержащий текстовые настройки приложения</summary>
            stringSettings = new Dictionary<SettingsKeys, string>
            {
                { SettingsKeys.Version, Properties.Settings.Default.Version },
                { SettingsKeys.LastPath, AppContext.BaseDirectory },
            };

            /// <summary>Словарь содержащий числовые настройки приложения</summary>
            numericSettings = new Dictionary<SettingsKeys, int>
            {
                { SettingsKeys.AppWidth, Properties.Settings.Default.AppWidth },
                { SettingsKeys.AppHeight, Properties.Settings.Default.AppHeight },
                { SettingsKeys.MaxLevelDefault, Properties.Settings.Default.MaxLevelDefault },
                { SettingsKeys.DirListAreaLine, Properties.Settings.Default.DirListAreaLine },
                { SettingsKeys.FileListAreaLine, Properties.Settings.Default.FileListAreaLine },
                { SettingsKeys.InfoAreaLine, Properties.Settings.Default.InfoAreaLine },
                { SettingsKeys.CommandInfoAreaLine, Properties.Settings.Default.CommandInfoAreaLine },
                { SettingsKeys.CommandAreaLine, Properties.Settings.Default.CommandAreaLine },
            };

        }

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
            catch (Exception e)
            {
                logger.LogWrite($"Method: |SaveSettings: *{e.Message}");
            }

        }

        /// <summary>Чтение настроек из файла
        /// Если файл отсутствует, то он создается заново с дефолтными настройками
        /// Если версия приложения в файле настроки не соответствует текущей версии приложения, то 
        /// файл настроек пересоздается с дефолтными настройками</summary>
        public void LoadSettings()
        {
            //Список для работы с настройками приложения
            List<Parameter> parameters = new List<Parameter>();

            if (File.Exists(settingsFileName))
            {
                try
                {
                    parameters = JsonSerializer.Deserialize<List<Parameter>>(File.ReadAllText(settingsFileName));
                    //Распихиваем содержимое конфига по настройкам
                    ReadSettings(parameters);
                }
                catch (Exception e)
                {
                    logger.LogWrite($"Method: |LoadSettings: *{e.Message}");
                }
            }
            else
            {
                SaveSettings();
            }

            //Если файл настроек от другой версии приложения, то перезаписываем его дефолтным для текущей версии
            if(stringSettings[SettingsKeys.Version] != Properties.Settings.Default.Version)
            {
                InitSettings();
                SaveSettings();
            }

        }

        /// <summary>Парсинг параметров из списка в словарь</summary>
        /// <param name="parameters">Список с текстовым представлением параметров и их значений</param>
        private void ReadSettings(List<Parameter> parameters)
        {
            //Сбрасываем номер версии для проверки фала настроек в дальнейшем
            stringSettings[SettingsKeys.Version] = string.Empty;

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
                            value = Math.Abs(value);//Убираем отрицательные значения, т.к. их в принципе быть не должно
                            numericSettings[key] = value;
                    }

                }
                catch (Exception e)
                {
                    logger.LogWrite($"Method: |ReadSettings: *{e.Message}");
                }
            }

            //Проверка некоторых параметров на валидность
            if (numericSettings[SettingsKeys.AppWidth] < Properties.Settings.Default.AppWidth)//Ширина приложения
                numericSettings[SettingsKeys.AppWidth] = Properties.Settings.Default.AppWidth;
            if (numericSettings[SettingsKeys.AppHeight] < Properties.Settings.Default.AppHeight)//Высота приложения
                numericSettings[SettingsKeys.AppHeight] = Properties.Settings.Default.AppHeight;
            if (numericSettings[SettingsKeys.CommandAreaLine] > numericSettings[SettingsKeys.AppHeight] - 2)//Положение командной строки
                numericSettings[SettingsKeys.CommandAreaLine] = numericSettings[SettingsKeys.AppHeight] - 2;
            if (numericSettings[SettingsKeys.CommandInfoAreaLine] > numericSettings[SettingsKeys.CommandAreaLine] - 2)//Положение информационной строки
                numericSettings[SettingsKeys.CommandInfoAreaLine] = numericSettings[SettingsKeys.CommandAreaLine] - 2;
            if (numericSettings[SettingsKeys.InfoAreaLine] > numericSettings[SettingsKeys.CommandInfoAreaLine] - 4)//Положение окна информации
                numericSettings[SettingsKeys.InfoAreaLine] = numericSettings[SettingsKeys.CommandInfoAreaLine] - 4;
            if (numericSettings[SettingsKeys.FileListAreaLine] > numericSettings[SettingsKeys.InfoAreaLine] - 4)//Положение списка файлов
                numericSettings[SettingsKeys.FileListAreaLine] = numericSettings[SettingsKeys.InfoAreaLine] - 4;
            numericSettings[SettingsKeys.DirListAreaLine] = Properties.Settings.Default.DirListAreaLine;//Окно дерева каталогов
            if (numericSettings[SettingsKeys.FileListAreaLine] < numericSettings[SettingsKeys.DirListAreaLine] + 10)//Положение списка файлов
                numericSettings[SettingsKeys.FileListAreaLine] = numericSettings[SettingsKeys.DirListAreaLine] + 10;

            //Проверка дефолтного каталога
            if (!new DirectoryInfo(stringSettings[SettingsKeys.LastPath]).Exists)
            {
                //Если отсутствует, то меняем на тот из которого запускалась программа.
                stringSettings[SettingsKeys.LastPath] = AppContext.BaseDirectory;
            }
        }

        #endregion
    }
}
