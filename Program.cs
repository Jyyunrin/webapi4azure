using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using StudentsMinimalApi.Data;
using StudentsMinimalApi.Models;
using StudentsMinimalApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//Adding services
builder.Services.AddOpenApi();

//Read connection string, and add db context and associate it with the connection string,
//And telling it we're using sqlite
//Now we can do migration with sqllite - you don't need to do anything different
//When you do migration - Entity Framework migration will look at your db and create 
//the commands accordingly - all db are proprietary with their own ways of doing things
//If you change db, need to delete migrations and recreate it
var connStr = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<SchoolDbContext>(option => option.UseSqlite(connStr));

// Add Cors
builder.Services.AddCors(o => o.AddPolicy("Policy", builder => {
  builder.AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader();
}));

var app = builder.Build();

app.UseCors("Policy");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseHttpsRedirection();
    // app.UseSwaggerUI(options => {
    //     options.RoutePrefix = "";
    //     options.SwaggerEndpoint("/openapi/v1.json", "My WebAPI");
    // });
    app.MapScalarApiReference(options => {
        options
            .WithTitle("My WebAPI")
            .WithTheme(ScalarTheme.Moon)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

//Decluttering code:
var studentsRoute = app.MapGroup("/api/students");

studentsRoute.MapGet("/", StudentService.GetAllStudents);
studentsRoute.MapGet("/school/{school}", StudentService.GetStudentsBySchool);
studentsRoute.MapGet("/{id}", StudentService.GetStudent);
studentsRoute.MapPost("/", StudentService.CreateStudent);
studentsRoute.MapPut("/{id}", StudentService.UpdateStudent);
studentsRoute.MapDelete("/{id}", StudentService.DeleteStudent);

using (var scope = app.Services.CreateScope()) {
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<SchoolDbContext>();    
    context.Database.Migrate();
}

app.Run();

