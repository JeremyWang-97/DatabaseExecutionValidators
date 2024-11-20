using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseExecutionValidators.Tests.Model
{
	internal class TestCase
	{
		public string TestName { get; set; }
		public Dictionary<string, object> Parameters { get; set; }
		public string Purpose { get; set; }
	}
}
