# ZyntraPay - Complete API Endpoints Reference

**Project**: ZyntraPay Microservices Backend  
**Framework**: ASP.NET Core 8 Web API  
**Architecture**: Microservices with Ocelot Gateway  
**Gateway Base URL**: `http://localhost:5000/gateway` (HTTP) or `https://localhost:5001/gateway` (HTTPS)  
**Authentication**: JWT Bearer Token

---

## Common Notes

- For protected endpoints, include header: `Authorization: Bearer {token}`.
- JSON property names are case-insensitive, but examples below use the actual DTO names.
- `AuthUserId` is taken from JWT claims by controllers (not passed in request body for user endpoints).

---

## Auth Service (`/gateway/auth`)

### 1) Register User
- **POST** `/gateway/auth/register`
- **Request Body**:
  ```json
  {
    "email": "john.doe@example.com",
    "phoneNumber": "9876543210",
    "password": "SecurePass123!"
  }
  ```
- **Success Response (200)**:
  ```json
  {
    "message": "User registered successfully"
  }
  ```

### 2) Register Admin
- **POST** `/gateway/auth/register-admin`
- **Request Body**:
  ```json
  {
    "email": "admin@company.com",
    "phoneNumber": "9000000001",
    "password": "AdminPass@123",
    "adminSecretKey": "ADMIN_SECRET_KEY_2024"
  }
  ```
- **Success Response (200)**:
  ```json
  {
    "message": "Admin registered successfully"
  }
  ```

### 3) Login
- **POST** `/gateway/auth/login`
- **Request Body**:
  ```json
  {
    "email": "john.doe@example.com",
    "password": "SecurePass123!"
  }
  ```
- **Success Response (200)**:
  ```json
  {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "email": "john.doe@example.com",
    "role": "User",
    "phoneUpdateRequired": false
  }
  ```

### 4) Google Login
- **POST** `/gateway/auth/google-login`
- **Request Body**:
  ```json
  {
    "idToken": "google-id-token"
  }
  ```
- **Success Response (200)**:
  ```json
  {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "email": "john.doe@example.com",
    "role": "User",
    "phoneUpdateRequired": true
  }
  ```

### 5) Send OTP
- **POST** `/gateway/auth/send-otp`
- **Request Body**:
  ```json
  {
    "email": "john.doe@example.com"
  }
  ```
- **Success Response (200)**:
  ```json
  {
    "message": "OTP sent successfully"
  }
  ```

### 6) Verify OTP
- **POST** `/gateway/auth/verify-otp`
- **Request Body**:
  ```json
  {
    "email": "john.doe@example.com",
    "otp": "123456"
  }
  ```
- **Success Response (200)**:
  ```json
  {
    "message": "OTP verified successfully"
  }
  ```

### 7) Forgot Password
- **POST** `/gateway/auth/forgot-password`
- **Request Body**:
  ```json
  {
    "email": "john.doe@example.com"
  }
  ```
- **Success Response (200)**:
  ```json
  {
    "message": "Password reset OTP sent"
  }
  ```

### 8) Reset Password
- **POST** `/gateway/auth/reset-password`
- **Request Body**:
  ```json
  {
    "email": "john.doe@example.com",
    "otp": "123456",
    "newPassword": "NewPass@123"
  }
  ```
- **Success Response (200)**:
  ```json
  {
    "message": "Password reset successful"
  }
  ```

### 9) Refresh Token
- **POST** `/gateway/auth/refresh-token`
- **Request Body**:
  ```json
  {
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
  ```
- **Success Response (200)**:
  ```json
  {
    "token": "new-jwt-token",
    "refreshToken": "new-refresh-token",
    "email": "john.doe@example.com",
    "role": "User",
    "phoneUpdateRequired": false
  }
  ```

### 10) Update Phone
- **PUT** `/gateway/auth/update-phone` (Authenticated)
- **Request Body**:
  ```json
  {
    "phoneNumber": "9123456789"
  }
  ```
- **Success Response (200)**:
  ```json
  {
    "message": "Phone number updated successfully"
  }
  ```

### 11) Get All Users (Admin)
- **GET** `/gateway/auth/admin/users` (Admin)
- **Success Response (200)**:
  ```json
  [
    {
      "id": 1,
      "email": "john.doe@example.com",
      "phoneNumber": "9876543210",
      "role": "User",
      "isActive": true,
      "createdAt": "2026-03-31T10:00:00Z"
    }
  ]
  ```

