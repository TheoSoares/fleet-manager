import { Component, inject } from '@angular/core';
import { Navbar } from "../navbar/navbar";
import { FormsModule, NgForm } from '@angular/forms';
import { Car } from '../../models/car';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-buy-car',
  imports: [Navbar, FormsModule],
  templateUrl: './buy-car.html',
  styleUrl: './buy-car.css',
})

export class BuyCar {
  brand: string = '';
  model: string = '';
  year?: number;
  licensePlate: string = '';
  color: string = '';
  price?: number;
  description: string = '';

  formattedPrice: string = ''

  http = inject(HttpClient)
  url = 'https://carsales-api-7lvg.onrender.com'

  carPostRequest(form: NgForm) {
    const regex = /^[A-Z]{3}-?(?:\d{4}|\d[A-Z]\d{2})$/i;
    const alertArrayVar = [this.brand, this.model, this.year, this.licensePlate, this.color, this.price]
    const alertArrayWarning = ['Marca', 'Modelo', 'Ano', 'Placa', 'Cor', 'Valor']
    for (let i = 0; i < alertArrayVar.length; i++) {
      if (!alertArrayVar[i]) {
        alert(`O campo ${alertArrayWarning[i]} não pode estar vazio!`);
        return;
      }
    }
    if (!this.year || this.year < 1900 || this.year > (new Date().getFullYear() + 1)) {
      alert(`Ano inválido!`);
      return;
    }
    if (!regex.test(this.licensePlate)) {
      alert(`Placa inválida!`);
      return;
    }

    // save car as object
    const newCar: Car = {
      brand: this.brand,
      model: this.model,
      year: this.year,
      licensePlate: this.licensePlate,
      color: this.color,
      boughtPrice: this.price! / 100,
      description: this.description
    }

    this.http.post<Car>(`${this.url}/api/buy-car`, newCar, { observe: 'response' }).subscribe({
      next: response => {
        form.resetForm({
          brand: '',
          model: '',
          year: undefined,
          licensePlate: '',
          color: '',
          price: '0,00',
          description: ''
        })
        this.price = undefined
        alert(`Carro registrado com sucesso!`);
      },
      error: err => {
        alert(`Erro: ${err.error}`)
      }
    });

    // console.log(this.brand, this.model, this.year, this.licensePlate, this.color, this.price, this.description)
  }

  onInputLicensePlate(event: Event) {
    const input = event.target as HTMLInputElement;
    this.licensePlate = input.value.toUpperCase();
    if (input.value.length == 3) {
      this.licensePlate = this.licensePlate.concat('-')
    }
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
