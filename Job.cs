using System.Collections.Generic;
using System.Linq;
using Console = Colorful.Console;

namespace Linkedin_Scrapper
{
    /// <summary>
    /// saves a work data
    /// </summary>
    public class Job
    {
        public string title;
        public string dateIterval;
        public string empDuration;
        public List<string> months = new List<string>() { "Jan ", "Feb ", "Mar ", "Apr ", "May ", "Jun ", "Jul ", "Aug ", "Sep ", "Oct ", "Nov ", "Dec " };

        public Job(List<string> jobInfos)
        {
            title       = jobInfos.IndexOf("Title") != -1 ? jobInfos[jobInfos.IndexOf("Title") + 1] : "??";
            empDuration = jobInfos.IndexOf("Employment Duration") != -1 ? jobInfos[jobInfos.IndexOf("Employment Duration") + 1] : "??";
            dateIterval = jobInfos.IndexOf("Dates Employed")      != -1 ? jobInfos[jobInfos.IndexOf("Dates Employed") + 1]      : "?? – ??";
            if(empDuration.Contains("less than a year"))
            {
                dateIterval = dateIterval + " – " + dateIterval;
            }
            months.Where(mm => dateIterval.Contains(mm)).ToList().ForEach(existmm => dateIterval = dateIterval.Replace(existmm, ""));
        }

        public Job(string title, string dateIterval, string empDuration)
        {
            this.title       = title;
            this.dateIterval = dateIterval;
            this.empDuration = empDuration;
        }

        internal string PrintToConsole()
        {
            var ret =
                "\n │\t"   + title +
                "\n │\t" + dateIterval +
                "\n │\t" + empDuration + "\n │";
            Console.Write(ret);
            return ret;
        }
    }
}
