# Implementation Plan: Odoo-Style Commercial Restaurant POS for Bangladesh Market

## Executive Summary & Commercial Vision
The goal of this plan is to upgrade our Restaurant POS into a **commercial-grade, market-ready Restaurant Management & POS software tailored specifically for the Bangladesh (BD) market**, heavily benchmarked against **Odoo Restaurant POS**. 

To successfully sell software to restaurants in Bangladesh (ranging from trendy cafes and multi-floor restaurants in Dhanmondi, Gulshan, and Banani to bustling biryani houses in Old Dhaka, fast food chains, and rooftop lounges), the solution must combine **Odoo's world-class visual table and dining workflow** with **Bangladesh's strict regulatory compliance (NBR Mushak 6.3), local payment systems (bKash/Nagad/Rocket/Cards), aggregator management (Foodpanda/Pathao), and local operational realities (cash drawer denomination control, offline resilience, and Bengali receipts)**.

---

## 1. Deep Comparison: Current State vs. Odoo Standard vs. Bangladesh Market Needs

| Capability | Current Basic POS | Odoo Restaurant POS Benchmark | Required for BD Commercial Sale |
| :--- | :--- | :--- | :--- |
| **Floor & Table View** | Static grid of table buttons | Multi-floor interactive visual canvas, drag-and-drop table layout editor, custom shapes, seat counts, live order status badges | Multi-floor visual map (e.g. Ground Floor, Family Section AC, Rooftop), table merging, table transfer, cover tracking |
| **Order Flow** | Cart resets or immediate checkout | Table-linked order sessions, multi-order tabs, hold/resume, waitstaff assignments | Waiter-friendly touch ordering, table hold, fast-switch between active tables, guest cover tracking |
| **Kitchen & KOT** | Single general KDS board | Course management (Starters, Mains, Desserts), station routing (Kitchen, Bar), delta KOT printing | Running KOT tokens, reprint delta KOTs (highlighting newly added items), course-wise kitchen fire notifications |
| **Bill Splitting** | Single bill settlement only | Split by products, split by seat/guest, split equally into N shares | Split by item, split by guest/seat, split equally (group dining in BD), combine bills |
| **Payments** | Basic dropdown selection | Multi-currency, partial payments, integrated terminal slips | **bKash & Nagad MFS with TrxID logging**, Card slip/Auth code, Cash denomination counter (৳1000 to ৳10), Credit/Due (বাকির খাতা) |
| **Tax & Invoicing** | Flat VAT calculation | Standard fiscal taxes | **NBR Mushak 6.3 (মুসক-৬.৩) compliance**, BIN/TIN, AC (10%/15%) vs Non-AC (5%) VAT, Service Charge rules, 80mm Bangla/English thermal receipt |
| **Cash Control** | Simple opening/closing balance | Cashier sessions, cash in/out, denomination counting, over/short variance | Cash float, daily kitchen bazaar/petty cash out, BD note counter (৳1000, ৳500, etc.), Cashier Z-Report |
| **Inventory & Food Costing** | Simple stock balance view | Multi-level Bill of Materials (BOM), automated manufacturing/consumption | Recipe BOM (e.g. Kacchi Biryani ingredients), auto raw-material stock deduction, kitchen spoilage log |
| **Aggregator Channels** | None | Omnichannel POS connectors | Foodpanda & Pathao Food order tagging, commission deduction tracking, rider pickup tags |

---

## 2. Core Modules & Business Logic Architecture

### Module 1: Odoo-Style Interactive Multi-Floor & Table Management
- **Multi-Floor & Section Support**: Support multiple dining areas (e.g., *Main Dining AC*, *1st Floor Family*, *Rooftop Garden*, *Private VIP Cabin*).
- **Interactive Visual Floor Plan**:
  - Tables rendered with true coordinates (`x, y`), width, height, shape (Round, Square, Rectangle), and seat count.
  - Live Table Status:
    - 🟢 **Available / Vacant**: Ready for seating.
    - 🔵 **Occupied / Seated**: Guests seated, ordering in progress.
    - 🟠 **KOT Fired**: Food cooking in kitchen (shows elapsed dining minutes).
    - 🟡 **Bill Printed (Pre-Bill)**: Pro-forma invoice issued, awaiting payment.
    - 🔴 **Reserved / VIP**.
