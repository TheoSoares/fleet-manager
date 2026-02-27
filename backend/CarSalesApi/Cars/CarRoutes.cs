using CarSalesApi.Data;
using Microsoft.EntityFrameworkCore;

namespace CarSalesApi.Cars;

public static class CarRoutes
{
    public static void BuyCarRoutes(this WebApplication app)
    {
        // Testar API
        app.MapGet("/api/get-test", () => "working!");
        
        // Adicionar Carro no DB
        app.MapPost("/api/buy-car", async (Car carRequest, AppDbContext context, CancellationToken ct) =>
        {
            var alreadyInStock = await context.Cars.AnyAsync(car => car.LicensePlate == carRequest.LicensePlate && !car.Sold, ct);

            if (alreadyInStock)
            {
                return Results.Conflict("O carro já está no estoque!");
            }
            
            var newCar = new Car(carRequest.Brand, carRequest.Model, carRequest.Year, carRequest.LicensePlate, carRequest.Color, carRequest.BoughtPrice, carRequest.Description);
            
            await context.Cars.AddAsync(newCar, ct);
            
            // Criar registro de histórico
            var carHistory = new CarHistory(newCar.Id);
            carHistory.SetBuy();
            
            // Adicionar registro na tabela
            await context.CarsHistory.AddAsync(carHistory, ct);
            
            // Salvar alterações
            await context.SaveChangesAsync(ct);
            
            return Results.Ok();
        });
    }

    public static void SellCarRoutes(this WebApplication app)
    {
        app.MapGet("/api/car-list/{page}",
            async (string page, HttpRequest request, AppDbContext context, CancellationToken ct) =>
            {
                int carsPerPage = int.Parse(request.Query["cars-per-page"]); 
                string filterBy = request.Query["filter-by"];
                string orderBy = request.Query["order-by"];
                string brand = request.Query["brand"];
                string model = request.Query["model"];
                string licensePlate = request.Query["license-plate"];

                var query = context.Cars.Where(car => !car.Sold).AsQueryable();
                
                // condicionais
                if (!string.IsNullOrEmpty(brand))
                {
                    query = query.Where(car => car.Brand.Contains(brand));
                }

                if (!string.IsNullOrEmpty(model))
                {
                    query = query.Where(car => car.Model.Contains(model));
                }

                if (!string.IsNullOrEmpty(licensePlate))
                {
                    query = query.Where(car => car.LicensePlate.Contains(licensePlate));
                }
                
                // ordenacao
                if (!string.IsNullOrEmpty(filterBy))
                {
                    if (orderBy == "descend")
                    {
                        query = query.OrderByDescending(e => EF.Property<object>(e, filterBy));
                    }
                    else
                    {
                        query = query.OrderBy(e => EF.Property<object>(e, filterBy));
                    }
                }
                
                int totalItems = await query.CountAsync(ct);
                int maxPages = (int)Math.Ceiling((double)totalItems / carsPerPage);
                
                var cars = await query.Skip((int.Parse(page) - 1) * carsPerPage).Take(carsPerPage).ToListAsync(ct);

                return Results.Ok(new
                {
                    maxPages,
                    cars
                });
            });
        
        app.MapGet("/api/car-info/{licensePlate}",
            async (string licensePlate, AppDbContext context, CancellationToken ct) =>
            {
                var query = await context.Cars.Where(car => !car.Sold).Where(car => car.LicensePlate == licensePlate)
                    .SingleOrDefaultAsync(ct);

                return Results.Ok(new CarSellInfoDTO(query.Brand, query.Model, query.Year, query.LicensePlate,  query.Color, query.BoughtPrice, query.Description));
            });

        app.MapPut("/api/sell-car/{licensePlate}",
            async (string licensePlate, SellCarDTO dto, AppDbContext context,
                CancellationToken ct) =>
            {
                var car = await context.Cars.Where(car => !car.Sold).Where(car => car.LicensePlate == licensePlate)
                    .SingleOrDefaultAsync(ct);

                if (car == null)
                {
                    return Results.NotFound();
                }

                car.SellCar(dto.SoldPrice, dto.SoldDescription);

                // Criar registro de histórico
                var carHistory = new CarHistory(car.Id);
                carHistory.SetSell();
                
                // Adicionar registro na Tabela
                await context.CarsHistory.AddAsync(carHistory, ct);
  
                // Salvar alterações
                await context.SaveChangesAsync(ct);

                return Results.Ok();
            });
    }

    public static void HistoryCarRoutes(this WebApplication app)
    {
        app.MapGet("/api/history-car-list/{page}",
            async (string page, HttpRequest request, AppDbContext context, CancellationToken ct) =>
            {
                int carsPerPage = int.Parse(request.Query["cars-per-page"]); 
                string filterBy = request.Query["filter-by"];
                int operation = int.Parse(request.Query["operation"]);
                string orderBy = request.Query["order-by"];
                string brand = request.Query["brand"];
                string model = request.Query["model"];
                string licensePlate = request.Query["license-plate"];
                string date = request.Query["date"];

                var query = context.Cars
                    .Include(c => c.History)
                    .SelectMany(c => c.History, (car, history) => new
                    {
                        car.Id,
                        car.Brand,
                        car.Model,
                        car.Year,
                        car.LicensePlate,
                        car.Color,
                        car.BoughtPrice,
                        car.SoldPrice,
                        car.Sold,
                        car.Description,
                        car.SoldDescription,
                        OperationId = history.Id,
                        history.Operation,
                        history.Date
                    })
                    .AsQueryable();
                
                // condicionais
                if (operation != -1)
                {
                    query = query.Where(car => (int)car.Operation == operation);
                }
                
                if (!string.IsNullOrEmpty(brand))
                {
                    query = query.Where(car => car.Brand.Contains(brand));
                }

                if (!string.IsNullOrEmpty(model))
                {
                    query = query.Where(car => car.Model.Contains(model));
                }

                if (!string.IsNullOrEmpty(licensePlate))
                {
                    query = query.Where(car => car.LicensePlate.Contains(licensePlate));
                }
                
                if (!string.IsNullOrEmpty(date))
                {
                    query = query.Where(car => car.Date.Contains(date));
                }
                
                // ordenacao
                if (!string.IsNullOrEmpty(filterBy))
                {
                    if (orderBy == "descend")
                    {
                        query = query.OrderByDescending(e => EF.Property<object>(e, filterBy));
                    }
                    else
                    {
                        query = query.OrderBy(e => EF.Property<object>(e, filterBy));
                    }
                }
                
                int totalItems = await query.CountAsync(ct);
                int maxPages = (int)Math.Ceiling((double)totalItems / carsPerPage);
                
                var cars = await query.Skip((int.Parse(page) - 1) * carsPerPage).Take(carsPerPage).ToListAsync(ct);

                return Results.Ok(new
                {
                    maxPages,
                    cars
                });
            });
        
        app.MapGet("/api/history-car-info/{id}",
            async (string id, AppDbContext context, CancellationToken ct) =>
            {
                var query = await context.Cars
                    .Include(c => c.History)
                    .SelectMany(c => c.History, (car, history) => new
                    {
                        car.Id,
                        car.Brand,
                        car.Model,
                        car.Year,
                        car.LicensePlate,
                        car.Color,
                        car.BoughtPrice,
                        car.SoldPrice,
                        car.Sold,
                        car.Description,
                        car.SoldDescription,
                        OperationId = history.Id,
                        history.Operation,
                        history.Date
                    })
                    .Where(car => car.OperationId == Guid.Parse(id))
                    .SingleOrDefaultAsync(ct);
                
                return Results.Ok(query);
            });
    }
}