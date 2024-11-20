using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DatabaseExecutionValidators.Tests.Engines;
using DatabaseExecutionValidators.Tests.Reporting;
using DatabaseExecutionValidators.Tests.Validators;

var configuration = new ConfigurationBuilder()
	.SetBasePath(AppContext.BaseDirectory)
	.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
	.Build();

var destinationConfig = configuration.GetSection("DatabaseSettings:Destination");
var targetConfig = configuration.GetSection("DatabaseSettings:Target");
var reportConfig = configuration.GetSection("ReportSettings");

var services = new ServiceCollection();
services.AddTransient<SqlEngine>();
services.AddTransient<PostgreSqlEngine>();
services.AddSingleton<IReportManager, ExtentReportManager>();
services.AddSingleton<DatabaseValidator>();

var provider = services.BuildServiceProvider();

var destinationEngine = provider.GetRequiredService<SqlEngine>();
var targetEngine = provider.GetRequiredService<PostgreSqlEngine>();
var reportManager = provider.GetRequiredService<IReportManager>();

destinationEngine.Connect(
	destinationConfig["ConnectionString"],
	destinationConfig["DatabaseName"]
);

targetEngine.Connect(
	targetConfig["ConnectionString"],
	targetConfig["DatabaseName"]
);

reportManager.Initialize(reportConfig["OutputPath"]);

var validator = provider.GetRequiredService<DatabaseValidator>();
validator.ValidateProcedures();

reportManager.FinalizeReport();

Console.WriteLine($"Report generated: {reportConfig["OutputPath"]}");