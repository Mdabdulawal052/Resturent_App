import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { DiningArea, Table, TableBooking } from '../../models';

export interface VisualChair {
  seatNumber: number;
  isBooked: boolean;
  bookedGuest?: string;
  bookingNo?: string;
  style: Record<string, string>;
}

@Component({
  selector: 'app-table-bookings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './table-bookings.component.html',
  styleUrl: './table-bookings.component.css'
})
export class TableBookingsComponent implements OnInit {
  private api = inject(ApiService);
  private router = inject(Router);

  // Hostess Profile & Login Simulation
  hostessName = signal<string>('Maliha Akter');
  hostessRole = signal<string>('Senior Hostess & Reservation Manager');
  hostessDesk = signal<string>('Dhanmondi Reception & Lobby Desk');
  isHostessPortalMode = signal<boolean>(true);

  // Active View Tab: 'grid' (Interactive Floor Map) | 'list' (Bookings Registry)
  activeTab = signal<'list' | 'grid'>('list');

  // Dining Areas and Tables from DB
  diningAreas = signal<DiningArea[]>([]);
  selectedAreaId = signal<number>(1);
  bookings = signal<TableBooking[]>([]);
  isLoading = signal<boolean>(false);
  toastMessage = signal<string | null>(null);

  // Filters
  selectedDate = signal<string>(new Date().toISOString().substring(0, 10));
  selectedTimeSlot = signal<string>('All');
  selectedTypeFilter = signal<'All' | 'Table' | 'Chair'>('All');
  selectedStatusFilter = signal<string>('All');
  searchQuery = signal<string>('');

  // Available Time Slots
  timeSlots = [
    'All',
    'Lunch (01:00 PM - 02:30 PM)',
    'Evening Tea (05:00 PM - 06:30 PM)',
    'Dinner (08:00 PM - 09:30 PM)',
    'Late Night (10:00 PM - 11:30 PM)'
  ];

  // Quick Date Options
  quickDates = computed(() => {
    const today = new Date();
    const tomorrow = new Date(today);
    tomorrow.setDate(today.getDate() + 1);
    const dayAfter = new Date(today);
    dayAfter.setDate(today.getDate() + 2);

    return [
      { label: 'Today', value: today.toISOString().substring(0, 10) },
      { label: 'Tomorrow', value: tomorrow.toISOString().substring(0, 10) },
      { label: 'Day After', value: dayAfter.toISOString().substring(0, 10) }
    ];
  });

  // Modal State for New Booking
  showCreateModal = signal<boolean>(false);
  newBookingType = signal<'Table' | 'Chair'>('Table');
  newBookingTableId = signal<number | null>(null);
  newBookingSeatNumber = signal<number | null>(null);
  newBookingCustomerName = signal<string>('');
  newBookingCustomerPhone = signal<string>('');
  newBookingCustomerEmail = signal<string>('');
  newBookingGuestCount = signal<number>(2);
  newBookingDate = signal<string>(new Date().toISOString().substring(0, 10));
  newBookingTimeSlot = signal<string>('Dinner (08:00 PM - 09:30 PM)');
  newBookingDeposit = signal<number>(500);
  newBookingIsDepositPaid = signal<boolean>(true);
  newBookingSpecialRequests = signal<string>('');
  isSubmitting = signal<boolean>(false);

  // Current Area & Tables
  currentArea = computed(() => {
    return this.diningAreas().find(a => a.id === this.selectedAreaId()) || this.diningAreas()[0] || null;
  });

  currentTables = computed(() => {
    return this.currentArea()?.tables || [];
  });

