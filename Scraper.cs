using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using System.Linq;
using static System.Console;
using Console = Colorful.Console;
using System.Threading;
using System.Drawing;
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
    class Scraper
    {
        static public IWebDriver driver     = new ChromeDriver("./");
        static public string loginID        = "YOURCREDS";
        static public string loginPassword  = "YOURCREDS";

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
                "https://www.linkedin.com/in/mikefertik/",
                "https://www.linkedin.com/in/michael-fertik-4b27692/",
                "https://tr.linkedin.com/in/can-yaman-71a89a3b/",
                "https://tr.linkedin.com/in/emremetin/",
                "https://tr.linkedin.com/in/mguctas/",
                "https://www.linkedin.com/in/gelfenbeyn/",
                "https://www.linkedin.com/in/kat-duarte-45b672a9/",
                "https://www.linkedin.com/in/erin-wright-348b3b7/",
                "https://www.linkedin.com/in/alex-mastrangelo-b992a898/",
                "https://www.linkedin.com/in/jessica-jackson-esq-a08a613/",
            };
            List<Person> personList = new List<Person> { };

            while (true)
            {
                Console.WriteLine("Example: https://www.linkedin.com/in/XYZ_PERSON_ID/");
                while (true)
                {
                    Console.WriteLine("Type a Linkedin URL to add: ");
                    var tPage = Console.ReadLine();
                    BURAYA EKLEE

                }

                foreach (string personPage in personPages)
                { // process each user
                    try
                    {
                        Console.WriteLineFormatted("===============================================================================================", Color.Red);
                        Scraper.driver.Navigate().GoToUrl(personPage);
                        // clicking show more buttons
                        var showMoreButtons = Scraper.driver.FindElements(By.ClassName("pv-profile-section__text-truncate-toggle")).ToList();
                        var expectedButtons = new List<string> { "more role", "more education", "more experience" };
                        var clickButtons = showMoreButtons.Where(button => expectedButtons.Count(expcButton => button.Text.Contains(expcButton)) > 0).ToList();
                        int count = clickButtons.Count;
                        foreach (var bt in clickButtons)
                            bt.Click();
                        while (true)
                        {
                            //wait to make sure buttons clicked and data is loaded properly
                            var showFewerButtons = Scraper.driver.FindElements(By.ClassName("pv-profile-section__text-truncate-toggle")).ToList();
                            var waitedButtons = new List<string> { "fewer role", "fewer education", "fewer experience" };
                            var fewButtons = showFewerButtons.Where(button => waitedButtons.Count(wButtons => button.Text.Contains(wButtons)) > 0).ToList();
                            if (fewButtons.Count != count)
                            {
                                Thread.Sleep(500);
                                showMoreButtons = Scraper.driver.FindElements(By.ClassName("pv-profile-section__text-truncate-toggle")).ToList();
                                expectedButtons = new List<string> { "more role", "more education", "more experience" };
                                clickButtons = showMoreButtons.Where(button => expectedButtons.Count(expcButton => button.Text.Contains(expcButton)) > 0).ToList();
                                foreach (var bt in clickButtons)
                                    bt.Click();
                            }
                            else
                                break;
                        }
                        // get user infos
                        Person tempPerson = new Person(personPage);
                        tempPerson.writeToExcel();
                        personList.Add(tempPerson);
                    }
                    catch (Exception ee)
                    {
                        Console.WriteLineFormatted("\n\t> Something wrong with person : " + personPage, Color.Red);
                        Console.WriteLineFormatted("\t> Exception : " + ee.Message, Color.Orange);
                    }
                }
                Console.WriteLineFormatted("============================    ALL DONE    =====================================", Color.Red);
                Console.WriteLine("\n\tType \"FF\" to finish, anything else to redo: ");
                var finish = ReadLine();
                if (finish == "FF")
                    break;
            }
            driver.Quit();
        }
    }
}
