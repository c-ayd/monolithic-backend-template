using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Template.Test.Integration.Api.Collections;
using Template.Test.Utility.Fixtures.Hosting;

namespace Template.Test.Integration.Api.Middlewares
{
    [Collection(nameof(TestHostCollection))]
    public class GlobalExceptionHandlerTest
    {
        private readonly TestHostFixture _testHostFixture;

        public GlobalExceptionHandlerTest(TestHostFixture testHostFixture)
        {
            _testHostFixture = testHostFixture;
        }

        [Fact]
        public async Task ExceptionEndpoint_WhenEndpointThrowsException_ShouldReturnCorrectResponse()
        {
            // Act
            var response = await _testHostFixture.Client.GetAsync("/test/exception");
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GlobalExceptionHandlerResult>(content, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Equal(MediaTypeNames.Application.ProblemJson, response.Content.Headers.ContentType.MediaType);

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, result.Status);
            Assert.Equal("Internal server error", result.Title);
            Assert.Equal("GET /test/exception",result.Instance);
            Assert.Equal("https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1", result.Type);
            Assert.NotNull(result.TraceId);
        }

        private class GlobalExceptionHandlerResult
        {
            public int? Status { get; set; }
            public string? Title { get; set; }
            public string? Detail { get; set; }
            public string? Instance { get; set; }
            public string? Type { get; set; }
            public string? TraceId { get; set; }
        }
    }
}
