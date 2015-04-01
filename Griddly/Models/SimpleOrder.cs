namespace Griddly.Models
{
    public class SimpleOrder
    {
        public int Id { get; set; }
        public string Item { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
        public bool IsApproved { get; set; }
        public SimplePerson Person { get; set; }
    }
}