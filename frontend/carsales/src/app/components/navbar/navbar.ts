import { Component } from '@angular/core';
import { RouterLink, RouterModule } from '@angular/router';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, RouterModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar {
}
