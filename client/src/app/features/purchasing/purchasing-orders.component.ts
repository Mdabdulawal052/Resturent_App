import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { PurchaseOrder, Supplier, InventoryItem, PurchaseOrderLine, GoodsReceipt } from '../../models';

@Component({
  selector: 'app-purchasing-orders',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './purchasing-orders.component.html',
  styleUrl: './purchasing-orders.component.css'
})
export class PurchasingOrdersComponent implements OnInit {
  private api = inject(ApiService);

  // Core Data Signals
  purchaseOrders = signal<PurchaseOrder[]>([]);
  suppliers = signal<Supplier[]>([]);
  inventoryItems = signal<InventoryItem[]>([]);
  goodsReceipts = signal<GoodsReceipt[]>([]);
  isLoading = signal<boolean>(true);

  // UI State Signals
  activeView = signal<'list' | 'kanban'>('list');
  activeFilter = signal<'all' | 'Draft' | 'Approved' | 'Received'>('all');
  searchQuery = signal<string>('');
  toastMessage = signal<string | null>(null);

  // Modals & Panels
  selectedPO = signal<PurchaseOrder | null>(null);
  showNewPOModal = signal<boolean>(false);
  showReceiveModal = signal<boolean>(false);
  showSupplierModal = signal<boolean>(false);

  // New PO Form State
  newPOSupplierId = signal<number>(1);
  newPOLines = signal<{
    inventoryItemId: number;
    itemName: string;
    unitCode: string;
    qty: number;
    unitCost: number;
    taxAmount: number;
    lineTotal: number;
  }[]>([
    { inventoryItemId: 1, itemName: 'Premium Basmati Rice', unitCode: 'KG', qty: 100, unitCost: 140, taxAmount: 0, lineTotal: 14000 }
  ]);

  // New Supplier Form State
  newSupName = signal<string>('');
  newSupPhone = signal<string>('');
  newSupAddress = signal<string>('');
  newSupEmail = signal<string>('');

  // Receive Goods Form State
  grnBatchNo = signal<string>('BATCH-' + new Date().toISOString().slice(5, 10).replace('-', ''));
  grnChallanNo = signal<string>('CHALLAN-' + Math.floor(10000 + Math.random() * 90000));

  // Computed Totals & Filters
  filteredPOs = computed(() => {
    let list = this.purchaseOrders();
    const filter = this.activeFilter();
    const query = this.searchQuery().toLowerCase().trim();

    if (filter !== 'all') {
      list = list.filter(po => po.status === filter);
    }

    if (query) {
      list = list.filter(po =>
        po.poNo.toLowerCase().includes(query) ||
        po.supplierName.toLowerCase().includes(query)
      );
    }

    return list;
  });

  totalPOValuation = computed(() => {
    return this.purchaseOrders().reduce((sum, po) => sum + po.total, 0);
  });

  pendingReceiptsCount = computed(() => {
    return this.purchaseOrders().filter(po => po.status === 'Approved' || po.status === 'Draft').length;
  });

  newPOSubtotal = computed(() => {
    return this.newPOLines().reduce((sum, l) => sum + (l.qty * l.unitCost), 0);
  });

  newPOTaxTotal = computed(() => {
    return this.newPOLines().reduce((sum, l) => sum + l.taxAmount, 0);
  });

