using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Ucommerce.Api;
using Ucommerce.Infrastructure;
using Ucommerce.Masterclass.Umbraco.Models;
using Ucommerce.Search;
using Ucommerce.Search.Models;
using Umbraco.Web.Mvc;

namespace Ucommerce.Masterclass.Umbraco.Controllers
{
    public class ProductController : RenderMvcController
    {
        public ICatalogContext CatalogContext => ObjectFactory.Instance.Resolve<ICatalogContext>();
        public ICatalogLibrary CatalogLibrary => ObjectFactory.Instance.Resolve<ICatalogLibrary>();
        
        [System.Web.Mvc.HttpGet]
        public ActionResult Index()
        {
            var currentProduct = CatalogContext.CurrentProduct;

            var productModel = new ProductViewModel()
            {
                Sku = currentProduct.Sku,
                Name = currentProduct.DisplayName,
                PrimaryImageUrl = currentProduct.PrimaryImageUrl,
                LongDescription = currentProduct.LongDescription,
                Prices = CatalogLibrary.CalculatePrices(new List<Guid>() { currentProduct.Guid }).Items,
                Variants = MapVariants(CatalogLibrary.GetVariants(currentProduct))
            };
            
            return View(productModel);
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult Index(string sku, string variantSku, int quantity)
        {
            return Index();
        }
        
        private IList<ProductViewModel> MapVariants(ResultSet<Product> variants)
        {
            return variants.Select(v => new ProductViewModel()
            {
                Name = v.DisplayName,
                VariantSku = v.VariantSku
            }).ToList();
        }
    }
}