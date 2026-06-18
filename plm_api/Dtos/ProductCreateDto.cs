namespace plm_api.Dtos
{
    public class ProductCreateDto
    {
        public string ProductName { get; set; }
        public decimal Price { get; set; } = 0;
        public DateTime Stt_Date { get; set; }
        public int MinStokValue { get; set; }

        public string? ImageUrl { get; set; }
        // Bu ürünün bağlanacağı kategori id'leri
        public List<int> CategoryIds { get; set; } = new();
    }
} 