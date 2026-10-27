import { Component, signal, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Recipe } from '../../models';
import { ApiService } from '../../core/services/api.service';

@Component({
  selector: 'app-recipe-bom',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './recipe-bom.component.html',
  styleUrl: './recipe-bom.component.css'
})
export class RecipeBomComponent implements OnInit {
  private api = inject(ApiService);

  recipes = signal<Recipe[]>([]);
  isLoading = signal<boolean>(true);

  ngOnInit(): void {
    this.isLoading.set(true);

    this.api.getProducts().subscribe({
      next: products => {
        if (!products || products.length === 0) {
          this.recipes.set([]);
          this.isLoading.set(false);
          return;
        }

        const recipeList: Recipe[] = [];
        let pending = products.length;

        for (const p of products) {
          this.api.getRecipe(p.id).subscribe({
            next: r => {
              if (r && r.lines && r.lines.length > 0) {
                recipeList.push({
                  productId: r.productId,
                  productName: r.productName,
                  sellingPrice: r.sellingPrice,
                  totalCost: r.totalCost,
                  marginPercent: r.grossMarginPercent,
                  ingredients: (r.lines || []).map((l: any) => ({
                    name: l.inventoryItemName,
                    unit: l.unitCode,
                    quantity: l.quantity,
                    wastePercent: l.wastePercent,
                    unitCost: Math.round((l.componentCost / (l.quantity || 1)) * 100) / 100,
                    lineCost: l.componentCost
                  }))
                });
              }
              pending--;
              if (pending <= 0) {
                this.recipes.set(recipeList);
                this.isLoading.set(false);
              }
            },
            error: () => {
              pending--;
              if (pending <= 0) {
                this.recipes.set(recipeList);
                this.isLoading.set(false);
              }
            }
          });
        }
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }
}
