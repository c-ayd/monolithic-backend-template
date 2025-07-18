using Microsoft.AspNetCore.Mvc;

namespace Template.Api.Utilities
{
    public static class JsonUtility
    {
        private const string StatusCode = "StatusCode";
        private const string IsSuccess = "IsSuccess";
        private const string Data = "Data";
        private const string Errors = "Errors";
        private const string Metadata = "Metadata";

        public static ObjectResult Success(int statusCode, object? metadata = null)
        {
            var response = new Dictionary<string, object?>()
            {
                { StatusCode, statusCode },
                { IsSuccess, true }
            };

            if (metadata != null)
            {
                response.Add(Metadata, metadata);
            }

            return new ObjectResult(response)
            {
                StatusCode = statusCode
            };
        }

        public static ObjectResult Success(int statusCode, object? data, object? metadata = null)
        {
            var response = new Dictionary<string, object?>()
            {
                { StatusCode, statusCode },
                { IsSuccess, true },
                { Data, data }
            };

            if (metadata != null)
            {
                response.Add(Metadata, metadata);
            }

            return new ObjectResult(response)
            {
                StatusCode = statusCode
            };
        }

        public static ObjectResult Fail(int statusCode, object? metadata = null)
        {
            var response = new Dictionary<string, object?>()
            {
                { StatusCode, statusCode },
                { IsSuccess, true }
            };

            if (metadata != null)
            {
                response.Add(Metadata, metadata);
            }

            return new ObjectResult(response)
            {
                StatusCode = statusCode
            };
        }

        public static ObjectResult Fail(int statusCode, object? errors, object? metadata = null)
        {
            var response = new Dictionary<string, object?>()
            {
                { StatusCode, statusCode },
                { IsSuccess, true },
                { Errors, errors }
            };

            if (metadata != null)
            {
                response.Add(Metadata, metadata);
            }

            return new ObjectResult(response)
            {
                StatusCode = statusCode
            };
        }
    }
}
