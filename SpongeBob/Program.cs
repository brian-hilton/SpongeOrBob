using Microsoft.EntityFrameworkCore;
using SpongeBob.Hubs;
namespace SpongeBob
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<GameDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection")));

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("SpongeBob",
                    policy => policy.WithOrigins("http://localhost:5173", "http://192.168.68.110:5173")
                                    .AllowAnyMethod()
                                    .AllowAnyHeader()
                                    .AllowCredentials());
            });
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSignalR();

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.UseCors("SpongeBob");
            app.MapControllers();

            app.Use(async (context, next) =>
            {
                context.Request.EnableBuffering();
                var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
                Console.WriteLine($"Request Body: {body}");
                await next();
            });

            app.MapHub<GameHub>("/gamehub");

            app.Run();
        }
    }
}
