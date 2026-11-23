using InterCase.Common.Dtos.Workers;

namespace InterCase.Service.Interfaces
{
    public interface IWorkerStatusService
    {
        WorkerStatusResponse GetStatus();
    }
}
