using Microsoft.Extensions.Configuration;
using ViolastroBot;
using ViolastroBot.Extensions;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var services = configuration.ConfigureServices();
var startup = new Startup(services);

await startup.Run();
