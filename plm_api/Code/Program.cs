using Microsoft.EntityFrameworkCore;
using plm_api.Code;

var builder = WebApplication.CreateBuilder(args);

//  CORS Servis İzni
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Controllers
builder.Services.AddControllers();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

// 🔥 2. BURAYA EKLEDİK: CORS Aktif Etme (UseStaticFiles civarı)
app.UseCors();

app.MapControllers();
app.UseStaticFiles(); // Resimlerin URL üzerinden açılabilmesini sağlar
app.Run();