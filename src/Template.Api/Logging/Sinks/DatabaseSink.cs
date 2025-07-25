using Cayd.AspNetCore.FlexLog.Logging;
using Cayd.AspNetCore.FlexLog.Sinks;
using Npgsql;
using System.Text;
using System.Text.Json;

namespace Template.Api.Logging.Sinks
{
    public class DatabaseSink : FlexLogSink
    {
        /**
         * NOTE: This logging sink is an example for the template.
         * Consider creating a new sink to save logs in a desired format in a desired location.
         */

        private readonly string _adminConnectionString;
        private readonly string _connectionString;
        private readonly string _databaseName;
        private readonly string _tableName;

        private bool _isOnline;

        public DatabaseSink(string connectionString)
        {
            _connectionString = connectionString;
            _adminConnectionString = new NpgsqlConnectionStringBuilder(connectionString)
            {
                Database = "postgres"
            }.ToString();

            var sections = connectionString.Split(';');
            var dbSection = sections
                .Where(s => s.StartsWith("database=", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault()!;
            _databaseName = dbSection.Split('=')[1];

            _tableName = "Logs";

            _isOnline = false;
        }

        public override async Task InitializeAsync()
        {
            try
            {
                await PrepareDatabase();
                _isOnline = true;
            }
            catch
            {
                _isOnline = false;
            }
        }

        private async Task PrepareDatabase()
        {
            using var adminConnection = new NpgsqlConnection(_adminConnectionString);
            await adminConnection.OpenAsync();

            if (!(await CheckIfDatabaseExistsAsync(adminConnection)))
            {
                await CreateDatabaseAsync(adminConnection);
                await CreateTableAsync();
            }
            else if (!(await CheckIfTableExistsAsync()))
            {
                await CreateTableAsync();
            }
        }

        private async Task<bool> CheckIfDatabaseExistsAsync(NpgsqlConnection adminConnection)
        {
            using var command = new NpgsqlCommand();
            command.Connection = adminConnection;
            command.CommandText = "SELECT 1 FROM pg_database WHERE datname = @dbName";
            command.Parameters.AddWithValue("dbName", _databaseName);

            var result = await command.ExecuteScalarAsync();
            return result != null;
        }

        private async Task CreateDatabaseAsync(NpgsqlConnection adminConnection)
        {
            using var command = new NpgsqlCommand();
            command.Connection = adminConnection;
            command.CommandText = $"CREATE DATABASE \"{_databaseName}\"";
            await command.ExecuteNonQueryAsync();
        }

        private async Task<bool> CheckIfTableExistsAsync()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand();
            command.Connection = connection;
            command.CommandText = new StringBuilder()
                .Append("SELECT EXISTS (SELECT 1 FROM information_schema.tables ")
                .Append("WHERE table_schema = 'public' AND table_name = @tableName)")
                .ToString();
            command.Parameters.AddWithValue("tableName", _tableName);

            var result = await command.ExecuteScalarAsync();
            return result != null && result is bool exists && exists;
        }

        private async Task CreateTableAsync()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand();
            command.Connection = connection;
            command.CommandText = new StringBuilder()
                .Append("CREATE TABLE public.\"").Append(_tableName).Append("\" (")
                .Append("\"Id\" UUID PRIMARY KEY,")
                .Append("\"TraceId\" TEXT,")
                .Append("\"Timestamp\" TIMESTAMP WITHOUT TIME ZONE NOT NULL,")
                .Append("\"ElapsedTimeInMs\" DOUBLE PRECISION NOT NULL,")
                .Append("\"Protocol\" TEXT NOT NULL,")
                .Append("\"Endpoint\" TEXT NOT NULL,")
                .Append("\"QueryString\" TEXT,")
                .Append("\"RequestBodyContentType\" TEXT,")
                .Append("\"RequestBody\" TEXT,")
                .Append("\"RequestBodySizeInBytes\" BIGINT,")
                .Append("\"Headers\" TEXT NOT NULL,")
                .Append("\"Claims\" TEXT NOT NULL,")
                .Append("\"ResponseStatusCode\" INTEGER,")
                .Append("\"ResponseContentType\" TEXT,")
                .Append("\"ResponseBody\" TEXT,")
                .Append("\"LogEntries\" TEXT NOT NULL)")
                .ToString();
            await command.ExecuteNonQueryAsync();
        }

