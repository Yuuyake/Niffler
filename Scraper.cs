using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using System.Linq;
using System.IO;
using static System.Console;
using Console = Colorful.Console;
using System.Threading;
using System.Drawing;
using Microsoft.Office.Interop.Excel;
using Linkedin_Scrapper.Properties;

/*

    işlemlerden sonra print yap, işlemi yaparken değil
    yani Person clasına print yaz

    proifle TITLES >>>

    LOCATION    class="t-16 t-black t-normal inline-block"  
    POSITION    class="mt1 t-18 t-black t-normal"	
    NAME        inline t-24 t-black t-normal break-words	

*/

namespace Linkedin_Scrapper
{
    class Scrapper
    {
        static public IWebDriver driver     = new ChromeDriver("../../");
        static public string loginID        = "YOUR CREDS";
        static public string loginPassword  = "YOUR CREDS";

        [STAThread]
        static void Main()
        {
            Console.Title = "Niffler";
            
            Console.WriteLineFormatted(Resources.banner, Color.LightGoldenrodYellow);
            Console.WriteLineFormatted("\tCurrent Code Page is  : " + Console.OutputEncoding.WebName,Color.LightGoldenrodYellow);
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLineFormatted("\tCode Page is set to   : " + Console.OutputEncoding.WebName,Color.LightGoldenrodYellow);

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
            driver.Navigate().GoToUrl("https://www.linkedin.com/login?trk=guest_homepage-basic_nav-header-signin");
            driver.FindElement(By.XPath("//*[@id=\"username\"]")).SendKeys(loginID);
            driver.FindElement(By.XPath("//*[@id=\"password\"]")).SendKeys(loginPassword);
            driver.FindElement(By.XPath("//*[@type=\"submit\"]")).Click();

            List<string> personPages = new List<string>{
                "https://www.linkedin.com/in/altan-demirdere-08a56915a/",
                "https://www.linkedin.com/in/esen-girit-t%C3%BCmer-b99a706/",
                "https://www.linkedin.com/in/fatih-islamoglu-11892a6/",
                "https://www.linkedin.com/in/zuhtusoylu/"
            };
            List<Person> personList = new List<Person> { };
            foreach (string personPage in personPages)
            { // process each user
                try
                {
                    Console.WriteLineFormatted("===============================================================================================", Color.Red);
                    Scrapper.driver.Navigate().GoToUrl(personPage);

                    // clicking show more buttons
                    var showMoreButtons = Scrapper.driver.FindElements(By.ClassName("pv-profile-section__text-truncate-toggle")).ToList();
                    var expectedButtons = new List<string> { "more role", "more education", "more experience" };
                    var clickButtons = showMoreButtons.Where(button => expectedButtons.Count(expcButton => button.Text.Contains(expcButton)) > 0).ToList();
                    foreach (var bt in clickButtons)
                        bt.Click();

                    //wait to make sure buttons clicked and data is loaded properly
                    while (true)
                    {
                        var showFewerButtons = Scrapper.driver.FindElements(By.ClassName("pv-profile-section__text-truncate-toggle")).ToList();
                        var waitedButtons = new List<string> { "fewer role", "fewer education", "fewer experience" };
                        var fewButtons = showFewerButtons.Where(button => waitedButtons.Count(wButtons => button.Text.Contains(wButtons)) > 0).ToList();
                        if (fewButtons.Count < clickButtons.Count)
                            Thread.Sleep(500);
                        else
                            break;
                    }
                    // get user infos
                    Person tempPerson = new Person(personPage);
                    tempPerson.writeToExcel();
                    personList.Add(tempPerson);
                }
                catch(Exception ee)
                {
                    Console.WriteLineFormatted("\n\t> Something wrong with person : " + personPage, Color.Red);
                    Console.WriteLineFormatted("\t> Exception : " + ee.Message, Color.Orange);
                }
            }
            Console.WriteLineFormatted("============================    ALL DONE    =====================================", Color.Red);
            ReadLine();
            driver.Quit();
        }
    }

    /// <summary>
    /// saves Person Data, exp, edu ...
    /// </summary>
    public class Person
    {
        static public List<Exp> experiences  = new List<Exp> { };
        static public List<Edu> educations   = new List<Edu> { };
        static public List<string> languages = new List<string>() {"Language1","Language2","Language3" };
        static public string fullName   = "Full Name";
        static public string currTitle  = "Current Title";
        static public string currPos    = "Current Position";
        static public string dateBrith  = "Birth Date";

