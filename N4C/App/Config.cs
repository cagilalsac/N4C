using AutoMapper;

namespace N4C.App
{
    public interface IServiceConfig
    {
        public string Culture { get; set; }
        public string TitleTR { get; set; }
        public string TitleEN { get; set; }
    }

    public class MapConfig : Profile
    {
    }
}
