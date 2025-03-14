using APP.Domain;
using N4C.App;
using N4C.App.Services;
using N4C.App.Services.Files;
using N4C.Attributes;
using N4C.Domain;

namespace APP.Features.Stores
{
    public class StoreCreateRequest : CreateRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 5)]
        public string Name { get; set; }

        public bool Virtual { get; set; }
    }

    public class StoreCreateHandler : Handler<Store, StoreCreateRequest>
    {
        public StoreCreateHandler(IDb db, HttpService httpService, FileService fileService, LogService logService) : base(db, httpService, fileService, logService)
        {
        }
    }
}
