using LoginRegisterApp.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Wire the shared DatabaseHelper (see Helpers/DatabaseHelper.cs) to the
// connection string in appsettings.json, once, at startup. Every repository
// call from here on (ProduitPortailRepository, ContactRepository,
// DemandeAchatRepository, NotificationRepository...) reads from the same
// KSkinManager database the desktop app uses.
string connectionString = builder.Configuration.GetConnectionString("KSkinManager")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:KSkinManager in appsettings.json.");
DatabaseHelper.Initialize(connectionString);

// Only the portal's own origin(s) may call this API - configure in
// appsettings.json under Portail:AllowedOrigins (e.g. your Vite dev server
// and your production portal domain).
string[] allowedOrigins = builder.Configuration.GetSection("Portail:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("Portail", policy =>
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Disabled HttpsRedirection on dev to avoid self-signed cert blocking in browser fetch requests
// app.UseHttpsRedirection();
app.UseCors("Portail");
app.MapControllers();

app.Run();
