namespace OData.HttpQuery.Moc.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreationDate { get; set; }
        public int ContactId { get; set; }
        public List<Item> Items { get; set; }

        public Contact Contact { get; set; }
    }
}
