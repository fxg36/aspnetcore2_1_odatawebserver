using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ODataWebserver.Global
{
    public class ConfigHelper
    {
        private static ConfigHelper instance;
        public static ConfigHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ConfigHelper();
                }

                return instance;
            }
        }

        private readonly dynamic configParsed;

        private ConfigHelper()
        {
#if DEBUG
            var configFilePath = AppDomain.CurrentDomain.BaseDirectory + @"\config.json";
#else
            var d = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            configFilePath = d.GetFiles("config.json").Single().FullName;
#endif
            configParsed = JsonConvert.DeserializeObject(File.ReadAllText(configFilePath));
        }

        public string Project => configParsed.Project;
        public string DbHost => configParsed.DbHost;
        public string DbName => configParsed.DbName;
        public string DbUser => configParsed.DbUser;
        public string DbPassword => configParsed.DbPassword;
        public bool ApiLogging => configParsed.ApiLogging.Equals("True");
        public Dictionary<string, string> ApiConsumers
        {
            get
            {
                var dict = new Dictionary<string, string>();
                foreach (var entry in configParsed.ApiConsumers)
                {
                    var split = entry.ToString().Split(":");
                    dict.Add(split[0], split[1]);
                }
                return dict;
            }
        }
    }
}
