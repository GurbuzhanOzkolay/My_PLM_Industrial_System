namespace plm_api
{
    public class Category
    {
        public int Id { get; set; }
        public required string Name { get; set; } // required ekledik

        //  Kendi Kendine Bağlanma (Category Tree) Kodları
        public int? ParentId { get; set; }
        public Category? Parent { get; set; }
        public List<Category> Children { get; set; } = new();

        //  Ürün İlişkisi (EKSİK OLAN KISIM): 
        // Bir kategorinin birden fazla ürünü olabilir.
        public List<Products> Products { get; set; } = new();
    }
}