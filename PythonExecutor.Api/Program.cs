using PythonExecutor.Api.Services;
using PythonExecutor.Api.Services.Languages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add all services
builder.Services.AddScoped<IDockerService, DockerService>();
builder.Services.AddScoped<ILanguageExecutor, PythonLanguageExecutor>();
builder.Services.AddScoped<ILanguageExecutor, CLanguageExecutor>();
builder.Services.AddScoped<ILanguageExecutor, CppExecutor>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());
});

builder.Services.AddHttpClient("PythonExecutor").SetHandlerLifetime(TimeSpan.FromMinutes(5));

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();