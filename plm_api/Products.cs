using plm_api;

public class Products
{
    public int Id { get; set; }

    public string ProductName { get; set; }

    public decimal Price { get; set; } = 0;

    public DateTime Stt_Date { get; set; }

    public int MinStokValue { get; set; }

    public string? Ingredients { get; set; }

     public int? CategoryId { get; set; }

    public Category? Category { get; set; }
}