namespace OData.HttpQuery.Moc.Models
{
    public class Item
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int Total { get; set; }
        public int ProductId { get; set; }

        public Product Product { get; set; }
    }
}
