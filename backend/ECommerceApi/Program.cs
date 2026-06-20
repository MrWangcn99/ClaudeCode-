using Microsoft.EntityFrameworkCore;
using ECommerceApi.Data;
using ECommerceApi.Repositories;
using ECommerceApi.Repositories.Interfaces;
using ECommerceApi.Services;
using ECommerceApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// IOC Dependency Injection Registration
// ========================================

// ---- Database (Scoped: EF Core DbContext is not thread-safe, MySQL) ----
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection")!));

// ---- Repositories (Scoped: one instance per HTTP request) ----
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// ---- Concurrency Manager (SINGLETON: the lock dictionary MUST be shared
//     across ALL requests. If this were Scoped, each request would get its own
//     locks, completely defeating the overselling prevention.) ----
builder.Services.AddSingleton<StockConcurrencyManager>();

// ---- Services (Scoped: depends on Scoped repositories) ----
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// ---- Controllers ----
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// ---- CORS (allow Vite dev server) ----
var corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>()
                  ?? new[] { "http://localhost:5173" };
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ---- Swagger (dev only) ----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "E-Commerce Management API",
        Version = "v1",
        Description = "E-commerce platform with concurrency-safe order processing using " +
                      "per-product SemaphoreSlim locks and database transactions."
    });
});

var app = builder.Build();

// ---- Auto-migrate database on startup (MySQL: creates DB + tables + seeds demo data) ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // EnsureCreatedAsync will create the database and tables if they don't exist
    await db.Database.EnsureCreatedAsync();
    Console.WriteLine("[Database] MySQL database initialized successfully.");
}

// ---- Middleware pipeline ----
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Commerce API v1");
        c.RoutePrefix = "swagger";
    });
}

// ---- Ensure wwwroot/uploads directory exists for uploaded images ----
var wwwrootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads");
if (!Directory.Exists(wwwrootPath))
    Directory.CreateDirectory(wwwrootPath);

app.UseStaticFiles(); // Serve uploaded images from wwwroot/
app.UseCors();
app.MapControllers();

// ========================================
// Concurrency Smoke Test
// ========================================
// Simulates 10 concurrent purchase attempts on a product with limited stock.
// Demonstrates that the StockConcurrencyManager prevents overselling.
_ = Task.Run(async () =>
{
    // Wait for app to fully start
    await Task.Delay(2000);

    using var testScope = app.Services.CreateScope();
    var db = testScope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Find or create a test product with exactly 5 stock
    var testProduct = await db.Products.FirstOrDefaultAsync(p => p.Name == "Concurrency Test Item");
    if (testProduct == null)
    {
        testProduct = new ECommerceApi.Models.Product
        {
            Name = "Concurrency Test Item",
            Price = 10.00m,
            Stock = 5,
            Description = "Special item for concurrency testing — exactly 5 in stock.",
            ImageUrl = "https://picsum.photos/seed/conctest/400/400",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Products.Add(testProduct);
        await db.SaveChangesAsync();
    }
    else
    {
        testProduct.Stock = 5;
        await db.SaveChangesAsync();
    }

    var productId = testProduct.Id;
    Console.WriteLine($"[ConcurrencyTest] Starting: Product #{productId} has 5 stock.");
    Console.WriteLine("[ConcurrencyTest] Firing 10 concurrent purchase requests (each buying 1)...");

    // Create 10 concurrent order requests
    var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
    var tasks = new List<Task<(int Index, bool Success, string Message)>>();

    for (int i = 0; i < 10; i++)
    {
        var index = i;
        tasks.Add(Task.Run(async () =>
        {
            try
            {
                var payload = new
                {
                    items = new[]
                    {
                        new { productId = productId, quantity = 1 }
                    }
                };
                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(payload),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = await httpClient.PostAsync("/api/orders", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                    return (index, true, $"SUCCESS ({(int)response.StatusCode})");
                else
                    return (index, false, $"FAILED ({(int)response.StatusCode}): {responseBody[..Math.Min(100, responseBody.Length)]}");
            }
            catch (Exception ex)
            {
                return (index, false, $"ERROR: {ex.Message[..Math.Min(100, ex.Message.Length)]}");
            }
        }));
    }

    var results = await Task.WhenAll(tasks);

    var successCount = results.Count(r => r.Success);
    var failCount = results.Count(r => !r.Success);

    // Verify final stock
    var finalProduct = await db.Products.AsNoTracking().FirstAsync(p => p.Id == productId);

    Console.WriteLine("============================================");
    Console.WriteLine($"[ConcurrencyTest] RESULTS:");
    Console.WriteLine($"  Successful orders: {successCount}/10");
    Console.WriteLine($"  Failed orders:     {failCount}/10");
    Console.WriteLine($"  Final stock:       {finalProduct.Stock} (expected: 0)");
    Console.WriteLine($"  Overselling?       {(finalProduct.Stock < 0 ? "YES — BUG!" : "NO — lock works correctly.")}");
    Console.WriteLine("============================================");

    foreach (var r in results.OrderBy(r => r.Index))
    {
        Console.WriteLine($"  [{r.Index:D2}] {r.Message}");
    }

    Console.WriteLine($"[ConcurrencyTest] Concurrency lock is {(finalProduct.Stock == 0 && successCount == 5 ? "WORKING ✓" : "BROKEN ✗")}");
});

app.Run();