  // Filtered Bookings
  filteredBookings = computed(() => {
    const list = this.bookings();
    const date = this.selectedDate();
    const slot = this.selectedTimeSlot();
    const type = this.selectedTypeFilter();
    const status = this.selectedStatusFilter();
    const q = this.searchQuery().trim().toLowerCase();

    return list.filter(b => {
      // Date filter
      const bDate = b.bookingDate ? b.bookingDate.substring(0, 10) : '';
      if (date && bDate && bDate !== date) return false;

      // Slot filter
      if (slot !== 'All' && b.timeSlot !== slot) return false;

      // Type filter
      if (type !== 'All' && b.bookingType !== type) return false;

      // Status filter
      if (status !== 'All' && b.status.toLowerCase() !== status.toLowerCase()) return false;

      // Query filter
      if (q) {
        const matchName = b.customerName?.toLowerCase().includes(q);
        const matchPhone = b.customerPhone?.toLowerCase().includes(q);
        const matchNo = b.bookingNo?.toLowerCase().includes(q);
        const matchTable = b.tableNo?.toLowerCase().includes(q);
        if (!matchName && !matchPhone && !matchNo && !matchTable) return false;
      }

      return true;
    });
  });

  // Summary counts
  totalCount = computed(() => this.filteredBookings().length);
  tableCount = computed(() => this.filteredBookings().filter(b => b.bookingType === 'Table').length);
  chairCount = computed(() => this.filteredBookings().filter(b => b.bookingType === 'Chair').length);
  seatedCount = computed(() => this.filteredBookings().filter(b => b.status === 'Seated').length);
  confirmedCount = computed(() => this.filteredBookings().filter(b => b.status === 'Confirmed').length);

  ngOnInit(): void {
    this.loadAreas();
    this.loadBookings();
  }

  loadAreas(): void {
    this.api.getDiningAreas(1).subscribe({
      next: (areas) => {
        if (areas && areas.length > 0) {
          this.diningAreas.set(areas);
          this.selectedAreaId.set(areas[0].id);
        }
      },
      error: (err) => console.error('Failed to load dining areas', err)
    });
  }

  loadBookings(): void {
    this.isLoading.set(true);
    const date = this.selectedDate();
    const slot = this.selectedTimeSlot() === 'All' ? undefined : this.selectedTimeSlot();

    this.api.getBookings(1, date, slot).subscribe({
      next: (data) => {
        this.bookings.set(data || []);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading bookings', err);
        this.isLoading.set(false);
      }
    });
  }

  onDateChange(newDate: string): void {
    this.selectedDate.set(newDate);
    this.newBookingDate.set(newDate);
    this.loadBookings();
  }

  onSlotChange(newSlot: string): void {
    this.selectedTimeSlot.set(newSlot);
    if (newSlot !== 'All') {
      this.newBookingTimeSlot.set(newSlot);
    }
    this.loadBookings();
  }

  openNewBookingModal(type: 'Table' | 'Chair' = 'Table', table?: Table, seatNumber?: number): void {
    this.newBookingType.set(type);
    if (table) {
      this.newBookingTableId.set(table.id);
      this.newBookingGuestCount.set(type === 'Chair' ? 1 : table.capacity);
    } else if (this.currentTables().length > 0) {
      this.newBookingTableId.set(this.currentTables()[0].id);
      this.newBookingGuestCount.set(type === 'Chair' ? 1 : 2);
    }

    if (seatNumber) {
      this.newBookingSeatNumber.set(seatNumber);
    } else {
      this.newBookingSeatNumber.set(type === 'Chair' ? 1 : null);
    }

    this.newBookingCustomerName.set('');
    this.newBookingCustomerPhone.set('');
    this.newBookingCustomerEmail.set('');
    this.newBookingDeposit.set(type === 'Chair' ? 300 : 1000);
    this.newBookingIsDepositPaid.set(true);
    this.newBookingSpecialRequests.set('');
    this.showCreateModal.set(true);
  }

  closeCreateModal(): void {
    this.showCreateModal.set(false);
  }

