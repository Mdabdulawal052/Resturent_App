import { Component, signal, computed, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';

@Component({
  selector: 'app-sales-reports',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './sales-reports.component.html',
  styleUrl: './sales-reports.component.css'
})
export class SalesReportsComponent implements OnInit {
  private api = inject(ApiService);

  isLoading = signal<boolean>(true);
  activeTab = signal<'sales' | 'vat' | 'payments' | 'foodcost' | 'procurement'>('sales');
  selectedPeriod = signal<'today' | 'week' | 'month'>('today');

  // KPI Signals
  grossRevenue = signal<number>(148500);
  netSales = signal<number>(137250);
  totalOrders = signal<number>(184);
  vatCollected = signal<number>(8250);
  serviceCharge = signal<number>(3000);
  averageTicket = computed(() => {
    return this.totalOrders() > 0 ? Math.round(this.grossRevenue() / this.totalOrders()) : 0;
  });

  // Payment Breakdown (Bangladesh Market)
  paymentsBreakdown = signal<{ method: string; icon: string; color: string; amount: number; percentage: number; trxCount: number }[]>([
    { method: 'bKash Merchant', icon: 'fa-mobile-screen', color: '#e2136e', amount: 56400, percentage: 38, trxCount: 68 },
    { method: 'Cash (BDT)', icon: 'fa-money-bill-1-wave', color: '#10b981', amount: 48900, percentage: 33, trxCount: 62 },
    { method: 'Debit / Credit Card (POS)', icon: 'fa-credit-card', color: '#3b82f6', amount: 28200, percentage: 19, trxCount: 34 },
    { method: 'Nagad', icon: 'fa-wallet', color: '#f97316', amount: 15000, percentage: 10, trxCount: 20 }
  ]);

  // NBR Mushak 6.3 Records
  vatRecords = signal<{
    category: string;
    rate: string;
    taxableBase: number;
    vatPayable: number;
    rebate: number;
    netPayable: number;
  }[]>([
    { category: 'Restaurant AC Dining (Mushak 6.3)', rate: '7.5%', taxableBase: 92000, vatPayable: 6900, rebate: 0, netPayable: 6900 },
    { category: 'Takeaway & Fast Counter (Mushak 6.3)', rate: '5.0%', taxableBase: 45250, vatPayable: 2262, rebate: 0, netPayable: 2262 },
    { category: 'Catering & External Delivery', rate: '15.0%', taxableBase: 11250, vatPayable: 1687, rebate: 0, netPayable: 1687 }
  ]);

  totalVatPayable = computed(() => {
    return this.vatRecords().reduce((sum, r) => sum + r.netPayable, 0);
  });

  // Food Cost & Recipe Margin Analysis
  menuCosting = signal<{
    name: string;
    category: string;
    unitsSold: number;
    revenue: number;
    recipeCost: number;
    marginPercent: number;
  }[]>([
    { name: "Sultan's Mutton Kacchi Biryani", category: 'Kacchi & Biryani', unitsSold: 84, revenue: 37800, recipeCost: 220, marginPercent: 51 },
    { name: 'Double Patty Gourmet Beef Burger', category: 'Burgers', unitsSold: 52, revenue: 18720, recipeCost: 165, marginPercent: 54 },
    { name: 'Flame-Grilled Quarter Chicken', category: 'Grill & BBQ', unitsSold: 46, revenue: 10120, recipeCost: 95, marginPercent: 57 },
    { name: 'Special Shahi Borhani (Glass)', category: 'Beverages', unitsSold: 110, revenue: 9900, recipeCost: 30, marginPercent: 67 },
    { name: 'Zafrani Matka Firni', category: 'Desserts', unitsSold: 38, revenue: 4560, recipeCost: 40, marginPercent: 67 }
  ]);

  // Procurement Spend Summary
  supplierSpend = signal<{
    supplier: string;
    hub: string;
    totalOrders: number;
    spendBDT: number;
    status: string;
  }[]>([
    { supplier: 'Bengal Meat Agro Ltd.', hub: 'Savar / Hazaribagh', totalOrders: 4, spendBDT: 185000, status: 'Active Verified' },
    { supplier: 'Moulvibazar Spices Syndicate', hub: 'Old Dhaka Wholesale', totalOrders: 6, spendBDT: 62400, status: 'Active Verified' },
    { supplier: 'Pran Dairy & Agro Supplies', hub: 'Tejgaon, Dhaka', totalOrders: 5, spendBDT: 48500, status: 'Active Verified' },
    { supplier: 'Karwan Bazar Fresh Produce Vendor', hub: 'Karwan Bazar', totalOrders: 8, spendBDT: 36200, status: 'Daily Sourcing' }
  ]);

  totalProcurementSpend = computed(() => {
    return this.supplierSpend().reduce((sum, s) => sum + s.spendBDT, 0);
  });

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);

    this.api.getSalesSummary(1).subscribe({
      next: res => {
        if (res && res.grossSales > 0) {
          this.grossRevenue.set(res.grossSales);
          this.totalOrders.set(res.totalOrders);
          this.vatCollected.set(res.totalTax);
          this.netSales.set(res.netSales || (res.grossSales - res.totalTax));
        }
      }
    });

    this.api.getVatReport(1).subscribe({
      next: res => {
        if (res && res.length > 0) {
          this.vatRecords.set(res.map(r => ({
            category: r.taxCategoryCode || 'Standard Restaurant (Mushak 6.3)',
            rate: `${r.vatRate}%`,
            taxableBase: r.taxableSalesBase,
            vatPayable: r.netVatPayable,
            rebate: 0,
            netPayable: r.netVatPayable
          })));
        }
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  printReport(): void {
    window.print();
  }

  exportCSV(): void {
    const csvContent = 'data:text/csv;charset=utf-8,Category,Rate,TaxableBase,VatPayable\n' +
      this.vatRecords().map(e => `${e.category},${e.rate},${e.taxableBase},${e.vatPayable}`).join('\n');
    const encodedUri = encodeURI(csvContent);
    const link = document.createElement('a');
    link.setAttribute('href', encodedUri);
    link.setAttribute('download', `NBR_Mushak_6.3_Report_${new Date().toISOString().slice(0, 10)}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }
}
