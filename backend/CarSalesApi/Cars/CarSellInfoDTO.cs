namespace CarSalesApi.Cars;

public record CarSellInfoDTO(string Brand, string Model, int Year, string LicensePlate, string Color, decimal BoughtPrice, string Description);