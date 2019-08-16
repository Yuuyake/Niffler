using System.Collections.Generic;
using System.Linq;
using Console = Colorful.Console;
using System.Drawing;

namespace Linkedin_Scrapper
{
    /// <summary>
    /// saves an Experience data ( meaning works on a company )
    /// </summary>
    public class Exp
    {
        public string companyName;
        public string totalDuration;
        public string dateIterval;
        public List<Job> jobs = new List<Job>(){ };
        public List<string> months = new List<string>() { "Jan ", "Feb ", "Mar ", "Apr ", "May ", "Jun ", "Jul ", "Aug ", "Sep ", "Oct ", "Nov ", "Dec " };

        public Exp(List<string> exp)
        {
            int companyIndex = exp.IndexOf("Company Name");
            companyName = companyIndex != -1 ? exp[companyIndex + 1] : "??";
            if (exp.IndexOf("Title") == -1 && companyIndex != -1) // First and only job in this Company, must be handled differently
            {
                totalDuration = exp.IndexOf("Employment Duration") != -1 ? exp[exp.IndexOf("Employment Duration") + 1] : "??";
                exp.RemoveRange(companyIndex, 2);
                exp.Insert(0, "Title");
            }
            else
            {
                totalDuration = exp.IndexOf("Total Duration") != -1 ? exp[exp.IndexOf("Total Duration") + 1] : "??";
                if (exp.IndexOf("Company Name") > -1)
                    exp.RemoveRange(0, exp.IndexOf("Title"));
            }

            int jobAmount = exp.Count(ss => ss == "Title");
            for (var i = 0; i < jobAmount; i++)
            {
                var indexOfFirsExp = exp.IndexOf("Title");
                if (jobAmount == 1)
                    jobs.Add(new Job(exp));
                else
                {
                    var indexOfNextExp = exp.IndexOf("Title", exp.IndexOf("Title") + 1);
                    jobs.Add(new Job(exp.GetRange(indexOfFirsExp, indexOfNextExp == -1 ? exp.Count : indexOfNextExp)));
                    exp.RemoveRange(indexOfFirsExp, indexOfNextExp == -1 ? exp.Count : indexOfNextExp);
                }
            }

            var tempStartDates = jobs.Select(jj => jj.dateIterval.Split('–')[0]).ToList();
            var tempEndDates   = jobs.Select(jj => jj.dateIterval.Split('–')[1]).ToList();
            tempStartDates.Sort();
            tempEndDates.Sort();
            dateIterval = tempStartDates.First() + " to " + tempEndDates.Last();
            months.Where(mm => dateIterval.Contains(mm)).ToList().ForEach(existmm => dateIterval = dateIterval.Replace(existmm,""));
        }
        internal string PrintToConsole()
        {
            var ret = "\n │\t" + companyName + " >> " + dateIterval;
            Console.Write(ret,Color.LightGoldenrodYellow);
            foreach (Job job in jobs)
                job.PrintToConsole();
            return ret;
        }
    }
}
