using System.Collections.Generic;

namespace ChampionsSkillDescriptionExpansion
{
    public class ChampionList
    {
        public Dictionary<string, Champion> Data { get; private set; }

        public ChampionList()
        {
            Data = new Dictionary<string, Champion>();
        }
    }
}
