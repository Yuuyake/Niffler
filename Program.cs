using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Linq;
using System.IO;

//          > user Class ı ekle ve infoları içine at
//          > edu işlemlerini education clasına bırak

namespace Linkedin_Scrapper
{
    class Program
    {
        static public IWebDriver driver      = new ChromeDriver(@"./../../");
        static public string loginID         =  "YOUR_LINKEDIN_MAIL"; 
        static public string loginPassword   =  "YOUR_LINKEDIN_PASS";

        [STAThread]
        static void Main()
        {
            Console.Title = "Kakkide";
            Console.WriteLine("\tCurrent Code Page is\t: " + Console.OutputEncoding.WebName);
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("\tCode Page is set to\t: " + Console.OutputEncoding.WebName);

            driver.Navigate().GoToUrl("https://www.linkedin.com/login?trk=guest_homepage-basic_nav-header-signin");
            driver.FindElement(By.XPath("//*[@id=\"username\"]")).SendKeys(loginID); 
            driver.FindElement(By.XPath("//*[@id=\"password\"]")).SendKeys(loginPassword);  
            driver.FindElement(By.XPath("//*[@type=\"submit\"]")).Click(); 

            List<string> userPages = new List<string>{
                "https://www.linkedin.com/in/esen-girit-t%C3%BCmer-b99a706/",
                "https://www.linkedin.com/in/altan-demirdere-08a56915a/",
                "https://www.linkedin.com/in/fatih-islamoglu-11892a6/",
                "https://www.linkedin.com/in/zuhtusoylu/"
            };
             getUserInfos(userPages);

            Console.WriteLine("DONE."); 
            driver.Quit(); 
        }
        static void getUserInfos(List<string> userPages)
        {
            foreach (string userPage in userPages)
            { // process each user
                driver.Navigate().GoToUrl(userPage);
                var showMoreButtons = driver.FindElements(By.ClassName("pv-profile-section__text-truncate-toggle"));
                var expectedButtons = new List<string> { "more role", "more education", "more experience" };
                var clickButtons    = showMoreButtons.Where(button => expectedButtons.Count(expcButton => button.Text.Contains(expcButton)) > 0);
                foreach (var bt in clickButtons)
                    bt.Click();

                var userInfos = driver.FindElements(By.XPath("//*[contains(@class,'pv-entity__summary-info')]"));
                var eduInfos  = driver.FindElements(By.ClassName("pv-education-entity"));
                var expInfos  = driver.FindElements(By.ClassName("pv-position-entity"));
                Console.Write("=========================================================================");

                var userID = userPage.Split('/')[4];
                cw("\n\t\t" + userID + "\n");
                cw(" |\n | EDUCATION:______________________________________________________________________________");
                File.WriteAllText(userID + ".txt", " |\n | EDUCATION:______________________________________________________________________________");

                foreach (var eduInfo in eduInfos)
                {
                    var attrs = eduInfo.Text.Replace("\r","").Split('\n').ToList();
                    var schoolName = attrs[0];
                    int fieldIndex  = attrs.IndexOf("Field Of Study");
                    int dateIndex   = attrs.IndexOf("Dates attended or expected graduation");
                    var field   = fieldIndex    != -1 ? attrs[fieldIndex + 1] : "??";
                    var date    = dateIndex     != -1 ? attrs[dateIndex + 1] : "??";
                    var newEdu  = new Edu(schoolName, date, field);
                    File.AppendAllText(userID + ".txt", newEdu.eprint());
                }
                cw("\n | EXP:______________________________________________________________________________\n");
                File.WriteAllText(userID + ".txt", "\n | EXP:______________________________________________________________________________\n");

                foreach (var expInfo in expInfos)
                {
                    var attrs = expInfo.Text.Replace("\r", "").Split('\n').ToList();
                    var newExp = new Exp(attrs);
                    File.AppendAllText(userID + ".txt", newExp.eprint());
                }
            }
        }
        static void cw(string str) { Console.Write(str); }
        static void cwl(string str) { Console.WriteLine(str); }
        static string cr(){ return Console.ReadLine(); }
    }
    internal class Exp
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
                jobs.Add(new Job(exp.GetRange(exp.IndexOf("Title"),6)));
                var indexOfSecondExp = exp.IndexOf("Title", exp.IndexOf("Title") + 1);
                if(indexOfSecondExp != -1 )
                    exp.RemoveRange(exp.IndexOf("Title"),indexOfSecondExp);
            }
        }

        internal string eprint()
        {   
            var ret = "\n\t>>" + companyName + "\t" + totalDuration;
            Console.WriteLine(ret);
            foreach(Job job in jobs)
                ret += job.eprint();
            return ret;
        }
    }

    public class Job
    {
        public string title;
        public string dateIterval;
        public string empDuration;

        public Job(List<string> jobInfos)
        {
            title       = jobInfos.IndexOf("Title") != -1               ? jobInfos[jobInfos.IndexOf("Title") + 1] : "??";
            empDuration = jobInfos.IndexOf("Employment Duration") != -1 ? jobInfos[jobInfos.IndexOf("Employment Duration") + 1] : "??";
            dateIterval = jobInfos.IndexOf("Dates Employed") != -1      ? jobInfos[jobInfos.IndexOf("Dates Employed") + 1] : "??";
        }

        public Job(string title,string dateIterval, string empDuration)
        {
            this.title = title;
            this.dateIterval = dateIterval;
            this.empDuration = empDuration;
        }

        internal string eprint()
        {
            var ret = "\n\t\t" + title + "\n\t\t" + dateIterval + "\n\t\t" + empDuration;
            Console.WriteLine(ret);
            return ret;
        }
    }

    public class Edu
    {
        public string schoolName;
        public string date;
        public string field;
        public string xx;
        public Edu(string schoolName = "??",string date="??",string field = "??",string xx = "??")
        {
            this.schoolName = schoolName;
            this.date       = date;
            this.field      = field;
            this.xx         = xx;
        }
        public string eprint()
        {
            var ret = "\n\t" + schoolName + "\n\t" + field + "\n\t" + date;
            Console.WriteLine(ret);
            return ret;
        }
    }

}
