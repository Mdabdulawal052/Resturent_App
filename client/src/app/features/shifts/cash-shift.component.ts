import { Component, signal, computed, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';

interface CashDenomination {
  note: number;
  label: string;
  count: number;
  subtotal: number;
}

@Component({
  selector: 'app-cash-shift',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './cash-shift.component.html',
  styleUrl: './cash-shift.component.css'
})
export class CashShiftComponent implements OnInit {
  private api = inject(ApiService);

  currentShiftId = signal<number>(1);
  cashierName = signal<string>('Kamrul Hasan (Counter 01)');
  shiftFloat = signal<number>(3000);
  todayCashSales = signal<number>(48900);
  pettyCashOut = signal<number>(1250);
  cashIn = signal<number>(500);
  isLoading = signal<boolean>(true);
  toastMessage = signal<string | null>(null);

  // Bangladesh Banknote Denominations
  denominations = signal<CashDenomination[]>([
    { note: 1000, label: '৳1,000 Note', count: 42, subtotal: 42000 },
    { note: 500, label: '৳500 Note', count: 12, subtotal: 6000 },
    { note: 200, label: '৳200 Note', count: 8, subtotal: 1600 },
    { note: 100, label: '৳100 Note', count: 11, subtotal: 1100 },
    { note: 50, label: '৳50 Note', count: 6, subtotal: 300 },
    { note: 20, label: '৳20 Note', count: 5, subtotal: 100 },
    { note: 10, label: '৳10 Note', count: 5, subtotal: 50 }
  ]);

  // Petty Cash / Bazaar Out Form
  showBazaarModal = signal<boolean>(false);
  bazaarAmount = signal<number>(350);
  bazaarReason = signal<string>('Emergency Kitchen Ice & Mint leaves');
  bazaarVoucherNo = signal<string>('VOUCH-' + Math.floor(1000 + Math.random() * 9000));

  // Z-Report Print Modal
  showZReportModal = signal<boolean>(false);
  shiftClosedTime = signal<string | null>(null);

  // Computations
  totalDenomCash = computed(() => {
    return this.denominations().reduce((sum, d) => sum + (d.count * d.note), 0);
  });

  expectedCash = computed(() => {
    return this.shiftFloat() + this.todayCashSales() + this.cashIn() - this.pettyCashOut();
  });

  variance = computed(() => {
    return this.totalDenomCash() - this.expectedCash();
  });

  ngOnInit(): void {
    this.loadShiftData();
  }

  showToast(msg: string): void {
    this.toastMessage.set(msg);
    setTimeout(() => this.toastMessage.set(null), 3500);
  }

  loadShiftData(): void {
    this.isLoading.set(true);
    this.api.openShift({
      branchId: 1,
      terminalId: 1,
      openingCash: 3000
    }).subscribe({
      next: res => {
        if (res && res.id) {
          this.currentShiftId.set(res.id);
          this.shiftFloat.set(res.openingCash || 3000);
        }
        this.api.getSalesSummary(1).subscribe(sales => {
          if (sales && sales.cashPayments > 0) {
            this.todayCashSales.set(sales.cashPayments);
          }
          this.isLoading.set(false);
        });
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  updateDenominationCount(index: number, count: number): void {
    const list = [...this.denominations()];
    const item = list[index];
    item.count = Math.max(0, count);
    item.subtotal = item.count * item.note;
    this.denominations.set(list);
  }

  openBazaarExpenseModal(): void {
    this.bazaarVoucherNo.set('VOUCH-' + Math.floor(1000 + Math.random() * 9000));
    this.showBazaarModal.set(true);
  }

  submitBazaarExpense(): void {
    const amt = this.bazaarAmount();
    if (amt <= 0) return;

    this.pettyCashOut.set(this.pettyCashOut() + amt);
    this.showBazaarModal.set(false);
    this.showToast(`Petty cash voucher ${this.bazaarVoucherNo()} recorded: -৳${amt} for ${this.bazaarReason()}`);
  }

  onCloseShift(): void {
    const denomMap = this.denominations().reduce((acc, d) => {
      acc[d.note] = d.count;
      return acc;
    }, {} as Record<number, number>);

    const req = {
      shiftId: this.currentShiftId(),
      closingCash: this.totalDenomCash(),
      notes: 'End of shift drawer reconciliation with BD denomination note counter',
      count1000: denomMap[1000] || 0,
      count500: denomMap[500] || 0,
      count200: denomMap[200] || 0,
      count100: denomMap[100] || 0,
      count50: denomMap[50] || 0,
      count20: denomMap[20] || 0,
      count10: denomMap[10] || 0,
      declaredCash: this.totalDenomCash()
    };

    this.api.closeShift(req).subscribe(() => {
      this.shiftClosedTime.set(new Date().toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' }));
      this.showZReportModal.set(true);
    });
  }

  printZReport(): void {
    window.print();
  }
}
