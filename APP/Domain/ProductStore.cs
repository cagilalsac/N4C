using N4C.Domain;

namespace APP.Domain
{
    public class ProductStore : Entity
    {
        public int ProductId { get; set; }

        public int StoreId { get; set; }

        public Product _Product { get; set; }

        public Store _Store { get; set; }
    }
}
