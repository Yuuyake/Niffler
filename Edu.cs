using System.Collections.Generic;
using Console = Colorful.Console;
using System.Drawing;

namespace Linkedin_Scrapper
{
    /// <summary>
    /// saves an Education data
    /// </summary>
    public class Edu
    {
        public string schoolName;
        public string date;
        public string field;
        public string xx;
        public Edu(string schoolName = "??", string date = "??", string field = "??", string xx = "??")
        {
            this.schoolName = schoolName;
            this.date = date;
            this.field = field;
            this.xx = xx;
        }
        public Edu(List<string> eduInfo)
        {

            var fieldIndex = eduInfo.IndexOf("Field Of Study");
            if (fieldIndex == -1)
                fieldIndex = eduInfo.IndexOf("Degree Name");
            var dateIndex = eduInfo.IndexOf("Dates attended or expected graduation");
            this.field = fieldIndex != -1 ? eduInfo[fieldIndex + 1] : "??";
            this.date = dateIndex != -1 ? eduInfo[dateIndex + 1] : "??";
            this.schoolName = eduInfo[0];
        }
        public string eduPrint()
        {
            var ret = "\r\n\t" + schoolName + "\r\n\t" + field + "\r\n\t" + date + "\r\n";
            Console.WriteLineFormatted("\t" + schoolName,Color.LightGoldenrodYellow);
            Console.WriteLine("\t" + field + "\r\n\t" + date + "\r\n");
            return ret;
        }
    }
}