- **Floor Plan Designer Mode (Odoo Pencil Tool)**:
  - Add Table, Move/Drag Table, Resize, Change Seats (2, 4, 6, 8, 12), Rename (e.g. *T-10*, *VIP-2*, *Roof-5*), Delete Table.
- **Table Operations**:
  - **Table Transfer**: Move table order from Table A to Table B with all items and KOT history intact.
  - **Table Merge**: Combine Table A and Table B for large family or corporate gatherings.
  - **Guest Count (Covers)**: Record number of guests seated (tracks Average Spend per Cover KPI).

### Module 2: Order Taking, Course Sequencing & Modifiers
- **Order Types**:
  - **Dine-In**: Bound to floor plan tables and seat covers.
  - **Takeaway / Parcel**: Quick customer phone/name capture, pickup token number.
  - **Delivery**: Address, delivery charge, rider assignment.
  - **Aggregator**: Foodpanda, Pathao Food (with commission deduction percentage).
- **Course Sequence**:
  - Assign items to **Starter / Appetizer**, **Main Course**, **Dessert**, **Beverage / Bar**.
  - **"Fire Course" Action**: Fire mains to kitchen once starters are served.
- **Modifiers & Kitchen Notes**:
  - Variants: e.g., Half / Full, Regular / Large.
  - Add-ons: Extra Cheese (+৳60), Extra Gravy (+৳40), Extra Potato (+৳30).
  - Cooking Instructions: "Less Spicy", "No Onion/Garlic", "Spicy / ঝাল বেশি", "Well Done".
- **Real-Time KOT Dispatch & Delta Tracking**:
  - When waitstaff taps **"Send to Kitchen (KOT)"**:
    - Only newly added items are dispatched.
    - Prints Kitchen Order Ticket (KOT) with sequence token.
    - Updates item status to `SentToKitchen`.

### Module 3: Odoo-Grade Bill Splitting Engine
- **Split by Product**:
  - Interactive split screen showing full order.
  - Click items or partial quantities to move them to **Bill 1**, leaving remaining items in **Bill 2**.
  - Settle Bill 1 immediately while Bill 2 remains open on the table.
- **Split by Guest / Seat**:
  - Assign items to Guest 1, Guest 2, Guest 3, etc.
  - One-click print individual pre-bills for each guest.
- **Split Equally**:
  - Divide total amount equally by $N$ guests (e.g. ৳4,500 total $\div$ 3 people = ৳1,500 each).

### Module 4: Bangladesh Payment Engine & MFS Reconciliation
- **Supported Payment Tenders**:
  - **Cash**: Smart Bangladesh currency quick buttons (৳1000, ৳500, ৳200, ৳100, ৳50, ৳20) + automatic change calculation.
  - **bKash Merchant**: Dynamic QR code display, mandatory 10-character Alphanumeric TrxID logging (e.g., `BK9X872L11`) to prevent cashier fraud.
  - **Nagad**: Merchant QR + TrxID verification.
  - **Rocket / Upay**: Alternative local MFS options.
  - **Card POS**: Visa / Mastercard / Amex with Bank Terminal Slip / Auth Code entry.
  - **Credit / Due Customer (বাকির খাতা)**: Corporate customer or VIP house account with credit limit and customer ledger.
  - **Split Tender**: Combine multiple modes on a single bill (e.g., ৳1,000 Cash + ৳1,500 bKash).

