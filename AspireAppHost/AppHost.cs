var builder = DistributedApplication.CreateBuilder(args);




builder.AddProject<Projects.Emustates_Site>("emustates-site");




builder.Build().Run();
