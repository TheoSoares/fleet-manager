import { Component, inject, OnInit } from '@angular/core';
import { Navbar } from "../navbar/navbar";
import { Car } from '../../models/car';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { ListCarInterface } from '../../models/listCarInterface';
import { AsyncPipe } from '@angular/common';
import { SellCarDTO } from '../../models/sellCarDto';
import { ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'app-fleet',
  imports: [Navbar, FormsModule, AsyncPipe],
  templateUrl: './fleet.html',
  styleUrl: './fleet.css',
})

export class Fleet implements OnInit {
  constructor(private cdr: ChangeDetectorRef) {}

  page: number = 1;
  maxPages: number = 1;
  carList$?: Observable<ListCarInterface<Car>>;
  loading: boolean = true;
  showCarInfos: boolean = false;
  selectedCar$?: Observable<Car>;
  formattedBoughtPrice: string = '';

  price: number = 0;
  formattedPrice: string = '';

  description: string = '';


  http = inject(HttpClient);
  url: string = 'https://carsales-api-7lvg.onrender.com';

  filterBy: string = '';
  orderBy: string = 'ascend';
  itemsPerPage: number = 10;
  brandFilter: string = '';
  modelFilter: string = '';
  licensePlateFilter: string = '';

  ngOnInit(): void {
    this.updateCarList()
  }

  updateCarList() {
    this.loading = true;
    this.carList$ = this.http.get<ListCarInterface<Car>>(`${this.url}/api/car-list/${this.page}`, {
      params: {
        'cars-per-page': this.itemsPerPage,
        'filter-by': this.filterBy,
        'order-by': this.orderBy,
        'brand': this.brandFilter,
        'model': this.modelFilter,
        'license-plate': this.licensePlateFilter
      }
    })

    this.carList$.subscribe(res => {
      this.maxPages = res.maxPages;
      this.loading = false;
    })
  }

  changePage(dir: string) {
    if (dir == 'left' && this.page > 1) {
      this.page -= 1
      this.updateCarList()
    }
    else if (dir == 'right' && this.page < this.maxPages) {
      this.page += 1
      this.updateCarList()
    }
  }

  selectCar(licensePlate: string) {
    this.showCarInfos = true;
    this.selectedCar$ = this.http.get<Car>(`${this.url}/api/car-info/${licensePlate}`)

    this.selectedCar$.subscribe(res => {
      this.formattedBoughtPrice = res.boughtPrice.toLocaleString('pt-BR', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
      });
    })
  }

  closeSelectCar() {
    this.showCarInfos = false;
    this.price = 0; 
    this.formattedPrice = ''
    this.description = '';
  }

  sellCar() {
    const soldDTO: SellCarDTO = {
      soldPrice: this.price / 100,
      soldDescription: this.description
    }

    this.selectedCar$?.subscribe(res => {
      this.http.put(`${this.url}/api/sell-car/${res.licensePlate}`, soldDTO, { observe: 'response' })
    .subscribe({
      next: response => {
        alert(`Carro vendido com sucesso!`);
        this.closeSelectCar();
        this.page = 1;
        this.updateCarList();

        // Forçar refresh
        this.cdr.detectChanges();
      },
      error: err => {
        alert(`Erro: ${err.error}`)
      }
    })});
  }

  onInputLicensePlate(event: Event) {
    const input = event.target as HTMLInputElement;
    this.licensePlateFilter = input.value.toUpperCase();
    if (input.value.length == 3) {
      this.licensePlateFilter = this.licensePlateFilter.concat('-')
    }
    
    this.updateCarList();
  }

  onInputPrice(event: Event) {
    const input = event.target as HTMLInputElement;

    // remove tudo que não for número
    const numeros = input.value.replace(/\D/g, '');

    // evita vazio
    const valor = numeros ? parseInt(numeros, 10) : 0;

    this.price = valor;

    // converte para reais (divide por 100)
    const valorEmReais = valor / 100;

    // formata padrão brasileiro
    this.formattedPrice = valorEmReais.toLocaleString('pt-BR', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    });
  }
}
