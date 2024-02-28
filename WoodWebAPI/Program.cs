using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using System.Text;
using WoodWebAPI.Auth;
using WoodWebAPI.Data;
using WoodWebAPI.Services;
using WoodWebAPI.Worker;
using WoodWebAPI.Worker.Controller.Commands;

namespace WoodWebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.Sources.Clear();
            IConfigurationRoot configuration;
            if (builder.Environment.IsDevelopment()) 
            {
                 configuration = builder
                .Configuration
                .AddJsonFile("appsettings.Development.json", false)
                .AddJsonFile("appsettings.local.json", true)
                .AddJsonFile("Properties\\launchSettings.json")                
                .Build();
            }
            else 
            {
                configuration = builder
                .Configuration
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile("Properties\\launchSettings.json", false)
                .Build();
            }
            

            // Add services to the container.

            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("ConnStrJWT")));

            // For Identity
            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Adding Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })

            // Adding Jwt Bearer
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = configuration["JWT:ValidAudience"],
                    ValidIssuer = configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]))
                };
            });

            builder.Services.AddControllers()
                            .AddNewtonsoftJson(
                options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                }
            );

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Wood Cut WEB API",
                    Version = "v1"
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                // Adding XML documentation to methods
                //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                //c.IncludeXmlComments(xmlPath);
            });

            builder.Services.AddDbContext<WoodDBContext>(options => options.UseSqlServer(configuration.GetConnectionString("ConnStrWood")));
            builder.Services.AddScoped<ICustomerManage, CustomerManageService>();
            builder.Services.AddScoped<IOrderManage, OrderManageService>();
            builder.Services.AddScoped<IOrderPositionManage, OrderPositionManageService>();
            builder.Services.AddSingleton<IWorkerCreds, TelegramWorkerCreds>();

            builder.Services.AddLogging();

            //var workingCreds = new TelegramWorkerCreds(
            //    telegramToken: configuration.GetValue<string>("TelegramToken") ?? throw new ArgumentNullException("TelegramToken", "Telegtam Token field must be specified"),
            //    ngrokURL: configuration.GetSection("ngrok").GetValue<string>("URL") ?? throw new ArgumentNullException("NGROK URL", " NGROK URL must be specified"),
            //    baseURL: configuration.GetSection("profiles").GetSection("http").GetValue<string>("applicationUrl") ?? throw new ArgumentNullException("BaseUrl", "BaseUrl field must be specified"),
            //    mainAdmin: configuration.GetSection("admin").GetValue<string>("Username") ?? throw new ArgumentNullException("Username", "Username must be declared"),
            //    telegramId: configuration.GetSection("admin").GetValue<string>("TelegramId") ?? throw new ArgumentException("TelegramId", "TelegramId must be declared"),
            //    price: configuration.GetValue<string>("price") ?? throw new ArgumentException("Price","Price must be defined"),
            //    paymentToken: configuration.GetValue<string>("paymentToken") ?? throw new ArgumentException("paymentToken", "paymentToken must be defined"),
            //    minPrice: configuration.GetValue<string>("minPrice") ?? throw new ArgumentException("minPrice", "minPrice must be defined")
            //    );

            // Adding Background service worker to work with Telegram
            builder.Services.AddHostedService(options => new TelegramWorker(
                options.GetRequiredService<ILogger<TelegramWorker>>(),
                options.GetRequiredService<IWorkerCreds>()                
                ));            
            

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
