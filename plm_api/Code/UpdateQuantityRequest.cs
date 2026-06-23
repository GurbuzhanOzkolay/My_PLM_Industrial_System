namespace plm_api.Code
{
    public class UpdateQuantityRequest
    {
        public int ParentProductId { get; set; }
        public int ChildProductId { get; set; }
        public decimal NewQuantity { get; set; }
    }
}