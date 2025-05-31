var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.N4C_Users_Web>("n4c-users-web");

builder.Build().Run();
