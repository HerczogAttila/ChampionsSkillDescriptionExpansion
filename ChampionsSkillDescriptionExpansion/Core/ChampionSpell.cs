using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace ChampionsSkillDescriptionExpansion
{
    public class ChampionSpell
    {
        //public Collection<double> Cooldown { get; private set; }
        public string CoolDownBurn { get; set; }
        //public Collection<int> Cost { get; private set; }
        public string CostBurn { get; set; }
        public string CostType { get; set; }
        //public string Description { get; set; }
        public Collection<string> EffectBurn { get; private set; }
        //public string Key { get; set; }
        //public LevelTipDto Leveltip { get; set; }
        //public int Maxrank { get; set; }
        public string Name { get; set; }
        //public Collection<string> Range { get; set; }
        public string RangeBurn { get; set; }
        public string Resource { get; set; }
        //public string SanitizedDescription { get; set; }
        //public string SanitizedTooltip { get; set; }
        public string Tooltip { get; set; }
        [JsonProperty("Vars")]
        public Collection<SpellVariables> Variables { get; private set; }

        public ChampionSpell()
        {
            //Cooldown = new Collection<double>();
            //Cost = new Collection<int>();
            EffectBurn = new Collection<string>();
            //Range = new Collection<string>();
            Variables = new Collection<SpellVariables>();
        }

        private static string ColorValue = "<font color='#5882FA'>";
        private static int error = 0;

        private static Collection<string> percentlist = new Collection<string>(new string[] { "spelldamage", "bonusattackdamage", "attackdamage", "armor", "bonushealth" });

        private string ReplaceVariable(string s, LanguageStrings Languages)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            s = s.Replace(Languages.Get("Cost_"), "");
            s = s.Replace("{{ cost }}", CostBurn);

            var i = 0;
            foreach (var v in EffectBurn)
            {
                s = s.Replace("@Effect" + i + "Amount*2@", v);
                /*if (string.IsNullOrEmpty(v) && i > 0)
                    Console.WriteLine(Name + ": e" + i + " / " + v);*/
                i++;
            }

            i = 0;
            foreach (var v in EffectBurn)
            {
                s = s.Replace("{{ e" + i + " }}", v);
                i++;
            }

            string temp;
            Collection<string> coeff;
            foreach (var v in Variables)
            {
                /*if (!list.Contains(v.Link))
                    list.Add(v.Link + ": " + v.Coeff.ToDesc());*/

                coeff = new Collection<string>();
                if (percentlist.Contains(v.Link))
                {
                    foreach (var v2 in v.Coefficient)
                        coeff.Add(v2 * 100 + "%");


                    temp = coeff.MakeString();
                }
                else
                    temp = v.Coefficient.MakeString();
                    

                var link = Languages.Get(v.Link.ToLower(CultureInfo.CurrentCulture));
                if (string.IsNullOrEmpty(link))
                    s = s.Replace("{{ " + v.Key + " }}", temp);
                else
                    s = s.Replace("{{ " + v.Key + " }}", temp + " " + link);

                /*if (string.IsNullOrEmpty(temp) && i > 0)
                    Console.WriteLine(Name + ": " + v.Key + " / " + v.Coeff.ToDesc());*/
                i++;
            }

            return s;
        }

        public string MakeDescription(LanguageStrings languages)
        {
            if (languages == null)
                return "";

            var sb = new StringBuilder();

            //cooldown
            sb.Append(languages.Get("CD_"));
            sb.Append(" ");
            sb.Append(ColorValue);
            sb.Append(CoolDownBurn);
            sb.Append("</font>");

            //cost
            if (!string.IsNullOrEmpty(Resource))
            {
                var cost = ReplaceVariable(Resource, languages);

                sb.Append("\t");
                sb.Append(languages.Get("Cost_"));
                sb.Append(" ");
                sb.Append(ColorValue);
                sb.Append(" ");
                sb.Append(cost);
                sb.Append("</font>");
            }

            //range
            sb.Append("\t");
            sb.Append(languages.Get("Range_"));
            sb.Append(" ");
            sb.Append(ColorValue);
            sb.Append(RangeBurn);
            sb.Append("</font>");

            //desc
            sb.Append("<br>");
            var desc = Tooltip.Replace("span", "font").Replace("color", "#").Replace("class", "color").Replace("\"", "'");
            desc = ReplaceVariable(desc, languages);

            if (desc.IndexOf("{{", StringComparison.Ordinal) > -1)
            {
                error++;
                Console.WriteLine(Name + ": Fel nem használt paraméter!error_count:" + error);
                //sb.Clear();
                //sb.Append(Description);
            }

            int start, end;
            start = desc.IndexOf("{{", StringComparison.Ordinal);
            while (start > -1)
            {
                end = desc.IndexOf("}}", StringComparison.Ordinal);
                desc = desc.Remove(start, end - start + 2);
                start = desc.IndexOf("{{", StringComparison.Ordinal);
            }

            sb.Append(desc);

            return sb.ToString();
        }
    }
}