using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using Telerik.Sitefinity.Mvc;
using SitefinityWebApp.Mvc.Models;

namespace SitefinityWebApp.Mvc.Controllers
{
    [ControllerToolboxItem(Name = "CsvContent", Title = "CsvContent", SectionName = "MvcWidgets")]
    public class CsvContentController : Controller
    {
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        [Category("Csv")]
        public string CsvFileUrl { get; set; }

		[Category("Csv")]
		public string YesMarker
		{
			get
			{
				return this.yesMarker;
			}
			set
			{
				this.yesMarker = value;
			}
		}

		[Category("Csv")]
		public string ViewName { get; set; }

        /// <summary>
        /// This is the default Action.
        /// </summary>
        public ActionResult Index()
        {
            var model = new CsvContentModel(this.CsvFileUrl, this.YesMarker);
			var viewName = string.IsNullOrEmpty(this.ViewName) ? "Default" : this.ViewName;
            return View(viewName, model);
        }

		private string yesMarker = "x";
    }
}