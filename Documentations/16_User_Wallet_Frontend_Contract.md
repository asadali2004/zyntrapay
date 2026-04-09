# User and Wallet Frontend Contract

## Purpose
This document defines the backend contract that frontend should follow for profile, KYC, wallet, balance, transfers, and transactions.

## Common Rules

- All endpoints in this document require `Authorization: Bearer {token}`.
- `AuthUserId` is always taken from the JWT on the backend.
- Frontend should not send `AuthUserId` even if a DTO includes it internally.
- Error responses now use:

```json
{
  "message": "Readable error message",
  "errorCode": "STABLE_ERROR_CODE"
}
```

## User Flow

### Create profile
- `POST /gateway/user/profile`

Request:
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

Success:
```json
{
  "message": "Profile created successfully"
}
```

Frontend notes:
- Call once after signup/login if profile does not exist yet.
- `pinCode` must be exactly 6 digits.

### Get profile
- `GET /gateway/user/profile`

Success:
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

Failure:
- `404` if profile does not exist yet

Frontend notes:
- Treat `404` as "profile setup pending", not as a broken app state.

Suggested user error codes:
- `PROFILE_ALREADY_EXISTS`
- `PROFILE_NOT_FOUND`
- `KYC_ALREADY_SUBMITTED`
- `KYC_NOT_FOUND`
- `KYC_STATUS_INVALID`
- `KYC_REJECTION_REASON_REQUIRED`
- `KYC_ALREADY_REVIEWED`
- `USER_VALIDATION_FAILED`

Fallback/system error codes (cross-service):
- `VALIDATION_FAILED`
- `UNAUTHORIZED`
- `RESOURCE_NOT_FOUND`
- `CONFLICT`
- `INTERNAL_SERVER_ERROR`

### Submit KYC
- `POST /gateway/user/kyc`

Request:
```json
{
  "documentType": "Aadhaar",
  "documentNumber": "123456789012"
}
```

Success:
```json
{
  "message": "KYC submitted successfully"
}
```

Frontend notes:
- `documentType` is free-text currently, but frontend should restrict values to known options such as `Aadhaar`, `PAN`, `Passport`.
- `documentNumber` must be at least 8 characters.

### Get KYC status
- `GET /gateway/user/kyc`

Success:
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

Failure:
- `404` if KYC has not been submitted yet

Frontend notes:
- Expected statuses are practically `Pending`, `Approved`, `Rejected`.
- If `Rejected`, show `rejectionReason`.

## Wallet Flow

### Create wallet
- `POST /gateway/wallet/create`

Request:
- no body

Success:
```json
{
  "message": "Wallet created successfully"
}
```

Frontend notes:
- Usually call once after profile setup or first wallet screen load if wallet does not exist.

### Get balance
- `GET /gateway/wallet/balance`

Success:
```json
{
  "id": 1,
  "authUserId": 1,
  "balance": 1500.0,
  "isActive": true,
  "createdAt": "2026-03-31T10:00:00Z"
}
```

Failure:
- `404` if wallet does not exist yet

Frontend notes:
- Treat `404` as "wallet not created yet" and offer wallet creation action.

Suggested wallet error codes:
- `WALLET_ALREADY_EXISTS`
- `WALLET_NOT_FOUND`
- `WALLET_DEACTIVATED`
- `INSUFFICIENT_BALANCE`
- `SELF_TRANSFER_NOT_ALLOWED`
- `RECEIVER_WALLET_NOT_FOUND`
- `TRANSACTION_NOT_FOUND`
- `WALLET_VALIDATION_FAILED`

### Top up wallet
- `POST /gateway/wallet/topup`

Request:
```json
{
  "amount": 1000.0,
  "description": "Wallet Top-Up"
}
```

Success:
```json
{
  "message": "Wallet top-up successful"
}
```

Frontend notes:
- Allowed amount range is `1` to `50000`.
- Keep `description` optional in UI if you want, but send a default like `Wallet Top-Up`.

### Transfer money
- `POST /gateway/wallet/transfer`

Request:
```json
{
  "receiverAuthUserId": 2,
  "receiverEmail": "receiver@example.com",
  "amount": 500.0,
  "description": "Fund Transfer"
}
```

Success:
```json
{
  "message": "Transfer successful"
}
```

Frontend notes:
- Allowed amount range is `1` to `25000`.
- `receiverEmail` is optional but useful for downstream notification flow.
- Frontend should validate sender cannot transfer to self if that UX rule is desired.

### Get transaction list
- `GET /gateway/wallet/transactions`

Success:
```json
[
  {
    "id": 1,
    "type": "Credit",
    "amount": 1000.0,
    "description": "Wallet Top-Up",
    "referenceId": "TOPUP-001",
    "createdAt": "2026-03-31T10:00:00Z"
  }
]
```

### Get transaction by id
- `GET /gateway/wallet/transactions/{id}`

Success:
```json
{
  "id": 1,
  "type": "Credit",
  "amount": 1000.0,
  "description": "Wallet Top-Up",
  "referenceId": "TOPUP-001",
  "createdAt": "2026-03-31T10:00:00Z"
}
```

## Suggested Frontend Routing

1. After login, call `GET /gateway/user/profile`
2. If profile missing, route to profile creation
3. After profile creation, show KYC status screen or KYC form
4. On wallet screens, call `GET /gateway/wallet/balance`
5. If wallet missing, offer `create wallet`

## Frontend Handling Guidance

- Prefer `errorCode` for UI logic.
- Use `message` for user-facing display text.
- Still keep HTTP status handling for routing decisions such as `404` profile/wallet missing states.
