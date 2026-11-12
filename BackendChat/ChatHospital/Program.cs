var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR(); // 1. Agregamos SignalR

// 2. Configuramos CORS para que Angular pueda conectarse
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsChat", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // URL de tu Angular local
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // OBLIGATORIO para SignalR
    });
});

var app = builder.Build();

app.UseCors("CorsChat");
app.UseAuthorization();
app.MapControllers();

// 3. Definimos la ruta donde escuchará el Hub
app.MapHub<ChatHub>("/chathub");

app.Run();
