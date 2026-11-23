using InterCase.Common.Dtos.Workers;

namespace InterCase.UI.Models
{
    public sealed record WorkerStatusViewModel(WorkerStatusResponse? Status)
    {
        public bool IsDown => Status is { IsRunning: false };
    }
}
