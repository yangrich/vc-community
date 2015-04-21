﻿#region

using System.Threading.Tasks;
using System.Web.Mvc;
using VirtoCommerce.Web.Models;
using VirtoCommerce.Web.Models.Helpers;
using VirtoCommerce.Web.Models.Services;

#endregion

namespace VirtoCommerce.Web.Controllers
{
    [RoutePrefix("pages")]
    public class PageController : BaseController
    {
        #region Public Methods and Operators
        [Route("{page}")]
        // GET: Pages
        public async Task<ActionResult> DisplayPageAsync(string page)
        {
            var context = SiteContext.Current;
            var model = await new PagesService().GetPageAsync(context, page);
            this.Context.Set("page", model);

            return View(model.Layout ?? "page");
        }
        #endregion
    }
}