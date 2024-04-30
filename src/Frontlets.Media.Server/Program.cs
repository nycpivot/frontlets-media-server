using Frontlets.Media.Server;

var builder = Host.CreateApplicationBuilder(args);

var configuration = builder.Configuration;

//configuration.SetBasePath(Directory.GetCurrentDirectory());

//configuration.AddJsonFile("appsettings.Production.json");

//builder.Services.Configure<MediaStorage>(configuration.GetSection(nameof(MediaStorage)));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
