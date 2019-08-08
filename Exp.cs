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
        public List<Job> jobs = new List<Job> { };
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
                jobs.Add(new Job(exp.GetRange(exp.IndexOf("Title"), 6)));
                var indexOfSecondExp = exp.IndexOf("Title", exp.IndexOf("Title") + 1);
                if (indexOfSecondExp != -1)
                    exp.RemoveRange(exp.IndexOf("Title"), indexOfSecondExp);
            }

            var tempStartDates = jobs.Select(jj => jj.dateIterval.Split('–')[0]).ToList();
            var tempEndDates = jobs.Select(jj => jj.dateIterval.Split('–')[1]).ToList();
            tempStartDates.Sort();
            tempEndDates.Sort();
            dateIterval = tempStartDates.First() + " to " + tempEndDates.Last();
            months.Where(mm => dateIterval.Contains(mm)).ToList().ForEach(existmm => dateIterval = dateIterval.Replace(existmm,""));
        }
        internal string expPrint()
        {
            var ret = "\r\n\t>>" + companyName + "\t" + dateIterval;
            Console.WriteLine(ret,Color.LightGoldenrodYellow);
            foreach (Job job in jobs)
                job.eprint();
            return ret;
        }
    }
}
