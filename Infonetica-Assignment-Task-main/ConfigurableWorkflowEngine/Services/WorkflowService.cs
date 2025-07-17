using ConfigurableWorkflowEngine.Models;
using ConfigurableWorkflowEngine.DTOs;
using ConfigurableWorkflowEngine.Services;

namespace ConfigurableWorkflowEngine.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly Dictionary<string, WorkflowDefinition> _workflows = new();
        private readonly Dictionary<string, WorkflowInstance> _instances = new();

        public async Task<WorkflowDefinition> CreateWorkflowAsync(CreateWorkflowRequest request)
        {
            // Validate workflow definition
            ValidateWorkflowDefinition(request);

            var workflow = new WorkflowDefinition
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                Description = request.Description,
                States = request.States.Select(s => new State
                {
                    Id = s.Id,
                    Name = s.Name,
                    IsInitial = s.IsInitial,
                    IsFinal = s.IsFinal,
                    Enabled = s.Enabled,
                    Description = s.Description
                }).ToList(),
                Actions = request.Actions.Select(a => new WorkflowAction
                {
                    Id = a.Id,
                    Name = a.Name,
                    Enabled = a.Enabled,
                    FromStates = a.FromStates,
                    ToState = a.ToState,
                    Description = a.Description
                }).ToList()
            };

            _workflows[workflow.Id] = workflow;
            return await Task.FromResult(workflow);
        }

        public async Task<WorkflowDefinition?> GetWorkflowDefinitionAsync(string id)
        {
            _workflows.TryGetValue(id, out var workflow);
            return await Task.FromResult(workflow);
        }

        public async Task<IEnumerable<WorkflowDefinition>> GetAllWorkflowDefinitionsAsync()
        {
            return await Task.FromResult(_workflows.Values);
        }

        public async Task<WorkflowInstance> StartWorkflowInstanceAsync(StartInstanceRequest request)
        {
            var workflow = await GetWorkflowDefinitionAsync(request.WorkflowDefinitionId);
            if (workflow == null)
                throw new ArgumentException($"Workflow definition with ID '{request.WorkflowDefinitionId}' not found.");

            var initialState = workflow.States.FirstOrDefault(s => s.IsInitial);
            if (initialState == null)
                throw new InvalidOperationException("Workflow definition must have exactly one initial state.");

            var instance = new WorkflowInstance
            {
                WorkflowDefinitionId = workflow.Id,
                CurrentStateId = initialState.Id
            };

            _instances[instance.Id] = instance;
            return instance;
        }

        public async Task<WorkflowInstance> ExecuteActionAsync(string instanceId, ExecuteActionRequest request)
        {
            var instance = await GetWorkflowInstanceAsync(instanceId);
            if (instance == null)
                throw new ArgumentException($"Workflow instance with ID '{instanceId}' not found.");

            var workflow = await GetWorkflowDefinitionAsync(instance.WorkflowDefinitionId);
            if (workflow == null)
                throw new InvalidOperationException("Associated workflow definition not found.");

            // Validate action execution
            ValidateActionExecution(workflow, instance, request.ActionId);

            var action = workflow.Actions.First(a => a.Id == request.ActionId);
            var currentState = workflow.States.First(s => s.Id == instance.CurrentStateId);
            var targetState = workflow.States.First(s => s.Id == action.ToState);

            // Execute action
            instance.History.Add(new ActionHistory
            {
                ActionId = action.Id,
                ActionName = action.Name,
                FromStateId = currentState.Id,
                ToStateId = targetState.Id
            });

            instance.CurrentStateId = targetState.Id;

            // Check if workflow is completed
            if (targetState.IsFinal)
            {
                instance.CompletedAt = DateTime.UtcNow;
            }

            return instance;
        }

        public async Task<WorkflowInstance?> GetWorkflowInstanceAsync(string instanceId)
        {
            _instances.TryGetValue(instanceId, out var instance);
            return await Task.FromResult(instance);
        }

        public async Task<IEnumerable<WorkflowInstance>> GetAllWorkflowInstancesAsync()
        {
            return await Task.FromResult(_instances.Values);
        }

        private void ValidateWorkflowDefinition(CreateWorkflowRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Workflow name is required.");

            if (!request.States.Any())
                throw new ArgumentException("Workflow must have at least one state.");

            // Check for duplicate state IDs
            var duplicateStateIds = request.States.GroupBy(s => s.Id)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            if (duplicateStateIds.Any())
                throw new ArgumentException($"Duplicate state IDs found: {string.Join(", ", duplicateStateIds)}");

            // Check for exactly one initial state
            var initialStates = request.States.Where(s => s.IsInitial).ToList();
            if (initialStates.Count != 1)
                throw new ArgumentException("Workflow must have exactly one initial state.");

            // Check for duplicate action IDs
            var duplicateActionIds = request.Actions.GroupBy(a => a.Id)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            if (duplicateActionIds.Any())
                throw new ArgumentException($"Duplicate action IDs found: {string.Join(", ", duplicateActionIds)}");

            // Validate action references
            var stateIds = request.States.Select(s => s.Id).ToHashSet();
            foreach (var action in request.Actions)
            {
                if (!stateIds.Contains(action.ToState))
                    throw new ArgumentException($"Action '{action.Id}' references unknown target state '{action.ToState}'.");

                foreach (var fromState in action.FromStates)
                {
                    if (!stateIds.Contains(fromState))
                        throw new ArgumentException($"Action '{action.Id}' references unknown source state '{fromState}'.");
                }
            }
        }

        private void ValidateActionExecution(WorkflowDefinition workflow, WorkflowInstance instance, string actionId)
        {
            var action = workflow.Actions.FirstOrDefault(a => a.Id == actionId);
            if (action == null)
                throw new ArgumentException($"Action with ID '{actionId}' not found in workflow definition.");

            if (!action.Enabled)
                throw new InvalidOperationException($"Action '{actionId}' is disabled.");

            var currentState = workflow.States.FirstOrDefault(s => s.Id == instance.CurrentStateId);
            if (currentState == null)
                throw new InvalidOperationException("Current state not found in workflow definition.");

            if (currentState.IsFinal)
                throw new InvalidOperationException("Cannot execute actions on instances in final state.");

            if (!action.FromStates.Contains(instance.CurrentStateId))
                throw new InvalidOperationException($"Action '{actionId}' cannot be executed from current state '{instance.CurrentStateId}'.");

            var targetState = workflow.States.FirstOrDefault(s => s.Id == action.ToState);
            if (targetState == null)
                throw new InvalidOperationException($"Target state '{action.ToState}' not found in workflow definition.");
        }
    }
}
