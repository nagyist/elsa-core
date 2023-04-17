using Elsa.EntityFrameworkCore.Extensions;
using Elsa.EntityFrameworkCore.Modules.Labels;
using Elsa.EntityFrameworkCore.Modules.Management;
using Elsa.EntityFrameworkCore.Modules.Runtime;
using Elsa.Extensions;
using Elsa.JavaScript.Options;
using Microsoft.Data.Sqlite;
using Proto.Persistence.Sqlite;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;
var sqliteConnectionString = configuration.GetConnectionString("Sqlite")!;
var identitySection = configuration.GetSection("Identity");
var identityTokenSection = identitySection.GetSection("Tokens");

// Add Elsa services.
services
    .AddElsa(elsa => elsa
        .AddActivitiesFrom<Program>()
        .UseIdentity(identity =>
        {
            identity.IdentityOptions = options => identitySection.Bind(options);
            identity.TokenOptions = options => identityTokenSection.Bind(options);
            identity.UseConfigurationBasedUserProvider(options => identitySection.Bind(options));
            identity.UseConfigurationBasedApplicationProvider(options => identitySection.Bind(options));
            identity.UseConfigurationBasedRoleProvider(options => identitySection.Bind(options));
        })
        .UseDefaultAuthentication()
        .UseWorkflowManagement(management =>
        {
            // Use EF core for workflow definitions and instances.
            management.UseEntityFrameworkCore(m => m.UseSqlite(sqliteConnectionString));
        })
        .UseWorkflowRuntime(runtime =>
        {
            // Use EF core for triggers and bookmarks.
            runtime.UseEntityFrameworkCore(ef => ef.UseSqlite(sqliteConnectionString));
            
            // Use EF core for execution log records.
            runtime.UseExecutionLogRecords(log => log.UseEntityFrameworkCore(ef => ef.UseSqlite(sqliteConnectionString)));
            
            // Install a workflow state exporter to capture workflow states and store them in IWorkflowInstanceStore.
            runtime.UseAsyncWorkflowStateExporter();
            
            // Use Proto.Actor for workflow execution.
            runtime.UseProtoActor(protoActor =>
            {
                protoActor.PersistenceProvider = _ => new SqliteProvider(new SqliteConnectionStringBuilder(sqliteConnectionString));
            });
        })
        .UseLabels(labels => labels.UseEntityFrameworkCore(ef => ef.UseSqlite(sqliteConnectionString)))
        .UseScheduling()
        .UseWorkflowsApi(api => api.AddFastEndpointsAssembly<Program>())
        .UseJavaScript()
        .UseLiquid()
        .UseHttp()
    );

services.Configure<JintOptions>(options => options.AllowClrAccess = true);
services.AddHandlersFrom<Program>();
services.AddHealthChecks();
services.AddCors(cors => cors.AddDefaultPolicy(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));
services.AddHttpContextAccessor();

// Configure middleware pipeline.
var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

// CORS.
app.UseCors();

// Health checks.
app.MapHealthChecks("/");

app.UseAuthentication();
app.UseAuthorization();

// Elsa API endpoints for designer.
app.UseWorkflowsApi();

// Captures unhandled exceptions and returns a JSON response.
app.UseJsonSerializationErrorHandler();

// Elsa HTTP Endpoint activities
app.UseWorkflows();

// Run.
app.Run();