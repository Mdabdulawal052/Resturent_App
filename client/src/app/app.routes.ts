import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'pos', pathMatch: 'full' },
  {
    path: 'pos',
    loadComponent: () => import('./features/pos/pos-terminal.component').then(m => m.PosTerminalComponent)
  },
  {
    path: 'kds',
    loadComponent: () => import('./features/kds/kds-board.component').then(m => m.KdsBoardComponent)
  },
  {
    path: 'tables',
    loadComponent: () => import('./features/tables/table-map.component').then(m => m.TableMapComponent)
  },
  {
    path: 'bookings',
    loadComponent: () => import('./features/bookings/table-bookings.component').then(m => m.TableBookingsComponent)
  },
  {
    path: 'menu',
    loadComponent: () => import('./features/menu/recipe-bom.component').then(m => m.RecipeBomComponent)
  },
  {
    path: 'purchasing',
    loadComponent: () => import('./features/purchasing/purchasing-orders.component').then(m => m.PurchasingOrdersComponent)
  },
  {
    path: 'inventory',
    loadComponent: () => import('./features/inventory/stock-balances.component').then(m => m.StockBalancesComponent)
  },
  {
    path: 'shifts',
    loadComponent: () => import('./features/shifts/cash-shift.component').then(m => m.CashShiftComponent)
  },
  {
    path: 'crm',
    loadComponent: () => import('./features/crm/customer-crm.component').then(m => m.CustomerCrmComponent)
  },
  {
    path: 'reports',
    loadComponent: () => import('./features/reports/sales-reports.component').then(m => m.SalesReportsComponent)
  },
  { path: '**', redirectTo: 'pos' }
];
