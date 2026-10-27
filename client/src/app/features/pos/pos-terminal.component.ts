import { Component, computed, signal, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Category, Product, CartItem, Table } from '../../models';
import { ApiService } from '../../core/services/api.service';
import { ReceiptPreviewComponent } from '../../shared/components/receipt-preview/receipt-preview.component';

@Component({
  selector: 'app-pos-terminal',
  standalone: true,
  imports: [CommonModule, FormsModule, ReceiptPreviewComponent],
  templateUrl: './pos-terminal.component.html',
  styleUrl: './pos-terminal.component.css'
})
export class PosTerminalComponent implements OnInit {
  private api = inject(ApiService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  orderTypes: Array<'Dine-In' | 'Takeaway' | 'Delivery' | 'Quick Sale'> = ['Dine-In', 'Takeaway', 'Delivery', 'Quick Sale'];
  selectedOrderType = signal<'Dine-In' | 'Takeaway' | 'Delivery' | 'Quick Sale'>('Dine-In');
  selectedTable = signal<Table | null>(null);
  guestCoverCount = signal<number>(2);
  selectedCategory = signal<number | null>(null);
  searchQuery = signal<string>('');
  isLoading = signal<boolean>(true);
  toastMessage = signal<string | null>(null);

  // Cart & Pricing
  cart = signal<CartItem[]>([]);
  discountPercent = signal<number>(0);
  serviceChargeRate = signal<number>(10);
  vatRate = signal<number>(5);
  isVatInclusive = signal<boolean>(true);

  // Modifier Customization
  activeProductForModifier = signal<Product | null>(null);
  selectedVariant = signal<any | null>(null);
  selectedModifierOptions = signal<number[]>([]);
  modifierInstruction = signal<string>('');
  selectedCourseForNewItem = signal<'Starter' | 'Main' | 'Dessert' | 'Beverage'>('Main');

  // Bangladesh Payment Modal
  isPaymentModalOpen = signal<boolean>(false);
  paymentMethod = signal<'Cash' | 'bKash' | 'Nagad' | 'Rocket' | 'Card' | 'Due'>('Cash');
  cashTendered = signal<number>(0);
  mfsTrxId = signal<string>('');
  cardSlipNo = signal<string>('');
  dueCustomerName = signal<string>('Gulshan Corporate Client (House Account)');

  // Split Payment (e.g. Cash + bKash)
  isSplitTender = signal<boolean>(false);
  splitCashAmount = signal<number>(0);
  splitMfsAmount = signal<number>(0);

  // Odoo Bill Splitting Modal
  isSplitModalOpen = signal<boolean>(false);
  splitCartBill2 = signal<CartItem[]>([]);
  equalSplitCount = signal<number>(2);
  isEqualSplitMode = signal<boolean>(false);

  // Pre-Bill & Receipt Modal
  isReceiptModalOpen = signal<boolean>(false);
  lastSettledOrder = signal<any | null>(null);
  isPreBillPrinted = signal<boolean>(false);
  activeOrderId = signal<number | null>(null);
  activeOrderNo = signal<string | null>(null);

  // Catalog Signals
  categories = signal<Category[]>([]);
  products = signal<Product[]>([]);

  // Calculations
  subtotal = computed(() => this.cart().reduce((sum, item) => sum + item.lineTotal, 0));
  discountAmount = computed(() => Math.round((this.subtotal() * this.discountPercent()) / 100));
  serviceChargeAmount = computed(() => {
    if (this.selectedOrderType() !== 'Dine-In') return 0;
    const net = Math.max(0, this.subtotal() - this.discountAmount());
    return Math.round((net * this.serviceChargeRate()) / 100);
  });
  taxAmount = computed(() => {
    const base = Math.max(0, this.subtotal() - this.discountAmount()) + this.serviceChargeAmount();
    return this.isVatInclusive() 
      ? Math.round(base - (base / (1 + (this.vatRate() / 100))))
      : Math.round((base * this.vatRate()) / 100);
  });
  grandTotal = computed(() => {
    const base = Math.max(0, this.subtotal() - this.discountAmount()) + this.serviceChargeAmount();
    return Math.round(this.isVatInclusive() ? base : (base + this.taxAmount()));
  });
  changeAmount = computed(() => Math.max(0, this.cashTendered() - this.grandTotal()));

  splitBill2Total = computed(() => this.splitCartBill2().reduce((sum, i) => sum + i.lineTotal, 0));

  activeCustomizedItemPrice = computed(() => {
    const prod = this.activeProductForModifier();
    if (!prod) return 0;
    const variant = this.selectedVariant();
    const base = prod.basePrice + (variant ? variant.priceDelta : 0);
    let modTotal = 0;
    for (const mod of prod.modifiers) {
      for (const opt of mod.options) {
        if (this.selectedModifierOptions().includes(opt.id)) {
          modTotal += opt.priceDelta;
        }
      }
    }
    return base + modTotal;
  });

  filteredProducts = computed(() => {
    let list = this.products();
    const cat = this.selectedCategory();
    if (cat !== null) list = list.filter(p => p.categoryId === cat);
    const q = this.searchQuery().trim().toLowerCase();
    if (q) list = list.filter(p => p.name.toLowerCase().includes(q) || (p.shortName && p.shortName.toLowerCase().includes(q)));
    return list;
  });

  unsentKitchenCount = computed(() => {
    return this.cart().filter(i => !i.isSentToKitchen).length;
  });

  ngOnInit(): void {
    this.checkRouteParams();
    this.loadCatalogFromApi();
  }

  showToast(msg: string): void {
    this.toastMessage.set(msg);
    setTimeout(() => this.toastMessage.set(null), 3500);
  }

  checkRouteParams(): void {
    this.route.queryParams.subscribe(params => {
      if (params['tableId'] && params['tableNo']) {
        const tableId = +params['tableId'];
        const tableNo = params['tableNo'];
        this.selectedOrderType.set('Dine-In');
        this.selectedTable.set({
          id: tableId,
          areaId: 1,
          areaName: 'Dining Room',
          tableNo: tableNo,
          capacity: 4,
          status: 'occupied',
          positionX: 50,
          positionY: 50,
          width: 140,
          height: 140,
          shape: 'square'
        });

        this.loadActiveTableOrder(tableId, tableNo);
      }
    });
  }

  loadActiveTableOrder(tableId: number, tableNo: string): void {
    this.api.getActiveOrders(1).subscribe((orders: any[]) => {
      const active = (orders || []).find((o: any) => o.tableId === tableId);
      if (active) {
        this.activeOrderId.set(active.id);
        this.activeOrderNo.set(active.orderNo);
        if (active.guestCoverCount > 0) {
          this.guestCoverCount.set(active.guestCoverCount);
        }

        const loadedCart: CartItem[] = (active.items || []).map((i: any, idx: number) => ({
          id: String(i.id || idx + 1),
          productId: i.productId,
          name: i.productName || 'Dish',
          unitPrice: i.unitPrice,
          quantity: i.qty || 1,
          lineTotal: (i.qty || 1) * i.unitPrice,
          variant: i.variantId ? { id: i.variantId, name: i.variantName, priceDelta: 0 } : undefined,
          modifiers: (i.modifiers || []).map((m: any) => ({
            optionId: m.id || m.modifierOptionId,
            name: m.optionName || 'Option',
            price: m.unitPrice || 0
          })),
          instructions: i.note || '',
          courseType: (i.courseType || 'Main') as any,
          guestSeatNo: i.guestSeatNo || 1,
          isSentToKitchen: true
        }));

        this.cart.set(loadedCart);
        this.showToast(`Active Order #${active.orderNo} loaded for Table ${tableNo} (${loadedCart.length} items)`);
      } else {
        this.activeOrderId.set(null);
        this.activeOrderNo.set(null);
      }
    });
  }

  loadCatalogFromApi(): void {
    this.isLoading.set(true);

    this.api.getCategories(1).subscribe(cats => {
      if (cats && cats.length > 0) {
        this.categories.set(cats.map(c => ({
          id: c.id,
          name: c.name,
          sortOrder: c.sortOrder,
          icon: this.getIconForCategory(c.name)
        })));
      } else {
        this.initFallbackCategories();
      }
    });

    this.api.getProducts(1).subscribe(prods => {
      if (prods && prods.length > 0) {
        this.products.set(prods.map(p => ({
          id: p.id,
          categoryId: p.categoryId,
          categoryName: p.categoryName || 'Dishes',
          name: p.name,
          shortName: p.shortName,
          description: p.description,
          basePrice: p.basePrice,
          costPrice: p.costPrice,
          isTaxInclusive: p.isTaxInclusive,
          variants: p.variants || [],
          modifiers: p.modifiers || [],
          imageEmoji: this.getEmojiForDish(p.name)
        })));
      } else {
        this.initFallbackProducts();
      }
      this.isLoading.set(false);
    });
  }

  private initFallbackCategories(): void {
    this.categories.set([
      { id: 1, name: 'Kacchi & Biryani', sortOrder: 1, icon: 'fa-bowl-rice' },
      { id: 2, name: 'Burgers & Sandwiches', sortOrder: 2, icon: 'fa-burger' },
      { id: 3, name: 'Grill, BBQ & Kebabs', sortOrder: 3, icon: 'fa-fire-burner' },
      { id: 4, name: 'Beverages & Borhani', sortOrder: 4, icon: 'fa-glass-water' },
      { id: 5, name: 'Traditional Desserts', sortOrder: 5, icon: 'fa-ice-cream' }
    ]);
  }

  private initFallbackProducts(): void {
    this.products.set([
      {
        id: 1,
        categoryId: 1,
        categoryName: 'Kacchi & Biryani',
        name: "Sultan's Special Mutton Kacchi Biryani",
        shortName: 'Mutton Kacchi',
        basePrice: 450,
        isTaxInclusive: true,
        imageEmoji: '🍛',
        variants: [
          { id: 1, name: 'Regular (1:1)', priceDelta: 0 },
          { id: 2, name: 'Jumbo (1:2)', priceDelta: 350 }
        ],
        modifiers: []
      },
      {
        id: 2,
        categoryId: 2,
        categoryName: 'Burgers & Sandwiches',
        name: 'Double Patty Gourmet Beef Burger',
        shortName: 'Beef Burger',
        basePrice: 360,
        isTaxInclusive: true,
        imageEmoji: '🍔',
        variants: [],
        modifiers: [
          {
            id: 1,
            name: 'Burger Add-ons',
            isRequired: false,
            minSelect: 0,
            maxSelect: 3,
            options: [
              { id: 101, name: 'Extra Melted Cheddar', priceDelta: 50 },
              { id: 102, name: 'Crispy Beef Bacon', priceDelta: 80 },
              { id: 103, name: 'Pickled Jalapeños', priceDelta: 30 }
            ]
          }
        ]
      },
      {
        id: 3,
        categoryId: 3,
        categoryName: 'Grill, BBQ & Kebabs',
        name: 'Flame-Grilled Quarter Chicken',
        shortName: 'Grill Chicken',
        basePrice: 220,
        isTaxInclusive: true,
        imageEmoji: '🍗',
        variants: [],
        modifiers: []
      },
      {
        id: 4,
        categoryId: 4,
        categoryName: 'Beverages & Borhani',
        name: 'Special Shahi Borhani (Glass)',
        shortName: 'Shahi Borhani',
        basePrice: 90,
        isTaxInclusive: true,
        imageEmoji: '🥛',
        variants: [],
        modifiers: []
      },
      {
        id: 5,
        categoryId: 5,
        categoryName: 'Traditional Desserts',
        name: 'Zafrani Matka Firni',
        shortName: 'Matka Firni',
        basePrice: 120,
        isTaxInclusive: true,
        imageEmoji: '🍮',
        variants: [],
        modifiers: []
      }
    ]);
  }

  private getIconForCategory(name: string): string {
    if (name.includes('Biryani')) return 'fa-bowl-rice';
    if (name.includes('Burger')) return 'fa-burger';
    if (name.includes('Grill') || name.includes('Kebab')) return 'fa-fire-burner';
    if (name.includes('Drink') || name.includes('Borhani')) return 'fa-glass-water';
    if (name.includes('Dessert')) return 'fa-ice-cream';
    return 'fa-utensils';
  }

  private getEmojiForDish(name: string): string {
    if (name.includes('Biryani') || name.includes('Kacchi')) return '🍛';
    if (name.includes('Burger')) return '🍔';
    if (name.includes('Chicken') || name.includes('Grill')) return '🍗';
    if (name.includes('Borhani')) return '🥛';
    if (name.includes('Firni')) return '🍮';
    return '🍽️';
  }

  // --- Cart Operations ---
  onProductClick(prod: Product): void {
    if (prod.variants.length > 0 || prod.modifiers.length > 0) {
      this.activeProductForModifier.set(prod);
      this.selectedVariant.set(prod.variants.length > 0 ? prod.variants[0] : null);
      this.selectedModifierOptions.set([]);
      this.modifierInstruction.set('');
      this.selectedCourseForNewItem.set(prod.categoryId === 4 ? 'Beverage' : (prod.categoryId === 5 ? 'Dessert' : 'Main'));
    } else {
      let defaultCourse: 'Starter' | 'Main' | 'Dessert' | 'Beverage' = 'Main';
      if (prod.categoryId === 4) defaultCourse = 'Beverage';
      if (prod.categoryId === 5) defaultCourse = 'Dessert';
      this.addSimpleProductToCart(prod, defaultCourse);
    }
  }

  addSimpleProductToCart(prod: Product, course: 'Starter' | 'Main' | 'Dessert' | 'Beverage'): void {
    const existing = this.cart().find(i => i.productId === prod.id && !i.variant && i.modifiers.length === 0 && i.courseType === course);
    if (existing) {
      this.updateQuantity(existing.id, 1);
    } else {
      const newItem: CartItem = {
        id: 'ITEM-' + Date.now() + '-' + Math.random().toString(36).substr(2, 4),
        productId: prod.id,
        name: prod.name,
        quantity: 1,
        unitPrice: prod.basePrice,
        modifiers: [],
        lineTotal: prod.basePrice,
        courseType: course,
        guestSeatNo: 1,
        isSentToKitchen: false
      };
      this.cart.set([...this.cart(), newItem]);
    }
  }

  addConfiguredProductToCart(): void {
    const prod = this.activeProductForModifier();
    if (!prod) return;

    const variant = this.selectedVariant();
    const mods: any[] = [];
    for (const mod of prod.modifiers) {
      for (const opt of mod.options) {
        if (this.selectedModifierOptions().includes(opt.id)) {
          mods.push({ optionId: opt.id, name: opt.name, price: opt.priceDelta });
        }
      }
    }

    const unitPrice = this.activeCustomizedItemPrice();
    const newItem: CartItem = {
      id: 'ITEM-' + Date.now() + '-' + Math.random().toString(36).substr(2, 4),
      productId: prod.id,
      name: prod.name,
      variant: variant || undefined,
      quantity: 1,
      unitPrice: unitPrice,
      modifiers: mods,
      instructions: this.modifierInstruction(),
      lineTotal: unitPrice,
      courseType: this.selectedCourseForNewItem(),
      guestSeatNo: 1,
      isSentToKitchen: false
    };

    this.cart.set([...this.cart(), newItem]);
    this.activeProductForModifier.set(null);
  }

  updateQuantity(itemId: string, delta: number): void {
    const current = [...this.cart()];
    const item = current.find(i => i.id === itemId);
    if (!item) return;

    item.quantity += delta;
    if (item.quantity <= 0) {
      this.cart.set(current.filter(i => i.id !== itemId));
    } else {
      item.lineTotal = item.quantity * item.unitPrice;
      this.cart.set(current);
    }
  }

  setCourseForItem(item: CartItem, course: 'Starter' | 'Main' | 'Dessert' | 'Beverage'): void {
    item.courseType = course;
    this.cart.set([...this.cart()]);
  }

  clearCart(): void {
    this.cart.set([]);
  }

  goToTables(): void {
    this.router.navigate(['/tables']);
  }

  // --- Standard Table-Wise Running Tab (Hold Table & Dispatch KOT) ---
  holdTableAndSendKOT(): void {
    if (this.cart().length === 0) {
      this.showToast('Please add items to cart before sending to kitchen.');
      return;
    }

    const table = this.selectedTable();
    const tableId = (this.selectedOrderType() === 'Dine-In' && table) ? table.id : null;
    const tableNo = table?.tableNo || this.selectedOrderType();

    const itemsDto = this.cart().map(item => ({
      productId: item.productId,
      variantId: item.variant?.id || null,
      qty: item.quantity,
      unitPrice: item.unitPrice,
      discountAmount: 0,
      note: item.instructions || '',
      modifiers: item.modifiers.map(m => ({
        modifierOptionId: m.optionId,
        qty: 1,
        unitPrice: m.price
      })),
      courseType: item.courseType || 'Main',
      guestSeatNo: item.guestSeatNo || 1
    }));

    if (this.activeOrderId()) {
      this.api.sendToKitchen(this.activeOrderId()!).subscribe(() => {
        for (const item of this.cart()) {
          item.isSentToKitchen = true;
        }
        this.cart.set([...this.cart()]);
        if (tableId) {
          this.api.updateTableStatus(tableId, 4).subscribe();
        }
        this.showToast(`🔥 KOT fired & Table ${tableNo} saved in Database! (Status: In Kitchen)`);
      });
    } else {
      const orderReq = {
        restaurantId: 1,
        branchId: 1,
        terminalId: 1,
        orderType: this.selectedOrderType() === 'Dine-In' ? 1 : 2,
        tableId: tableId,
        customerId: null,
        guestCoverCount: this.guestCoverCount(),
        items: itemsDto,
        discountAmount: this.discountAmount(),
        serviceChargeRate: this.serviceChargeRate(),
        vatRate: this.vatRate(),
        isVatInclusive: this.isVatInclusive()
      };

      this.api.createOrder(orderReq).subscribe((created: any) => {
        const orderId = created?.id;
        if (orderId) {
          this.activeOrderId.set(orderId);
          this.activeOrderNo.set(created.orderNo);
          this.api.sendToKitchen(orderId).subscribe();
        }
        if (tableId) {
          this.api.updateTableStatus(tableId, 4).subscribe();
        }
        for (const item of this.cart()) {
          item.isSentToKitchen = true;
        }
        this.cart.set([...this.cart()]);
        this.showToast(`🔥 Order #${created?.orderNo || 'NEW'} held on Table ${tableNo}! KOT Fired.`);
      });
    }
  }

  sendToKitchen(): void {
    this.holdTableAndSendKOT();
  }

  // --- Pre-Bill Action ---
  printPreBill(): void {
    this.isPreBillPrinted.set(true);
    const table = this.selectedTable();
    if (table) {
      this.api.updateTableStatus(table.id, 3).subscribe();
    }
    this.showToast(`🧾 Pre-Bill printed for Table ${table?.tableNo || 'Counter'} (Total: ৳${this.grandTotal()}). Status: Pre-Billed.`);
  }

  // --- Odoo Split Bill Engine ---
  openSplitBillModal(): void {
    this.splitCartBill2.set([]);
    this.isEqualSplitMode.set(false);
    this.isSplitModalOpen.set(true);
  }

  moveItemToBill2(item: CartItem): void {
    if (item.quantity <= 1) {
      // Move entire item
      this.cart.set(this.cart().filter(i => i.id !== item.id));
      this.splitCartBill2.set([...this.splitCartBill2(), { ...item }]);
    } else {
      // Move 1 qty
      item.quantity -= 1;
      item.lineTotal = item.quantity * item.unitPrice;
      this.cart.set([...this.cart()]);

      const existingBill2 = this.splitCartBill2().find(i => i.productId === item.productId && i.variant?.id === item.variant?.id);
      if (existingBill2) {
        existingBill2.quantity += 1;
        existingBill2.lineTotal = existingBill2.quantity * existingBill2.unitPrice;
        this.splitCartBill2.set([...this.splitCartBill2()]);
      } else {
        const splitClone: CartItem = {
          ...item,
          id: 'SPLIT-' + Date.now(),
          quantity: 1,
          lineTotal: item.unitPrice
        };
        this.splitCartBill2.set([...this.splitCartBill2(), splitClone]);
      }
    }
  }

  moveItemBackToBill1(item: CartItem): void {
    if (item.quantity <= 1) {
      this.splitCartBill2.set(this.splitCartBill2().filter(i => i.id !== item.id));
      this.cart.set([...this.cart(), { ...item }]);
    } else {
      item.quantity -= 1;
      item.lineTotal = item.quantity * item.unitPrice;
      this.splitCartBill2.set([...this.splitCartBill2()]);

      const existingBill1 = this.cart().find(i => i.productId === item.productId && i.variant?.id === item.variant?.id);
      if (existingBill1) {
        existingBill1.quantity += 1;
        existingBill1.lineTotal = existingBill1.quantity * existingBill1.unitPrice;
        this.cart.set([...this.cart()]);
      } else {
        const clone: CartItem = {
          ...item,
          id: 'RETURN-' + Date.now(),
          quantity: 1,
          lineTotal: item.unitPrice
        };
        this.cart.set([...this.cart(), clone]);
      }
    }
  }

  settleSplitBill2(): void {
    this.isSplitModalOpen.set(false);
    this.showToast(`Bill 2 (৳${this.splitBill2Total()}) settled! Bill 1 remains active on table.`);
    this.splitCartBill2.set([]);
  }

  // --- Payment Modal Actions ---
  openPaymentModal(): void {
    this.cashTendered.set(this.grandTotal());
    this.mfsTrxId.set('');
    this.cardSlipNo.set('');
    this.isSplitTender.set(false);
    this.isPaymentModalOpen.set(true);
  }

  setTenderAmount(amt: number): void {
    this.cashTendered.set(amt);
  }

  confirmSettlement(): void {
    const table = this.selectedTable();
    const tableNo = table?.tableNo || this.selectedOrderType();
    const tableId = (this.selectedOrderType() === 'Dine-In' && table) ? table.id : null;
    const orderNoStr = this.activeOrderNo() || `ORD-${new Date().getFullYear()}${String(new Date().getMonth() + 1).padStart(2, '0')}-${Math.floor(1000 + Math.random() * 9000)}`;

    const settled = {
      orderNo: orderNoStr,
      tableNo: tableNo,
      cashier: 'Kamrul Hasan',
      date: new Date(),
      items: this.cart(),
      subtotal: this.subtotal(),
      discount: this.discountAmount(),
      serviceCharge: this.serviceChargeAmount(),
      tax: this.taxAmount(),
      grandTotal: this.grandTotal(),
      paymentMethod: this.paymentMethod(),
      trxReference: this.mfsTrxId() || this.cardSlipNo() || (this.paymentMethod() === 'Due' ? this.dueCustomerName() : ''),
      change: this.changeAmount()
    };

    const methodId = this.paymentMethod() === 'Cash' ? 1 :
      (this.paymentMethod() === 'bKash' ? 2 :
      (this.paymentMethod() === 'Nagad' ? 3 :
      (this.paymentMethod() === 'Rocket' ? 4 : 5)));

    // Settle in Database
    if (this.activeOrderId()) {
      this.api.settleOrder({
        orderId: this.activeOrderId()!,
        payments: [{
          paymentMethodId: methodId,
          amount: this.grandTotal(),
          mfsTrxId: this.mfsTrxId() || null,
          cardSlipNo: this.cardSlipNo() || null
        }]
      }).subscribe();
    } else {
      // Direct Pay on new order
      const orderReq = {
        restaurantId: 1,
        branchId: 1,
        terminalId: 1,
        orderType: this.selectedOrderType() === 'Dine-In' ? 1 : 2,
        tableId: tableId,
        customerId: null,
        guestCoverCount: this.guestCoverCount(),
        items: this.cart().map(item => ({
          productId: item.productId,
          variantId: item.variant?.id || null,
          qty: item.quantity,
          unitPrice: item.unitPrice,
          discountAmount: 0,
          note: item.instructions || '',
          modifiers: item.modifiers.map(m => ({
            modifierOptionId: m.optionId,
            qty: 1,
            unitPrice: m.price
          })),
          courseType: item.courseType || 'Main',
          guestSeatNo: item.guestSeatNo || 1
        })),
        discountAmount: this.discountAmount(),
        serviceChargeRate: this.serviceChargeRate(),
        vatRate: this.vatRate(),
        isVatInclusive: this.isVatInclusive()
      };

      this.api.createOrder(orderReq).subscribe((created: any) => {
        if (created?.id) {
          this.api.settleOrder({
            orderId: created.id,
            payments: [{
              paymentMethodId: methodId,
              amount: this.grandTotal(),
              mfsTrxId: this.mfsTrxId() || null,
              cardSlipNo: this.cardSlipNo() || null
            }]
          }).subscribe();
        }
      });
    }

    // Free the table in database
    if (tableId) {
      this.api.updateTableStatus(tableId, 1).subscribe();
    }

    this.lastSettledOrder.set(settled);
    this.isPaymentModalOpen.set(false);
    this.isReceiptModalOpen.set(true);
    this.cart.set([]);
    this.activeOrderId.set(null);
    this.activeOrderNo.set(null);
    this.showToast(`✅ Order ${settled.orderNo} settled via ${settled.paymentMethod}! Table ${tableNo} is now Vacant.`);
  }
}
