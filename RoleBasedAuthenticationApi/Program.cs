using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RoleBasedAuthenticationApi.Configuration;
using RoleBasedAuthenticationApi.Data;
using RoleBasedAuthenticationApi.Interfaces;
using RoleBasedAuthenticationApi.Models;
using RoleBasedAuthenticationApi.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


//DATABASE
builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


//SMTP SETTING
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));





//JWTSETTINGS

//Register IOptions injection accross app and bind JWTSettings and JWT
builder.Services.AddOptions<JWTSettings>()
    .Bind(builder.Configuration.GetSection("JWT"))
    .ValidateDataAnnotations()
    .ValidateOnStart(); // App crashes immediately on startup if Key, Issuer, or Audience are missing


//Bind locally for Program.cs setup independently
var jwtSettings = builder.Configuration.GetSection("JWT").Get<JWTSettings>();

if (jwtSettings == null || string.IsNullOrWhiteSpace(jwtSettings.Key) || string.IsNullOrWhiteSpace(jwtSettings.Issuer) || string.IsNullOrWhiteSpace(jwtSettings.Audience))
{
    throw new InvalidOperationException("JWT configuration section is missing or invalid");
}





//IDENTITY
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Lockout.AllowedForNewUsers = true;

        //users can only attempt 5 times of login using wrong credentials
        options.Lockout.MaxFailedAccessAttempts = 5;

        //designed for temporary lockout - after 10 minutes user can try again
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
    option.MapInboundClaims = false; // disable mapping jwtRegisteredClaimNames.Sub to ClaimTypes.NameIdentifier url
    option.TokenValidationParameters = new TokenValidationParameters()
    {      
        //no extra 5 minutes time added to token lifespan after creation
        ClockSkew = TimeSpan.Zero, //or TimeSpan.FromSeconds(0)


        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),       
       

        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,


        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,


        ValidateLifetime = true,


        // Match role/name claim types to what's actually in the token ("role"/"name"),
        // since MapInboundClaims = false stops ASP.NET Core reverting them to its long-URI defaults
        RoleClaimType = "role",
        NameClaimType = JwtRegisteredClaimNames.Name
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
builder.Services.AddScoped<IEmailServices, EmailService>();




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

    app.MapGet("/debug/endpoints", (IEnumerable<EndpointDataSource> endpointSources) =>
    {
        var endpoints = endpointSources.SelectMany(source => source.Endpoints);

        return endpoints.Select(e => new
        {
            DisplayName = e.DisplayName,
            RoutePattern = (e as RouteEndpoint)?.RoutePattern?.RawText,
            Methods = e.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault()?.HttpMethods
        });
    }).AllowAnonymous();
}

app.UseExceptionHandler();

app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();


