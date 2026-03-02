import { Component, inject, OnInit } from '@angular/core';
import { Navbar } from "../navbar/navbar";
import { Car } from '../../models/car';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { ListCarInterface } from '../../models/listCarInterface';
import { AsyncPipe } from '@angular/common';
import { ChangeDetectorRef } from '@angular/core';
import { CarHistory } from '../../models/historyCars';

@Component({
  selector: 'app-history',
  imports: [Navbar, FormsModule, AsyncPipe],
  templateUrl: './history.html',
  styleUrl: './history.css',
})
export class History implements OnInit {
  constructor(private cdr: ChangeDetectorRef) {}

  page: number = 1;
  maxPages: number = 1;
  carList$?: Observable<ListCarInterface<CarHistory>>;
  loading: boolean = true;
  showCarInfos: boolean = false;
  selectedCar$?: Observable<CarHistory>;
  formattedBoughtPrice: string = '';
  formattedSoldPrice: string = '';
  operationDate: string = '';

  http = inject(HttpClient)
  url: string = 'https://carsales-api-7lvg.onrender.com'

  filterBy: string = '';
  operation: number = -1;
  orderBy: string = 'ascend';
  itemsPerPage: number = 10;
  brandFilter: string = '';
  modelFilter: string = '';
  licensePlateFilter: string = '';
  dateFilterStart: string = '';
  dateFilterEnd: string = '';

  ngOnInit(): void {
    this.updateCarList()
  }

  updateCarList() {
    this.loading = true;
    this.carList$ = this.http.get<ListCarInterface<CarHistory>>(`${this.url}/api/history-car-list/${this.page}`, {
      params: {
        'cars-per-page': this.itemsPerPage,
        'filter-by': this.filterBy,
        'operation': this.operation,
        'order-by': this.orderBy,
        'brand': this.brandFilter,
        'model': this.modelFilter,
        'license-plate': this.licensePlateFilter,
        'date-start': this.dateFilterStart,
        'date-end': this.dateFilterEnd
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

  selectCar(operationId: string) {
    this.showCarInfos = true;
    this.selectedCar$ = this.http.get<CarHistory>(`${this.url}/api/history-car-info/${operationId}`)

    this.selectedCar$.subscribe(res => {
      this.formattedBoughtPrice = res.boughtPrice.toLocaleString('pt-BR', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
      });
      this.formattedSoldPrice = res.soldPrice.toLocaleString('pt-BR', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
      });
      this.operationDate = new Date(res.date).toLocaleDateString('pt-BR') + ' ' + new Date(res.date).toLocaleTimeString('pt-BR', { hour12: false });

      // Forçar refresh
      this.cdr.detectChanges();
    })
  }

  onInputLicensePlate(event: Event) {
    const input = event.target as HTMLInputElement;
    this.licensePlateFilter = input.value.toUpperCase();
    if (input.value.length == 3) {
      this.licensePlateFilter = this.licensePlateFilter.concat('-')
    }
    
    this.updateCarList();
  }

  closeSelectCar() {
    this.showCarInfos = false;
    this.formattedBoughtPrice = '';
    this.formattedSoldPrice = ''
  }
}
