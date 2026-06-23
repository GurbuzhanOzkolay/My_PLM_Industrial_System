namespace plm_api.Code
{
    public class Category
    {
        public int Id { get; set; }
        public required string Name { get; set; } 

        
        public int? ParentId { get; set; }
        public Category? Parent { get; set; }
        public List<Category> Children { get; set; } = new();

        
       
        public List<Products> Products { get; set; } = new();
    }
}