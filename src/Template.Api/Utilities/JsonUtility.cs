using Microsoft.AspNetCore.Mvc;

namespace Template.Api.Utilities
{
    public static class JsonUtility
    {
        public const string StatusCodeKey = "statusCode";
        public const string IsSuccessKey = "isSuccess";
        public const string DataKey = "data";
        public const string ErrorsKey = "errors";
        public const string MetadataKey = "metadata";

        public static ObjectResult Success(int statusCode, object? metadata = null)
        {
            var response = new Dictionary<string, object?>()
            {
                { StatusCodeKey, statusCode },
                { IsSuccessKey, true }
            };

            if (metadata != null)
            {
                response.Add(MetadataKey, metadata);
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
                { StatusCodeKey, statusCode },
                { IsSuccessKey, true },
                { DataKey, data }
            };

            if (metadata != null)
            {
                response.Add(MetadataKey, metadata);
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
                { StatusCodeKey, statusCode },
                { IsSuccessKey, false }
            };

            if (metadata != null)
            {
                response.Add(MetadataKey, metadata);
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
                { StatusCodeKey, statusCode },
                { IsSuccessKey, false },
                { ErrorsKey, errors }
            };

            if (metadata != null)
            {
                response.Add(MetadataKey, metadata);
            }

            return new ObjectResult(response)
            {
                StatusCode = statusCode
            };
        }
    }
}
