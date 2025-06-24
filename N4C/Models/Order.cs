using N4C.Extensions;

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
                Expression = Expression.HasNotAny() ? _expressions?.FirstOrDefault().Key.HasNotAny(string.Empty) : Expression;
            }
        }

        public Order()
        {
            Expressions = new Dictionary<string, string>();
        }
    }
}
