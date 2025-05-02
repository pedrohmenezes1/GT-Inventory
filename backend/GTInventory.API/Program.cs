using GTInventory.Domain.Interfaces;
using GTInventory.Infrastructure.Configuration;
using GTInventory.Infrastructure.Services.LdapService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuração básica
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configuração do Swagger/OpenAPI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GTInventory API", Version = "v1" });
    
    // Configuração de segurança JWT no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Insira o token JWT no formato: Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
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
            Array.Empty<string>()
        }
    });
});

// Configurações de autenticação JWT
var jwtConfig = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(jwtConfig["Secret"])),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Configurações de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:5173")
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// Configurações de infraestrutura
builder.Services.Configure<LdapConfig>(builder.Configuration.GetSection("Ldap"));
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("Jwt"));

// Validação das configurações
builder.Services.AddOptions<LdapConfig>()
    .Validate(config => 
    {
        config.Validate();
        return true;
    });

builder.Services.AddOptions<JwtConfig>()
    .Validate(config => 
    {
        if (string.IsNullOrEmpty(config.Secret))
            throw new InvalidOperationException("JWT Secret não configurado");
        return true;
    });

// Registro de serviços
builder.Services.AddScoped<IUserService, LdapUserService>();
builder.Services.AddSingleton(provider => 
    provider.GetRequiredService<ILoggerFactory>().CreateLogger("GlobalLogger"));

var app = builder.Build();

// Pipeline de execução
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GTInventory API v1"));
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Middleware de tratamento de erros global
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger>();
        logger.LogError("Erro não tratado ocorreu");
        
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync("Ocorreu um erro interno. Tente novamente mais tarde.");
    });
});

app.Run();