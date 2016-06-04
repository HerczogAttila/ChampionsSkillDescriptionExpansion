using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace ChampionsSkillDescriptionExpansion
{
    public class SpellVariables
    {
        [JsonProperty("Coeff")]
        public Collection<double> Coefficient { get; private set; }
        //public string Dyn { get; set; }
        public string Key { get; set; }
        public string Link { get; set; }
        //public string RanksWith { get; set; }

        public SpellVariables()
        {
            Coefficient = new Collection<double>();
        }
    }
}