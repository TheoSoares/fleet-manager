using System.Globalization;
using CarSalesApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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

                Console.WriteLine(request.Query["date-start"]);
                if (DateTime.TryParseExact(request.Query["date-start"], "yyyy-MM-dd", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var dateStart))
                {
                    query = query.Where(car => car.Date >= dateStart);
                };
                
                if (DateTime.TryParseExact(request.Query["date-end"], "yyyy-MM-dd", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var dateEnd))
                {
                    query = query.Where(car => car.Date < dateEnd.Date.AddDays(1));
                };
                
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
                        Date = history.Date
                    })
                    .Where(car => car.OperationId == Guid.Parse(id))
                    .SingleOrDefaultAsync(ct);
                
                return Results.Ok(query);
            });
    }

    public static void DashboardRoutes(this WebApplication app)
    {
        app.MapGet("/api/dashboard/top-dashboard-infos", async (AppDbContext context, CancellationToken ct) =>
        {
            var profit = await context.Cars.Where(car => car.Sold).Select(car => car.SoldPrice).SumAsync(ct) - await context.Cars.Select(car => car.BoughtPrice).SumAsync(ct);

            var soldVehicles = await context.Cars.Where(car => car.Sold).Select(car => car.Sold).CountAsync(ct);
            
            var fleetSize = await context.Cars.Where(car => !car.Sold).Select(car => car.Sold).CountAsync(ct);

            return Results.Ok(new {profit, soldVehicles, fleetSize});
        });

        app.MapGet("/api/dashboard/last-year-profit",
            async (HttpRequest request, AppDbContext context, CancellationToken ct) =>
            {
                int month =  int.Parse(request.Query["month"]);
                int year = int.Parse(request.Query["year"]);

                var profit = new List<decimal>();

                for (int i = 0; i < 12; i++)
                {
                    var query = context.Cars
                        .Include(car => car.History)
                        .SelectMany(c => c.History, (car, history) => new
                        {
                            car.BoughtPrice,
                            car.SoldPrice,
                            history.Operation,
                            history.Date
                        })
                        .Where(car => car.Date.Month == month)
                        .Where(car => car.Date.Year == year)
                        .AsQueryable();
                    
                    var monthProfit = await query
                        .Where(car => car.Operation == CarHistory.OperationType.Sell)
                        .Select(car => car.SoldPrice)
                        .SumAsync(ct) - await query
                        .Where(car => car.Operation == CarHistory.OperationType.Purchase)
                        .Select(car => car.BoughtPrice)
                        .SumAsync(ct);
                    
                    profit.Add(monthProfit);

                    month -= 1;
                    if (month == 0)
                    {
                        month = 12;
                        year -= 1;
                    }
                }

                profit.Reverse();
                
                return Results.Ok(profit);
            });
        app.MapGet("/api/dashboard/last-year-spents",
            async (HttpRequest request, AppDbContext context, CancellationToken ct) =>
            {
                int month =  int.Parse(request.Query["month"]);
                int year = int.Parse(request.Query["year"]);

                var spents = new List<decimal>();

                for (int i = 0; i < 12; i++)
                {
                    var monthSpents = await context.Cars
                        .Include(car => car.History)
                        .SelectMany(c => c.History, (car, history) => new
                        {
                            car.BoughtPrice,
                            history.Operation,
                            history.Date
                        })
                        .Where(car => car.Operation == CarHistory.OperationType.Purchase)
                        .Where(car => car.Date.Month == month)
                        .Where(car => car.Date.Year == year)
                        .Select(car => car.BoughtPrice)
                        .SumAsync(ct);
                    
                    spents.Add(monthSpents);

                    month -= 1;
                    if (month == 0)
                    {
                        month = 12;
                        year -= 1;
                    }
                }

                spents.Reverse();
                
                return Results.Ok(spents);
            });
        
        app.MapGet("/api/dashboard/last-year-sales",
            async (HttpRequest request, AppDbContext context, CancellationToken ct) =>
            {
                int month =  int.Parse(request.Query["month"]);
                int year = int.Parse(request.Query["year"]);

                var sales = new List<decimal>();

                for (int i = 0; i < 12; i++)
                {
                    var monthSales = await context.Cars
                        .Include(car => car.History)
                        .SelectMany(c => c.History, (car, history) => new
                        {
                            car.SoldPrice,
                            history.Operation,
                            history.Date
                        })
                        .Where(car => car.Operation == CarHistory.OperationType.Sell)
                        .Where(car => car.Date.Month == month)
                        .Where(car => car.Date.Year == year)
                        .Select(car => car.SoldPrice)
                        .SumAsync(ct);
                    
                    sales.Add(monthSales);
                    
                    Console.WriteLine($"Mes: {month} | Ano: {year} | Vendas: {monthSales}");

                    month -= 1;
                    if (month == 0)
                    {
                        month = 12;
                        year -= 1;
                    }
                }

                sales.Reverse();
                
                return Results.Ok(sales);
            });
    }
}