### Module 5: Bangladesh NBR VAT & Mushak 6.3 Compliance Engine
- **National Board of Revenue (NBR) Compliant Invoicing**:
  - Formatted under the **VAT & SD Act 2016** as **Mushak 6.3 (মুসক-৬.৩)** tax invoice.
  - Registered Business Identification Number (**BIN**) and **TIN** display.
  - Unbroken sequential invoice numbering.
  - Dual VAT Rates:
    - **AC Restaurant**: Standard 10% or 15% VAT.
    - **Non-AC Restaurant**: 5% or 7.5% VAT.
    - **Fast Food / Bakery**: Configurable tax classes.
  - Service Charge calculation (5% - 10%) with selectable VAT mode:
    - *Mode A*: VAT calculated on Subtotal only.
    - *Mode B*: VAT calculated on (Subtotal + Service Charge) per NBR guidelines.
  - Thermal 80mm & 58mm printer receipts with bilingual (Bangla / English) support, BDT currency sign (৳), and verification QR code.

### Module 6: Cashier Sessions, Shift Control & BD Currency Denomination
- **Opening Session (Float)**:
  - Cashier enters opening drawer float (e.g., ৳3,000).
- **Petty Cash / Bazaar Out**:
  - Cash Out for urgent kitchen supplies (e.g., ৳450 for eggs, ৳300 for ice) with reason and expense voucher.
  - Cash In for drawer top-ups.
- **Closing Session with Bangladesh Currency Denomination**:
  - Cashier inputs counts for each Bangladeshi banknote:
    - ৳1,000 notes $\times$ count
    - ৳500 notes $\times$ count
    - ৳200 notes $\times$ count
    - ৳100 notes $\times$ count
    - ৳50 notes $\times$ count
    - ৳20 notes $\times$ count
    - ৳10 notes $\times$ count
  - System calculates total counted vs. expected cash (Opening Float + Cash Sales + Cash In - Cash Out).
  - Automatically highlights **Over / Short Variance** and generates the **Cashier Shift Z-Report**.

### Module 7: Recipe / BOM Food Costing & Kitchen Inventory
- **Bill of Materials (BOM)**:
  - Link menu items to raw ingredients (e.g., *1 Plate Kacchi Biryani* = 250g Mutton, 180g Basmati Rice, 25g Ghee, 1 Potato, Spices).
- **Auto Stock Consumption**:
  - Upon order settlement or KOT completion, raw materials are automatically deducted from the kitchen inventory ledger.
- **Kitchen Spoilage & Wastage Entry**:
  - Daily entry of damaged or spoiled raw materials with cost attribution.

### Module 8: Kitchen Display System (KDS) & Multi-Station Routing
- Real-time kitchen dashboard with audio chime on new orders.
- Color-coded urgency alerts:
  - 🟢 **Normal**: < 10 minutes.
  - 🟡 **Warning**: 10 - 20 minutes.
  - 🔴 **Delayed**: > 20 minutes.
- Station filters: *All Kitchen*, *Hot Kitchen / Grill*, *Bar / Beverages*, *Desserts*.
- Interactive Bump Bar: One-click status update (`In Prep` $\rightarrow$ `Ready to Serve` $\rightarrow$ `Served`).

---

## 3. Database Schema Changes & Entity Upgrades

We will enrich the domain models and database tables:
1. **PosEntities**:
   - `DiningArea`: Add `FloorNumber`, `Width`, `Height`.
   - `RestaurantTable`: Add `PositionX`, `PositionY`, `Width`, `Height`, `Shape` (*Round*, *Square*, *Rectangle*), `ColorCode`.
   - `TableSession`: Add `ActiveWaitstaffId`, `CoverCount`, `PreBillPrintedAtUtc`.
2. **OrderEntities**:
   - `Order`: Add `CourseType`, `GuestSeatNo`, `MushakChallanNo`, `AggregatorName`, `AggregatorCommissionPercent`, `IsPreBilled`, `SplitParentOrderId`.
   - `OrderItem`: Add `CourseType` (*Starter*, *Main*, *Dessert*, *Drink*), `IsSentToKitchen`, `KotTokenNo`, `GuestSeatNo`.
