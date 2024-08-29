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


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("ConnectionStrings"));


// Add services to the container.
builder.Services.AddTransient<PostRepository>();
builder.Services.AddTransient<PostService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "CorsRule",
    builder =>
    {
        builder.WithOrigins("http://localhost:8080")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();
app.UseCors("CorsRule");
app.UseRouting();



app.UseHttpsRedirection();


app.UseAuthorization();

app.MapControllers();

// Here you can see we make sure it doesn't start with /api, if it does, it'll 404 within .NET if it can't be found.
//app.MapWhen(x => !x.Request.Path.StartsWithSegments("/api"), x =>
//{
//    app.UseSpa(spa =>
//    {
//        if (!builder.Environment.IsDevelopment())
//            spa.Options.SourcePath = "wwwroot/clientapp";
//        else
//            spa.Options.SourcePath = @"C:\Data\Repos\KEYnet\InternJohan.Dev.App\wwwroot\clientapp";
//    });
//});
app.Run();
