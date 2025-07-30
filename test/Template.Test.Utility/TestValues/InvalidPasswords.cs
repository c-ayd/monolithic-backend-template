using Cayd.Test.Generators;

namespace Template.Test.Utility.TestValues
{
    public static partial class TestValues
    {
        public static IEnumerable<object?[]> GetInvalidPassword()
            => new List<object?[]>
            {
                new object?[] { null },
                new object?[] { "" },
                new object?[] { " " },
                new object?[] { PasswordGenerator.Generate() }
            };
    }
}
