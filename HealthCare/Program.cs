using System.Text;
using HealthCare.Data;
using HealthCare.Endpoints;
using HealthCare.Filters;
using HealthCare.Middleware;
using HealthCare.Models.Entities;
using HealthCare.Services.Implementations;
using HealthCare.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database ────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Identity ────────────────────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit           = true;
    options.Password.RequiredLength         = 8;
    options.Password.RequireUppercase       = false;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail         = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ── JWT Authentication ───────────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtSettings["Issuer"],
        ValidAudience            = jwtSettings["Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(
                                       Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)),
    };
});

// ── Authorization ────────────────────────────────────────────────────────────
builder.Services.AddAuthorization();

// ── HTTP Context & Application Services ─────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IJwtTokenService,JwtTokenService>();
builder.Services.AddScoped<ITenantService,TenantService>();
builder.Services.AddScoped<IAuthService,AuthService>();
builder.Services.AddScoped<IHealthcareCenterService, HealthcareCenterService>();
builder.Services.AddScoped<IPatientService,PatientService>();
builder.Services.AddScoped<IDoctorService,DoctorService>();
builder.Services.AddScoped<IAppointmentService,AppointmentService>();
builder.Services.AddScoped<IMedicalRecordService,MedicalRecordService>();
builder.Services.AddScoped<IPrescriptionService,PrescriptionService>();

// ── OpenAPI / Scalar ─────────────────────────────────────────────────────────
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((doc, _, _) =>
    {
        doc.Info.Title       = "Healthcare Mini ERP API";
        doc.Info.Version     = "v1";
        doc.Info.Description =
            "Multi-tenant REST API for managing patients, doctors, staff, appointments, " +
            "medical records, and prescriptions across multiple healthcare centers. " +
            "\n\n**Authentication:** Use POST /api/auth/login to obtain a JWT token, " +
            "then click 'Authorize' and enter: Bearer {token}";
        return Task.CompletedTask;
    });
});

// ── Build App ────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Middleware Pipeline ───────────────────────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Healthcare Mini ERP API";
        options.Theme = ScalarTheme.Purple;
        options.AddPreferredSecuritySchemes("Bearer");
        options.AddHttpAuthentication("Bearer", bearer => bearer.Token = "your-jwt-token-here");
    });
}

// ── Endpoint Registration ─────────────────────────────────────────────────────

// Auth endpoints — anonymous, no tenant filter
app.MapGroup("/api/auth")
    .MapAuthEndpoints();

// All other endpoints — require authentication + tenant isolation filter
var api = app.MapGroup("/api")
    .AddEndpointFilter<TenantIsolationFilter>()
    .RequireAuthorization();

api.MapGroup("/healthcare-centers").MapHealthcareCentersEndpoints();
api.MapGroup("/patients").MapPatientsEndpoints();
api.MapGroup("/doctors").MapDoctorsEndpoints();
api.MapGroup("/appointments").MapAppointmentsEndpoints();
api.MapGroup("/medical-records").MapMedicalRecordsEndpoints();
api.MapGroup("/prescriptions").MapPrescriptionsEndpoints();

// ── Seed Database (Development Only) ─────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();
