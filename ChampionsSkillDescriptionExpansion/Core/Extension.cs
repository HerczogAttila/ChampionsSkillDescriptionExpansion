using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChampionsSkillDescriptionExpansion
{
    public static class Extension
    {
        public static string MakeString<T>(this Collection<T> list)
        {
            if (list == null)
                return "";

            var sb = new StringBuilder();
            if (list.Distinct().Skip(1).Any())
            {
                foreach (var v in list)
                {
                    sb.Append("/");
                    sb.Append(v.ToString());
                }

                sb.Remove(0, 1);
            }
            else if (list.Count > 0)
                sb.Append(list[0]);

            return sb.ToString();
        }
        public async static Task<string> Download(this string link)
        {
            WebResponse response = null;
            Stream dataStream = null;
            StreamReader reader = null;
            string ve = "";
            try
            {
                var request = WebRequest.Create(link);
                response = await request.GetResponseAsync();
                dataStream = response.GetResponseStream();
                reader = new StreamReader(dataStream);
                ve = reader.ReadToEnd();
            }
            finally
            {
                if (reader != null)
                    reader.Dispose();
                if (dataStream != null)
                    dataStream.Dispose();
                if (response != null)
                    response.Dispose();
            }

            return ve;
        }
        public async static Task<string> DownloadAndSave(this string link, string path)
        {
            var data = await link.Download();
            File.WriteAllText(path, data);

            return data;
        }
    }
}
