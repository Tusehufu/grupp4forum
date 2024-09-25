using Grupp4forum.Dev.Infrastructure;
using Grupp4forum.Dev.Infrastructure.Configuration;
using Grupp4forum.Dev.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Grupp4forum.Dev.App;
using Grupp4forum.Dev.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Grupp4forum.Dev.Infrastructure.Services;
using Grupp4forum.Dev.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;


var builder = WebApplication.CreateBuilder(args);
//// Lägg till DbContext för migrationshantering
builder.Services.AddDbContext<MigrationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("ConnectionStrings"));


// Add services to the container.
builder.Services.AddTransient<PostRepository>();
builder.Services.AddTransient<PostService>();
builder.Services.AddTransient<RoleRepository>();
builder.Services.AddTransient<RoleService>();
builder.Services.AddTransient<UserRepository>();
builder.Services.AddTransient<UserService>();
builder.Services.AddTransient<CategoryRepository>();
builder.Services.AddTransient<CategoryService>();
builder.Services.AddTransient<ReplyRepository>();
builder.Services.AddTransient<ReplyService>();

builder.Services.AddControllers();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "CorsRule",
    builder =>
    {
        builder.AllowAnyOrigin() 
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

    var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    
}

app.UseSwagger();
app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dispatch API V1");
        c.RoutePrefix = "swagger";
    });
app.UseStaticFiles();
app.UseCors("CorsRule");
app.UseRouting();




app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapFallbackToFile("index.html");
});

app.MapControllers();

app.Run();
