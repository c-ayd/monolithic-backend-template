namespace Template.Test.Utility.TestValues
{
    public static partial class TestValues
    {
        public static IEnumerable<object?[]> GetInvalidStrings()
            => new List<object?[]>
            {
                new object?[] { null },
                new object?[] { "" },
                new object?[] { " " }
            };
    }
}
