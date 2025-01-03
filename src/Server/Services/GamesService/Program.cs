//using GamesService.Services;

using GamesService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;

var builder = WebApplication.CreateBuilder(args);
var auth = builder.Configuration.GetRequiredSection("Authentication");
var validIssuers = auth.GetSection("ValidIssuers").Get<IEnumerable<string>>() ?? throw new ArgumentException("Valid issuers not declared");
string metadataAddress = auth.GetValue<string>("MetadataAddress") ?? throw new ArgumentException("Metadata address is null");
bool requireHttpsMetadata = auth.GetValue<bool>("HttpsMetadata");
string adminRoleClaim = auth.GetValue<string>("AdminRoleClaim") ?? throw new ArgumentException("Admin role claim is null");
bool ignoreSslValidation = auth.GetValue<bool>("IgnoreSslCertificateValidation");
builder.Services.AddAuthentication("PrivateClientScheme")
    .AddJwtBearer("PrivateClientScheme", options =>
    {
        options.RequireHttpsMetadata = requireHttpsMetadata;
        options.Audience = auth.GetValue<string>("PrivateAudience") ?? throw new ArgumentException("Private client audience is null");
        options.MetadataAddress = metadataAddress;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidIssuers = validIssuers,
        };
        if (ignoreSslValidation)
        {
            options.BackchannelHttpHandler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
        }
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                if (claimsIdentity is null) return Task.CompletedTask;
                claimsIdentity.AddClaim(new Claim("AuthenticationScheme", "PrivateClientScheme"));
                return Task.CompletedTask;
            }
        };

    })
    .AddJwtBearer("PublicClientScheme", options =>
    {
        options.RequireHttpsMetadata = requireHttpsMetadata;
        options.Audience = auth.GetValue<string>("PublicAudience") ?? throw new ArgumentException("Public client audience is null");
        options.MetadataAddress = metadataAddress;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidIssuers = validIssuers,
        };
        if (ignoreSslValidation)
        {
            options.BackchannelHttpHandler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
        }
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                if (claimsIdentity is null) return Task.CompletedTask;
                claimsIdentity.AddClaim(new Claim("AuthenticationScheme", "PublicClientScheme"));

                var realmAccessClaim = claimsIdentity.FindFirst("realm_access");
                if (realmAccessClaim is null) return Task.CompletedTask;

                var realmAccess = JsonNode.Parse(realmAccessClaim.Value);
                if (realmAccess is null) return Task.CompletedTask;

                var roles = realmAccess["roles"]?.AsArray();
                if (roles is null) return Task.CompletedTask;
                foreach (var role in roles)
                {
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.Deserialize<string>()));
                }

                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddAuthorization(options =>
{

    options.AddPolicy("AdminOrPrivateClient", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AuthenticationSchemes.Add("PublicClientScheme");
        policy.AuthenticationSchemes.Add("PrivateClientScheme");
        policy.RequireAssertion((context) =>
        {
            var scheme = context.User.Claims.FirstOrDefault(c => c.Type == "AuthenticationScheme");
            if (scheme is null)
                return false;

            if (scheme.Value == "PublicClientScheme" && context.User.IsInRole(adminRoleClaim))
                return true;
            else if (scheme.Value == "PrivateClientScheme")
                return true;
            else
                return false;
        });
    });
    options.AddPolicy("AllAuthenticatedUsers", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AuthenticationSchemes.Add("PublicClientScheme");
        policy.AuthenticationSchemes.Add("PrivateClientScheme");
    });
});
// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddDbContext<GamesService.GamesServerDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Database"));
});
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<GamesServerDbContext>();
    await context.Database.EnsureCreatedAsync();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");
app.MapGrpcService<GamesService.Services.GamesService>();
app.Run();
