import { Injectable, signal } from '@angular/core';

export interface UserSession {
  userId: number;
  userName: string;
  displayName: string;
  role: string;
  restaurantId: number;
  branchId: number;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  currentSession = signal<UserSession>({
    userId: 2,
    userName: 'cashier1',
    displayName: 'Kamrul Hasan',
    role: 'Cashier',
    restaurantId: 1,
    branchId: 1
  });

  restaurantInfo = signal({
    name: "Sultan's Grand Dine & Café",
    legalName: 'Sultans Food & Hospitality Ltd.',
    bin: '002345678-0101',
    branch: 'Dhanmondi Branch (Satmasjid Road)',
    address: 'Plot 34, Satmasjid Road, Dhanmondi, Dhaka 1209',
    phone: '+8801811223344'
  });

  isLoggedIn = signal(true);
}
