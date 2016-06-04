using System;
using System.Collections.Generic;

namespace ChampionsSkillDescriptionExpansion
{
    public class LanguageStrings
    {
        public Dictionary<string, string> Data { get; private set; }

        public LanguageStrings()
        {
            Data = new Dictionary<string, string>();
        }

        public string Get(string key)
        {
            string value = "";
            Data.TryGetValue(key, out value);
            return value;
        }
        public string GetLang(string key)
        {
            if (key == null)
                return "";

            var i = key.IndexOf("_", StringComparison.Ordinal);
            if (i == -1)
                return "";

            key = "native_" + key.Remove(i);
            string value = "";
            Data.TryGetValue(key, out value);

            return value;
        }
    }
}
