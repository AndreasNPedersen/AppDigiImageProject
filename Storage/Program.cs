namespace Storage
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            //builder.WebHost.UseKestrel().UseUrls("http://0.0.0.0:44303");
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<StorageService, StorageService>();
            builder.Services.AddCors(x => x.AddPolicy("allowall",
                x => x.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod()));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("allowall");
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}