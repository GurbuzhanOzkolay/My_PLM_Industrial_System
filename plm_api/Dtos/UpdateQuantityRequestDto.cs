namespace plm_api.Dtos
{
    public class UpdateQuantityRequestDto
    {
        public int ParentProductId { get; set; }
        public int ChildProductId { get; set; }
        public decimal NewQuantity { get; set; }
    }
}
