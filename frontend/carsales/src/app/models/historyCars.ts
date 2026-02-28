export interface CarHistory {
    id: string,
    brand: string,
    model: string,
    year: number,
    licensePlate: string,
    color: string,
    boughtPrice: number,
    soldPrice: number,
    sold: boolean,
    description: string,
    soldDescription: string,
    operationId: string,
    operation: number,
    date: Date
}