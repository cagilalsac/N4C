using AutoMapper;

namespace N4C.App
{
    public class MapConfig : Profile
    {
    }

    public class QueryConfig : MapConfig
    {
        public bool NoTracking { get; set; } = true;
        public bool SplitQuery { get; set; }
    }
}
