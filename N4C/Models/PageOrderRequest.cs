namespace N4C.Models
{
    public class PageOrderRequest
    {
        public Page Page { get; set; } = new Page();
        public string OrderExpression { get; set; }
        public bool PageOrderSession { get; set; }
    }
}
