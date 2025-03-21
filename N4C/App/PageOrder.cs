﻿namespace N4C.App
{
    public class PageOrder
    {
        public int PageNumber { get; set; }
        public string RecordsPerPageCount { get; set; }
        public List<string> RecordsPerPageCounts { get; set; }
        public int TotalRecordsCount { get; set; }
        public string OrderExpression { get; set; }
        public Dictionary<string, string> OrderExpressions { get; set; }
        public bool Session { get; set; }

        public List<int> PageNumbers
        {
            get
            {
                var pageNumbers = new List<int>();
                int recordsPerPageCount;
                if (TotalRecordsCount > 0 && int.TryParse(RecordsPerPageCount, out recordsPerPageCount))
                {
                    var numberOfPages = Convert.ToInt32(Math.Ceiling(TotalRecordsCount / Convert.ToDecimal(recordsPerPageCount)));
                    for (int page = 1; page <= numberOfPages; page++)
                    {
                        pageNumbers.Add(page);
                    }
                }
                else
                {
                    pageNumbers.Add(1);
                }
                return pageNumbers;
            }
        }

        public PageOrder()
        {
            PageNumber = 1;
            RecordsPerPageCount = "10";
            RecordsPerPageCounts = new List<string>();
            OrderExpression = string.Empty;
            OrderExpressions = new Dictionary<string, string>();
        }
    }
}
