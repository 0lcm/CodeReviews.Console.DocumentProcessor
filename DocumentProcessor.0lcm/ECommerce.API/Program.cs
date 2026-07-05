using System.Text.Json.Serialization;
using ECommerce.API.Data;
using ECommerce.API.Interfaces.Import_Export;
using ECommerce.API.Interfaces.Repositories;
using ECommerce.API.Interfaces.Services;
using ECommerce.API.Models;
using ECommerce.API.Models.Import_Models;
using ECommerce.API.Repositories;
using ECommerce.API.Services;
using ECommerce.API.Services.Import_Export;
using ECommerce.API.Services.Import_Export.ServiceFactories;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appSettings.json", false, true);

builder.Services.AddSingleton<SoftDeleteInterceptor>();

builder.Services.AddDbContext<ApiDbContext>((sp, options) =>
    options
        .UseSqlite(DbConfig.GetConnectionString())
        .AddInterceptors(sp.GetRequiredService<SoftDeleteInterceptor>()));

builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ITagService, TagService>();

builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IItemService, ItemService>();

builder.Services.AddScoped<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<ISaleService, SaleService>();

builder.Services.AddScoped<XlsxService>();
builder.Services.AddScoped<CsvService>();
builder.Services.AddScoped<XlsService>();
builder.Services.AddScoped<ImportServiceFactory>();

builder.Services.AddScoped<PdfService>();
builder.Services.AddScoped<ExportServiceFactory>();

QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
var importFactory = scope.ServiceProvider.GetRequiredService<ImportServiceFactory>();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

var dbDirectory = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "ECommerce");
Directory.CreateDirectory(dbDirectory);

var isFirstRun = !await db.Database.CanConnectAsync();
var shouldSeed = true;
if (!isFirstRun)
{
    shouldSeed = (!args.Contains("--skip-tag-seeding") && !await db.Tags.AnyAsync()) 
                 || (!args.Contains("--skip-item-seeding") && !await db.Items.AnyAsync())
                 || (!args.Contains("--skip-sale-seeding") && !await db.Sales.AnyAsync());
}

await db.Database.MigrateAsync();

if (isFirstRun || shouldSeed) await DbSeeder.RunSeederAsync(db, importFactory, logger, new SeedingOptions
{
    SkipSeeding = args.Contains("--skip-seeding"),
    SkipItemSeeding = args.Contains("--skip-item-seeding"),
    SkipTagSeeding = args.Contains("--skip-tag-seeding"),
    SkipSaleSeeding = args.Contains("--skip-sale-seeding")
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseExceptionHandler(error => error.Run(async context =>
{
    context.Response.StatusCode = 500;
    await context.Response.WriteAsync("An unexpected exception occurred during runtime.");
}));
app.MapControllers();

app.Run();