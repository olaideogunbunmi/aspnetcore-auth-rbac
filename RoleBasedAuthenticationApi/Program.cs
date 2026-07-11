using RoleBasedAuthenticationApi.Data;
using RoleBasedAuthenticationApi.Interfaces;
using RoleBasedAuthenticationApi.Models;
using RoleBasedAuthenticationApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


//DATABASE
builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));





//IDENTITY
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);


        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();




//AUTHENTICATION
builder.Services.AddAuthentication(option =>
{
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
    option.RequireHttpsMetadata = false;
    option.SaveToken = true;
    option.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]!)),
        ValidateIssuer = true,
        ValidateAudience = true,

        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"]
    };
});




//AUTHORIZATION - GLOBALLY BOTH DEFAULT AND FALLBACK
builder.Services.AddAuthorizationBuilder()
    // .SetDefaultPolicy(new AuthorizationPolicyBuilder()
    //.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
    //.RequireAuthenticatedUser()
    //.RequireRole("Admin")
    //.Build())

    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
    .RequireAuthenticatedUser()
    .Build());



//GLOBAL NULL - TO IGNORE A null PROPERTY in JSON Serialisation TO CLIENT
//TO ALSO SUPPORT PATCH
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    });


//DEPENDENCY INJECTION
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRoleService,RoleService>();
builder.Services.AddScoped<IUserService, UserService>();




//AUTOMAPPER
builder.Services.AddAutoMapper(cfg =>
{
    /*optional global config */
}, 
typeof(Program).Assembly);




//PROBLEM DETAILS
builder.Services.AddProblemDetails();





// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();


