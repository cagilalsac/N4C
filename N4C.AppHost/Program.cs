var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.N4C_User_Web>("n4c-user-web");

builder.Build().Run();
