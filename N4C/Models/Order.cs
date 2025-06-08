namespace N4C.Models
{
    public class Order
    {
        public string Expression { get; set; }

        private Dictionary<string, string> _expressions;
        public Dictionary<string, string> Expressions
        {
            get
            {
                return _expressions;
            }
            set
            {
                _expressions = value;
                Expression = string.IsNullOrWhiteSpace(Expression) ? _expressions?.FirstOrDefault().Key ?? string.Empty : Expression;
            }
        }

        public Order()
        {
            Expressions = new Dictionary<string, string>();
        }
    }
}
