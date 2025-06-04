using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using N4C.Extensions;
using N4C.Models;
using N4C.Services;
using N4C.Users.App.Domain;
using N4C.Users.App.Models;

namespace N4C.Users.App.Services
{
    public class N4CStatusService : Service<N4CStatus, N4CStatusRequest, N4CStatusResponse>
    {
        public N4CStatusService(N4CUsersDb db, IHttpContextAccessor httpContextAccessor, ILogger<Service> logger) : base(db, httpContextAccessor, logger)
        {
        }

        protected override void Set(Action<ServiceConfig<N4CStatus, N4CStatusRequest, N4CStatusResponse>> config)
        {
            base.Set(config =>
            {
                config.SetResponse()
                    .Map(destination => destination.Users, source => string.Join("<br>", source.Users));
                config.SetTitle("Durum", "Status");
            });
        }

        protected override IQueryable<N4CStatus> Query()
        {
            return base.Query().Include(status => status.Users).OrderBy(status => status.Title);
        }

        public override Task<Result<N4CStatusRequest>> Delete(N4CStatusRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            Validate(GetEntity(request).Users);
            return base.Delete(request, save, cancellationToken);
        }
    }
}
