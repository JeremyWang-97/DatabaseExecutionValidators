using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseExecutionValidators.Tests.Engines
{
	internal interface IEngine : IDisposable
	{
		void Connect(string connectionString, string databaseName);
		List<string> GetAllStoredProcedures();
		(bool success, List<Dictionary<string, object>> result, string errorMessage) ExecuteStoredProcedure(
			string procedureName,
			Dictionary<string, object> parameters
		);
		string GetProcedureContent(string procedureName);
	}
}
