namespace plm_api
{
    public class UpdateQuantityRequest
    {
        public int ParentProductId { get; set; }
        public int ChildProductId { get; set; }
        public decimal NewQuantity { get; set; }
    }
}