namespace InterCase.Common
{
    public interface ICorrelationContext
    {
        string CorrelationId { get; set; }
    }

    public sealed class CorrelationContext : ICorrelationContext
    {
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString("N");
    }
}
