namespace TestHelper
{
    //http://stackoverflow.com/questions/22093843
    /// <summary>
    /// Single set of arguments for a theory.
    /// </summary>
    public interface ITheoryDatum
    {
        object[] ToParameterArray();
    }
}