3. **FinanceEntities**:
   - `CashShift`: Add denomination breakdown columns (`Count1000`, `Count500`, `Count200`, `Count100`, `Count50`, `Count20`, `Count10`).
   - `Payment`: Add `MfsTrxId`, `CardSlipNo`, `AggregatorRefNo`.
4. **CrmEntities**:
   - `CustomerLedger`: Due/Baki balance, credit limit, and due payment vouchers.

---

## 4. UI / UX Design & Component Layout (Angular 22)

### A. Floor Plan Component (`/tables`)
- Interactive canvas with tabs for each floor/section.
- **Top Bar**: Floor selector, Quick Search Table, Table Stats (Vacant / Occupied / Billed), **"Edit Floor" Toggle Button**.
- **Edit Canvas Mode**:
  - Palette of shapes: Round Table, Square Table, Long Table.
  - Drag tables to exact positions, resize, adjust seat count.
  - Save Layout button.
- **Table Tile**:
  - Displays Table Name, Capacity icon, Occupancy time, Order Subtotal badge, Status indicator.
  - Double click or tap directly opens the POS Order screen for that table.

### B. POS Terminal Component (`/pos`)
- **Header Bar**:
  - Back to Floor Map button (with current table pill: e.g., `Table T-04 (Floor 1)`).
  - Order Mode selector: Dine-In, Takeaway, Delivery, Foodpanda, Pathao.
  - Waiter / Cashier badge, Active Shift badge.
- **Product Area**:
  - Fast Category pills with icons.
  - Visual product cards with price in `৳`, dietary tag (Halal/Spicy/Chef Special), stock badge.
  - Quick modifier popup when clicking customizable items.
- **Order Panel (Right Side)**:
  - Table name, Guest Cover count input (+ / -).
  - Item list grouped by Course (*Starters*, *Mains*, *Desserts*).
  - Visual indicators: 🟢 Sent to Kitchen vs 🟡 New (Unsaved).
  - Quick Actions toolbar:
    - 🍽️ **Send KOT** (dispatches new items to kitchen).
    - 🔀 **Transfer / Merge Table**.
    - ✂️ **Split Bill** (opens Odoo-style split modal).
    - 🧾 **Print Pre-Bill** (generates guest pro-forma check).
    - 💳 **Pay & Settle** (opens BD Multi-Tender payment modal).

### C. Split Bill Modal (`split-bill-modal`)
- Shows **Current Bill** on left and **New Bill** on right.
- Three split strategies:
  1. **By Item / Quantity**: Click arrow buttons to transfer items between bills.
  2. **By Seat / Guest**: Automatically group items by guest seat number.
  3. **Equal Split**: Enter number of payers (e.g. 4) $\rightarrow$ generates 4 equal payments.

### D. BD Payment & Settlement Modal
- Payment tabs: **Cash**, **bKash**, **Nagad**, **Card POS**, **Due / Credit**.
- **Cash**: Numpad + Quick note buttons (+৳1000, +৳500, +৳200, +৳100).
- **bKash / Nagad**: Display merchant QR code + Required input for 10-digit TrxID.
- **Card**: Input for Card Brand (Visa/Mastercard) + POS Terminal Auth Code.
- **Multi-Tender**: Split between Cash + bKash on the fly.
- Immediate trigger of **NBR Mushak 6.3 Receipt**.

### E. Cash Shift Closing Modal (`shift-closing-modal`)
- Denomination grid for Bangladesh currency notes:
  - ৳1,000 $\times$ input = subtotal
  - ৳500 $\times$ input = subtotal
  - ৳200 $\times$ input = subtotal
  - ৳100 $\times$ input = subtotal
  - ৳50 $\times$ input = subtotal
  - ৳20 $\times$ input = subtotal
  - ৳10 $\times$ input = subtotal
- Live calculation of Total Counted vs. Expected System Cash.
- Shows Variance (+Over / -Short).
- Prints **Daily Shift Z-Report**.

---

## 5. Proposed File Changes & Implementation Steps