  saveBooking(): void {
    const tableId = this.newBookingTableId();
    const table = this.findTableById(tableId);
    if (!table) {
      this.showToast('Please select a valid table.');
      return;
    }

    const name = this.newBookingCustomerName().trim();
    const phone = this.newBookingCustomerPhone().trim();
    if (!name || !phone) {
      this.showToast('Customer Name and Mobile Phone are required.');
      return;
    }

    const type = this.newBookingType();
    const seatNum = type === 'Chair' ? this.newBookingSeatNumber() : null;

    if (type === 'Chair' && !seatNum) {
      this.showToast('Please specify which chair/seat number to reserve.');
      return;
    }

    this.isSubmitting.set(true);

    const payload = {
      branchId: 1,
      bookingType: type,
      tableId: table.id,
      tableNo: table.tableNo,
      seatNumber: seatNum,
      customerName: name,
      customerPhone: phone,
      customerEmail: this.newBookingCustomerEmail().trim() || null,
      guestCount: type === 'Chair' ? 1 : Number(this.newBookingGuestCount()),
      bookingDate: this.newBookingDate(),
      timeSlot: this.newBookingTimeSlot(),
      depositAmount: Number(this.newBookingDeposit()),
      isDepositPaid: this.newBookingIsDepositPaid(),
      specialRequests: this.newBookingSpecialRequests().trim() || null
    };

    this.api.createBooking(payload).subscribe({
      next: (created) => {
        this.isSubmitting.set(false);
        this.showCreateModal.set(false);
        this.showToast(`✅ ${type === 'Chair' ? 'Chair #' + seatNum : 'Table ' + table.tableNo} booked for ${name}!`);
        this.loadBookings();
      },
      error: (err) => {
        this.isSubmitting.set(false);
        console.error('Failed to create booking', err);
        this.showToast('❌ Error creating booking. Please try again.');
      }
    });
  }

  seatGuest(booking: TableBooking): void {
    this.api.updateBookingStatus(booking.id, 'Seated').subscribe({
      next: () => {
        this.showToast(`🍽️ Guest ${booking.customerName} seated at ${booking.tableNo}!`);
        this.loadBookings();
      },
      error: (err) => {
        console.error('Failed to seat guest', err);
        this.showToast('Failed to update booking status.');
      }
    });
  }

  cancelBooking(booking: TableBooking): void {
    if (confirm(`Cancel reservation for ${booking.customerName} (${booking.bookingNo})?`)) {
      this.api.cancelBooking(booking.id).subscribe({
        next: () => {
          this.showToast(`Booking ${booking.bookingNo} cancelled.`);
          this.loadBookings();
        },
        error: (err) => {
          console.error('Failed to cancel booking', err);
          this.showToast('Error cancelling booking.');
        }
      });
    }
  }

  openPosForBooking(booking: TableBooking): void {
    this.router.navigate(['/pos'], {
      queryParams: {
        tableId: booking.tableId,
        tableNo: booking.tableNo,
        bookingId: booking.id,
        guestName: booking.customerName
      }
    });
  }

  findTableById(id: number | null): Table | undefined {
    if (!id) return undefined;
    for (const area of this.diningAreas()) {
      const found = area.tables.find(t => t.id === id);
      if (found) return found;
    }
    return undefined;
  }

  // --- Visual Floor Helpers ---
  isTableFullyBooked(table: Table): boolean {
    const list = this.filteredBookings();
    return list.some(b => b.tableId === table.id && b.bookingType === 'Table' && b.status !== 'Cancelled');
  }

  getTableBooking(table: Table): TableBooking | undefined {
    const list = this.filteredBookings();
    return list.find(b => b.tableId === table.id && b.bookingType === 'Table' && b.status !== 'Cancelled');
  }

  getBookedChairsForTable(table: Table): TableBooking[] {
    const list = this.filteredBookings();
    return list.filter(b => b.tableId === table.id && b.bookingType === 'Chair' && b.status !== 'Cancelled');
  }

