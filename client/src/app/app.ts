import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  restaurantName = "Sultan's Grand Dine & Café";
  restaurantBIN = '002345678-0101';
  branchName = 'Dhanmondi Branch (Satmasjid Road)';
  cashierName = 'Kamrul Hasan';
  shiftFloat = signal(2000);
  currentTime = signal(new Date());

  constructor() {
    setInterval(() => {
      this.currentTime.set(new Date());
    }, 1000);
  }
}
