namespace CarSalesApi.Cars;

public class CarHistory
{
    public Guid Id { get; init; }
    public Guid CarId { get; private set; }
    public OperationType Operation { get; private set; }
    public DateTime Date { get; private set; }
    public Car Car { get; private set; } = null!;
    
    public enum OperationType
    {
        Purchase = 0,
        Sell = 1
    }

    public CarHistory(Guid carId)
    {
        this.Id = Guid.NewGuid();
        this.CarId = carId;
        this.Date = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,  TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"));
        //this.Date = String.Format("{0:dd/MM/yyyy HH:mm:ss}", TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,  TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")));
    }

    public void SetBuy()
    {
        this.Operation = OperationType.Purchase;
    }

    public void SetSell()
    {
        this.Operation = OperationType.Sell;
    }
}