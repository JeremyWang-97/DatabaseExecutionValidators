using DatabaseExecutionValidators.Tests.Engines;
using DatabaseExecutionValidators.Tests.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseExecutionValidators.Tests.Validators
{
	internal class DatabaseValidator
	{
		private readonly IEngine _destinationEngine;
		private readonly IEngine _targetEngine;
		private readonly IReportManager _reportManager;

		public DatabaseValidator(IEngine destinationEngine, IEngine targetEngine, IReportManager reportManager)
		{
			_destinationEngine = destinationEngine;
			_targetEngine = targetEngine;
			_reportManager = reportManager;
		}

		public void ValidateProcedures()
		{
			var procedures = _destinationEngine.GetAllStoredProcedures();
			foreach (var procedure in procedures)
			{
				var (destSuccess, destResult, destError) = _destinationEngine.ExecuteStoredProcedure(procedure, new());
				var (targetSuccess, targetResult, targetError) = _targetEngine.ExecuteStoredProcedure(procedure, new());

				if (!destSuccess || !targetSuccess)
				{
					_reportManager.LogTest(procedure, "Fail", $"Error: {destError ?? targetError}");
					continue;
				}

				if (destResult.SequenceEqual(targetResult))
				{
					_reportManager.LogTest(procedure, "Pass", "Results matched.");
				}
				else
				{
					_reportManager.LogTest(procedure, "Fail", $"Result mismatch.\nExpected: {destResult}\nActual: {targetResult}");
				}
			}
		}
	}
}
