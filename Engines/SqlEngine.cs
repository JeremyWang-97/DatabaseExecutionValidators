using Microsoft.Data.SqlClient;
using MongoDB.Driver.Core.Configuration;
using System.Data;
using System.Text;

namespace DatabaseExecutionValidators.Tests.Engines
{
	internal class SqlEngine : IEngine
	{
		private SqlConnection _connection;

		public void Connect(string connectionString, string databaseName)
		{
			_connection = new SqlConnection($"{connectionString};Database={databaseName}");
			_connection.Open();
		}

		public List<string> GetAllStoredProcedures()
		{
			var procedures = new List<string>();
			string query = "SELECT name FROM sys.procedures";

			using var command = new SqlCommand(query, _connection);
			using var reader = command.ExecuteReader();
			while (reader.Read())
			{
				procedures.Add(reader.GetString(0));
			}

			return procedures;
		}

		public string GetProcedureContent(string procedureName)
		{
			var procedureContent = new StringBuilder();
			string query = $"EXEC sp_helptext '{procedureName}'";

			using (var command = new SqlCommand(query, _connection))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					procedureContent.AppendLine(reader.GetString(0));
				}
			}

			return procedureContent.ToString();
		}

		public (bool success, List<Dictionary<string, object>> result, string errorMessage) ExecuteStoredProcedure(
			string procedureName,
			Dictionary<string, object> parameters
		)
		{
			try
			{
				using var command = new SqlCommand(procedureName, _connection)
				{
					CommandType = CommandType.StoredProcedure
				};

				foreach (var param in parameters)
				{
					command.Parameters.AddWithValue(param.Key, param.Value);
				}

				using var reader = command.ExecuteReader();
				var result = new List<Dictionary<string, object>>();
				while (reader.Read())
				{
					var row = new Dictionary<string, object>();
					for (int i = 0; i < reader.FieldCount; i++)
					{
						row[reader.GetName(i)] = reader.GetValue(i);
					}
					result.Add(row);
				}

				return (true, result, null);
			}
			catch (Exception ex)
			{
				return (false, null, ex.Message);
			}
		}

		public void Dispose() => _connection?.Dispose();
	}
}