### 12) Toggle User Status (Admin)
- **PUT** `/gateway/auth/admin/users/{id}/toggle` (Admin)
- **Success Response (200)**:
  ```json
  {
    "message": "User status updated successfully"
  }
  ```

### 13) Get User Email by AuthUserId (Internal)
- **GET** `/gateway/auth/users/{authUserId}/email`
- **Success Response (200)**:
  ```json
  "john.doe@example.com"
  ```

---

## User Service (`/gateway/user`)

### 1) Create Profile
- **POST** `/gateway/user/profile` (Authenticated)
- **Request Body**:
  ```json
  {
    "fullName": "John Doe",
    "dateOfBirth": "1995-06-15T00:00:00",
    "address": "123 Main Street",
    "city": "Mumbai",
    "state": "Maharashtra",
    "pinCode": "400001"
  }
  ```
- **Success Response (200)**:
  ```json
  {
    "message": "Profile created successfully"
  }
  ```

### 2) Get Profile
- **GET** `/gateway/user/profile` (Authenticated)
- **Success Response (200)**:
  ```json
  {
    "id": 1,
    "authUserId": 1,
    "fullName": "John Doe",
    "dateOfBirth": "1995-06-15T00:00:00",
    "address": "123 Main Street",
    "city": "Mumbai",
    "state": "Maharashtra",
    "pinCode": "400001"
  }
  ```

### 3) Submit KYC
- **POST** `/gateway/user/kyc` (Authenticated)
- **Request Body**:
  ```json
  {
    "documentType": "Aadhaar",
    "documentNumber": "123456789012"
  }
  ```
- **Success Response (200)**:
  ```json
  {
    "message": "KYC submitted successfully"
  }
  ```

### 4) Get KYC Status
- **GET** `/gateway/user/kyc` (Authenticated)
- **Success Response (200)**:
  ```json
  {
    "id": 1,
    "authUserId": 1,
    "documentType": "Aadhaar",
    "documentNumber": "123456789012",
    "status": "Pending",
    "rejectionReason": null,
    "submittedAt": "2026-03-31T10:00:00Z"
  }
  ```

### 5) Get Pending KYCs (Admin)
- **GET** `/gateway/user/admin/kyc/pending` (Admin)
- **Success Response (200)**:
  ```json
  [
    {
      "id": 1,
      "authUserId": 1,
      "documentType": "Aadhaar",
      "documentNumber": "123456789012",
      "status": "Pending",
      "rejectionReason": null,
      "submittedAt": "2026-03-31T10:00:00Z"
    }
  ]
  ```

### 6) Review KYC (Admin)
- **PUT** `/gateway/user/admin/kyc/{kycId}/review` (Admin)
- **Request Body**:
  ```json
  {
    "status": "Approved",
    "rejectionReason": null
  }
  ```
- **Success Response (200)**:
  ```json
  {
    "message": "KYC reviewed successfully"
  }
  ```

---

## Wallet Service (`/gateway/wallet`)

### 1) Create Wallet
- **POST** `/gateway/wallet/create` (Authenticated)
- **Request Body**: none
- **Success Response (200)**:
  ```json
  {
    "message": "Wallet created successfully"
  }
  ```

### 2) Get Balance
- **GET** `/gateway/wallet/balance` (Authenticated)
- **Success Response (200)**:
  ```json
  {
    "id": 1,
    "authUserId": 1,
    "balance": 1500.00,
    "isActive": true,
    "createdAt": "2026-03-31T10:00:00Z"
  }
  ```

### 3) Top Up
- **POST** `/gateway/wallet/topup` (Authenticated)
- **Request Body**:
  ```json
  {
    "amount": 1000.00,
    "description": "Wallet Top-Up"
  }
  ```
- **Success Response (200)**:
  ```json
  {
    "message": "Wallet top-up successful"
  }
  ```

### 4) Transfer
- **POST** `/gateway/wallet/transfer` (Authenticated)
- **Request Body**:
  ```json
  {
    "receiverAuthUserId": 2,
    "receiverEmail": "receiver@example.com",
    "amount": 500.00,
    "description": "Fund Transfer"
  }
  ```
- **Success Response (200)**:
  ```json
  {
    "message": "Transfer successful"
  }
  ```

### 5) Get Transactions
- **GET** `/gateway/wallet/transactions` (Authenticated)
- **Success Response (200)**:
  ```json
  [
    {
      "id": 1,
      "type": "Credit",
      "amount": 1000.00,
      "description": "Wallet Top-Up",
      "referenceId": "TOPUP-001",
      "createdAt": "2026-03-31T10:00:00Z"
    }
  ]
  ```

