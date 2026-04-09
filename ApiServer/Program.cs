var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger 在 Development 環境開放（容器內預設 Production，如需開放請調整 ASPNETCORE_ENVIRONMENT）
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 容器化部署時由 Nginx 負責 TLS，應用層不做 HTTPS 重導
// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
