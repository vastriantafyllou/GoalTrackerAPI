using System.Reflection;
using System.Text;
using GoalTrackerApp.Configuration;
using GoalTrackerApp.Core.Helpers;
using GoalTrackerApp.Core.RateLimiting;
using GoalTrackerApp.Core.Captcha;
using GoalTrackerApp.Data;
using GoalTrackerApp.Repositories;
using GoalTrackerApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;

namespace GoalTrackerApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        var dbServer = builder.Configuration["DB_SERVER"]
                       ?? throw new InvalidOperationException("DB_SERVER environment variable is not set.");
        var dbName = builder.Configuration["DB_NAME"]
                     ?? throw new InvalidOperationException("DB_NAME environment variable is not set.");
        var dbUser = builder.Configuration["DB_USER"]
                     ?? throw new InvalidOperationException("DB_USER environment variable is not set.");
        var dbPassword = builder.Configuration["DB_PASS"]
                         ?? throw new InvalidOperationException("DB_PASS environment variable is not set.");

        var connString =
            $"Server={dbServer};Database={dbName};User={dbUser};Password={dbPassword};MultipleActiveResultSets=True;TrustServerCertificate=True;";
        
        builder.Services.AddDbContext<GoalTrackerAppDbContext>(options => options.UseSqlServer(connString));
        builder.Services.AddRepositories();
        builder.Services.AddScoped<IApplicationService, ApplicationService>();
        
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        builder.Services.AddSingleton<PasswordRecoveryRateLimiter>();
        builder.Services.AddHttpClient<ICaptchaService, CaptchaService>();
        
        builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MapperConfig>());
        builder.Host.UseSerilog((ctx, lc) => 
            lc.ReadFrom.Configuration(ctx.Configuration));
        
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            var jwtSettings = builder.Configuration.GetSection("Authentication");
            var secretKey = jwtSettings["SecretKey"] 
                ?? throw new InvalidOperationException("JWT SecretKey not found in configuration.");
            
            options.IncludeErrorDetails = true;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "https://localhost:5001",

                ValidateAudience = true,
                ValidAudience = "https://localhost:5001",

                ValidateLifetime = true, // ensure not expired
                
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend",
                policy => policy.WithOrigins("http://localhost:5173")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());

            options.AddPolicy("AllowAll",
                policy => policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });
        
        builder.Services.AddControllers().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.NullValueHandling = NullValueHandling.Include;
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
            options.SerializerSettings.Converters.Add(new StringEnumConverter());
        });
        
        builder.Services.AddEndpointsApiExplorer();
        
        
        builder.Services.AddSwaggerGen(options =>
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath); 
            
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Goal Tracker App", Version = "v1" });
            
            // options.SupportNonNullableReferenceTypes(); // default true > .NET 6
            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme,
                new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    BearerFormat = "JWT"
                });
            options.OperationFilter<AuthorizeOperationFilter>();
        });

        var app = builder.Build();
        
        // Seed SuperAdmin user if not exists
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<GoalTrackerAppDbContext>();
            if (!context.Users.Any(u => u.UserRole == GoalTrackerApp.Core.Enums.UserRole.SuperAdmin))
            {
                var superAdminPassword = builder.Configuration["SuperAdminPassword"] ?? "SuperAdmin@123!";
                
                var superAdmin = new User
                {
                    Username = "superadmin",
                    Email = "superadmin@goaltracker.com",
                    Password = GoalTrackerApp.Core.Security.EncryptionUtil.Encrypt(superAdminPassword),
                    Firstname = "Super",
                    Lastname = "Administrator",
                    UserRole = GoalTrackerApp.Core.Enums.UserRole.SuperAdmin
                };
                context.Users.Add(superAdmin);
                context.SaveChanges();
            }
        }
        
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Goal Tracker App v1"));
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowFrontend");

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<ErrorHandlerMiddleware>();
        app.MapControllers();

        app.Run();
    }
}