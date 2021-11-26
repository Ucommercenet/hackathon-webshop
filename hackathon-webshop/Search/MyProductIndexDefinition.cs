using Ucommerce.Search.Definitions;
using Ucommerce.Search.Extensions;

namespace hackathon_webshop.Search
{
    public class MyProductIndexDefinition : DefaultProductsIndexDefinition
    {
        public MyProductIndexDefinition() : base()
        {
            this.Field(p => p["Colour"], typeof(string)).Facet();
        }
    }
}