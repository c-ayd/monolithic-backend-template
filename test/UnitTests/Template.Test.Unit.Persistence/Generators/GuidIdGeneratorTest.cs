using Template.Persistence.Generators;

namespace Template.Test.Unit.Persistence.Generators
{
    public class GuidIdGeneratorTest
    {
        private readonly GuidIdGenerator _generator;

        private readonly int _numberOfGuidsToGenerate = 1000;

        public GuidIdGeneratorTest()
        {
            _generator = new GuidIdGenerator();
        }

        [Fact]
        public void Next_ShouldGenerateUniqueGuids()
        {
            // Arrange
            var guids = new List<Guid>();

            // Act
            for (int i = 0; i < _numberOfGuidsToGenerate; ++i)
            {
                guids.Add(_generator.Next(null));
            }

            // Assert
            var guidSet = guids.ToHashSet();
            Assert.Equal(guids.Count, guidSet.Count);
        }
    }
}
