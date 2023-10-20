using TitleGeneratorGPT;
using Ume_Chat_API_Feedback;
using Ume_Chat_KeyVaultProvider;
using Ume_Chat_Utilities;

var builder = WebApplication.CreateBuilder(args);
var isDevelopment = builder.Environment.IsDevelopment();

builder.Configuration.AddTitleGenerator(isDevelopment);
builder.Configuration.AddAzureKeyVaultWithReferenceSupport();
builder.Configuration.AddVariables();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddScopedClients();
builder.Services.AddFeedbackDatabase();

var app = builder.Build();

app.MigrateDatabase();

if (isDevelopment)
    app.InitializeSwagger();

app.UseRouting();
app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.UseHttpsRedirection();

app.Run();