### 6) Get Transaction By Id
- **GET** `/gateway/wallet/transactions/{id}` (Authenticated)
- **Success Response (200)**:
  ```json
  {
    "id": 1,
    "type": "Credit",
    "amount": 1000.00,
    "description": "Wallet Top-Up",
    "referenceId": "TOPUP-001",
    "createdAt": "2026-03-31T10:00:00Z"
  }
  ```

---

## Rewards Service (`/gateway/rewards`)

### 1) Get Summary
- **GET** `/gateway/rewards/summary` (Authenticated)
- **Success Response (200)**:
  ```json
  {
    "authUserId": 1,
    "totalPoints": 120,
    "tier": "Silver"
  }
  ```

### 2) Get Catalog
- **GET** `/gateway/rewards/catalog` (Authenticated)
- **Success Response (200)**:
  ```json
  [
    {
      "id": 1,
      "title": "Coffee Voucher",
      "description": "Redeem at partner stores",
      "pointsCost": 100,
      "stock": 50
    }
  ]
  ```

### 3) Redeem Reward
- **POST** `/gateway/rewards/redeem` (Authenticated)
- **Request Body**:
  ```json
  {
    "rewardCatalogId": 1
  }
  ```
- **Success Response (200)**:
  ```json
  {
    "message": "Reward redeemed successfully"
  }
  ```

### 4) Get Redemption History
- **GET** `/gateway/rewards/history` (Authenticated)
- **Success Response (200)**:
  ```json
  [
    {
      "id": 1,
      "rewardTitle": "Coffee Voucher",
      "pointsSpent": 100,
      "redeemedAt": "2026-03-31T10:00:00Z"
    }
  ]
  ```

---

## Notification Service (`/gateway/notification`)

### 1) Get All Notifications
- **GET** `/gateway/notification` (Authenticated)
- **Success Response (200)**:
  ```json
  [
    {
      "id": 1,
      "title": "Wallet Top-Up",
      "message": "Your wallet was credited with ₹1000",
      "isRead": false,
      "createdAt": "2026-03-31T10:00:00Z"
    }
  ]
  ```

### 2) Mark Notification as Read
- **PUT** `/gateway/notification/{id}/read` (Authenticated)
- **Success Response (200)**:
  ```json
  {
    "message": "Notification marked as read"
  }
  ```

---

## Admin Service (`/gateway/admin`)

### 1) Get Pending KYC
- **GET** `/gateway/admin/kyc/pending` (Admin)
- **Success Response (200)**:
  ```json
  [
    {
      "id": 1,
      "authUserId": 2,
      "documentType": "PAN",
      "documentNumber": "ABCDE1234F",
      "status": "Pending",
      "rejectionReason": null,
      "submittedAt": "2026-03-31T10:00:00Z"
    }
  ]
  ```

### 2) Review KYC
- **PUT** `/gateway/admin/kyc/{kycId}/review` (Admin)
- **Request Body**:
  ```json
  {
    "status": "Approved",
    "rejectionReason": null
  }
  ```
- **Success Response (200)**:
  ```json
  {
    "message": "KYC reviewed successfully"
  }
  ```

### 3) Get All Users
- **GET** `/gateway/admin/users` (Admin)
- **Success Response (200)**:
  ```json
  [
    {
      "id": 1,
      "email": "john.doe@example.com",
      "phoneNumber": "9876543210",
      "role": "User",
      "isActive": true,
      "createdAt": "2026-03-31T10:00:00Z"
    }
  ]
  ```

### 4) Toggle User Status
- **PUT** `/gateway/admin/users/{userId}/toggle` (Admin)
- **Success Response (200)**:
  ```json
  {
    "message": "User status updated successfully"
  }
  ```

### 5) Get Dashboard
- **GET** `/gateway/admin/dashboard` (Admin)
- **Success Response (200)**:
  ```json
  {
    "totalUsers": 25,
    "activeUsers": 20,
    "pendingKyc": 4,
    "approvedKyc": 18,
    "rejectedKyc": 3
  }
  ```

---

## Endpoint Counts

| Service | Endpoints |
|---|---:|
| AuthService | 13 |
| UserService | 6 |
| WalletService | 6 |
| RewardsService | 4 |
| NotificationService | 2 |
| AdminService | 5 |
| **Total** | **36** |
