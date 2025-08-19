using Cayd.AspNetCore.FlexLog.Logging;
using Cayd.AspNetCore.FlexLog.Sinks;
using Cayd.Uuid;
using System.Text;
using System.Text.Json;

namespace Template.Api.Logging.Sinks
{
    public class FileSink : FlexLogSink
    {
        /**
         * NOTE: This logging sink is an example for the template.
         * Consider creating a new sink to save logs in a desired format in a desired location.
         */

        private readonly string _folderPath;

        public FileSink()
        {
            _folderPath = Path.GetFullPath("./Logging/Logs/");
        }

        public override async Task InitializeAsync()
        {
            if (!Directory.Exists(_folderPath))
            {
                Directory.CreateDirectory(_folderPath);
            }
        }

        public override async Task SaveLogsAsync(IReadOnlyList<FlexLogContext> buffer)
        {
            var filePath = Path.Combine(_folderPath, $"Logs_{DateTime.UtcNow.ToString("yyyy-MM-dd")}.txt");
            await using var streamWriter = new StreamWriter(filePath, append: true);

            var strBuilder = new StringBuilder();
            foreach (var log in buffer)
            {
                strBuilder.Clear();
                strBuilder.Append("ID: ").Append(Uuid.V7.Generate()).Append(" - ")
                    .Append("Trace ID: ").Append(log.TraceId).Append(" - ")
                    .Append("Timestamp: ").Append(log.Timestamp).Append(" - ")
                    .Append("Elapsed Time: ").Append(log.ElapsedTimeInMilliseconds).Append(" ms - ")
                    .Append("Protocol: ").Append(log.Protocol).Append(" - ")
                    .Append("Endpoint: ").Append(log.Endpoint).Append(" - ")
                    .Append("Query String: ").Append(log.QueryString).Append(" - ")
                    .Append("Request Body Content Type: ").Append(log.RequestBodyContentType).Append(" - ")
                    .Append("Request Body: ").Append(log.RequestBody).Append(" - ")
                    .Append("Request Body Size: ").Append(log.RequestBodySizeInBytes).Append(" bytes - ")
                    .Append("Headers: ").Append(JsonSerializer.Serialize(log.Headers)).Append(" - ")
                    .Append("Claims: ").Append(JsonSerializer.Serialize(log.Claims)).Append(" - ")
                    .Append("Response Status Code: ").Append(log.ResponseStatusCode).Append(" - ")
                    .Append("Response Content Type: ").Append(log.ResponseBodyContentType).Append(" - ")
                    .Append("Response Body: ").Append(log.ResponseBody).Append(" - ")
                    .Append("Log Entries: ").Append(JsonSerializer.Serialize(log.LogEntries
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

                await streamWriter.WriteLineAsync(strBuilder.ToString());
            }
        }
    }
}
