using SmartDocValidation.Agents;
using SmartDocValidation.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<PdfService>();
builder.Services.AddSingleton<OcrService>();
builder.Services.AddSingleton<LlmService>();
builder.Services.AddSingleton<ExcelService>();
builder.Services.AddSingleton<VectorService>();
builder.Services.AddSingleton<ResolutionAgent>();
builder.Services.AddSingleton<MailAgent>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
    );
});

var app = builder.Build();

// 🔥 CLEAN INITIALIZATION
await InitializeVectorStore(app);

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// 🔥 HELPER METHOD
static async Task InitializeVectorStore(WebApplication app)
{
    using var scope = app.Services.CreateScope();

    var excelService = scope.ServiceProvider.GetRequiredService<ExcelService>();
    var vectorService = scope.ServiceProvider.GetRequiredService<VectorService>();

    var excelData = excelService.ReadExcel("data.xlsx");

    foreach (var row in excelData)
    {
        var text =
            $"Policy {row["PolicyNumber"]} belongs to {row["Name"]} with email {row["Email"]} at {row["Company"]}";
        await vectorService.AddDocument(text);
    }
}
