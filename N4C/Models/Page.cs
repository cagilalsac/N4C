namespace N4C.Models
{
    public class Page
    {
        public int Number { get; set; }

        public List<int> Numbers
        {
            get
            {
                var numbers = new List<int>();
                int recordsPerPageCount;
                if (TotalRecordsCount > 0 && int.TryParse(RecordsPerPageCount, out recordsPerPageCount))
                {
                    var numberOfPages = Convert.ToInt32(Math.Ceiling(TotalRecordsCount / Convert.ToDecimal(recordsPerPageCount)));
                    for (int page = 1; page <= numberOfPages; page++)
                    {
                        numbers.Add(page);
                    }
                }
                else
                {
                    numbers.Add(1);
                }
                return numbers;
            }
        }

        public string RecordsPerPageCount { get; set; }

        private List<string> _recordsPerPageCounts;
        public List<string> RecordsPerPageCounts
        {
            get => _recordsPerPageCounts;
            set
            {
                _recordsPerPageCounts = value;
                RecordsPerPageCount = string.IsNullOrWhiteSpace(RecordsPerPageCount) ? _recordsPerPageCounts?.FirstOrDefault() ?? string.Empty : RecordsPerPageCount;
            }
        }

        public int TotalRecordsCount { get; set; }

        public Page()
        {
            Number = 1;
            RecordsPerPageCounts = new List<string>();
        }
    }
}
