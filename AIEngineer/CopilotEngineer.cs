using DatabaseExecutionValidators.Tests.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DatabaseExecutionValidators.Tests.AIEngineer
{
	internal class CopilotEngineer : IAIEngineer
	{
		public async Task<List<TestCase>> GenerateTestCasesAsync(string procedureName, string procedureContent)
		{
			var prompt = GeneratePrompt(procedureName, procedureContent);
			var copilotResponse = await CallCopilotCliAsync(prompt);

			return ParseResponse(copilotResponse);
		}

		private string GeneratePrompt(string procedureName, string procedureContent)
		{
			return $@"
You are a database testing assistant.
Analyze the stored procedure '{procedureName}' and generate test cases with varied inputs.
```sql
{procedureContent}";
		}

		private async Task<string> CallCopilotCliAsync(string prompt)
		{
			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "copilot",
					Arguments = $"chat --message \"{prompt}\"",
					RedirectStandardOutput = true,
					UseShellExecute = false,
					CreateNoWindow = true
				}
			};

			process.Start();
			var output = await process.StandardOutput.ReadToEndAsync();
			process.WaitForExit();

			return output;
		}

		private List<TestCase> ParseResponse(string copilotResponse)
		{
			try
			{
				return JsonSerializer.Deserialize<List<TestCase>>(copilotResponse) ?? new List<TestCase>();
			}
			catch
			{
				return new List<TestCase>();
			}
		}
	}
}
