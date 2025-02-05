var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.MVC>("mvc");

builder.Build().Run();
