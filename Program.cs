
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuizHub.Data;
using QuizHub.Data.Repository;
using QuizHub.Data.Repository.Base;
using QuizHub.Models;
using QuizHub.Services.Admin_Services;
using QuizHub.Services.Admin_Services.Interface;
using QuizHub.Services.Authentication.Login_And_Sing_Up;
using QuizHub.Services.Authentication.Login_And_Sing_Up.Interface;
using QuizHub.Services.Authorization.Policies;
using QuizHub.Services.SubAdmin_Services;
using QuizHub.Services.SubAdmin_Services.Interface;
using System;
using System.Text;


namespace QuizHub
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            //Add Permission Services
            builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            //Connection to Db
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddIdentity<AppUser, IdentityRole>(options => {
                options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequiredLength = 8;  
                options.Password.RequireDigit = false; 
                options.Password.RequireLowercase = false;  
                options.Password.RequireUppercase = false;  
                options.Password.RequireNonAlphanumeric = false;  
                options.Password.RequiredUniqueChars = 1;
            }
            )
               .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultUI();

            //Add Repo Services
            builder.Services.AddTransient(typeof(IRepository<>), typeof(MainRepository<>));

            //Add college services
            builder.Services.AddScoped<ICollegeService, CollegeService>();
            builder.Services.AddScoped<IDepartmentService, DepartmentService>();
            builder.Services.AddScoped<ISubAdminService, SubAdminService>();
            builder.Services.AddScoped<ITeacherService, TeacherService>();
            builder.Services.AddScoped<ISubjectService, SubjectService>();
            builder.Services.AddScoped<ILearningOutComesService, LearningOutComesService>();
            builder.Services.AddScoped<ILearningOutComesService, LearningOutComesService>();
            builder.Services.AddScoped<IClassService, ClassService>();
            builder.Services.AddScoped<IBatchService, BatchService>();



            //Add Cors services
            builder.Services.AddCors(builderOption =>
            {
                builderOption.AddPolicy("MyPolicy", crosPolicyBuilder =>
                {
                    crosPolicyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            // Add servicesservices to the container.
            builder.Services.AddControllers()
     .AddJsonOptions(options =>
     {
         options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
     });


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //Services Authentication
            builder.Services.AddScoped<IAuthenService, AuthenService>();


            //[Authorized] used JWT token instead cookies
            builder.Services.AddAuthentication(option =>
            {
                option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;


            }).AddJwtBearer(option =>
            {
                option.SaveToken = true;
                option.RequireHttpsMetadata = true;
                option.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["JWT:validIssure"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:validAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"])),
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            });

            //swager authentication
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "dotnetClaimAuthorization", Version = "v1" });
                c.AddSecurityDefinition(name: JwtBearerDefaults.AuthenticationScheme, securityScheme: new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter 'Bearer' [space] and then your valid token",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            },
            new string[] {}
        }
    });
            });




            var app = builder.Build();

            //Seed User and Role
            using var scop = app.Services.CreateScope();
            var scopServices = scop.ServiceProvider;

            var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("App");
            try
            {
                var userManger = scopServices.GetRequiredService<UserManager<AppUser>>();
                var roleManger = scopServices.GetRequiredService<RoleManager<IdentityRole>>();

                await Data.Seeds.DefaultRoles.SeedRoleAsync(roleManger);
                await Data.Seeds.DefaultUsers.seedsAdminAsync(userManger, roleManger);
                await Data.Seeds.DefaultClaims.seedsClaims(roleManger);

                logger.LogInformation("Data Seeded");
                logger.LogInformation("Application Started");

            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "an error occured while seeding data");
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
