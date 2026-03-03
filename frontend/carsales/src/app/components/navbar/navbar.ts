import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { RouterLink, RouterModule } from '@angular/router';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, RouterModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})

export class Navbar implements OnInit {
  constructor(private cdr: ChangeDetectorRef) {}
  
  lastInnerWidth!: boolean;
  navbarHidden!: boolean;

  ngOnInit(): void {
    this.checkScreen();

    window.addEventListener('resize', () => {
      this.checkScreen();
    });
  }

  checkScreen() {
    if ((this.lastInnerWidth == undefined) || (window.innerWidth <= 768) != this.lastInnerWidth) {
      this.navbarHidden = window.innerWidth <= 768;
      this.lastInnerWidth = window.innerWidth <= 768;
      // Forçar refresh
      this.cdr.detectChanges();
    }
  }

  changeNavbarState() {
    this.navbarHidden = !this.navbarHidden
  }
}