  getTableChairs(table: Table): VisualChair[] {
    const capacity = table.capacity || 4;
    const width = table.width || 120;
    const height = table.height || 100;
    const shape = table.shape || 'rectangle';

    const bookedChairBookings = this.getBookedChairsForTable(table);
    const isFullTableBooked = this.isTableFullyBooked(table);

    const chairs: VisualChair[] = [];

    if (shape === 'round') {
      const radius = width / 2 + 18;
      const angleStep = (2 * Math.PI) / capacity;
      for (let i = 0; i < capacity; i++) {
        const seatNo = i + 1;
        const angle = i * angleStep - Math.PI / 2;
        const x = width / 2 + radius * Math.cos(angle);
        const y = height / 2 + radius * Math.sin(angle);

        const chairBooking = bookedChairBookings.find(b => b.seatNumber === seatNo);
        chairs.push({
          seatNumber: seatNo,
          isBooked: isFullTableBooked || !!chairBooking,
          bookedGuest: chairBooking?.customerName || (isFullTableBooked ? 'Table Booked' : undefined),
          bookingNo: chairBooking?.bookingNo,
          style: {
            left: `${x}px`,
            top: `${y}px`,
            transform: 'translate(-50%, -50%)'
          }
        });
      }
      return chairs;
    }

    // Rectangle shape
    let seatIdx = 1;
    // Left chair
    const leftBooking = bookedChairBookings.find(b => b.seatNumber === seatIdx);
    chairs.push({
      seatNumber: seatIdx,
      isBooked: isFullTableBooked || !!leftBooking,
      bookedGuest: leftBooking?.customerName || (isFullTableBooked ? 'Table Booked' : undefined),
      bookingNo: leftBooking?.bookingNo,
      style: { top: `${height / 2}px`, left: `-16px`, transform: `translate(-100%, -50%)` }
    });
    seatIdx++;

    // Right chair
    const rightBooking = bookedChairBookings.find(b => b.seatNumber === seatIdx);
    chairs.push({
      seatNumber: seatIdx,
      isBooked: isFullTableBooked || !!rightBooking,
      bookedGuest: rightBooking?.customerName || (isFullTableBooked ? 'Table Booked' : undefined),
      bookingNo: rightBooking?.bookingNo,
      style: { top: `${height / 2}px`, right: `-16px`, transform: `translate(100%, -50%)` }
    });
    seatIdx++;

    const remaining = Math.max(0, capacity - 2);
    const topCount = Math.ceil(remaining / 2);
    const bottomCount = remaining - topCount;

    for (let i = 0; i < topCount; i++) {
      const seatNo = seatIdx;
      const leftPos = (width / (topCount + 1)) * (i + 1);
      const bkg = bookedChairBookings.find(b => b.seatNumber === seatNo);
      chairs.push({
        seatNumber: seatNo,
        isBooked: isFullTableBooked || !!bkg,
        bookedGuest: bkg?.customerName || (isFullTableBooked ? 'Table Booked' : undefined),
        bookingNo: bkg?.bookingNo,
        style: { left: `${leftPos}px`, top: `-16px`, transform: `translate(-50%, -100%)` }
      });
      seatIdx++;
    }

    for (let i = 0; i < bottomCount; i++) {
      const seatNo = seatIdx;
      const leftPos = (width / (bottomCount + 1)) * (i + 1);
      const bkg = bookedChairBookings.find(b => b.seatNumber === seatNo);
      chairs.push({
        seatNumber: seatNo,
        isBooked: isFullTableBooked || !!bkg,
        bookedGuest: bkg?.customerName || (isFullTableBooked ? 'Table Booked' : undefined),
        bookingNo: bkg?.bookingNo,
        style: { left: `${leftPos}px`, bottom: `-16px`, transform: `translate(-50%, 100%)` }
      });
      seatIdx++;
    }

    return chairs;
  }

  showToast(msg: string): void {
    this.toastMessage.set(msg);
    setTimeout(() => this.toastMessage.set(null), 4000);
  }
}
