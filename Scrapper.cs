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

//           işlemlerden sonra print yap

namespace Linkedin_Scrapper
{
    class Scrapper
    {
        static public IWebDriver driver     = new ChromeDriver(@"YOUR chromedriver.exe PATH"); //new ChromeDriver(@"./../../");
        static public string loginID        = "YOUR_LOGIN_CRED_MAIL";
        static public string loginPassword  = "YOUR_LOGIN_CRED_PASS";

        [STAThread]
        static void Main()
        {
            Console.Title = "Niffler";
            
            Console.WriteLineFormatted(Resources.banner, Color.LightGoldenrodYellow);
            Console.WriteLineFormatted("\tCurrent Code Page is  : " + Console.OutputEncoding.WebName,Color.LightGoldenrodYellow);
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLineFormatted("\tCode Page is set to   : " + Console.OutputEncoding.WebName,Color.LightGoldenrodYellow);

            writeToExcel();
            return;

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
                int clr = 1 + personPages.IndexOf(personPage)*7;
                //Console.BackgroundColor =  Color.FromArgb(clr,clr,clr); 
                Console.WriteLineFormatted("===============================================================================",Color.Red);
                personList.Add(new Person(personPage));
            }
            Console.WriteLineFormatted("============================    DONE    =====================================", Color.Red);
            ReadLine();
            driver.Quit();
        }

        public static void writeToExcel()
        {
            string excelLocation = @"YOUR_EXCEL_FILE_PATH";
            Application app = new Application();
            Workbook workbook = app.Workbooks.Open(excelLocation);
            Worksheet worksheet = workbook.Worksheets[1];

            worksheet.Name = "sheet1";
            int totalRow = 20;

            Range line = (Range)worksheet.Rows[3];
            Enumerable.Range(0, totalRow-3).ToList().ForEach(i => line.Insert());

            for (int row = 2; row < totalRow; row++)
            {
                for (int column = 1; column < 5; column++)
                {
                    worksheet.Cells[row, column].Value = row * column;
                }
            }

            workbook.Save();
            workbook.Close();
            app.Quit();

        }
    }


    /// <summary>
    /// saves Person Data, exp, edu ...
    /// </summary>
    public class Person
    {
        public List<Exp> experiences = new List<Exp> { };
        public List<Edu> educations  = new List<Edu> { };
        public string fullName = "??";

        public Person(string personPage)
        {
            Scrapper.driver.Navigate().GoToUrl(personPage);

            // clicking show more areas

            var showMoreButtons = Scrapper.driver.FindElements(By.ClassName("pv-profile-section__text-truncate-toggle")).ToList();
            var expectedButtons = new List<string> { "more role", "more education", "more experience" };
            var clickButtons    = showMoreButtons.Where(button => expectedButtons.Count(expcButton => button.Text.Contains(expcButton)) > 0).ToList();
            foreach (var bt in clickButtons)
                bt.Click();

            //wait to make sure buttons clicked and data is loaded properly
            while (true)
            {
                var showFewerButtons = Scrapper.driver.FindElements(By.ClassName("pv-profile-section__text-truncate-toggle")).ToList();
                var waitedButtons    = new List<string> { "fewer role", "fewer education", "fewer experience" };
                var fewButtons       = showFewerButtons.Where(button => waitedButtons.Count(wButtons => button.Text.Contains(wButtons)) > 0).ToList();
                if (fewButtons.Count < clickButtons.Count)
                    Thread.Sleep(500);
                else
                    break;
            }

            var userInfos = Scrapper.driver.FindElements(By.XPath("//*[contains(@class,'pv-entity__summary-info')]"));
            var eduInfos  = Scrapper.driver.FindElements(By.ClassName("pv-education-entity"));
            var expInfos  = Scrapper.driver.FindElements(By.ClassName("pv-position-entity"));

            var userID = personPage.Split('/')[4];
            Write("\r\n\t\t" + userID + "\r\n");
            Console.WriteFormatted(" |\r\n | EDUCATION:__________________________________________________________________", Color.Cyan);
            File.WriteAllText(userID + ".txt",
                " |\r\n | EDUCATION:__________________________________________________________________");

            foreach (var eduInfo in eduInfos)
            {
                var attrs   = eduInfo.Text.Replace("\r", "").Split('\n').ToList();
                var newEdu  = new Edu(attrs);
                File.AppendAllText(userID + ".txt", newEdu.eduPrint());
                educations.Add(newEdu);
            }
            Console.WriteFormatted(" |\r\n | EXPERIENCE:__________________________________________________________________", Color.Cyan);
            File.WriteAllText(userID + ".txt",
                "\r\n | EXPERIENCE:__________________________________________________________________\r\n");

            foreach (var expInfo in expInfos)
            {
                var attrs   = expInfo.Text.Replace("\r", "").Split('\n').ToList();
                var newExp  = new Exp(attrs);
                File.AppendAllText(userID + ".txt", newExp.expPrint());
                experiences.Add(newExp);
            }
        }
    }
    /// <summary>
    /// saves an Experience data ( meaning works on a company )
    /// </summary>
    public class Exp
    {
        public string companyName;
        public string totalDuration;
        public List<Job> jobs = new List<Job> { };

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
        }
        internal string expPrint()
        {
            var ret = "\r\n\t>>" + companyName + "\t" + totalDuration + "\r\n";
            foreach (Job job in jobs)
                ret += job.eprint();
            Console.WriteLine(ret);
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

        public Job(List<string> jobInfos)
        {
            title       = jobInfos.IndexOf("Title") != -1 ? jobInfos[jobInfos.IndexOf("Title") + 1] : "??";
            empDuration = jobInfos.IndexOf("Employment Duration") != -1 ? jobInfos[jobInfos.IndexOf("Employment Duration") + 1] : "??";
            dateIterval = jobInfos.IndexOf("Dates Employed") != -1 ? jobInfos[jobInfos.IndexOf("Dates Employed") + 1] : "??";
        }

        public Job(string title, string dateIterval, string empDuration)
        {
            this.title       = title;
            this.dateIterval = dateIterval;
            this.empDuration = empDuration;
        }

        internal string eprint()
        {
            var ret = "\r\n " + title + "\r\n\t" + dateIterval + "\r\n\t" + empDuration + "\r";
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
            Console.WriteLine(ret);
            return ret;
        }
    }
}
