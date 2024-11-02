namespace WorshipDomain.Core.Entities
{
    public class ResultFilter<T>
    {
        public IEnumerable<T> Data { get; set; }
        public int TotalRecords { get; set; }
    }
}
