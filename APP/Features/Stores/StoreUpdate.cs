using APP.Domain;
using N4C.App;
using N4C.App.Services;
using N4C.App.Services.Files;
using N4C.Attributes;

namespace APP.Features.Stores
{
    public class StoreUpdateRequest : UpdateRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 5)]
        public string Name { get; set; }

        public bool Virtual { get; set; }
    }

    public class StoreUpdateHandler : Handler<Store, StoreUpdateRequest>
    {
        public StoreUpdateHandler(IAppDb db, HttpService httpService, FileService fileService, LogService logService) : base(db, httpService, fileService, logService)
        {
        }
    }
}
