using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using System.Linq;
using System.IO;
using static System.Console;
using Console = Colorful.Console;
using System.Drawing;
using Microsoft.Office.Interop.Excel;

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
    /// <summary>
    /// saves Person Data, exp, edu ...
    /// </summary>
    public class Person
    {
        static private List<Exp> experiences;
        static private List<Edu> educations;
        static private List<string> languages;
        static public string fullName   = "Full Name";
        static public string currTitle  = "Current Title";
        static public string currPos    = "Current Position";
        static public string dateBrith  = "Birth Date";

        public Person(string personPage)
        {
            experiences = new List<Exp>() { };
            educations  = new List<Edu>() { };
            languages   = new List<string>() { "Language1", "Language2", "Language3" };

            var userInfos = Scraper.driver.FindElements(By.XPath("//*[contains(@class,'pv-entity__summary-info')]"));
            var eduInfos  = Scraper.driver.FindElements(By.ClassName("pv-education-entity"));
            var expInfos  = Scraper.driver.FindElements(By.ClassName("pv-position-entity"));

            List<string> currentInfos = Scraper.driver.FindElement(
                By.CssSelector(".flex-1.mr5")).Text.Replace("\r","").Replace("Contact info","").Split('\n').ToList();
            int trashIndex = currentInfos.FindIndex(inf => inf.Contains("degree connection") == true);
            if(trashIndex > -1)
                currentInfos.RemoveRange(trashIndex,2);

            currentInfos = currentInfos.Where(info => (
                info.Contains("account") ||
                info.Contains("degree connection") ||
                info.Contains("influencer account")
                ) == false).ToList();

            fullName    = currentInfos[0];
            currTitle   = currentInfos[1];
            var trahser = currentInfos[2].Split(' ').ToList();//
            int tindex  = trahser.IndexOf("connections") > trahser.IndexOf("followers") ? trahser.IndexOf("connections") : trahser.IndexOf("followers");
            currPos     = tindex == -1 ? currentInfos[2] : String.Join(",", trahser.GetRange(0, tindex - 1 ));
            currPos     = currPos.Replace(",,", ",");

            Write("\n │\n │\t");
            Console.BackgroundColor = Color.Gray;
            Write(fullName);
            Console.BackgroundColor = Color.Black;
            Write("\n │");
            Console.Write("\n │ EDUCATION:\n │");
            foreach (var eduInfo in eduInfos)
            {

                var attrs   = eduInfo.Text.Replace("\r", "").Split('\n').ToList();
                var newEdu  = new Edu(attrs);
                newEdu.ePrint();
                educations.Add(newEdu);
            }

            Console.Write("\n │\n │ EXPERIENCE:\n │");
            foreach (var expInfo in expInfos)
            {
                var attrs   = expInfo.Text.Replace("\r", "").Split('\n').ToList();
                var newExp  = new Exp(attrs);
                newExp.ePrint();
                experiences.Add(newExp);
            }
        }

        public void writeToExcel() 
        {
            string templateFile = "./template.xlsx"; // @"YOUR_EXCEL_FILE_PATH";
            string userFile     = "./" + fullName + ".xlsx";
            File.Copy(templateFile, userFile,true);
            Application app = new Application();
            Workbook workbook = app.Workbooks.Open(Directory.GetCurrentDirectory() + "/" + userFile);
            Worksheet worksheet = workbook.Worksheets[1];

            worksheet.Name = "sheet1";
            int maxRow = new int[]{ educations.Count * 2 , (from x in experiences select x.jobs.Count).Sum() + experiences.Count, 7}.Max();

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
}
