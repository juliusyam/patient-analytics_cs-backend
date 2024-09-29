using System.Text;
using System.Text.Json.Serialization;
using Blazored.Toast;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PatientAnalytics.Blazor;
using PatientAnalytics.Hubs;
using PatientAnalytics.Middleware;
using PatientAnalytics.Models;
using PatientAnalytics.Services;
using PatientAnalytics.Services.PatientMetrics;
using PatientAnalytics.Utils;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddSignalR();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Patient Analytics", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
    opt.OperationFilter<AddRequiredHeaderParameter>();
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddMvc();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
                ClockSkew = new TimeSpan(0, 0, 5)
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });

builder.Services.AddResponseCompression(options =>
{
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

var connectionString = builder.Configuration.GetConnectionString("PatientAnalyticsContext");

//SqliteConnectionAccess.EstablishConnection(connectionString);

builder.Services.AddDbContext<Context>(opt =>
    opt.UseNpgsql(connectionString));
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RegistrationService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<PatientService>();
builder.Services.AddScoped<PatientMetricsTemperatureService>();
builder.Services.AddScoped<PatientMetricsBloodPressureService>();
builder.Services.AddScoped<PatientMetricsHeightService>();
builder.Services.AddScoped<PatientMetricsWeightService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policyBuilder =>
    {
        policyBuilder
            .WithOrigins("http://localhost:5173", "http://localhost:57492")
            .AllowCredentials()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
});

builder.Services.AddSingleton<AuthenticationDataMemoryStorage>();
builder.Services.AddScoped<PatientAnalyticsUserService>();
builder.Services.AddScoped<PatientAnalyticsAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<PatientAnalyticsAuthStateProvider>());

builder.Services.AddAuthenticationCore();

builder.Services.AddLocalization();

builder.Services.AddBlazoredToast();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpStatusCodeExceptionMiddleware();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseAntiforgery();

app.UseRequestLocalization(new RequestLocalizationOptions()
    .AddSupportedCultures(new[] { "en", "de", "zh" })
    .AddSupportedUICultures(new[] { "en", "de", "zh" }));

app.MapRazorPages();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.MapControllers();

app.UseResponseCompression();

app.MapHub<PatientHub>(app.Configuration["HubConnection:Route"]!);

app.UseCors("AllowSpecificOrigin");

app.UseStaticFiles();

app.Run();