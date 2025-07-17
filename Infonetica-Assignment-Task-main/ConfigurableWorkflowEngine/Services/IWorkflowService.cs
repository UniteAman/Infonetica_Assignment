using ConfigurableWorkflowEngine.Models;
using ConfigurableWorkflowEngine.DTOs;

namespace ConfigurableWorkflowEngine.Services
{
    public interface IWorkflowService
    {
        Task<WorkflowDefinition> CreateWorkflowAsync(CreateWorkflowRequest request);
        Task<WorkflowDefinition?> GetWorkflowDefinitionAsync(string id);
        Task<IEnumerable<WorkflowDefinition>> GetAllWorkflowDefinitionsAsync();
        Task<WorkflowInstance> StartWorkflowInstanceAsync(StartInstanceRequest request);
        Task<WorkflowInstance> ExecuteActionAsync(string instanceId, ExecuteActionRequest request);
        Task<WorkflowInstance?> GetWorkflowInstanceAsync(string instanceId);
        Task<IEnumerable<WorkflowInstance>> GetAllWorkflowInstancesAsync();
    }
}
