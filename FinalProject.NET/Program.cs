
using System.Text;
using Booking.API.Models;
using FinalProject.NET.DBcontext;
using FinalProject.NET.Models;
using FinalProject.NET.Repositories.Implementations;
using FinalProject.NET.Services.Cloudinary;
using FinalProject.NET.Services.Middleware;
using FinalProject.NET.Services.Register;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace FinalProject.NET
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDistributedMemoryCache();

            // 🔹 ربط DbContext مع Connection String
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 🔹 إعداد Identity
            builder.Services.AddIdentity<Person, IdentityRole<Guid>>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true; // ???? ????? ???????
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
            // ------------------------
            // email_service 
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddSingleton<ICloudService, CloudinaryService>();
            // end email_service
            // Repositories
            builder.Services.AddScoped<ILawyerRepository, LawyerRepository>();


            // Services
            builder.Services.AddScoped<ILawyerService, LawyerService>();
            builder.Services.AddScoped<IAccountService, AccountService>();

            // Services for JWT (❁´◡`❁) 
            builder.Services.AddSingleton<JwtTokenService>(); // token generator
            var jwtKey = builder.Configuration["Jwt:Key"];
            var jwtIssuer = builder.Configuration["Jwt:Issuer"];
            var jwtAudience = builder.Configuration["Jwt:Audience"];

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            //builder.Services.AddSwaggerGen();
            #region Swagger Setting

            builder.Services.AddSwaggerGen(swagger =>
            {
                // Generate default Swagger UI
                swagger.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "ASP.NET 8 Web API",
                    Description = "ITI Project"
                });

                // Enable JWT authentication in Swagger
                swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' followed by your JWT token.\r\n\r\nExample: \"Bearer eyJhbGciOi...\""
                });

                swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
            });

            #endregion

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseJwtAuth();

            app.UseHttpsRedirection();
            app.UseAuthentication();  // must come BEFORE UseAuthorization
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
