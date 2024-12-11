import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../../_services/account.service';
import { Products } from '../../_models/products';
import { Cart } from '../../_models/cart';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.css']
})
// cart.component.ts
export class CartComponent implements OnInit {
  accountService = inject(AccountService);
  router = inject(Router);
  toastr = inject(ToastrService);

  products: any;
  carts: any;
  newCart: Cart = {
    productId: 0,
    userId: this.getCurrentUserId() // Add method to get current user ID
  };

  private getCurrentUserId(): number {
    const user = this.accountService.currentUser();
    return user ? Number(user.id) : 0;
  }

  ngOnInit() {
    this.loadProducts();
    this.loadCarts();

    // Set userId when component initializes
    const userId = this.getCurrentUserId();
    if (userId) {
      this.newCart.userId = userId;
    } else {
      this.toastr.error('Please login first');
      this.router.navigateByUrl('/');
    }
  }

  loadProducts() {
    this.accountService.getProducts().subscribe({
      next: (products: any) => {
        this.products = Array.isArray(products) ? products : [products];
        console.log('Products loaded:', this.products);
      },
      error: (error) => {
        this.toastr.error('Failed to load products');
        console.error('Error loading products:', error);
      }
    });
  }

  loadCarts() {
    this.accountService.getCart().subscribe({
      next: (carts: any) => {
        this.carts = Array.isArray(carts) ? carts : [carts];
        console.log('Carts loaded:', this.carts);
      },
      error: (error) => {
        this.toastr.error('Failed to load cart');
        console.error('Error loading cart:', error);
      }
    });
  }

  // Add missing methods
  addCart() {
    if (!this.newCart.userId) {
      this.toastr.error('Please login first');
      return;
    }

    if (this.newCart.productId === 0) {
      this.toastr.error('Please select a product');
      return;
    }

    this.accountService.addCart(this.newCart).subscribe({
      next: () => {
        this.loadCarts();
        this.toastr.success('Added to cart');
       
        this.newCart.productId = 0;
      },
      error: (error) => {
        this.toastr.error('Failed to add to cart');
        console.error('Error adding to cart:', error);
      }
    });
  }

  navigateToCart(id: number) {
    this.router.navigateByUrl(`/cart/${id}`);
  }
}