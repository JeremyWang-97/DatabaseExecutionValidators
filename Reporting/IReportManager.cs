using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseExecutionValidators.Tests.Reporting
{
	internal interface IReportManager
	{
		void Initialize(string reportPath);
		void CreateTest(string procedureName);
		void LogPass(string details);
		void LogFail(string details);
		void FinalizeReport();
	}
}
