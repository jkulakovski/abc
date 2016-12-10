using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace EEBank.Methods
{
    public class ExcelResult : ActionResult
    {
        /// <summary>
        /// Создает экземпляр класса, которые выдает файл Excel
        /// </summary>
        /// <param name="fileName">наименование файла для экспорта</param>
        /// <param name="report">готовый набор данные для экпорта</param>
        public ExcelResult(string fileName, string report)
        {
            this.Filename = fileName;
            this.Report = report;
        }
        public string Report { get; private set; }
        public string Filename { get; private set; }

        public override void ExecuteResult(ControllerContext context)
        {
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
            HttpContext.Current.Response.BufferOutput = true;
            HttpContext.Current.Response.AddHeader("content-disposition",
                                 string.Format("attachment; filename={0}", Filename));
            HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
            HttpContext.Current.Response.Charset = "utf-8";
            HttpContext.Current.Response.Write(Report);
            HttpContext.Current.Response.Flush();
            HttpContext.Current.Response.End();
        }
    }
}