using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using N4C.Services;

namespace N4C.Controllers
{
    public class FilesController : MvcController
    {
        public FilesController(Service service, IModelMetadataProvider modelMetaDataProvider) : base(service, modelMetaDataProvider)
        {
        }

        public virtual IActionResult DownloadFile(string path)
        {
            var result = Service.GetFile(path);
            if (result.Success)
                return File(result.Data.FileStream, result.Data.FileContentType, result.Data.FileName);
            return View("_N4Cmessage", result);
        }
    }
}
