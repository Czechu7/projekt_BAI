import { Component, OnInit, inject } from '@angular/core';
import { AccountService } from '../../_services/account.service';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-cart-id',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './cart-id.component.html',
  styleUrl: './cart-id.component.css'
})
export class CartIdComponent implements OnInit {
  accountService = inject(AccountService);
  route = inject(ActivatedRoute);
  router = inject(Router);
  toastr = inject(ToastrService);
  
  cart: any;
  id: number = 0;

  ngOnInit() {
    this.id = this.route.snapshot.params['id'];
    this.loadCart();
  }

  loadCart() {
    this.accountService.getCartById(this.id).subscribe({
      next: cart => {
        this.cart = cart;
        this.checkCartOwner();
      },
      error: error => {
        console.error(error);
        this.toastr.error('Failed to load cart');
        this.router.navigateByUrl('/cart');
      }
    });
  }

  private checkCartOwner() {
    const currentUser = this.accountService.currentUser();
    if (!currentUser) {
      this.toastr.error('Please login first');
      this.router.navigateByUrl('/');
      return;
    }

    if (this.cart.user.id !== currentUser.id) {
      this.toastr.error('You can only view your own cart');
      this.router.navigateByUrl('/cart');
    }
  }
}