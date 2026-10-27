export interface Category {
  id: number;
  name: string;
  icon?: string;
  sortOrder: number;
}

export interface ModifierOption {
  id: number;
  name: string;
  priceDelta: number;
}

export interface Modifier {
  id: number;
  name: string;
  isRequired: boolean;
  minSelect: number;
  maxSelect: number;
  options: ModifierOption[];
}

export interface ProductVariant {
  id: number;
  name: string;
  priceDelta: number;
}

export interface Product {
  id: number;
  categoryId: number;
  categoryName: string;
  name: string;
  shortName?: string;
  description?: string;
  basePrice: number;
  costPrice?: number;
  isTaxInclusive: boolean;
  variants: ProductVariant[];
  modifiers: Modifier[];
  imageEmoji: string;
}

export interface CartItemModifier {
  optionId: number;
  name: string;
  price: number;
}

export interface CartItem {
  id: string; // unique key
  productId: number;
  name: string;
  variant?: ProductVariant;
  quantity: number;
  unitPrice: number;
  modifiers: CartItemModifier[];
  instructions?: string;
  lineTotal: number;
  courseType?: 'Starter' | 'Main' | 'Dessert' | 'Beverage';
  guestSeatNo?: number;
  isSentToKitchen?: boolean;
}

export interface Table {
  id: number;
  areaId: number;
  areaName: string;
  tableNo: string;
  capacity: number;
  status: 'available' | 'occupied' | 'billed' | 'reserved' | 'cooking';
  currentOrderId?: number;
  currentOrderNo?: string;
  guestCount?: number;
  occupiedSince?: string;
  positionX: number;
  positionY: number;
  width: number;
  height: number;
  shape: 'square' | 'round' | 'rectangle';
  colorCode?: string;
  activeOrderTotal?: number;
  activeOrderMinutes?: number;
}

export interface DiningArea {
  id: number;
  branchId: number;
  name: string;
  floorNumber: number;
  sortOrder: number;
  tables: Table[];
}

export interface KOTItem {
  name: string;
  quantity: number;
  variant?: string;
  modifiers: string[];
  notes?: string;
}

export interface KOT {
  id: number;
  kotNo: string;
  orderNo: string;
  tableNo?: string;
  orderType: 'Dine-In' | 'Takeaway' | 'Delivery' | 'Quick Sale';
  station: string;
  status: 'Queued' | 'Preparing' | 'Ready' | 'Served';
  elapsedMinutes: number;
  items: KOTItem[];
  sentAt: string;
}

export interface Customer {
  id: number;
  name: string;
  phone: string;
  loyaltyPoints: number;
  loyaltyTier: 'Bronze' | 'Silver' | 'Gold' | 'Platinum';
  address: string;
  zone: string;
}

export interface RecipeIngredient {
  name: string;
  unit: string;
  quantity: number;
  wastePercent: number;
  unitCost: number;
  lineCost: number;
}

export interface Recipe {
  productId: number;
  productName: string;
  sellingPrice: number;
  totalCost: number;
  marginPercent: number;
  ingredients: RecipeIngredient[];
}

export interface InventoryItem {
  id: number;
  name: string;
  unit: string;
  currentStock: number;
  reorderLevel: number;
  avgCost: number;
  category?: string;
}

export interface Supplier {
  id: number;
  code: string;
  name: string;
  phone?: string;
  email?: string;
  address?: string;
  openingBalance: number;
}

export interface PurchaseOrderLine {
  id?: number;
  inventoryItemId: number;
  itemName: string;
  unitCode: string;
  qty: number;
  unitCost: number;
  taxAmount: number;
  lineTotal: number;
}

export interface PurchaseOrder {
  id: number;
  branchId: number;
  supplierId: number;
  supplierName: string;
  supplierPhone?: string;
  supplierAddress?: string;
  poNo: string;
  poDateUtc: string;
  status: 'Draft' | 'Approved' | 'PartiallyReceived' | 'Received' | 'Cancelled';
  total: number;
  linesCount: number;
  lines?: PurchaseOrderLine[];
}

export interface GoodsReceipt {
  id: number;
  branchId: number;
  supplierId: number;
  supplierName: string;
  grnNo: string;
  receiptDateUtc: string;
  status: 'Draft' | 'Posted' | 'Cancelled';
  total: number;
}

export interface StockMovement {
  id: number;
  itemName: string;
  txnType: 'PO Receipt' | 'Order Consumption' | 'Manual Adjustment' | 'Wastage';
  referenceNo: string;
  qtyChange: number;
  unitCost: number;
  balanceAfter: number;
  timestamp: string;
}

export interface CashShift {
  id: number;
  branchId: number;
  userId: number;
  userName: string;
  openedAtUtc: string;
  openingCash: number;
  closedAtUtc?: string;
  closingCash?: number;
  expectedCash?: number;
  variance?: number;
  notes?: string;
  count1000: number;
  count500: number;
  count200: number;
  count100: number;
  count50: number;
  count20: number;
  count10: number;
  declaredCash: number;
  movements: CashMovement[];
}

export interface CashMovement {
  id: number;
  movementType: 'CashIn' | 'CashOut' | 'Drop';
  amount: number;
  reason: string;
  createdAtUtc: string;
}

export interface TableBooking {
  id: number;
  branchId: number;
  bookingNo: string;
  bookingType: 'Table' | 'Chair';
  tableId: number;
  tableNo: string;
  seatNumber?: number | null;
  customerName: string;
  customerPhone: string;
  customerEmail?: string;
  guestCount: number;
  bookingDate: string;
  timeSlot: string;
  status: 'Pending' | 'Confirmed' | 'Seated' | 'Cancelled' | 'Completed';
  specialRequests?: string;
  depositAmount: number;
  isDepositPaid: boolean;
  createdAtUtc: string;
}



