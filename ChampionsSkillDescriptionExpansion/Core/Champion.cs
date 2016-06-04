using System.Collections.ObjectModel;

namespace ChampionsSkillDescriptionExpansion
{
    public class Champion
    {
        public string Id { get; set; }
        public string Key { get; set; }
        public Collection<ChampionSpell> Spells { get; private set; }
        //public PassiveDto Passive { get; set; }

        public Champion()
        {
            Spells = new Collection<ChampionSpell>();
        }
    }
}