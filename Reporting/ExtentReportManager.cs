using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;

namespace DatabaseExecutionValidators.Tests.Reporting
{
	internal class ExtentReportManager : IReportManager
	{
		private ExtentReports _extent;
		private ExtentTest _currentTest;

		public void Initialize(string reportPath)
		{
			var htmlReporter = new ExtentHtmlReporter(reportPath);
			_extent = new ExtentReports();
			_extent.AttachReporter(htmlReporter);
		}

		public void CreateTest(string procedureName)
		{
			_currentTest = _extent.CreateTest(procedureName);
		}

		public void LogPass(string details)
		{
			_currentTest?.Pass(details);
		}

		public void LogFail(string details)
		{
			_currentTest?.Fail(details);
		}

		public void FinalizeReport()
		{
			_extent.Flush();
		}
	}
}
