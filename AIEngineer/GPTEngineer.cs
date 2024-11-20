using DatabaseExecutionValidators.Tests.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DatabaseExecutionValidators.Tests.AIEngineer
{
	internal class GPTEngineer : IAIEngineer
	{
		private readonly HttpClient _httpClient;
		private readonly string _chatGptApiKey;

		public GPTEngineer(HttpClient httpClient, string chatGptApiKey)
		{
			_httpClient = httpClient;
			_chatGptApiKey = chatGptApiKey;
		}

		public async Task<List<TestCase>> GenerateTestCasesAsync(string procedureName, string procedureContent)
		{
			var prompt = GeneratePrompt(procedureName, procedureContent);
			var aiResponse = await CallChatGptApiAsync(prompt);

			return ParseResponse(aiResponse);
		}

		private string GeneratePrompt(string procedureName, string procedureContent)
		{
			return $@"
You are an expert database engineer specializing in SQL stored procedure testing. Analyze the following stored procedure:

Name: '{procedureName}'
Content:
```sql
{procedureContent}";
		}

		private async Task<string> CallChatGptApiAsync(string prompt)
		{
			var requestBody = new
			{
				model = "gpt-4",
				messages = new[]
				{
				new { role = "system", content = "You are a database testing assistant." },
				new { role = "user", content = prompt }
			},
				temperature = 0.7
			};

			var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
			_httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_chatGptApiKey}");

			var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
			response.EnsureSuccessStatusCode();

			return await response.Content.ReadAsStringAsync();
		}

		private List<TestCase> ParseResponse(string aiResponse)
		{
			try
			{
				var jsonResponse = JsonDocument.Parse(aiResponse);
				var content = jsonResponse.RootElement.GetProperty("choices")[0]
									 .GetProperty("message").GetProperty("content").GetString();
				return JsonSerializer.Deserialize<List<TestCase>>(content) ?? new List<TestCase>();
			}
			catch
			{
				return new List<TestCase>();
			}
		}
	}
}
