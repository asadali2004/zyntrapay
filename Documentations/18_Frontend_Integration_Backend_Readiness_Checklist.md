# Frontend Integration Backend Readiness Checklist

## Purpose
This document is the final backend-to-frontend readiness summary for ZyntraPay before Dockerization and Angular integration.

## Current Readiness Status

Backend is ready for frontend integration with the following baseline:

- Solution rebuild succeeds in IDE workflow
- All tests are green
- Total tests currently passing: `159`
- API contracts are documented for Auth, User, Wallet, Rewards, Notification, and Admin
- Error responses are now standardized across the main frontend-facing services
- API Gateway remains the single frontend entry point

## Test Baseline

Current passing test suites:

- `AdminService.Tests` -> `14`
- `AuthService.Tests` -> `31`
- `NotificationService.Tests` -> `17`
- `RewardsService.Tests` -> `25`
- `UserService.Tests` -> `17`
- `WalletService.Tests` -> `25`
- `ZyntraPay.IntegrationTests` -> `30`

Frontend work should begin only against the gateway URLs, not by calling services directly.

## Canonical Frontend Entry Points

Use gateway-prefixed routes only:

- `/gateway/auth/*`
- `/gateway/user/*`
- `/gateway/wallet/*`
- `/gateway/rewards/*`
- `/gateway/notification/*`
- `/gateway/admin/*`

Do not connect Angular directly to internal microservice base URLs.

## Recommended Angular Implementation Order

1. Auth module
2. Profile module
3. KYC module
4. Wallet module
5. Rewards module
6. Notification module
7. Admin module later

This keeps the user journey aligned with backend dependencies.

## Recommended First User Journey

1. Register request OTP
2. Verify OTP
3. Register
4. Login
5. Check profile
6. Create profile if missing
7. Submit or view KYC
8. Create wallet if missing
9. View balance and transactions
10. Use rewards and notifications

## Auth Contract Rules

- Store `token`, `refreshToken`, `email`, and `role` after login
- Send `Authorization: Bearer {token}` for protected endpoints
- On refresh success, replace both tokens
- On refresh failure with `REFRESH_TOKEN_INVALID`, clear auth state and redirect to login
- If login response returns `phoneUpdateRequired = true`, route user to the phone update screen before normal dashboard flow

Reference:

- [14_Auth_Signup_Frontend_Contract.md](e:\DotNet_Learning\ZyntraPay\Documentations\14_Auth_Signup_Frontend_Contract.md)

## Profile, KYC, and Wallet Rules

- Treat `GET /gateway/user/profile` returning `404` as profile-not-created state
- Treat `GET /gateway/user/kyc` returning `404` as KYC-not-submitted state
- Treat `GET /gateway/wallet/balance` returning `404` as wallet-not-created state
- Frontend should guide the user forward instead of treating these as hard failures

Reference:

- [16_User_Wallet_Frontend_Contract.md](e:\DotNet_Learning\ZyntraPay\Documentations\16_User_Wallet_Frontend_Contract.md)

## Rewards, Notification, and Admin Rules

- Rewards and notifications are safe to integrate after auth/profile/wallet flow is stable
- Admin should be treated as a separate UI area and not part of the normal customer journey
- Frontend should use `errorCode` first for deterministic behavior

Reference:

- [17_Rewards_Notification_Admin_Frontend_Contract.md](e:\DotNet_Learning\ZyntraPay\Documentations\17_Rewards_Notification_Admin_Frontend_Contract.md)

## Standard Error Handling Contract

Preferred frontend handling order:

1. Check HTTP status
2. Check `errorCode`
3. Show `message`

Standard error shape:

```json
{
  "message": "Readable error message",
  "errorCode": "STABLE_ERROR_CODE"
}
```

Global fallback codes frontend should understand:

- `VALIDATION_FAILED`
- `UNAUTHORIZED`
- `RESOURCE_NOT_FOUND`
- `CONFLICT`
- `INTERNAL_SERVER_ERROR`

## Required Frontend Environment Values

Angular should expect at least:

- Gateway base URL
- Token storage strategy
- Refresh handling strategy
- Route guard behavior for protected routes

Suggested starting environment keys:

```ts
export const environment = {
  production: false,
  apiBaseUrl: "https://localhost:<gateway-port>/gateway"
};
```

Use the gateway as the only public backend base URL.

## Backend Items Considered Ready Before Docker

- Auth flow contract
- Signup OTP flow contract
- Login and refresh contract
- Standardized error code handling for major frontend-facing controllers
- Profile, KYC, wallet, rewards, and notification endpoint documentation
- Passing unit and integration tests

## Known Non-Blocking Items

- Coverage is improved but not yet the main focus for the next step
- Some build artifact file locks can still appear from local machine state
- Docker and CI/CD are not set up yet
- Admin frontend can be implemented after the main customer flow

## Final Go/No-Go Decision

Status: `Go for frontend integration preparation`

Recommended next step:

1. Dockerize backend and supporting infrastructure
2. Then start Angular project setup against the gateway contract