        public Person(string personPage)
        {   
            var userInfos = Scrapper.driver.FindElements(By.XPath("//*[contains(@class,'pv-entity__summary-info')]"));
            var eduInfos  = Scrapper.driver.FindElements(By.ClassName("pv-education-entity"));
            var expInfos  = Scrapper.driver.FindElements(By.ClassName("pv-position-entity"));

            List<string> currentInfos = Scrapper.driver.FindElement(By.CssSelector(".flex-1.mr5")).Text.Replace("\r","").Split('\n').ToList();
            int trashIndex = currentInfos.FindIndex(inf => inf.Contains("degree connection") == true);
            if(trashIndex > -1)
                currentInfos.RemoveRange(trashIndex,2);
            currentInfos = currentInfos.Where(inf => inf.Contains("has a account") == false).ToList();

            fullName    = currentInfos[0];
            currTitle   = currentInfos[1];
            trashIndex  = currentInfos[2].Split(' ').ToList().IndexOf("connections");
            currPos     = String.Join(",", currentInfos[2].Split(' ').ToList().GetRange(0, trashIndex - 1));

            Write("\r\n\t\t" + fullName + "\r\n");
            Console.WriteFormatted(" |\r\n | EDUCATION:__________________________________________________________________\n", Color.Cyan);
            foreach (var eduInfo in eduInfos)
            {
                var attrs   = eduInfo.Text.Replace("\r", "").Split('\n').ToList();
                var newEdu  = new Edu(attrs);
                newEdu.eduPrint();
                educations.Add(newEdu);
            }

            Console.WriteFormatted(" |\r\n | EXPERIENCE:__________________________________________________________________\n", Color.Cyan);
            foreach (var expInfo in expInfos)
            {
                var attrs   = expInfo.Text.Replace("\r", "").Split('\n').ToList();
                var newExp  = new Exp(attrs);
                newExp.expPrint();
                experiences.Add(newExp);
            }
        }

        public void writeToExcel() 
        {
            string templateFile = "../../Resources/template.xlsx"; // @"YOUR_EXCEL_FILE_PATH";
            string userFile     = "./" + fullName + ".xlsx";
            File.Copy(templateFile, userFile);
            Application app = new Application();
            Workbook workbook = app.Workbooks.Open(Directory.GetCurrentDirectory() + "/" + userFile);
            Worksheet worksheet = workbook.Worksheets[1];

            worksheet.Name = "sheet1";
            int maxRow = new int[]{ educations.Count * 2 , (from x in experiences select x.jobs.Count).Sum() + experiences.Count, 5}.Max();

            Range line = (Range)worksheet.Rows[3]; // insert middle line with number of needed rows - 3
            Enumerable.Range(0, maxRow - 3).ToList().ForEach(i => line.Insert());

            int rowCounter = 1;
            foreach (Edu edu in educations) // write education column(4)
            {
                worksheet.Cells[rowCounter * 2, 4].Value = edu.schoolName + ", " +  edu.date ;
                worksheet.Cells[rowCounter * 2, 4].Font.Bold = true;
                worksheet.Cells[rowCounter * 2 + 1, 4].Value = edu.field;
                rowCounter++;
            }
            rowCounter = 2;
            foreach (Exp exp in experiences) // write experiences columns(2,3)
            {   
                worksheet.Cells[rowCounter, 2].Value = exp.dateIterval;
                worksheet.Cells[rowCounter, 2].Font.Bold = true;
                worksheet.Cells[rowCounter, 3].Value = exp.companyName;
                worksheet.Cells[rowCounter, 3].Font.Bold = true;
                rowCounter++;
                foreach (Job job in exp.jobs)
                {   
                    worksheet.Cells[rowCounter, 2].Value = job.dateIterval ;
                    worksheet.Cells[rowCounter, 3].Value = job.title ;
                    rowCounter++;
                }
            }
            // write user info column(1)
            worksheet.Cells[2, 1].Value = fullName;
            worksheet.Cells[2, 1].Font.Bold = true;

            worksheet.Cells[3, 1].Value = currTitle ;
            worksheet.Cells[4, 1].Value = currPos;

            worksheet.Cells[5, 1].Value = "Date Birth";
            worksheet.Cells[5, 1].Font.Bold = true;
            worksheet.Cells[6, 1].Value = dateBrith;

            worksheet.Cells[7, 1].Value = "Languages";
            worksheet.Cells[7, 1].Font.Bold = true;
            for (var i = 0; i < languages.Count; i++)
                worksheet.Cells[8 + i,1].Value = languages[i];

            workbook.Save();
            workbook.Close();
            app.Quit();

        }
    }
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
            dateIterval = jobInfos.IndexOf("Dates Employed") != -1 ? jobInfos[jobInfos.IndexOf("Dates Employed") + 1] : "??";
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

        internal string eprint()
        {
            var ret = 
                "\t" + title +
                "\r\n\t" + dateIterval +
                "\r\n\t" + empDuration + "\r";
            Console.WriteLine(ret);
            return ret;
        }
    }
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
