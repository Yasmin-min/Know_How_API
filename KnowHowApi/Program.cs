using System.Text;
using KnowHowApi.Data;
using KnowHowApi.Data.Repositories;
using KnowHowApi.Domain.Configurations;
using KnowHowApi.Domain.Interfaces;
using KnowHowApi.Hubs;
using KnowHowApi.Services;
using KnowHowApi.Services.Interfaces;
using KnowHowApi.Services.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "KnowHowApi",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Description = "Informe apenas o token JWT (sem o prefixo 'Bearer')."
    });

    options.AddSecurityRequirement(document => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        { new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document), new List<string>() }
    });
});

builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAreaInteresseRepository, AreaInteresseRepository>();
builder.Services.AddScoped<ICryptography, Cryptography>();
builder.Services.AddScoped<IProfessorService, ProfessorService>();
builder.Services.AddScoped<IProfessorRepository, ProfessorRepository>();
builder.Services.AddScoped<IAulaRepository, AulaRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAlunoService, AlunoService>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IAulaAgendadaRepository, AulaAgendadaRepository>();
builder.Services.AddScoped<IAulaAgendadaService, AulaAgendadaService>();
builder.Services.AddScoped<IProfessorAlunoRepository, ProfessorAlunoRepository>();
builder.Services.AddScoped<IProfessorAlunoService, ProfessorAlunoService>();
builder.Services.AddScoped<IPlanosRepository, PlanosRepository>();
builder.Services.AddScoped<IPlanosService, PlanosService>();
builder.Services.AddScoped<IAvaliacaoRepository, AvaliacaoRepository>();
builder.Services.AddScoped<IAvaliacaoService, AvaliacaoService>();
builder.Services.AddScoped<IChamadaService, ChamadaService>();
builder.Services.AddSingleton<SalasChamada>();
builder.Services.Configure<TurnSettings>(builder.Configuration.GetSection("Turn"));
builder.Services.AddHttpClient<ICredenciaisTurnService, CloudflareTurnService>(client => client.Timeout = TimeSpan.FromSeconds(5));

builder.Services.AddSignalR();

builder.Services.AddDbContext<Context>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );
    options.LogTo(Console.WriteLine);
});

var jwtSettings = new JWTSettings();
new ConfigureFromConfigurationOptions<JWTSettings>(builder.Configuration.GetSection("Jwt")).Configure(jwtSettings);
builder.Services.AddSingleton(jwtSettings);

var smtpSettings = new SmtpSettings();
new ConfigureFromConfigurationOptions<SmtpSettings>(builder.Configuration.GetSection("Smtp")).Configure(smtpSettings);
builder.Services.AddSingleton(smtpSettings);

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
        };

        // O cliente WebSocket do SignalR não envia o header Authorization; o token
        // vem via query string (?access_token=...) na conexão com o hub.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/api/chatHub"))
                    context.Token = accessToken;

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true);
    });
});

var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    app.Urls.Add($"http://0.0.0.0:{port}");
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseCors("AllowAll");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/api/chatHub");

app.Run();
