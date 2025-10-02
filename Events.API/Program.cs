using Events.Infrastructure.DI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCors", policy =>
            policy.WithOrigins(
                    "http://localhost:8080",   // your frontend
                    "http://127.0.0.1:8080")  // optional
                .AllowAnyMethod()
                .AllowAnyHeader()
        // .AllowCredentials() // only if you use cookies; then fetch(..., { credentials: "include" })
    );
});

builder.Services.AddEventsInfrastructureDI();
builder.Services.AddControllers();           // ⬅️ add this
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// builder.Services.AddDbContext<AppDb>(...);

var app = builder.Build();

//app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("FrontendCors");
//app.UseAuthorization();

app.MapControllers();                        // ⬅️ replace app.MapGet/MapPost... with this

app.Run();