  newPOGrandTotal = computed(() => {
    return this.newPOSubtotal() + this.newPOTaxTotal();
  });

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);

    this.api.getSuppliers(1).subscribe(res => {
      if (res && res.length) this.suppliers.set(res);
    });

    this.api.getInventoryItems(1).subscribe(res => {
      if (res && res.length) {
        this.inventoryItems.set(res.map(i => ({
          id: i.id,
          name: i.name,
          unit: i.baseUnitCode || 'KG',
          currentStock: i.stockQuantity || 150,
          reorderLevel: i.reorderLevel || 40,
          avgCost: i.averageCost || 120
        })));
      }
    });

    this.api.getPurchaseOrders(1).subscribe(res => {
      if (res) this.purchaseOrders.set(res);
      this.isLoading.set(false);
    });

    this.api.getGoodsReceipts(1).subscribe(res => {
      if (res) this.goodsReceipts.set(res);
    });
  }

  showToast(msg: string): void {
    this.toastMessage.set(msg);
    setTimeout(() => this.toastMessage.set(null), 3500);
  }

  // --- Purchase Order Actions ---
  openCreatePOModal(): void {
    if (this.inventoryItems().length > 0 && this.newPOLines().length === 0) {
      const first = this.inventoryItems()[0];
      this.newPOLines.set([{
        inventoryItemId: first.id,
        itemName: first.name,
        unitCode: first.unit,
        qty: 50,
        unitCost: first.avgCost,
        taxAmount: 0,
        lineTotal: 50 * first.avgCost
      }]);
    }
    this.showNewPOModal.set(true);
  }

  addPOLine(): void {
    const defaultItem = this.inventoryItems()[0] || { id: 1, name: 'Raw Produce', unit: 'KG', avgCost: 100 };
    const current = this.newPOLines();
    this.newPOLines.set([
      ...current,
      {
        inventoryItemId: defaultItem.id,
        itemName: defaultItem.name,
        unitCode: defaultItem.unit,
        qty: 20,
        unitCost: defaultItem.avgCost,
        taxAmount: 0,
        lineTotal: 20 * defaultItem.avgCost
      }
    ]);
  }

  removePOLine(index: number): void {
    const lines = [...this.newPOLines()];
    if (lines.length > 1) {
      lines.splice(index, 1);
      this.newPOLines.set(lines);
    }
  }

  onItemSelectionChange(index: number, itemId: number): void {
    const item = this.inventoryItems().find(i => i.id == itemId);
    if (!item) return;

    const lines = [...this.newPOLines()];
    lines[index].inventoryItemId = item.id;
    lines[index].itemName = item.name;
    lines[index].unitCode = item.unit;
    lines[index].unitCost = item.avgCost;
    lines[index].lineTotal = lines[index].qty * item.avgCost + lines[index].taxAmount;
    this.newPOLines.set(lines);
  }

  onLineQtyOrCostChange(index: number): void {
    const lines = [...this.newPOLines()];
    const line = lines[index];
    line.lineTotal = (line.qty * line.unitCost) + line.taxAmount;
    this.newPOLines.set(lines);
  }

  submitCreatePO(): void {
    const supplierId = this.newPOSupplierId();
    const sup = this.suppliers().find(s => s.id == supplierId);
    const supplierName = sup ? sup.name : 'Supplier';

    const req = {
      branchId: 1,
      supplierId: supplierId,
      lines: this.newPOLines().map(l => ({
        inventoryItemId: l.inventoryItemId,
        qty: l.qty,
        unitCost: l.unitCost,
        taxAmount: l.taxAmount
      }))
    };

    this.api.createPurchaseOrder(req).subscribe({
      next: (created) => {
        const newPO: PurchaseOrder = {
          id: created.id || Date.now(),
          branchId: 1,
          supplierId: supplierId,
          supplierName: supplierName,
          poNo: created.poNo || `PO-${new Date().getFullYear()}${String(new Date().getMonth() + 1).padStart(2, '0')}-${Math.floor(1000 + Math.random() * 9000)}`,
          poDateUtc: new Date().toISOString(),
          status: 'Draft',
          total: this.newPOGrandTotal(),
          linesCount: this.newPOLines().length,
          lines: this.newPOLines().map((l, idx) => ({
            id: idx + 1,
            inventoryItemId: l.inventoryItemId,
            itemName: l.itemName,
            unitCode: l.unitCode,
            qty: l.qty,
            unitCost: l.unitCost,
            taxAmount: l.taxAmount,
            lineTotal: l.lineTotal
          }))
        };

        this.purchaseOrders.set([newPO, ...this.purchaseOrders()]);
        this.showNewPOModal.set(false);
        this.showToast(`Purchase Order ${newPO.poNo} created in Draft.`);
        this.viewPODetails(newPO);
      }
    });
  }

  viewPODetails(po: PurchaseOrder): void {
    // If lines not loaded yet, populate default lines from inventory
    if (!po.lines || po.lines.length === 0) {
      po.lines = [
        {
          id: 1,
          inventoryItemId: 1,
          itemName: 'Premium Basmati / Sourced Ingredient',
          unitCode: 'KG',
          qty: 100,
          unitCost: po.total / 100,
          taxAmount: 0,
          lineTotal: po.total
        }
      ];
    }
    this.selectedPO.set(po);
  }

  closePODetails(): void {
    this.selectedPO.set(null);
  }

  confirmOrder(po: PurchaseOrder): void {
    po.status = 'Approved';
    this.api.updatePOStatus(po.id, 2).subscribe();
    this.showToast(`PO ${po.poNo} confirmed and sent to supplier!`);
  }

  openReceiveGoodsModal(po: PurchaseOrder): void {
    this.selectedPO.set(po);
    this.showReceiveModal.set(true);
  }

  submitReceiveGoods(): void {
    const po = this.selectedPO();
    if (!po) return;

    const req = {
      branchId: 1,
      supplierId: po.supplierId,
      purchaseOrderId: po.id,
      lines: (po.lines || []).map(l => ({
        inventoryItemId: l.inventoryItemId,
        qty: l.qty,
        unitCost: l.unitCost,
        batchNo: this.grnBatchNo(),
        expiryDate: new Date(Date.now() + 90 * 86400000).toISOString()
      }))
    };

    this.api.receiveGoods(req).subscribe({
      next: (grn) => {
        po.status = 'Received';
        this.showReceiveModal.set(false);
        this.showToast(`Goods received! Challan ${this.grnChallanNo()} posted. Stock updated.`);
        this.loadData();
      }
    });
  }

  // --- Supplier Directory Actions ---
  openSupplierModal(): void {
    this.showSupplierModal.set(true);
  }

  submitCreateSupplier(): void {
    if (!this.newSupName()) return;

    const req = {
      code: `SUP-${Math.floor(100 + Math.random() * 900)}`,
      name: this.newSupName(),
      phone: this.newSupPhone() || '+8801700000000',
      address: this.newSupAddress() || 'Karwan Bazar, Dhaka',
      email: this.newSupEmail(),
      openingBalance: 0
    };

    this.api.createSupplier(req).subscribe(created => {
      this.suppliers.set([...this.suppliers(), created]);
      this.newSupName.set('');
      this.newSupPhone.set('');
      this.newSupAddress.set('');
      this.newSupEmail.set('');
      this.showToast(`Supplier ${created.name} added successfully!`);
    });
  }

  printPO(po: PurchaseOrder): void {
    window.print();
  }
}
