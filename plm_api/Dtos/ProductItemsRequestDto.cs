namespace plm_api.Dtos
{
    public class ProductItemsRequestDto
    {
            public int ParentProductId { get; set; }
            public int ChildProductId { get; set; }
            public decimal Quantity { get; set; }
        
    }
}
