import { Routes } from '@angular/router';
import { HomeComponent } from './features/public/pages/home/home.component';
import { AboutComponent } from './features/public/pages/about/about.component';
import { ContactComponent } from './features/public/pages/contact/contact.component';
import { LoginComponent } from './features/auth/pages/login/login.component';
import { RegisterComponent } from './features/auth/pages/register/register.component';
import { UpdatePhoneComponent } from './features/auth/pages/update-phone/update-phone.component';
import { DashboardComponent } from './features/dashboard/pages/dashboard/dashboard.component';
import { AdminDashboardComponent } from './features/admin/pages/dashboard/admin-dashboard.component';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';
import { guestGuard } from './core/guards/guest.guard';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'about', component: AboutComponent },
  { path: 'contact', component: ContactComponent },
  { path: 'login', component: LoginComponent, canActivate: [guestGuard] },
  { path: 'register', component: RegisterComponent, canActivate: [guestGuard] },
  { path: 'auth/update-phone', component: UpdatePhoneComponent, canActivate: [authGuard] },
  { path: 'dashboard', component: DashboardComponent, canActivate: [authGuard] },
  { path: 'admin/dashboard', component: AdminDashboardComponent, canActivate: [adminGuard] },
  { path: '**', redirectTo: '' }
];
