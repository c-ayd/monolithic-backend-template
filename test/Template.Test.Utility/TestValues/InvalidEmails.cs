namespace Template.Test.Utility.TestValues
{
    public static partial class TestValues
    {
        public static IEnumerable<object?[]> GetInvalidEmails()
            => new List<object?[]>
            {
                new object?[] { null },
                new object?[] { "" },
                new object?[] { " " },
                new object?[] { " test@test.com " },
                new object?[] { "abcdfg" },
            };
    }
}