```
Resturent/
├── backend/src/
│   ├── Restaurant.Domain/
│   │   └── Entities/
│   │       ├── PosEntities.cs (Add X,Y coordinates, shape, floor number, cover count)
│   │       ├── OrderEntities.cs (Add courses, split parent, KOT tracking, aggregator)
│   │       └── FinanceEntities.cs (Add BD denomination fields, MFS trx reference)
│   ├── Restaurant.Application/
│   │   ├── Services/
│   │   │   ├── TableService.cs (Table transfer, merge, layout update)
│   │   │   ├── OrderService.cs (KOT dispatch, course sequencing, split bill)
│   │   │   └── ShiftService.cs (Denomination reconciliation, Z-report)
│   ├── Restaurant.Api/
│   │   └── Controllers/
│   │       ├── TablesController.cs (Layout update, transfer, merge endpoints)
│   │       ├── OrdersController.cs (Split order, fire KOT, pre-bill endpoints)
│   │       └── ShiftsController.cs (Denomination closing endpoint)
└── client/src/app/
    ├── features/
    │   ├── tables/
    │   │   ├── table-map.component.ts (Interactive Odoo floor plan + edit mode)
    │   │   ├── table-map.component.html (Visual canvas with draggable tables)
    │   │   └── table-map.component.css (Glassmorphic floor canvas styling)
    │   ├── pos/
    │   │   ├── pos-terminal.component.ts (Course grouping, KOT delta, bill actions)
    │   │   ├── pos-terminal.component.html (Odoo layout: floor link, cover count, split button)
    │   │   └── pos-terminal.component.css
    │   ├── pos/modals/
    │   │   ├── split-bill-modal.component.ts (Product/guest/equal split engine)
    │   │   └── table-transfer-modal.component.ts (Transfer & merge UI)
    │   ├── shifts/
    │   │   ├── cash-shift.component.ts (BD Denomination note counter, Z-Report)
    │   │   └── cash-shift.component.html
    │   └── kds/
    │       ├── kds-board.component.ts (Course-wise kitchen tickets, audio alerts)
    │       └── kds-board.component.html
    └── shared/components/
        └── receipt-preview/
            ├── receipt-preview.component.ts (NBR Mushak 6.3 engine, Bangla font)
            └── receipt-preview.component.html
```

---

## 6. Verification Plan

### Automated Verification
1. **Backend Build & Unit Tests**:
   - Run `dotnet build` on the backend solution to ensure zero compilation errors.
2. **Frontend Angular Build**:
   - Run `npm run build` inside `client/` to verify TypeScript typings and template bindings.

### Manual & Interactive Verification
1. **Floor Plan & Table Designer**:
   - Switch floors (Ground Floor vs. Rooftop).
   - Enter Edit Mode, move a table, change seat capacity, save, and verify persistence.
   - Click a table $\rightarrow$ ensure POS opens with table pre-selected.
2. **Order & Kitchen Dispatch**:
   - Add Starters and Mains, send KOT $\rightarrow$ verify KOT ticket generation and items marked as sent.
   - Add another item $\rightarrow$ send KOT $\rightarrow$ verify only new item is in KOT delta.
   - Verify tickets appear on `/kds` board with course tags and elapsed timers.
3. **Bill Splitting & Pre-Bill**:
   - Open active table order $\rightarrow$ Click "Split Bill" $\rightarrow$ split 2 items to Bill 2 $\rightarrow$ settle Bill 1 $\rightarrow$ verify Bill 2 remains open.
   - Click "Print Pre-Bill" $\rightarrow$ verify table turns to `Billed` status on floor map.
4. **Bangladesh Payment & Mushak 6.3**:
   - Settle bill using bKash $\rightarrow$ input TrxID $\rightarrow$ verify NBR Mushak 6.3 receipt with BIN, sequential number, and Bangla/English print layout.
5. **Cash Drawer & Denomination Closing**:
   - Open `/shifts` $\rightarrow$ count BD notes (৳1000, ৳500, etc.) $\rightarrow$ verify variance calculation and Z-Report output.
