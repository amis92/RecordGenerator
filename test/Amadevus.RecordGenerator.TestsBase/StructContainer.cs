namespace Amadevus.RecordGenerator.TestsBase
{
    [Record(Features.Default | Features.Equality)]
    public readonly partial struct StructContainer 
    {
        public int Id { get; }
    }
}
