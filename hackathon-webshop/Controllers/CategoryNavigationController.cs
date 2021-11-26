using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using Ucommerce.Api;
using Ucommerce.Infrastructure;
using Ucommerce.Masterclass.Umbraco.Models;
using Ucommerce.Search;
using Ucommerce.Search.Models;
using Ucommerce.Search.Slugs;
using Umbraco.Web.Mvc;

namespace Ucommerce.Masterclass.Umbraco.Controllers
{
    public class CategoryNavigationController : SurfaceController
    {
        public ICatalogLibrary CatalogLibrary => ObjectFactory.Instance.Resolve<ICatalogLibrary>();
        public ICatalogContext CatalogContext => ObjectFactory.Instance.Resolve<ICatalogContext>();
        public IUrlService UrlService => ObjectFactory.Instance.Resolve<IUrlService>();
    
        public ActionResult CategoryNavigation()
        {
            var model = new CategoryNavigationViewModel();

            var categories = CatalogLibrary.GetRootCategories();

            model.Categories = MapCategories(categories.Results);

            return View("/views/CategoryNavigation/index.cshtml", model);
        }

        private IList<CategoryViewModel> MapCategories(IList<Category> categories)
        {
            var models = new List<CategoryViewModel>();
            foreach (var category in categories)
            {
                models.Add(new CategoryViewModel()
                {
                    Guid = category.Guid,
                    Name = category.DisplayName,
                    Categories = MapCategories(CatalogLibrary.GetCategories(category.Categories).Results),
                    Url = UrlService.GetUrl(CatalogContext.CurrentCatalog, new List<Category> () { category })
                });
            }

            return models;
        }
    }
}