using ConfigurableWorkflowEngine.Extensions;
using ConfigurableWorkflowEngine.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add workflow services
builder.Services.AddWorkflowServices();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            c.RoutePrefix = string.Empty; // Optional: serves Swagger UI at root
        });

}


app.UseHttpsRedirection();

// Map workflow endpoints
app.MapWorkflowEndpoints();

app.Run();
