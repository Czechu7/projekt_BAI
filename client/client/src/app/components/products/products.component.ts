// products.component.ts
import { Component, inject, OnInit } from '@angular/core';
import { AccountService } from '../../_services/account.service';
import { FormsModule } from '@angular/forms';
import { Products } from '../../_models/products';


@Component({
  selector: 'app-products',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './products.component.html',
  styleUrl: './products.component.css'
})
export class ProductsComponent implements OnInit {
  accountService = inject(AccountService);
  products: any;
  newProduct: Products = {
    name: '',
    quantity: 0,
    price: 0
  };

  ngOnInit() {
    this.loadProducts();
  }

  loadProducts() {
    this.accountService.getProducts().subscribe({
      next: (products) => this.products = products,
      error: (error) => console.log(error),
      complete: () => console.log('Products loaded')
    });
  }

  addProduct() {
    this.accountService.addProduct(this.newProduct).subscribe({
      next: () => {
        this.loadProducts();
        this.resetForm();
      },
      error: (error) => console.log(error),
      complete: () => console.log('Product added')
    });
  }

  private resetForm() {
    this.newProduct = {
      name: '',
      quantity: 0,
      price: 0
    };
  }
}