        public override async Task SaveLogsAsync(IReadOnlyList<FlexLogContext> buffer)
        {
            if (!_isOnline)
            {
                await PrepareDatabase();
            }

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new NpgsqlCommand();
            command.Connection = connection;

            var cmdBuilder = new StringBuilder()
                .Append("INSERT INTO \"").Append(_tableName).Append("\"(")
                .Append("\"Id\",")
                .Append("\"TraceId\",")
                .Append("\"Timestamp\",")
                .Append("\"ElapsedTimeInMs\",")
                .Append("\"Protocol\",")
                .Append("\"Endpoint\",")
                .Append("\"QueryString\",")
                .Append("\"RequestBodyContentType\",")
                .Append("\"RequestBody\",")
                .Append("\"RequestBodySizeInBytes\",")
                .Append("\"Headers\",")
                .Append("\"Claims\",")
                .Append("\"ResponseStatusCode\",")
                .Append("\"ResponseContentType\",")
                .Append("\"ResponseBody\",")
                .Append("\"LogEntries\") VALUES ");

            for (int i = 0; i < buffer.Count; ++i)
            {
                cmdBuilder.Append("(")
                    .Append("@Id").Append(i).Append(',')
                    .Append("@TraceId").Append(i).Append(',')
                    .Append("@Timestamp").Append(i).Append(',')
                    .Append("@ElapsedTimeInMs").Append(i).Append(',')
                    .Append("@Protocol").Append(i).Append(',')
                    .Append("@Endpoint").Append(i).Append(',')
                    .Append("@QueryString").Append(i).Append(',')
                    .Append("@RequestBodyContentType").Append(i).Append(',')
                    .Append("@RequestBody").Append(i).Append(',')
                    .Append("@RequestBodySizeInBytes").Append(i).Append(',')
                    .Append("@Headers").Append(i).Append(',')
                    .Append("@Claims").Append(i).Append(',')
                    .Append("@ResponseStatusCode").Append(i).Append(',')
                    .Append("@ResponseContentType").Append(i).Append(',')
                    .Append("@ResponseBody").Append(i).Append(',')
                    .Append("@LogEntries").Append(i).Append(')');

                if (i != buffer.Count - 1)
                {
                    cmdBuilder.Append(',');
                }

                command.Parameters.AddWithValue($"Id{i}", buffer[i].Id);
                command.Parameters.AddWithValue($"TraceId{i}", buffer[i].TraceId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue($"Timestamp{i}", buffer[i].Timestamp);
                command.Parameters.AddWithValue($"ElapsedTimeInMs{i}", buffer[i].ElapsedTimeInMilliseconds);
                command.Parameters.AddWithValue($"Protocol{i}", buffer[i].Protocol);
                command.Parameters.AddWithValue($"Endpoint{i}", buffer[i].Endpoint);
                command.Parameters.AddWithValue($"QueryString{i}", buffer[i].QueryString ?? (object)DBNull.Value);
                command.Parameters.AddWithValue($"RequestBodyContentType{i}", buffer[i].RequestBodyContentType ?? (object)DBNull.Value);
                command.Parameters.AddWithValue($"RequestBody{i}", buffer[i].RequestBody ?? (object)DBNull.Value);
                command.Parameters.AddWithValue($"RequestBodySizeInBytes{i}", buffer[i].RequestBodySizeInBytes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue($"Headers{i}", JsonSerializer.Serialize(buffer[i].Headers));
                command.Parameters.AddWithValue($"Claims{i}", JsonSerializer.Serialize(buffer[i].Claims));
                command.Parameters.AddWithValue($"ResponseStatusCode{i}", buffer[i].ResponseStatusCode ?? (object)DBNull.Value);
                command.Parameters.AddWithValue($"ResponseContentType{i}", buffer[i].ResponseBodyContentType ?? (object)DBNull.Value);
                command.Parameters.AddWithValue($"ResponseBody{i}", buffer[i].ResponseBody ?? (object)DBNull.Value);
                command.Parameters.AddWithValue($"LogEntries{i}", JsonSerializer.Serialize(buffer[i].LogEntries
                    .Select(e => new
                    {
                        e.Category,
                        LogLevel = e.LogLevel.ToString(),
                        e.Message,
                        Exception = new
                        {
                            e.Exception?.Message,
                            e.Exception?.StackTrace
                        },
                        e.Metadata
                    })
                    .ToList()));
            }

            command.CommandText = cmdBuilder.ToString();
            await command.ExecuteNonQueryAsync();
        }
    }
}
