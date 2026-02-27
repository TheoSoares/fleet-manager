namespace CarSalesApi.Cars;

public class Car
{
    public string Brand { get; private set; }
    public string Model { get; private set; }
    public int Year { get; private set; }
    public string LicensePlate { get; private set; }
    public string Color { get; private set; }
    public decimal BoughtPrice { get; private set; }
    public decimal SoldPrice { get; private set; }
    public string Description { get; private set; }
    public bool Sold { get; private set; }
    public string SoldDescription { get; private set; }
    public Guid Id { get; init; }
    public ICollection<CarHistory> History { get; private set; } = new List<CarHistory>();

    public Car(string brand, string model, int year, string licensePlate, string color, decimal boughtPrice,
        string description)
    {
        Brand = brand;
        Model = model;
        Year = year;
        LicensePlate = licensePlate;
        Color = color;
        BoughtPrice = boughtPrice;
        SoldPrice = 0;
        Description = description;
        Sold = false;
        SoldDescription = string.Empty;
        Id = Guid.NewGuid();
    }

    public void SellCar(decimal soldPrice, string soldDescription)
    {
        Sold = true;
        SoldPrice = soldPrice;
        SoldDescription = soldDescription;
    }
}