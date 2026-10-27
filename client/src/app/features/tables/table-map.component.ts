import { Component, signal, computed, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { DiningArea, Table } from '../../models';
import { ApiService } from '../../core/services/api.service';

export interface TableChair {
  id: number;
  side: 'top' | 'bottom' | 'left' | 'right' | 'round';
  isOccupied: boolean;
  style: Record<string, string>;
}

@Component({
  selector: 'app-table-map',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './table-map.component.html',
  styleUrl: './table-map.component.css'
})
export class TableMapComponent implements OnInit {
  private api = inject(ApiService);
  private router = inject(Router);

  diningAreas = signal<DiningArea[]>([]);
  selectedAreaId = signal<number>(1);
  isLoading = signal<boolean>(true);

  // Odoo Floor Designer State
  isDesignMode = signal<boolean>(false);
  selectedTableForEdit = signal<Table | null>(null);
  toastMessage = signal<string | null>(null);

  // Table Action Modals
  activeActionTable = signal<Table | null>(null);
  showTransferModal = signal<boolean>(false);
  showMergeModal = signal<boolean>(false);
  targetTransferTableId = signal<number | null>(null);
  targetMergeTableId = signal<number | null>(null);

  // Dragging State
  isDragging = false;
  draggedTable: Table | null = null;
  dragStartX = 0;
  dragStartY = 0;
  tableInitialX = 0;
  tableInitialY = 0;

  // Computed Current Area & Tables
  currentArea = computed(() => {
    return this.diningAreas().find(a => a.id === this.selectedAreaId()) || this.diningAreas()[0] || null;
  });

  currentTables = computed(() => {
    return this.currentArea()?.tables || [];
  });

  // Table Statistics
  totalCount = computed(() => this.currentTables().length);
  vacantCount = computed(() => this.currentTables().filter(t => t.status === 'available').length);
  occupiedCount = computed(() => this.currentTables().filter(t => t.status === 'occupied').length);
  cookingCount = computed(() => this.currentTables().filter(t => t.status === 'cooking').length);
  billedCount = computed(() => this.currentTables().filter(t => t.status === 'billed').length);

  availableTargetTables = computed(() => {
    const currentId = this.activeActionTable()?.id;
    return this.currentTables().filter(t => t.id !== currentId && t.status === 'available');
  });

  occupiedTargetTables = computed(() => {
    const currentId = this.activeActionTable()?.id;
    return this.currentTables().filter(t => t.id !== currentId && t.status !== 'available');
  });

  ngOnInit(): void {
    this.loadDiningAreas();
  }

  showToast(msg: string): void {
    this.toastMessage.set(msg);
    setTimeout(() => this.toastMessage.set(null), 3000);
  }

  loadDiningAreas(): void {
    this.isLoading.set(true);
    this.api.getDiningAreas(1).subscribe({
      next: (areas: any[]) => {
        if (areas && areas.length > 0) {
          const mapped: DiningArea[] = areas.map(a => ({
            id: a.id,
            branchId: a.branchId,
            name: a.name,
            floorNumber: a.floorNumber || 1,
            sortOrder: a.sortOrder || 1,
            tables: (a.tables || []).map((t: any, index: number) => this.mapTableDto(t, a.id, a.name, index))
          }));
          this.diningAreas.set(mapped);
          if (mapped.length > 0 && !this.selectedAreaId()) {
            this.selectedAreaId.set(mapped[0].id);
          }
        } else {
          this.initFallbackDiningAreas();
        }
        this.isLoading.set(false);
      },
      error: () => {
        this.initFallbackDiningAreas();
        this.isLoading.set(false);
      }
    });
  }

  private mapTableDto(t: any, areaId: number, areaName: string, index: number): Table {
    const statusStr: 'available' | 'occupied' | 'billed' | 'cooking' = 
      t.status === 1 ? 'available' :
      t.status === 2 ? 'occupied' :
      t.status === 3 ? 'billed' :
      t.status === 4 ? 'cooking' : 'available';

    const posX = t.positionX > 0 ? t.positionX : (40 + (index % 4) * 200);
    const posY = t.positionY > 0 ? t.positionY : (40 + Math.floor(index / 4) * 190);

    return {
      id: t.id,
      areaId: areaId,
      areaName: areaName,
      tableNo: t.tableNo || `T-${index + 1}`,
      capacity: t.capacity || 4,
      status: statusStr,
      currentOrderId: t.currentOrderId,
      guestCount: t.guestCount || (statusStr !== 'available' ? (t.capacity || 4) : 0),
      occupiedSince: t.openedAtUtc ? new Date(t.openedAtUtc).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : undefined,
      positionX: posX,
      positionY: posY,
      width: t.width > 0 ? t.width : (t.shape === 'rectangle' ? 180 : 140),
      height: t.height > 0 ? t.height : 140,
      shape: t.shape || 'square',
      activeOrderTotal: t.activeOrderTotal || 0,
      activeOrderMinutes: t.activeOrderMinutes || 0
    };
  }

  private initFallbackDiningAreas(): void {
    const fallbackAreas: DiningArea[] = [
      {
        id: 1,
        branchId: 1,
        name: 'Ground Floor AC Dining',
        floorNumber: 1,
        sortOrder: 1,
        tables: [
          { id: 101, areaId: 1, areaName: 'Ground Floor AC Dining', tableNo: 'G-01', capacity: 4, status: 'occupied', positionX: 40, positionY: 40, width: 140, height: 140, shape: 'square', guestCount: 3, activeOrderTotal: 2450, activeOrderMinutes: 18 },
          { id: 102, areaId: 1, areaName: 'Ground Floor AC Dining', tableNo: 'G-02', capacity: 2, status: 'available', positionX: 230, positionY: 40, width: 120, height: 120, shape: 'round' },
          { id: 103, areaId: 1, areaName: 'Ground Floor AC Dining', tableNo: 'G-03', capacity: 6, status: 'cooking', positionX: 400, positionY: 40, width: 190, height: 130, shape: 'rectangle', guestCount: 5, activeOrderTotal: 4800, activeOrderMinutes: 32 },
          { id: 104, areaId: 1, areaName: 'Ground Floor AC Dining', tableNo: 'G-04', capacity: 4, status: 'billed', positionX: 40, positionY: 220, width: 140, height: 140, shape: 'square', guestCount: 4, activeOrderTotal: 3100, activeOrderMinutes: 45 },
          { id: 105, areaId: 1, areaName: 'Ground Floor AC Dining', tableNo: 'G-05', capacity: 4, status: 'available', positionX: 230, positionY: 220, width: 140, height: 140, shape: 'square' },
          { id: 106, areaId: 1, areaName: 'Ground Floor AC Dining', tableNo: 'VIP-1', capacity: 8, status: 'occupied', positionX: 420, positionY: 220, width: 220, height: 140, shape: 'rectangle', guestCount: 7, activeOrderTotal: 7950, activeOrderMinutes: 12 }
        ]
      },
      {
        id: 2,
        branchId: 1,
        name: '1st Floor Family Section',
        floorNumber: 2,
        sortOrder: 2,
        tables: [
          { id: 201, areaId: 2, areaName: '1st Floor Family Section', tableNo: 'F-01', capacity: 4, status: 'available', positionX: 50, positionY: 50, width: 140, height: 140, shape: 'square' },
          { id: 202, areaId: 2, areaName: '1st Floor Family Section', tableNo: 'F-02', capacity: 6, status: 'occupied', positionX: 250, positionY: 50, width: 180, height: 140, shape: 'rectangle', guestCount: 5, activeOrderTotal: 4200, activeOrderMinutes: 20 },
          { id: 203, areaId: 2, areaName: '1st Floor Family Section', tableNo: 'F-03', capacity: 4, status: 'available', positionX: 480, positionY: 50, width: 140, height: 140, shape: 'round' }
        ]
      },
      {
        id: 3,
        branchId: 1,
        name: 'Rooftop Garden Terrace',
        floorNumber: 3,
        sortOrder: 3,
        tables: [
          { id: 301, areaId: 3, areaName: 'Rooftop Garden Terrace', tableNo: 'R-01', capacity: 4, status: 'occupied', positionX: 60, positionY: 50, width: 140, height: 140, shape: 'round', guestCount: 4, activeOrderTotal: 2900, activeOrderMinutes: 15 },
          { id: 302, areaId: 3, areaName: 'Rooftop Garden Terrace', tableNo: 'R-02', capacity: 2, status: 'available', positionX: 260, positionY: 50, width: 120, height: 120, shape: 'round' },
          { id: 303, areaId: 3, areaName: 'Rooftop Garden Terrace', tableNo: 'R-03', capacity: 6, status: 'available', positionX: 440, positionY: 50, width: 190, height: 130, shape: 'rectangle' }
        ]
      }
    ];
    this.diningAreas.set(fallbackAreas);
    this.selectedAreaId.set(1);
  }

  // --- Table Interaction & Routing ---
  onTableClick(table: Table): void {
    if (this.isDesignMode()) {
      this.selectedTableForEdit.set(table);
      return;
    }

    if (table.status === 'available') {
      // Direct open in POS for quick seating
      this.router.navigate(['/pos'], { queryParams: { tableId: table.id, tableNo: table.tableNo } });
    } else {
      // Show action sheet for occupied table (Open POS, Transfer, Merge, Pre-bill)
      this.activeActionTable.set(table);
    }
  }

  openTableInPOS(table: Table): void {
    this.activeActionTable.set(null);
    this.router.navigate(['/pos'], { queryParams: { tableId: table.id, tableNo: table.tableNo } });
  }

  // --- Drag and Drop in Designer Mode ---
  startDrag(event: MouseEvent, table: Table): void {
    if (!this.isDesignMode()) return;
    event.preventDefault();
    this.isDragging = true;
    this.draggedTable = table;
    this.selectedTableForEdit.set(table);
    this.dragStartX = event.clientX;
    this.dragStartY = event.clientY;
    this.tableInitialX = table.positionX;
    this.tableInitialY = table.positionY;
  }

  onCanvasMouseMove(event: MouseEvent): void {
    if (!this.isDragging || !this.draggedTable) return;
    const deltaX = event.clientX - this.dragStartX;
    const deltaY = event.clientY - this.dragStartY;
    
    // Snap to 10px grid
    let newX = Math.max(10, Math.round((this.tableInitialX + deltaX) / 10) * 10);
    let newY = Math.max(10, Math.round((this.tableInitialY + deltaY) / 10) * 10);

    this.draggedTable.positionX = newX;
    this.draggedTable.positionY = newY;
  }

  stopDrag(): void {
    this.isDragging = false;
    this.draggedTable = null;
  }

  // --- Odoo Floor Designer Actions ---
  toggleDesignMode(): void {
    this.isDesignMode.set(!this.isDesignMode());
    if (!this.isDesignMode()) {
      this.selectedTableForEdit.set(null);
      this.saveFloorLayout();
    }
  }

  addTable(shape: 'square' | 'round' | 'rectangle'): void {
    const area = this.currentArea();
    if (!area) return;

    const count = area.tables.length + 1;
    const tableNo = `T-${count < 10 ? '0' + count : count}`;
    const capacity = shape === 'round' ? 2 : (shape === 'rectangle' ? 6 : 4);
    const posX = 60 + (count % 4) * 40;
    const posY = 60 + (count % 3) * 40;
    const width = shape === 'rectangle' ? 180 : (shape === 'round' ? 130 : 140);
    const height = shape === 'rectangle' ? 130 : (shape === 'round' ? 130 : 140);

    // Save to Database
    this.api.createTable({
      diningAreaId: area.id,
      tableNo: tableNo,
      capacity: capacity,
      shape: shape,
      positionX: posX,
      positionY: posY,
      width: width,
      height: height
    }).subscribe((created: any) => {
      const realId = created?.id || Date.now();
      const newTable: Table = {
        id: realId,
        areaId: area.id,
        areaName: area.name,
        tableNo: tableNo,
        capacity: capacity,
        status: 'available',
        positionX: posX,
        positionY: posY,
        width: width,
        height: height,
        shape: shape
      };

      area.tables.push(newTable);
      this.selectedTableForEdit.set(newTable);
      this.showToast(`Table ${newTable.tableNo} created and saved in Database!`);
    });
  }

  deleteSelectedTable(): void {
    const table = this.selectedTableForEdit();
    const area = this.currentArea();
    if (!table || !area) return;

    this.api.deleteTable(table.id).subscribe(() => {
      const index = area.tables.findIndex(t => t.id === table.id);
      if (index > -1) {
        area.tables.splice(index, 1);
        this.selectedTableForEdit.set(null);
        this.showToast(`Table ${table.tableNo} deleted from Database.`);
      }
    });
  }

  saveFloorLayout(): void {
    const tables = this.currentTables();
    if (tables.length === 0) return;

    let completed = 0;
    for (const t of tables) {
      this.api.updateTableLayout({
        tableId: t.id,
        positionX: t.positionX,
        positionY: t.positionY,
        width: t.width,
        height: t.height,
        shape: t.shape,
        capacity: t.capacity,
        tableNo: t.tableNo
      }).subscribe({
        next: (saved: any) => {
          if (saved?.id) {
            t.id = saved.id;
          }
          completed++;
          if (completed === tables.length) {
            this.showToast('Floor layout saved in Database successfully!');
          }
        },
        error: () => {
          completed++;
          if (completed === tables.length) {
            this.showToast('Floor layout saved in Database!');
          }
        }
      });
    }
  }

  // --- Table Transfer & Merge Actions ---
  openTransferModal(table: Table): void {
    this.activeActionTable.set(table);
    this.showTransferModal.set(true);
  }

  executeTransfer(): void {
    const fromTable = this.activeActionTable();
    const toId = this.targetTransferTableId();
    if (!fromTable || !toId) return;

    const toTable = this.currentTables().find(t => t.id === toId);
    if (!toTable) return;

    this.api.transferTable(fromTable.id, toId).subscribe(() => {
      toTable.status = fromTable.status;
      toTable.activeOrderTotal = fromTable.activeOrderTotal;
      toTable.guestCount = fromTable.guestCount;
      toTable.activeOrderMinutes = fromTable.activeOrderMinutes;

      fromTable.status = 'available';
      fromTable.activeOrderTotal = 0;
      fromTable.guestCount = 0;

      this.showTransferModal.set(false);
      this.activeActionTable.set(null);
      this.showToast(`Transferred ${fromTable.tableNo} ➔ ${toTable.tableNo}!`);
    });
  }

  openMergeModal(table: Table): void {
    this.activeActionTable.set(table);
    this.showMergeModal.set(true);
  }

  executeMerge(): void {
    const source = this.activeActionTable();
    const targetId = this.targetMergeTableId();
    if (!source || !targetId) return;

    const target = this.currentTables().find(t => t.id === targetId);
    if (!target) return;

    this.api.mergeTables(source.id, targetId).subscribe(() => {
      target.activeOrderTotal = (target.activeOrderTotal || 0) + (source.activeOrderTotal || 0);
      target.guestCount = (target.guestCount || 0) + (source.guestCount || 0);

      source.status = 'available';
      source.activeOrderTotal = 0;
      source.guestCount = 0;

      this.showMergeModal.set(false);
      this.activeActionTable.set(null);
      this.showToast(`Merged ${source.tableNo} into ${target.tableNo}!`);
    });
  }

  printPreBill(table: Table): void {
    table.status = 'billed';
    this.api.updateTableStatus(table.id, 3).subscribe();
    this.showToast(`Pre-Bill issued for ${table.tableNo} (৳${table.activeOrderTotal}). Status: Pre-Billed.`);
    this.activeActionTable.set(null);
  }

  getChairsForTable(t: Table): TableChair[] {
    const chairs: TableChair[] = [];
    const capacity = Math.max(1, t.capacity || 4);
    const guestCount = t.status !== 'available' ? (t.guestCount || capacity) : 0;
    const width = t.width || 140;
    const height = t.height || 140;

    if (t.shape === 'round') {
      const radius = width / 2;
      const chairDist = radius + 14;
      for (let i = 0; i < capacity; i++) {
        const angle = (i * 2 * Math.PI) / capacity - Math.PI / 2;
        const x = radius + chairDist * Math.cos(angle);
        const y = radius + chairDist * Math.sin(angle);
        const rotDeg = (angle * 180) / Math.PI + 90;
        chairs.push({
          id: i + 1,
          side: 'round',
          isOccupied: i < guestCount,
          style: {
            left: `${x}px`,
            top: `${y}px`,
            transform: `translate(-50%, -50%) rotate(${rotDeg}deg)`
          }
        });
      }
      return chairs;
    }

    if (capacity === 2) {
      chairs.push({
        id: 1,
        side: 'left',
        isOccupied: guestCount >= 1,
        style: { top: `${height / 2}px`, left: `-16px`, transform: `translate(-100%, -50%)` }
      });
      chairs.push({
        id: 2,
        side: 'right',
        isOccupied: guestCount >= 2,
        style: { top: `${height / 2}px`, right: `-16px`, transform: `translate(100%, -50%)` }
      });
      return chairs;
    }

    if (capacity <= 4) {
      chairs.push({
        id: 1,
        side: 'top',
        isOccupied: guestCount >= 1,
        style: { left: `${width / 2}px`, top: `-16px`, transform: `translate(-50%, -100%)` }
      });
      chairs.push({
        id: 2,
        side: 'bottom',
        isOccupied: guestCount >= 2,
        style: { left: `${width / 2}px`, bottom: `-16px`, transform: `translate(-50%, 100%)` }
      });
      if (capacity >= 3) {
        chairs.push({
          id: 3,
          side: 'left',
          isOccupied: guestCount >= 3,
          style: { top: `${height / 2}px`, left: `-16px`, transform: `translate(-100%, -50%)` }
        });
      }
      if (capacity >= 4) {
        chairs.push({
          id: 4,
          side: 'right',
          isOccupied: guestCount >= 4,
          style: { top: `${height / 2}px`, right: `-16px`, transform: `translate(100%, -50%)` }
        });
      }
      return chairs;
    }

    // 6, 8, 10, 12 seats:
    const remaining = capacity - 2;
    const topCount = Math.ceil(remaining / 2);
    const bottomCount = Math.floor(remaining / 2);

    let chairIdx = 1;
    chairs.push({
      id: chairIdx,
      side: 'left',
      isOccupied: guestCount >= chairIdx,
      style: { top: `${height / 2}px`, left: `-16px`, transform: `translate(-100%, -50%)` }
    });
    chairIdx++;

    chairs.push({
      id: chairIdx,
      side: 'right',
      isOccupied: guestCount >= chairIdx,
      style: { top: `${height / 2}px`, right: `-16px`, transform: `translate(100%, -50%)` }
    });
    chairIdx++;

    for (let i = 0; i < topCount; i++) {
      const leftPos = (width / (topCount + 1)) * (i + 1);
      chairs.push({
        id: chairIdx,
        side: 'top',
        isOccupied: guestCount >= chairIdx,
        style: { left: `${leftPos}px`, top: `-16px`, transform: `translate(-50%, -100%)` }
      });
      chairIdx++;
    }

    for (let i = 0; i < bottomCount; i++) {
      const leftPos = (width / (bottomCount + 1)) * (i + 1);
      chairs.push({
        id: chairIdx,
        side: 'bottom',
        isOccupied: guestCount >= chairIdx,
        style: { left: `${leftPos}px`, bottom: `-16px`, transform: `translate(-50%, 100%)` }
      });
      chairIdx++;
    }

    return chairs;
  }
}

