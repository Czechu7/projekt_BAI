import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { User } from '../_models/user';
import { map } from 'rxjs';
import { Products } from '../_models/products';
import { Cart } from '../_models/cart';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private http = inject(HttpClient);
  baseUrl = "https://localhost:5001/api/";
  currentUser = signal<User | null>(null)

  login(model: any) {
    return this.http.post<User>(this.baseUrl + "account/login", model).pipe(
      map(user => {
        if (user) {
          localStorage.setItem('tempUser', JSON.stringify({
            id : user.id,
            username: user.username,
            totpCode: user.totpCode
          }));
        }
        return user;
      })
    );
  }

  verifyTotp(totpCode: string) {
    const tempUser = JSON.parse(localStorage.getItem('tempUser') || '{}');
    return this.http.post<User>(this.baseUrl + "account/verify-totp", {
      id: tempUser.id,
      username: tempUser.username,
      totpCode: totpCode
    }).pipe(
      map(user => {
        if (user) {
          localStorage.removeItem('tempUser');
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUser.set(user);
        }
        return user;
      })
    );
  }


  register(model: any){
    return this.http.post<User>(this.baseUrl + "account/register", model).pipe(
      map(user =>{
        if(user){
          localStorage.setItem('user', JSON.stringify(user))
          this.currentUser.set(user);
        }
        return user;
      })
    )
  }

  logout(){
    localStorage.removeItem('user');
    this.currentUser.set(null);
  }

  getUsers(){
    return this.http.get<User>(this.baseUrl + "users");
  }

  getProducts(){
    return this.http.get<Products>(this.baseUrl + "product")
  }

  addProduct(model: Products){
    return this.http.post<Products>(this.baseUrl + "product", model)
  }

  getCart(){
    return this.http.get<Cart>(this.baseUrl + "cart")
  }

  addCart(model: Cart){
    return this.http.post<Cart>(this.baseUrl + "cart", model)
  }

  getProductById(id: number) {
    return this.http.get<Products>(`${this.baseUrl}products/${id}`);
  }
  
  getCartById(id: number) {
    return this.http.get<Cart>(`${this.baseUrl}cart/${id}`);
  }

}
