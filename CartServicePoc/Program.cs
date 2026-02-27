using CartServicePoc;
using CartServicePoc.Infrastructure.Database;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")!;

DbMigrator.MigrateDatabase(connectionString);

var host = builder.Build();
host.Run();