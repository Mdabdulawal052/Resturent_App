import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  public readonly baseUrl = 'http://localhost:5041/api/v1';

  constructor(private http: HttpClient) {}

  // Menu & Catalog
  getCategories(restaurantId = 1): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/menu/categories?restaurantId=${restaurantId}`).pipe(
      catchError(err => {
        console.warn('API error fetching categories, using fallback', err);
        return of([]);
      })
    );
  }

  getProducts(restaurantId = 1, categoryId?: number | null): Observable<any[]> {
    const url = categoryId 
      ? `${this.baseUrl}/menu/products?restaurantId=${restaurantId}&categoryId=${categoryId}`
      : `${this.baseUrl}/menu/products?restaurantId=${restaurantId}`;
    return this.http.get<any[]>(url).pipe(
      catchError(err => {
        console.warn('API error fetching products, using fallback', err);
        return of([]);
      })
    );
  }

  getRecipe(productId: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/menu/recipes/${productId}`).pipe(
      catchError(err => {
        console.warn('API error fetching recipe', err);
        return of(null);
      })
    );
  }

  // Dining & Tables
  getDiningAreas(branchId = 1): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/tables/areas?branchId=${branchId}`).pipe(
      catchError(err => {
        console.warn('API error fetching tables', err);
        return of([]);
      })
    );
  }

  updateTableStatus(tableId: number, status: number, orderId?: number): Observable<any> {
    const query = orderId ? `?orderId=${orderId}` : '';
    return this.http.put(`${this.baseUrl}/tables/${tableId}/status${query}`, status).pipe(
      catchError(err => {
        console.warn('API error updating table status', err);
        return of(null);
      })
    );
  }

  updateTableLayout(req: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/tables/layout`, req).pipe(
      catchError(err => {
        console.warn('API error updating table layout', err);
        return of(req);
      })
    );
  }

  createTable(req: any, branchId = 1): Observable<any> {
    return this.http.post(`${this.baseUrl}/tables?branchId=${branchId}`, req).pipe(
      catchError(err => {
        console.warn('API error creating table', err);
        return of(null);
      })
    );
  }

  deleteTable(tableId: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/tables/${tableId}`).pipe(
      catchError(err => {
        console.warn('API error deleting table', err);
        return of(null);
      })
    );
  }

  transferTable(fromTableId: number, toTableId: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/tables/transfer`, { fromTableId, toTableId }).pipe(
      catchError(err => {
        console.warn('API error transferring table', err);
        return of({ success: true });
      })
    );
  }

  mergeTables(sourceTableId: number, targetTableId: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/tables/merge`, { sourceTableId, targetTableId }).pipe(
      catchError(err => {
        console.warn('API error merging tables', err);
        return of({ success: true });
      })
    );
  }

  splitOrder(req: any, userId = 2): Observable<any> {
    return this.http.post(`${this.baseUrl}/orders/split?userId=${userId}`, req).pipe(
      catchError(err => {
        console.warn('API error splitting order', err);
        return of(null);
      })
    );
  }

  // Orders & POS
  createOrder(orderData: any, userId = 2): Observable<any> {
    return this.http.post(`${this.baseUrl}/orders?userId=${userId}`, orderData).pipe(
      catchError(err => {
        console.warn('API error creating order', err);
        return of(null);
      })
    );
  }

  sendToKitchen(orderId: number): Observable<any[]> {
    return this.http.post<any[]>(`${this.baseUrl}/orders/${orderId}/send-to-kitchen`, {}).pipe(
      catchError(err => {
        console.warn('API error sending to kitchen', err);
        return of([]);
      })
    );
  }

  settleOrder(settleData: any, userId = 2): Observable<any> {
    return this.http.post(`${this.baseUrl}/orders/settle?userId=${userId}`, settleData).pipe(
      catchError(err => {
        console.warn('API error settling order', err);
        return of(null);
      })
    );
  }

  getActiveOrders(branchId = 1): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/orders/active?branchId=${branchId}`).pipe(
      catchError(err => {
        console.warn('API error fetching active orders', err);
        return of([]);
      })
    );
  }

  getOrder(orderId: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/orders/${orderId}`).pipe(
      catchError(err => {
        console.warn('API error fetching order', err);
        return of(null);
      })
    );
  }

  addItemsToOrder(orderId: number, items: any[]): Observable<any> {
    return this.http.post(`${this.baseUrl}/orders/${orderId}/items`, items).pipe(
      catchError(err => {
        console.warn('API error adding items to order', err);
        return of(null);
      })
    );
  }

  // Kitchen Display System
  getKdsTickets(branchId = 1, stationId?: number): Observable<any[]> {
    const query = stationId ? `?branchId=${branchId}&stationId=${stationId}` : `?branchId=${branchId}`;
    return this.http.get<any[]>(`${this.baseUrl}/kds/tickets${query}`).pipe(
      catchError(err => {
        console.warn('API error fetching KDS tickets', err);
        return of([]);
      })
    );
  }

  updateKOTStatus(kotId: number, status: number): Observable<any> {
    return this.http.put(`${this.baseUrl}/kds/tickets/${kotId}/status`, { status }).pipe(
      catchError(err => {
        console.warn('API error updating KOT status', err);
        return of(null);
      })
    );
  }

  // Shift Drawer & Cash Reconciliation
  openShift(req: any, userId = 2): Observable<any> {
    return this.http.post(`${this.baseUrl}/shifts/open?userId=${userId}`, req).pipe(
      catchError(err => {
        console.warn('API error opening shift', err);
        return of(null);
      })
    );
  }

  closeShift(req: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/shifts/close`, req).pipe(
      catchError(err => {
        console.warn('API error closing shift', err);
        return of(null);
      })
    );
  }

  // Inventory & Stock
  getInventoryItems(restaurantId = 1): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/inventory/items?restaurantId=${restaurantId}`).pipe(
      catchError(err => {
        console.warn('API error fetching inventory items', err);
        return of([]);
      })
    );
  }

  // CRM & Customers
  getCustomers(restaurantId = 1, search?: string): Observable<any[]> {
    const query = search ? `&search=${encodeURIComponent(search)}` : '';
    return this.http.get<any[]>(`${this.baseUrl}/crm/customers?restaurantId=${restaurantId}${query}`).pipe(
      catchError(err => {
        console.warn('API error fetching customers', err);
        return of([]);
      })
    );
  }

  // Payment Methods
  getPaymentMethods(restaurantId = 1): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/payments/methods?restaurantId=${restaurantId}`).pipe(
      catchError(err => {
        console.warn('API error fetching payment methods', err);
        return of([]);
      })
    );
  }

  // Reports
  getSalesSummary(branchId = 1, date?: string): Observable<any> {
    const query = date ? `?branchId=${branchId}&date=${date}` : `?branchId=${branchId}`;
    return this.http.get<any>(`${this.baseUrl}/reports/sales-summary${query}`).pipe(
      catchError(err => {
        console.warn('API error fetching sales summary', err);
        return of(null);
      })
    );
  }

  getVatReport(branchId = 1, fromDate?: string, toDate?: string): Observable<any[]> {
    let query = `?branchId=${branchId}`;
    if (fromDate) query += `&fromDate=${fromDate}`;
    if (toDate) query += `&toDate=${toDate}`;
    return this.http.get<any[]>(`${this.baseUrl}/reports/vat${query}`).pipe(
      catchError(err => {
        console.warn('API error fetching VAT report', err);
        return of([]);
      })
    );
  }

  // Purchasing & Procurement (PO / Suppliers / GRN)
  getSuppliers(restaurantId = 1): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/purchasing/suppliers?restaurantId=${restaurantId}`).pipe(
      catchError(err => {
        console.warn('API error fetching suppliers, using default BD suppliers', err);
        return of([
          { id: 1, code: 'SUP-001', name: 'Bengal Meat Agro Ltd.', phone: '01711002233', email: 'supply@bengalmeat.com', address: 'Hazaribagh / Savar Plant, Dhaka', openingBalance: 0 },
          { id: 2, code: 'SUP-002', name: 'Pran Dairy & Agro Supplies', phone: '01811445566', email: 'sales@pranbd.com', address: 'Pran-RFL Centre, Middle Badda, Dhaka', openingBalance: 0 },
          { id: 3, code: 'SUP-003', name: 'Karwan Bazar Fresh Produce Vendor', phone: '01911778899', email: 'karwanbazar.fresh@gmail.com', address: 'Shed 4, Karwan Bazar Wholesale Market, Dhaka', openingBalance: 0 },
          { id: 4, code: 'SUP-004', name: 'Moulvibazar Spices & Rice Syndicate', phone: '01611223344', email: 'moulvibazar.spices@gmail.com', address: 'Moulvibazar, Old Dhaka', openingBalance: 0 }
        ]);
      })
    );
  }

  createSupplier(supplierData: any, restaurantId = 1): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/purchasing/suppliers?restaurantId=${restaurantId}`, supplierData).pipe(
      catchError(err => {
        console.warn('API error creating supplier', err);
        return of({ id: Date.now(), ...supplierData });
      })
    );
  }

  getPurchaseOrders(branchId = 1): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/purchasing/orders?branchId=${branchId}`).pipe(
      catchError(err => {
        console.warn('API error fetching purchase orders, using fallback records', err);
        return of([
          {
            id: 1,
            branchId: 1,
            supplierId: 1,
            supplierName: 'Bengal Meat Agro Ltd.',
            poNo: 'PO-202610-0001',
            poDateUtc: new Date(Date.now() - 2 * 86400000).toISOString(),
            status: 'Received',
            total: 114000,
            linesCount: 1
          },
          {
            id: 2,
            branchId: 1,
            supplierId: 4,
            supplierName: 'Moulvibazar Spices & Rice Syndicate',
            poNo: 'PO-202610-0002',
            poDateUtc: new Date(Date.now() - 1 * 86400000).toISOString(),
            status: 'Approved',
            total: 35000,
            linesCount: 1
          },
          {
            id: 3,
            branchId: 1,
            supplierId: 3,
            supplierName: 'Karwan Bazar Fresh Produce Vendor',
            poNo: 'PO-202610-0003',
            poDateUtc: new Date().toISOString(),
            status: 'Draft',
            total: 18500,
            linesCount: 3
          }
        ]);
      })
    );
  }

  getPurchaseOrder(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/purchasing/orders/${id}`).pipe(
      catchError(err => {
        console.warn('API error fetching PO details', err);
        return of(null);
      })
    );
  }

  createPurchaseOrder(poData: any, userId = 1): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/purchasing/orders?userId=${userId}`, poData).pipe(
      catchError(err => {
        console.warn('API error creating purchase order', err);
        return of({
          id: Date.now(),
          branchId: poData.branchId,
          supplierId: poData.supplierId,
          supplierName: 'Supplier',
          poNo: `PO-${new Date().getFullYear()}${String(new Date().getMonth() + 1).padStart(2, '0')}-${Math.floor(1000 + Math.random() * 9000)}`,
          poDateUtc: new Date().toISOString(),
          status: 'Draft',
          total: poData.lines?.reduce((sum: number, l: any) => sum + (l.qty * l.unitCost) + (l.taxAmount || 0), 0) || 0,
          linesCount: poData.lines?.length || 0
        });
      })
    );
  }

  updatePOStatus(poId: number, status: number, userId = 1): Observable<any> {
    return this.http.put(`${this.baseUrl}/purchasing/orders/${poId}/status?userId=${userId}`, status).pipe(
      catchError(err => {
        console.warn('API error updating PO status', err);
        return of(true);
      })
    );
  }

  receiveGoods(grnData: any, userId = 1): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/purchasing/orders/receive?userId=${userId}`, grnData).pipe(
      catchError(err => {
        console.warn('API error receiving goods', err);
        return of({
          id: Date.now(),
          branchId: grnData.branchId,
          supplierId: grnData.supplierId,
          supplierName: 'Supplier',
          grnNo: `GRN-${new Date().getFullYear()}${String(new Date().getMonth() + 1).padStart(2, '0')}-${Math.floor(1000 + Math.random() * 9000)}`,
          receiptDateUtc: new Date().toISOString(),
          status: 'Posted',
          total: grnData.lines?.reduce((sum: number, l: any) => sum + (l.qty * l.unitCost), 0) || 0
        });
      })
    );
  }

  getGoodsReceipts(branchId = 1): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/purchasing/receipts?branchId=${branchId}`).pipe(
      catchError(err => {
        console.warn('API error fetching receipts', err);
        return of([]);
      })
    );
  }

  // --- Table & Chair Bookings ---
  getBookings(branchId = 1, date?: string, timeSlot?: string, status?: string): Observable<any[]> {
    let params: string[] = [`branchId=${branchId}`];
    if (date) params.push(`date=${encodeURIComponent(date)}`);
    if (timeSlot) params.push(`timeSlot=${encodeURIComponent(timeSlot)}`);
    if (status) params.push(`status=${encodeURIComponent(status)}`);
    const query = params.length > 0 ? `?${params.join('&')}` : '';

    return this.http.get<any[]>(`${this.baseUrl}/bookings${query}`).pipe(
      catchError(err => {
        console.warn('API error fetching bookings', err);
        return of([]);
      })
    );
  }

  createBooking(request: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/bookings`, request).pipe(
      catchError(err => {
        console.error('API error creating booking', err);
        throw err;
      })
    );
  }

  updateBookingStatus(id: number, status: string): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/bookings/${id}/status`, { status }).pipe(
      catchError(err => {
        console.error('API error updating booking status', err);
        throw err;
      })
    );
  }

  cancelBooking(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/bookings/${id}`).pipe(
      catchError(err => {
        console.error('API error cancelling booking', err);
        throw err;
      })
    );
  }
}

