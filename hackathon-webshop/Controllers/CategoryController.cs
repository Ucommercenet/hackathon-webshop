using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ucommerce.Api;
using Ucommerce.Infrastructure;
using Ucommerce.Masterclass.Umbraco.Models;
using Ucommerce.Search.Extensions;
using Ucommerce.Search.Facets;
using Ucommerce.Search.Models;
using Ucommerce.Search.Slugs;
using Umbraco.Core;
using Umbraco.Core.Packaging;
using Umbraco.Web.Mvc;

namespace Ucommerce.Masterclass.Umbraco.Controllers
{
    #region facetHelper
    public static class FacetedQueryStringExtensions
    {
        public static IList<Facet> ToFacets(this NameValueCollection target)
        {
            var parameters = new Dictionary<string, string>();
            foreach (var queryString in HttpContext.Current.Request.QueryString.AllKeys)
            {
                parameters[queryString] = HttpContext.Current.Request.QueryString[queryString];
            }

            parameters.RemoveAll(kvp =>
                new [] { "umbDebugShowTrace", "product", "variant", "category", "categories", "catalog"}
                    .Contains(kvp.Key));

            var facetsForQuerying = new List<Facet>();

            foreach (var parameter in parameters)
            {
                var facet = new Facet {FacetValues = new List<FacetValue>(), Name = parameter.Key};
                foreach (var value in parameter.Value.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries))
                {
                    facet.FacetValues.Add(new FacetValue() {Value = value});
                }

                facetsForQuerying.Add(facet);
            }

            return facetsForQuerying;
        }
    }
    #endregion

    public class CategoryController : RenderMvcController
    {
        public ICatalogContext CatalogContext => ObjectFactory.Instance.Resolve<ICatalogContext>();
        public ICatalogLibrary CatalogLibrary => ObjectFactory.Instance.Resolve<ICatalogLibrary>();
        public IUrlService UrlService => ObjectFactory.Instance.Resolve<IUrlService>();

        public CategoryController()
        {
            
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult Index(string sku)
        {
            return Index();
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult Index()
        {
            var categoryModel = new CategoryViewModel();

            var currentCategory = CatalogContext.CurrentCategory;

            var facetDictionary = System.Web.HttpContext.Current.Request.QueryString.ToFacets().ToFacetDictionary();

            var facetResultSet = CatalogLibrary.GetProducts(currentCategory.Guid, facetDictionary, 0, 300);

            categoryModel.Products = MapProducts(facetResultSet.Results);
            categoryModel.Facets = MapFacets(facetResultSet.Facets);

            return View("/views/category/index.cshtml", categoryModel);
        }

        private IList<FacetsViewModel> MapFacets(IList<Facet> facets)
        {
            var facetsToReturn = new List<FacetsViewModel>();

            foreach (var facet in facets)
            {
                facetsToReturn.Add(new FacetsViewModel()
                {
                    Key = facet.Name,
                    DisplayName = facet.DisplayName,
                    FacetValues = facet.FacetValues.Select(x => new FacetValueViewModel()
                    {
                        Count = x.Count,
                        Key = x.Value
                    }).ToList()
                        
                });
            }
            
            return facetsToReturn;
        }

        private IList<ProductViewModel> MapProducts(IList<Product> products)
        {
            var prices = CatalogLibrary.CalculatePrices(products.Select(x => x.Guid).ToList());

            var productListModel = new List<ProductViewModel>();

            foreach (var product in products)
            {
                productListModel.Add(new ProductViewModel()
                {
                    LongDescription = product.LongDescription,
                    Name = product.DisplayName,
                    Prices = prices.Items.Where(x => x.ProductGuid == product.Guid && x.PriceGroupGuid == CatalogContext.CurrentPriceGroup.Guid).ToList(),
                    Url = UrlService.GetUrl(CatalogContext.CurrentCatalog, new List<Category> () { CatalogContext.CurrentCategory }, product),
                    Sku = product.Sku,
                    Sellable = product.ProductType == ProductType.Product || product.ProductType == ProductType.ProductFamily,
                });
            }
            
            return productListModel;
        }
    }
}