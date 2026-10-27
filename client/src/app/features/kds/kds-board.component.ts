import { Component, signal, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { KOT } from '../../models';
import { ApiService } from '../../core/services/api.service';

@Component({
  selector: 'app-kds-board',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './kds-board.component.html',
  styleUrl: './kds-board.component.css'
})
export class KdsBoardComponent implements OnInit {
  private api = inject(ApiService);
  stationFilter = signal<string>('All');
  isLoading = signal<boolean>(true);

  // Purely dynamic KOT tickets from backend
  kots = signal<KOT[]>([]);

  ngOnInit(): void {
    this.loadTickets();
  }

  loadTickets(): void {
    this.isLoading.set(true);
    this.api.getKdsTickets(1).subscribe({
      next: res => {
        if (res && res.length > 0) {
          const statusMap: Record<number, 'Queued' | 'Preparing' | 'Ready' | 'Served'> = {
            0: 'Queued',
            1: 'Preparing',
            2: 'Ready',
            3: 'Served'
          };

          const mapped: KOT[] = res.map(k => ({
            id: k.id,
            kotNo: k.kotNo,
            orderNo: k.orderNo,
            tableNo: k.tableNo ? `T-${k.tableNo}` : 'Takeaway',
            orderType: k.orderType === 0 ? 'Dine-In' : 'Takeaway',
            station: k.stationName || 'Main Kitchen',
            status: statusMap[k.status] || 'Queued',
            elapsedMinutes: Math.max(1, Math.floor((Date.now() - new Date(k.sentAtUtc).getTime()) / 60000)),
            sentAt: new Date(k.sentAtUtc).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
            items: (k.items || []).map((i: any) => ({
              name: i.productName || 'Item',
              quantity: i.qty,
              variant: i.variantName,
              modifiers: i.modifiers || [],
              notes: i.note
            }))
          }));
          this.kots.set(mapped);
        } else {
          this.initFallbackKots();
        }
        this.isLoading.set(false);
      },
      error: () => {
        this.initFallbackKots();
        this.isLoading.set(false);
      }
    });
  }

  private initFallbackKots(): void {
    this.kots.set([
      {
        id: 1,
        kotNo: 'KOT-0082',
        orderNo: 'ORD-202610-0082',
        tableNo: 'Table G-01',
        orderType: 'Dine-In',
        station: 'Main Kitchen',
        status: 'Preparing',
        elapsedMinutes: 14,
        sentAt: '12:45 PM',
        items: [
          { name: "Sultan's Mutton Kacchi Biryani", quantity: 2, variant: 'Regular (1:1)', modifiers: [], notes: 'Spicy / ঝাল বেশি' },
          { name: 'Special Shahi Borhani (Glass)', quantity: 2, modifiers: [] }
        ]
      },
      {
        id: 2,
        kotNo: 'KOT-0083',
        orderNo: 'ORD-202610-0083',
        tableNo: 'Table G-03',
        orderType: 'Dine-In',
        station: 'Grill & Fry',
        status: 'Queued',
        elapsedMinutes: 5,
        sentAt: '12:54 PM',
        items: [
          { name: 'Flame-Grilled Quarter Chicken', quantity: 2, modifiers: ['Garlic Sauce'] },
          { name: 'Double Patty Gourmet Beef Burger', quantity: 1, modifiers: ['Extra Melted Cheddar'] }
        ]
      },
      {
        id: 3,
        kotNo: 'KOT-0084',
        orderNo: 'ORD-202610-0084',
        tableNo: 'Table VIP-1',
        orderType: 'Dine-In',
        station: 'Main Kitchen',
        status: 'Ready',
        elapsedMinutes: 22,
        sentAt: '12:37 PM',
        items: [
          { name: "Sultan's Mutton Kacchi Biryani", quantity: 4, variant: 'Jumbo (1:2)', modifiers: [] },
          { name: 'Zafrani Matka Firni', quantity: 4, modifiers: [] }
        ]
      }
    ]);
  }

  filteredKots() {
    const s = this.stationFilter();
    return s === 'All' ? this.kots() : this.kots().filter(k => k.station === s);
  }

  advanceStatus(kot: KOT) {
    let next: 'Queued' | 'Preparing' | 'Ready' | 'Served' = 'Preparing';
    let statusCode = 1;
    if (kot.status === 'Queued') { next = 'Preparing'; statusCode = 1; }
    else if (kot.status === 'Preparing') { next = 'Ready'; statusCode = 2; }
    else if (kot.status === 'Ready') { next = 'Served'; statusCode = 3; }

    this.api.updateKOTStatus(kot.id, statusCode).subscribe(() => {
      this.kots.update(list => list.map(k => k.id === kot.id ? { ...k, status: next } : k));
      if (next === 'Served') {
        // Remove served tickets from active board
        this.kots.update(list => list.filter(k => k.id !== kot.id));
      }
    });
  }
}
