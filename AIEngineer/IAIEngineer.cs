using DatabaseExecutionValidators.Tests.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseExecutionValidators.Tests.AIEngineer
{
	internal interface IAIEngineer
	{
		Task<List<TestCase>> GenerateTestCasesAsync(string procedureName, string procedureContent);
	}
}
