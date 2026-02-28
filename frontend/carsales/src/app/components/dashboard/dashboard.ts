import { Component, ViewChild, AfterViewInit, ElementRef, inject, ChangeDetectorRef, Query } from '@angular/core';
import { Navbar } from "../navbar/navbar";
import { Chart, registerables } from 'chart.js';
import { HttpClient } from '@angular/common/http';
import { DashboardSummary } from '../../models/dashboardSummary';
import { forkJoin } from 'rxjs';

Chart.register(...registerables)

@Component({
  selector: 'app-dashboard',
  imports: [Navbar],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})

export class Dashboard implements AfterViewInit {
  constructor(private cdr: ChangeDetectorRef) {}

  http = inject(HttpClient)
  url: string = 'https://carsales-api-7lvg.onrender.com'

  @ViewChild('lineCanvas') lineCanvas!: ElementRef<HTMLCanvasElement>;
  lineChart!: Chart;
  @ViewChild('doughnutCanvas') doughnutCanvas!: ElementRef<HTMLCanvasElement>;
  doughnutChart!: Chart;
  profit!: string;
  soldVehicles!: Number;
  fleetSize!: Number;
  yearSales!: any;
  yearSpents!: any;
  yearProfit!: any;

  ngAfterViewInit(): void {
    this.doughnutChartMethod();
    this.getTopDashboardInfos();
    this.loadChartData();
  }

  getTopDashboardInfos(): void {
    this.http.get<DashboardSummary>(`${this.url}/api/dashboard/top-dashboard-infos`).subscribe(res => {
      this.profit = res.profit.toLocaleString('pt-BR', {minimumFractionDigits: 2, maximumFractionDigits: 2});
      this.soldVehicles = res.soldVehicles;
      this.fleetSize = res.fleetSize;

      this.cdr.detectChanges();
    })
  }

  loadChartData() {
    forkJoin({
      sales: this.http.get(`${this.url}/api/dashboard/last-year-sales`, {
        params: {
          'month': new Date().getMonth() + 1,
          'year': new Date().getFullYear()
        }
      }),
      spents: this.http.get(`${this.url}/api/dashboard/last-year-spents`, {
        params: {
          'month': new Date().getMonth() + 1,
          'year': new Date().getFullYear()
        }
      }),
      profit: this.http.get(`${this.url}/api/dashboard/last-year-profit`, {
        params: {
          'month': new Date().getMonth() + 1,
          'year': new Date().getFullYear()
        }
      })
    }).subscribe(({sales, spents, profit}) => {
      this.yearSales = sales;
      this.yearSpents = spents;
      this.yearProfit = profit;
      this.lineChartMethod();
    })
  }

  getLastMonths(): Array<string> {
    const months: Array<string> = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez']
    const month = new Date().getMonth();
    var m: Array<string> = [];
    for (let i = 1; i < 13; i++) {
      m.push(months[(month + i) % 12])
    }
    return m;
  }

  lineChartMethod(): void {
    console.log(this.yearProfit)
    this.lineChart = new Chart(this.lineCanvas.nativeElement, {
      type: 'line',
      data: {
        labels: this.getLastMonths(),
        datasets: [{
          fill: true,
          label: 'Vendas',
          data: this.yearSales
        },{
          fill: true,
          label: 'Gastos',
          data: this.yearSpents
        }, {
          fill: true,
          label: 'Lucro',
          data: this.yearProfit
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        aspectRatio: 2
      }
    })
  }

  doughnutChartMethod(): void {
    this.doughnutChart = new Chart(this.doughnutCanvas.nativeElement, {
      type: 'doughnut',
      data: {
        labels: ['Compras', 'Reparos'],
        datasets: [{
          data: [53, 0]
        }]
      },
      options: {
        responsive: true
      }
    })
  }
}
