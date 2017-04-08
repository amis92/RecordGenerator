namespace TestHelper
{

    /// <summary>
    /// Provides factory method <see cref="Factory{TSystemUnderTest, TExecptedOutput}(TSystemUnderTest, TExecptedOutput, string)"/>.
    /// </summary>
    public static class TheoryDatum
    {
        public static ITheoryDatum Factory<TSystemUnderTest, TExecptedOutput>(TSystemUnderTest sut, TExecptedOutput expectedOutput, string description)
        {
            var datum = new TheoryDatum<TSystemUnderTest, TExecptedOutput>()
            {
                SystemUnderTest = sut,
                Description = description,
                ExpectedOutput = expectedOutput
            };
            return datum;
        }
    }

    /// <summary>
    /// Type-parametrized data set for theory tests, used by <see cref="TheoryDatum.Factory{TSystemUnderTest, TExecptedOutput}(TSystemUnderTest, TExecptedOutput, string)"/>.
    /// </summary>
    /// <typeparam name="TSystemUnderTest"></typeparam>
    /// <typeparam name="TExecptedOutput"></typeparam>
    public class TheoryDatum<TSystemUnderTest, TExecptedOutput> : ITheoryDatum
    {
        public TSystemUnderTest SystemUnderTest { get; set; }

        public string Description { get; set; }

        public TExecptedOutput ExpectedOutput { get; set; }

        public object[] ToParameterArray()
        {
            var output = new object[3];
            output[0] = SystemUnderTest;
            output[1] = ExpectedOutput;
            output[2] = Description;
            return output;
        }
    }
}
