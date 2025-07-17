using ConfigurableWorkflowEngine.Services;
using ConfigurableWorkflowEngine.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ConfigurableWorkflowEngine.Endpoints
{
    public static class WorkflowEndpoints
    {
        public static void MapWorkflowEndpoints(this WebApplication app)
        {
            var workflowGroup = app.MapGroup("/api/workflows")
                .WithTags("Workflows");

            var instanceGroup = app.MapGroup("/api/instances")
                .WithTags("Workflow Instances");

            // Workflow Definition Endpoints
            workflowGroup.MapPost("/", async (IWorkflowService service, CreateWorkflowRequest request) =>
            {
                try
                {
                    var workflow = await service.CreateWorkflowAsync(request);
                    return Results.Created($"/api/workflows/{workflow.Id}", workflow);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            })
            .WithName("CreateWorkflow")
            .WithSummary("Create a new workflow definition");

            workflowGroup.MapGet("/{id}", async (IWorkflowService service, string id) =>
            {
                var workflow = await service.GetWorkflowDefinitionAsync(id);
                return workflow != null ? Results.Ok(workflow) : Results.NotFound();
            })
            .WithName("GetWorkflow")
            .WithSummary("Get a workflow definition by ID");

            workflowGroup.MapGet("/", async (IWorkflowService service) =>
            {
                var workflows = await service.GetAllWorkflowDefinitionsAsync();
                return Results.Ok(workflows);
            })
            .WithName("GetAllWorkflows")
            .WithSummary("Get all workflow definitions");

            // Workflow Instance Endpoints
            instanceGroup.MapPost("/", async (IWorkflowService service, StartInstanceRequest request) =>
            {
                try
                {
                    var instance = await service.StartWorkflowInstanceAsync(request);
                    return Results.Created($"/api/instances/{instance.Id}", instance);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            })
            .WithName("StartWorkflowInstance")
            .WithSummary("Start a new workflow instance");

            instanceGroup.MapPost("/{instanceId}/actions", async (IWorkflowService service, string instanceId, ExecuteActionRequest request) =>
            {
                try
                {
                    var instance = await service.ExecuteActionAsync(instanceId, request);
                    return Results.Ok(instance);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            })
            .WithName("ExecuteAction")
            .WithSummary("Execute an action on a workflow instance");

            instanceGroup.MapGet("/{instanceId}", async (IWorkflowService service, string instanceId) =>
            {
                var instance = await service.GetWorkflowInstanceAsync(instanceId);
                return instance != null ? Results.Ok(instance) : Results.NotFound();
            })
            .WithName("GetWorkflowInstance")
            .WithSummary("Get a workflow instance by ID");

            instanceGroup.MapGet("/", async (IWorkflowService service) =>
            {
                var instances = await service.GetAllWorkflowInstancesAsync();
                return Results.Ok(instances);
            })
            .WithName("GetAllWorkflowInstances")
            .WithSummary("Get all workflow instances");
        }
    }
}
