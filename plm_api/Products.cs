namespace plm_api
{
    public class Products
    {                            
        public int Id { get; set; }

        public string ProductName { get; set; }

        public decimal Price { get; set; } = 0;

        public DateTime Stt_Date { get; set; }   

        public int MinStokValue { get; set; }

        public string? ImageUrl { get; set; } 


        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}