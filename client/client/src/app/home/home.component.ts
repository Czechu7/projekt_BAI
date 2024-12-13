import { Component, OnInit, inject } from '@angular/core';
import { RegisterComponent } from "../register/register.component";
//import { ContactComponent } from "../contact/contact.component"; // Add this import
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { CarouselModule } from 'ngx-bootstrap/carousel';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    RegisterComponent,
 //   ContactComponent,
    CommonModule,
    CarouselModule
  ],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  http = inject(HttpClient);
  registerMode = false;
  contactMode = false; 
  users: any;
  
  slideConfig = {
    slidesToShow: 1,
    slidesToScroll: 1,
    autoplay: true,
    autoplaySpeed: 3000,
    arrows: false,
    dots: true,
    fade: true
  };

  testimonialConfig = {
    slidesToShow: 3,
    slidesToScroll: 1,
    autoplay: true,
    autoplaySpeed: 4000,
    arrows: true,
    dots: true,
    responsive: [
      {
        breakpoint: 768,
        settings: {
          slidesToShow: 1
        }
      }
    ]
  };

  ngOnInit(): void {
    this.getUsers();
  }

  registerToggle() {
    this.registerMode = !this.registerMode;
  }

  contactToggle() {
    this.contactMode = !this.contactMode;
  }

  cancelRegisterMode(event: boolean) {
    this.registerMode = event;
  }

  cancelContactMode(event: boolean) {
    this.contactMode = event;
  }
  
  getUsers() {
    this.http.get('https://localhost:5001/api/users').subscribe({
      next: response => this.users = response,
      error: error => console.log(error),
      complete: () => console.log('Request has been completed')
    });
  }
}