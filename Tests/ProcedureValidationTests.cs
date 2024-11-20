using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using DatabaseExecutionValidators.Tests.Engines;
using DatabaseExecutionValidators.Tests.Reporting;
using System;
using DatabaseExecutionValidators.Tests.AIEngineer;

namespace DatabaseExecutionValidators.Tests.Tests
{
	[TestFixture]
	public class ProcedureValidationTests
	{
		private IConfiguration _configuration;
		private IEngine _destinationEngine;
		private IEngine _targetEngine;
		private IAIEngineer _aiEngineer;
		private IReportManager _reportManager;

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			_configuration = new ConfigurationBuilder()
				.SetBasePath(AppContext.BaseDirectory)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.Build();

			var services = new ServiceCollection();
			services.AddTransient<SqlEngine>(provider =>
				new SqlEngine());
			services.AddTransient<PostgreSqlEngine>(provider =>
				new PostgreSqlEngine());
			services.AddSingleton<IAIEngineer>(sp =>
			{
				var useCopilot = true;
				if (useCopilot)
					return new CopilotEngineer();
				else
					return new GPTEngineer(new HttpClient(), _configuration["AIEngineer:GPTToken"]);
			});
			services.AddSingleton<IReportManager, ExtentReportManager>();

			var serviceProvider = services.BuildServiceProvider();

			_destinationEngine = serviceProvider.GetRequiredService<SqlEngine>();
			_targetEngine = serviceProvider.GetRequiredService<PostgreSqlEngine>();
			_reportManager = serviceProvider.GetRequiredService<IReportManager>();

			_destinationEngine.Connect(
				_configuration["DatabaseSettings:Destination:ConnectionString"],
					_configuration["DatabaseSettings:Destination:DatabaseName"]);
			_targetEngine.Connect(
				_configuration["DatabaseSettings:Target:ConnectionString"],
					_configuration["DatabaseSettings:Target:DatabaseName"]);

			_reportManager.Initialize(_configuration["ReportSettings:OutputPath"]);
		}

		[Test]
		public async Task ValidateStoredProceduresWithGeneratedTestCases()
		{
			var destinationProcedures = _destinationEngine.GetAllStoredProcedures();

			foreach (var procedureName in destinationProcedures)
			{
				var procedureContent = _destinationEngine.GetProcedureContent(procedureName);
				var testCases = await _aiEngineer.GenerateTestCasesAsync(procedureName, procedureContent);

				foreach (var testCase in testCases)
				{
					_reportManager.CreateTest(testCase.TestName);

					try
					{
						var destinationResult = _destinationEngine.ExecuteStoredProcedure(procedureName, testCase.Parameters);
						var targetResult = _targetEngine.ExecuteStoredProcedure(procedureName, testCase.Parameters);

						var comparison = CompareResults(destinationResult, targetResult);

						if (comparison.Passed)
						{
							_reportManager.LogPass($"{testCase.Purpose}: Passed. Results matched.");
						}
						else
						{
							_reportManager.LogFail(
								$"{testCase.Purpose}: Failed. Mismatched results.\n" +
								$"Expected: {comparison.Expected}\n" +
								$"Actual: {comparison.Actual}"
							);
						}
					}
					catch (Exception ex)
					{
						_reportManager.LogFail($"{testCase.Purpose}: Error - {ex.Message}");
					}
				}
			}
		}

		private ResultComparison CompareResults(dynamic destinationResult, dynamic targetResult)
		{
			if (destinationResult.RowCount != targetResult.RowCount)
			{
				return new ResultComparison
				{
					Passed = false,
					Expected = $"Row count: {destinationResult.RowCount}",
					Actual = $"Row count: {targetResult.RowCount}"
				};
			}

			for (int i = 0; i < destinationResult.RowCount; i++)
			{
				if (!Equals(destinationResult.Rows[i], targetResult.Rows[i]))
				{
					return new ResultComparison
					{
						Passed = false,
						Expected = $"Row {i + 1}: {destinationResult.Rows[i]}",
						Actual = $"Row {i + 1}: {targetResult.Rows[i]}"
					};
				}
			}

			return new ResultComparison { Passed = true };
		}

		[OneTimeTearDown]
		public void Cleanup()
		{
			_destinationEngine.Dispose();
			_targetEngine.Dispose();
		}
	}

	public class ResultComparison
	{
		public bool Passed { get; set; }
		public string Expected { get; set; }
		public string Actual { get; set; }
	}
}
