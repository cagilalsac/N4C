﻿using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using N4C.Extensions;
using N4C.Models;
using N4C.Services;
using N4C.User.App.Domain;
using N4C.User.App.Models;

namespace N4C.User.App.Services
{
    public class N4CStatusService : Service<N4CStatus, N4CStatusRequest, N4CStatusResponse>
    {
        public N4CStatusService(N4CUserDb db, IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory, ILogger<Service> logger) 
            : base(db, httpContextAccessor, httpClientFactory, logger)
        {
        }

        protected override IQueryable<N4CStatus> Query(Action<ServiceConfig<N4CStatus, N4CStatusRequest, N4CStatusResponse>> config = default)
        {
            return base.Query(config =>
            {
                config.SetResponse()
                    .Map(destination => destination.Users, source => string.Join("<br>", source.Users.Select(user => user.UserName)));
                config.SetTitle("Durum", "Status");
            }).Include(status => status.Users).OrderBy(status => status.Title);
        }

        public override Task<Result<N4CStatusRequest>> Delete(N4CStatusRequest request, bool save = true, CancellationToken cancellationToken = default)
        {
            Validate(GetEntity(request).Users);
            return base.Delete(request, save, cancellationToken);
        }
    }
}
