using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Calabonga.Xml.Exports;
using EEBank.Methods;

namespace EEBank.Controllers
{

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        [Authorize]
        public ActionResult Archive_Index()
        {
            return View();
        }

        public ActionResult Export()
        {
            string result = string.Empty;
            Workbook wb = new Workbook();

            wb.Properties.Author = "Calabonga";
            wb.Properties.Created = DateTime.Today;
            wb.Properties.LastAutor = "Calabonga";
            wb.Properties.Version = "14";

            wb.ExcelWorkbook.ActiveSheet = 1;
            wb.ExcelWorkbook.DisplayInkNotes = false;
            wb.ExcelWorkbook.FirstVisibleSheet = 1;
            wb.ExcelWorkbook.ProtectStructure = false;
            wb.ExcelWorkbook.WindowHeight = 800;
            wb.ExcelWorkbook.WindowTopX = 0;
            wb.ExcelWorkbook.WindowTopY = 0;
            wb.ExcelWorkbook.WindowWidth = 600;

            Style s1 = new Style("s1");
            s1.Font.Bold = true;
            s1.Font.Italic = true;
            s1.Font.Color = "#FF0000";
            wb.AddStyle(s1);

            Style s2 = new Style("s2");
            s2.Font.Bold = true;
            s2.Font.Italic = true;
            s2.Font.Size = 12;
            s2.Borders.Add(new Border());
            s2.Font.Color = "#0000FF";
            wb.AddStyle(s2);

            Worksheet ws = new Worksheet("Лист 1");

            ws.AddCell(0, 0, 1);
            ws.AddCell(0, 1, "qwerty2");
            ws.AddCell(0, 2, "qwerty3");
        }
        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}