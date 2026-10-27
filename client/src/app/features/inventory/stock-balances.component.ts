import { Component, signal, computed, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { InventoryItem, StockMovement } from '../../models';
import { ApiService } from '../../core/services/api.service';

@Component({
  selector: 'app-stock-balances',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './stock-balances.component.html',
  styleUrl: './stock-balances.component.css'
})
export class StockBalancesComponent implements OnInit {
  private api = inject(ApiService);
  private router = inject(Router);

  inventoryItems = signal<InventoryItem[]>([]);
  stockLedger = signal<StockMovement[]>([]);
  isLoading = signal<boolean>(true);

  // Filter & Search Signals
  activeView = signal<'list' | 'kanban'>('list');
  activeTab = signal<'balances' | 'ledger'>('balances');
  selectedCategory = signal<string>('All');
  onlyLowStock = signal<boolean>(false);
  searchQuery = signal<string>('');
  toastMessage = signal<string | null>(null);

  // Stock Adjustment Modal
  showAdjustmentModal = signal<boolean>(false);
  adjustingItem = signal<InventoryItem | null>(null);
  adjustmentCount = signal<number>(0);
  adjustmentReason = signal<string>('Physical Count Correction');

  categories = ['All', 'Meat & Poultry', 'Rice & Grains', 'Dairy & Spices', 'Bakery & Buns'];

  // Computed Values
  filteredItems = computed(() => {
    let list = this.inventoryItems();
    const cat = this.selectedCategory();
    const lowOnly = this.onlyLowStock();
    const query = this.searchQuery().toLowerCase().trim();

    if (cat !== 'All') {
      list = list.filter(i => (i.category || 'Meat & Poultry') === cat);
    }

    if (lowOnly) {
      list = list.filter(i => i.currentStock <= i.reorderLevel);
    }

    if (query) {
      list = list.filter(i => i.name.toLowerCase().includes(query));
    }

    return list;
  });

  totalValuation = computed(() => {
    return this.inventoryItems().reduce((sum, i) => sum + (i.currentStock * i.avgCost), 0);
  });

  lowStockCount = computed(() => {
    return this.inventoryItems().filter(i => i.currentStock <= i.reorderLevel).length;
  });

  healthyStockCount = computed(() => {
    return this.inventoryItems().filter(i => i.currentStock > i.reorderLevel).length;
  });

  ngOnInit(): void {
    this.loadInventory();
    this.initLedgerMock();
  }

  showToast(msg: string): void {
    this.toastMessage.set(msg);
    setTimeout(() => this.toastMessage.set(null), 3500);
  }

  loadInventory(): void {
    this.isLoading.set(true);
    this.api.getInventoryItems(1).subscribe({
      next: res => {
        if (res && res.length > 0) {
          const catMap: Record<string, string> = {
            'Basmati': 'Rice & Grains',
            'Rice': 'Rice & Grains',
            'Mutton': 'Meat & Poultry',
            'Beef': 'Meat & Poultry',
            'Patty': 'Meat & Poultry',
            'Chicken': 'Meat & Poultry',
            'Bun': 'Bakery & Buns',
            'Bread': 'Bakery & Buns',
            'Cheese': 'Dairy & Spices',
            'Yogurt': 'Dairy & Spices',
            'Ghee': 'Dairy & Spices'
          };

          this.inventoryItems.set(res.map(i => {
            let cat = 'Rice & Grains';
            for (const key of Object.keys(catMap)) {
              if (i.name.includes(key)) {
                cat = catMap[key];
                break;
              }
            }

            return {
              id: i.id,
              name: i.name,
              unit: i.baseUnitCode || 'KG',
              currentStock: i.stockQuantity || 120,
              reorderLevel: i.reorderLevel || 30,
              avgCost: i.averageCost || 120,
              category: cat
            };
          }));
        } else {
          // Provide default rich BD ingredients
          this.inventoryItems.set([
            { id: 1, name: 'Premium Basmati Polao Rice', unit: 'KG', currentStock: 250, reorderLevel: 50, avgCost: 140, category: 'Rice & Grains' },
            { id: 2, name: 'Fresh Baby Mutton (Kacchi Cut)', unit: 'KG', currentStock: 22, reorderLevel: 30, avgCost: 950, category: 'Meat & Poultry' },
            { id: 3, name: 'Prime Beef Patty 150g', unit: 'PCS', currentStock: 300, reorderLevel: 100, avgCost: 75, category: 'Meat & Poultry' },
            { id: 4, name: 'Brioche Gourmet Burger Buns', unit: 'PCS', currentStock: 45, reorderLevel: 100, avgCost: 25, category: 'Bakery & Buns' },
            { id: 5, name: 'Imported Cheddar Cheese Slices', unit: 'PCS', currentStock: 200, reorderLevel: 50, avgCost: 18, category: 'Dairy & Spices' },
            { id: 6, name: 'Pure Baghabari Cow Ghee', unit: 'KG', currentStock: 18, reorderLevel: 25, avgCost: 1200, category: 'Dairy & Spices' },
            { id: 7, name: 'Shahi Borhani Roasted Spices & Mint', unit: 'KG', currentStock: 15, reorderLevel: 10, avgCost: 450, category: 'Dairy & Spices' }
          ]);
        }
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  initLedgerMock(): void {
    this.stockLedger.set([
      { id: 101, itemName: 'Fresh Baby Mutton (Kacchi Cut)', txnType: 'PO Receipt', referenceNo: 'GRN-202610-0001', qtyChange: 120, unitCost: 950, balanceAfter: 142, timestamp: 'Today, 08:30 AM' },
      { id: 102, itemName: 'Fresh Baby Mutton (Kacchi Cut)', txnType: 'Order Consumption', referenceNo: 'ORD-202610-0082', qtyChange: -20, unitCost: 950, balanceAfter: 122, timestamp: 'Today, 01:15 PM' },
      { id: 103, itemName: 'Premium Basmati Polao Rice', txnType: 'PO Receipt', referenceNo: 'GRN-202610-0002', qtyChange: 250, unitCost: 140, balanceAfter: 250, timestamp: 'Yesterday, 11:00 AM' },
      { id: 104, itemName: 'Brioche Gourmet Burger Buns', txnType: 'Order Consumption', referenceNo: 'ORD-202610-0078', qtyChange: -55, unitCost: 25, balanceAfter: 45, timestamp: 'Yesterday, 09:40 PM' },
      { id: 105, itemName: 'Brioche Gourmet Burger Buns', txnType: 'Wastage', referenceNo: 'WST-004', qtyChange: -5, unitCost: 25, balanceAfter: 40, timestamp: 'Yesterday, 10:15 PM' }
    ]);
  }

  openAdjustment(item: InventoryItem): void {
    this.adjustingItem.set(item);
    this.adjustmentCount.set(item.currentStock);
    this.showAdjustmentModal.set(true);
  }

  saveAdjustment(): void {
    const item = this.adjustingItem();
    if (!item) return;

    const newQty = this.adjustmentCount();
    const diff = newQty - item.currentStock;

    item.currentStock = newQty;

    // Record in ledger
    const newEntry: StockMovement = {
      id: Date.now(),
      itemName: item.name,
      txnType: diff < 0 && this.adjustmentReason().includes('Spoilage') ? 'Wastage' : 'Manual Adjustment',
      referenceNo: `ADJ-${Math.floor(1000 + Math.random() * 9000)}`,
      qtyChange: diff,
      unitCost: item.avgCost,
      balanceAfter: newQty,
      timestamp: 'Just now'
    };

    this.stockLedger.set([newEntry, ...this.stockLedger()]);
    this.showAdjustmentModal.set(false);
    this.showToast(`Stock updated for ${item.name} to ${newQty} ${item.unit}.`);
  }

  reorderItem(item: InventoryItem): void {
    this.router.navigate(['/purchasing']);
  }
}
