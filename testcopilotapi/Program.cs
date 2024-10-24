using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<ICarRepository, InMemoryCarRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Car API", Version = "v1" });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Car API v1"));
}

app.UseCors("AllowAll");

app.MapGet("/cars", (ICarRepository repo) =>
    Results.Ok(repo.GetAll()))
    .WithName("GetAllCars")
    .WithTags("Cars");

app.MapGet("/cars/{id}", (int id, ICarRepository repo) =>
    repo.GetById(id) is Car car
        ? Results.Ok(car)
        : Results.NotFound())
    .WithName("GetCarById")
    .WithTags("Cars");

app.MapPost("/cars", (Car car, ICarRepository repo) =>
{
    repo.Add(car);
    return Results.Created($"/cars/{car.Id}", car);
})
    .WithName("CreateCar")
    .WithTags("Cars");

app.MapPut("/cars/{id}", (int id, Car inputCar, ICarRepository repo) =>
{
    if (repo.GetById(id) is Car car)
    {
        car.Make = inputCar.Make;
        car.Model = inputCar.Model;
        car.Year = inputCar.Year;
        repo.Update(car);
        return Results.NoContent();
    }

    return Results.NotFound();
})
    .WithName("UpdateCar")
    .WithTags("Cars");

app.MapDelete("/cars/{id}", (int id, ICarRepository repo) =>
{
    if (repo.GetById(id) is Car car)
    {
        repo.Delete(id);
        return Results.Ok(car);
    }

    return Results.NotFound();
})
    .WithName("DeleteCar")
    .WithTags("Cars");

app.Run();

public class Car
{
    public int Id { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
}

public interface ICarRepository
{
    IEnumerable<Car> GetAll();
    Car GetById(int id);
    void Add(Car car);
    void Update(Car car);
    void Delete(int id);
}

public class InMemoryCarRepository : ICarRepository
{
    private readonly List<Car> _cars = new();
    private int _nextId = 1;

    public InMemoryCarRepository()
    {
        // Add some initial cars
        Add(new Car { Make = "Toyota", Model = "Corolla", Year = 2020 });
        Add(new Car { Make = "Honda", Model = "Civic", Year = 2019 });
        Add(new Car { Make = "Ford", Model = "Mustang", Year = 2021 });
    }

    public IEnumerable<Car> GetAll() => _cars;

    public Car GetById(int id) => _cars.FirstOrDefault(c => c.Id == id);

    public void Add(Car car)
    {
        car.Id = _nextId++;
        _cars.Add(car);
    }

    public void Update(Car car)
    {
        var index = _cars.FindIndex(c => c.Id == car.Id);
        if (index != -1)
        {
            _cars[index] = car;
        }
    }

    public void Delete(int id)
    {
        var car = GetById(id);
        if (car != null)
        {
            _cars.Remove(car);
        }
    }
}

