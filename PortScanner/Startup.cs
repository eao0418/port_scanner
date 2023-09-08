using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PortScanner.Config;
using PortScanner.Converter;
using PortScanner.Provider;

[assembly: FunctionsStartup(typeof(PortScanner.Startup))]
namespace PortScanner
{
    class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            IConfigurationBuilder configBuilder = ConfigLoader.LoadConfig();
            IConfigurationRoot config = configBuilder
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            builder.Services.Configure<PortScannerOptions>(config.GetSection("portScannerOptions"));

            builder.Services.AddSingleton<Scanner>();
            builder.Services.AddSingleton<ITokenCredentialProvider, TokenCredentialProvider>();

            if (Util.Utils.IsDevelopmentEnvironment())
            {
                builder.Services.AddSingleton<ITableClientFactory>(provider =>
                {
                    return new TableClientFactory();
                });
            }
            else
            {
                builder.Services.AddSingleton<ITableClientFactory, TableClientFactory>();
            }

            builder.Services.AddLogging();

            builder.Services.AddMvcCore().AddNewtonsoftJson(options => {
                options.SerializerSettings.Converters.Add(new IPAddressConverter());
            });
        }
    }
}