import { Component, signal, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Customer } from '../../models';
import { ApiService } from '../../core/services/api.service';

@Component({
  selector: 'app-customer-crm',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './customer-crm.component.html',
  styleUrl: './customer-crm.component.css'
})
export class CustomerCrmComponent implements OnInit {
  private api = inject(ApiService);

  customers = signal<Customer[]>([]);
  isLoading = signal<boolean>(true);

  ngOnInit(): void {
    this.isLoading.set(true);
    this.api.getCustomers(1).subscribe({
      next: res => {
        if (res && res.length > 0) {
          this.customers.set(res.map(c => ({
            id: c.id,
            name: c.name,
            phone: c.phone,
            loyaltyPoints: c.loyaltyPoints || 0,
            loyaltyTier: (c.loyaltyTier as any) || 'Bronze',
            address: c.addresses && c.addresses.length > 0 ? c.addresses[0].addressLine : 'Dhaka, Bangladesh',
            zone: c.addresses && c.addresses.length > 0 ? (c.addresses[0].area || c.addresses[0].district) : 'Dhaka'
          })));
        } else {
          this.customers.set([]);
        }
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }
}
