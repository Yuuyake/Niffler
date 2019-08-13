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
        static public IWebDriver driver;
        static public string chromeBrowserLoc  = @".\ChromePortable\App\Chrome-bin\chrome.exe";
        static public string chromeDriverLoc   = @".\ChromePortable";
        static public string loginID        = "YOURCREDS";
        static public string loginPassword  = "YOURCREDS";
        static public string banner         = Resources.banner;
        static public List<string> personPages = new List<string>{
                //"https://www.linkedin.com/in/mikefertik/",
                //"https://www.linkedin.com/in/michael-fertik-4b27692/",
                //"https://www.linkedin.com/in/can-yaman-71a89a3b/",
                //"https://www.linkedin.com/in/emremetin/",
                //"https://www.linkedin.com/in/mguctas/",
                //"https://www.linkedin.com/in/gelfenbeyn/",
                //"https://www.linkedin.com/in/kat-duarte-45b672a9/",
                //"https://www.linkedin.com/in/erin-wright-348b3b7/",
                "https://www.linkedin.com/in/alex-mastrangelo-b992a898/",
                "https://www.linkedin.com/in/jessica-jackson-esq-a08a613/",
            };
        [STAThread]
        static void Main()
        {
            Console.Title = "Niffler";
            
            Console.WriteLineFormatted(banner, Color.LightGoldenrodYellow);
            Console.WriteLineFormatted("\tCurrent Code Page is  : " + Console.OutputEncoding.WebName,Color.LightGoldenrodYellow);
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLineFormatted("\tCode Page is set to   : " + Console.OutputEncoding.WebName,Color.LightGoldenrodYellow);

            printMenu();

            Console.WriteLineFormatted("\n ╟\n ╟► Opening chrome browser \"" + chromeBrowserLoc + "\" . . .\n", Color.Cyan);
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("--no-sandbox");
            chromeOptions.BinaryLocation = chromeBrowserLoc;
            driver = new ChromeDriver(chromeDriverLoc,chromeOptions);

            Console.Clear();
            Console.WriteLineFormatted(banner, Color.LightGoldenrodYellow);
            Console.Write("\n ╔═════════════════════════════════════════════════════════════════════════════════════════════════\n ║");
            Console.WriteLine("\n ╟► Opening login page \"https://www.linkedin.com/login?trk=guest_homepage-basic_nav-header-signin\" . . .");

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
            driver.Navigate().GoToUrl("https://www.linkedin.com/login?trk=guest_homepage-basic_nav-header-signin");
            Console.WriteLine(" ╟\n ╟► Giving credentials to login page . . .\n\tLoginID : " + loginID + "\n\tPassword: ********** ");

            driver.FindElement(By.XPath("//*[@id=\"username\"]")).SendKeys(loginID);
            driver.FindElement(By.XPath("//*[@id=\"password\"]")).SendKeys(loginPassword);
            driver.FindElement(By.XPath("//*[@type=\"submit\"]")).Click();

            List<Person> personList = new List<Person> { };

            while (true)
            {
                foreach (string personPage in personPages)
                { // process each user
                    try
                    {
                        Console.Write("\n ┌─────────────────────────────────────────────────────────────────────────────────────────────────");
                        Console.WriteFormatted("\n │► Opening user page: \"" + personPage + "\"",Color.Cyan);
                        Scraper.driver.Navigate().GoToUrl(personPage);
                        // clicking show more buttons
                        var showMoreButtons = Scraper.driver.FindElements(By.ClassName("pv-profile-section__text-truncate-toggle")).ToList();
                        var expectedButtons = new List<string> { "more role", "more education", "more experience" };
                        var clickButtons = showMoreButtons.Where(button => expectedButtons.Count(expcButton => button.Text.Contains(expcButton)) > 0).ToList();
                        int count = clickButtons.Count;
                        Console.WriteFormatted("\n │► Clicking for \"show more\" buttons . . .", Color.Cyan);
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
                        Console.WriteFormatted("\n │► Saving users info . . .", Color.Cyan);
                        Person tempPerson = new Person(personPage);
                        tempPerson.writeToExcel();
                        personList.Add(tempPerson);
                    }
                    catch (Exception ee)
                    {
                        Console.Write("\n │\t> Something wrong with person : " + personPage, Color.Red);
                        Console.Write("\n │\t> Exception : " + ee.Message, Color.Orange);
                    }
                    Console.Write("\n └─────────────────────────────────────────────────────────────────────────────────────────────────");
                }
                Console.WriteLineFormatted("============================    ALL DONE    =====================================", Color.Red);
                Console.WriteLine("\n\tType \"FF\" to finish, anything else to redo: ");
                var finish = ReadLine();
                if (finish == "FF")
                    break;
            }
            driver.Quit();
        }
        static void printMenu()
        {
            var menuItems = new List<string>() { };
            var descItems = new List<string>() { };

            string descTemplate = "\td=delete | a=add | enter=finish ";
            for (int i = 0; i < personPages.Count; i++)
            {
                menuItems.Add(personPages[i].PadRight(60));
                descItems.Add("                                             ");
            }
            //new SoundPlayer(URLbliss.Properties.Resources.gurg).Play();
            Console.ForegroundColor = Color.FromArgb(0, 255, 0);
            Console.Clear();
            int currChoice = 0;
            Console.WriteLineFormatted(banner, Color.LightGoldenrodYellow);
            Console.Write("\n ╔═════════════════════════════════════════════════════════════════════════════════════════════════\n ║");
            while (true)
            {
                if (menuItems.Count == 0)
                {
                    Console.WriteLineFormatted("\t Empty like my hearth . . .", Color.Red);
                    Console.ReadLine();
                    Environment.Exit(0);
                }
                //menuItems = menuItems.Count == 0 ? menuItems.Add(" No logger No cry . . .");
                currChoice = currChoice < 0 ? currChoice + menuItems.Count : currChoice % menuItems.Count;
                //setting cursor position to 0 gives slightly higher performance than Console.Clear()
                Console.SetCursorPosition(0, banner.Count(f => f == '\n') + 2); // normally 18 is the height of URLBLISS banner
                descItems[currChoice] = descTemplate;
                for (int i = 0; i < menuItems.Count; i++)
                { // 6 element is the length of selective menu
                    if (currChoice - 1 == i)
                        Console.Write("\n ╟  " + menuItems[i]);
                    else if (currChoice == i)
                        Console.WriteFormatted("\n ╟►  " + menuItems[i], Color.White);
                    else
                        Console.WriteFormatted("\n ╟ " + menuItems[i], Color.FromArgb(0, 255, 0));
                    Console.WriteFormatted(descItems[i], Color.FromArgb(255, 0, 0));
                }
                descItems[currChoice] = "                                             ";

                ConsoleKeyInfo pressed = Console.ReadKey(true);
                if (pressed.Key == ConsoleKey.DownArrow)
                {
                    currChoice++;
                    //new SoundPlayer(URLbliss.Properties.Resources.slideup).Play();
                }
                else if (pressed.Key == ConsoleKey.UpArrow)
                {
                    currChoice--;
                    //new SoundPlayer(URLbliss.Properties.Resources.slidedown).Play();
                }
                else if (pressed.Key == ConsoleKey.D)
                {
                    menuItems.RemoveAt(currChoice);
                    personPages.RemoveAt(currChoice);
                    Console.Clear();
                    Console.WriteLineFormatted(banner, Color.LightGoldenrodYellow);
                    Console.Write("\n ╔═════════════════════════════════════════════════════════════════════════════════════════════════\n ║");
                    //new SoundPlayer(URLbliss.Properties.Resources.slidedown).Play();
                }
                else if (pressed.Key == ConsoleKey.A)
                {
                    Console.SetCursorPosition(0, banner.Count(f => f == '\n') + 2 + menuItems.Count + 2); // normally 18 is the height of URLBLISS banner
                    Console.WriteLine("Example: https://www.linkedin.com/in/XYZ_PERSON_ID/");
                    Console.WriteLine("\nType a Linkedin URL to add: ");
                    var tPerson = Console.ReadLine();
                    menuItems.Add(tPerson);
                    personPages.Add(tPerson);
                    descItems.Add("                                             ");
                    Console.Clear();
                    Console.WriteLineFormatted(banner, Color.LightGoldenrodYellow);
                    Console.Write("\n ╔═════════════════════════════════════════════════════════════════════════════════════════════════\n ║");
                    //new SoundPlayer(URLbliss.Properties.Resources.slidedown).Play();
                }
                else if (pressed.Key == ConsoleKey.Enter)
                {
                    break; // if Enters exit from Choice screen and call proper function
                }
            } // while 
            switch ((currChoice + 1).ToString())
            {
                case "1":
                    break;
                default:
                    break;
            } // switch 
        }
    }
}
