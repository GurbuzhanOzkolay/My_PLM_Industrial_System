namespace plm_api
{
    public class ProductItem
    {
        public int Id { get; set; }

        
        public int ParentProductId { get; set; }
        public Products ParentProduct { get; set; }

       
        public int ChildProductId { get; set; }
        public Products ChildProduct { get; set; }

       
        public decimal Quantity { get; set; }
    }
} 