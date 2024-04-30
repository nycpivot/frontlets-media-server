namespace Frontlets.Media.Server
{
    internal class Catalog
    {
        void AddItem(string key, string value)
        {

        }

        internal IList<CatalogItem> Christopher { get; set; } = new List<CatalogItem>();
        internal IList<CatalogItem> Classical { get; set; } = new List<CatalogItem>();
    }
}
