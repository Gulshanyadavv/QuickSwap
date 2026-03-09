using FluentValidation.AspNetCore;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using O_market.Models;
using O_market.Services;
using O_market.Repositories;
using O_market.Validators;
using O_market.Interfaces.Repositories;
using O_market.Interfaces.Services;
using Microsoft.Extensions.FileProviders;
using O_market.Hubs;

var builder = WebApplication.CreateBuilder(args);

// ==========================
// CONTROLLERS + VALIDATION
// ==========================
builder.Services.AddControllers()
    .AddFluentValidation(fv =>
    {
        fv.RegisterValidatorsFromAssemblyContaining<AdCreateWithDynamicValidator>();
        fv.DisableDataAnnotationsValidation = true;
    });

builder.Services.AddValidatorsFromAssemblyContaining<AdCreateWithDynamicValidator>();

// ==========================
// SWAGGER
// ==========================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using Bearer scheme",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ==========================
// DATABASE
// ==========================
builder.Services.AddDbContext<OlxdbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Server=LAPTOP-LLKF91PS\\SQLEXPRESS;Database=OLXDB;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;"
    )
);

// ==========================
// DEPENDENCY INJECTION
// ==========================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAdRepository, AdRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IRecommendationRepository, RecommendationRepository>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAdService, AdService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddSignalR();





// ==========================
// AUTOMAPPER
// ==========================
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// ==========================
// JWT AUTHENTICATION
// ==========================
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key missing");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            )
        };
    });

// ==========================
// CORS (FIXED)
// ==========================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200",
                "https://quick-swap-frontend.vercel.app"
            )
            .SetIsOriginAllowedToAllowWildcardSubdomains()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


// ==========================
// LOGGING
// ==========================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ==========================
// OPTIONAL SERVICES
// ==========================
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();
builder.Services.AddHttpClient();
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings")
);
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings")
);
builder.Services.AddHttpContextAccessor();
var app = builder.Build();

// ==========================
// MIDDLEWARE PIPELINE
// ==========================

// Swagger + Dev tools
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

//  GLOBAL EXCEPTION HANDLER (MUST BE EARLY)
app.UseMiddleware<GlobalExceptionMiddleware>();

// Static files (images, etc.)
app.UseStaticFiles();

// Robust path discovery for uploads
string rootPath = app.Environment.ContentRootPath;
// If running from bin folder, go up to project root
if (rootPath.Contains("bin"))
{
    rootPath = rootPath.Split(new string[] { "\\bin" }, StringSplitOptions.None)[0];
}

var uploadsPath = Path.Combine(rootPath, "wwwroot", "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

Console.WriteLine($"---> Uploads path: {uploadsPath}");

// Routing
app.UseRouting();


//  CORS
app.UseCors("AllowAngular");

// Auth
app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.MapControllers();
app.MapHub<ChatHub>("/chatHub");


//app.UseResponseCompression();
app.Run();
