# Auth Frontend Contract

## Purpose
This document defines the auth-related backend contract that frontend should follow.

## Canonical Signup Flow
1. `POST /gateway/auth/register/request-otp`
2. `POST /gateway/auth/register/verify-otp`
3. `POST /gateway/auth/register`

Legacy aliases still exist:

- `POST /gateway/auth/send-otp`
- `POST /gateway/auth/verify-otp`

Those aliases are for backward compatibility only and should not be used by new frontend code.

## Canonical Login Flow
1. `POST /gateway/auth/login`
2. Store `token`, `refreshToken`, `email`, `role`
3. If `phoneUpdateRequired` is `true`, route user to phone update screen
4. Otherwise route user to dashboard/home

## Authenticated Refresh Flow
1. When access token expires, call `POST /gateway/auth/refresh-token`
2. Replace stored `token` and `refreshToken`
3. If refresh fails with `REFRESH_TOKEN_INVALID`, force logout and return to login

## Success Responses

### Request registration OTP

```json
{
  "message": "OTP sent to your email. Valid for 10 minutes.",
  "nextStep": "verify-otp"
}
```

### Verify registration OTP

```json
{
  "message": "Email verified successfully. You can now complete registration.",
  "nextStep": "complete-registration"
}
```

### Register

```json
{
  "message": "Registration successful.",
  "nextStep": "login"
}
```

### Login

```json
{
  "token": "jwt-token",
  "refreshToken": "refresh-token",
  "email": "john.doe@example.com",
  "role": "User",
  "phoneUpdateRequired": false
}
```

### Refresh token

```json
{
  "token": "new-jwt-token",
  "refreshToken": "new-refresh-token",
  "email": "john.doe@example.com",
  "role": "User",
  "phoneUpdateRequired": false
}
```

### Update phone

```json
{
  "message": "Phone number updated successfully."
}
```

## Error Response Shape

```json
{
  "message": "Email already registered. Please login or use forgot password.",
  "errorCode": "EMAIL_ALREADY_REGISTERED"
}
```

## Important Error Codes

- `EMAIL_ALREADY_REGISTERED`
- `PHONE_ALREADY_REGISTERED`
- `EMAIL_NOT_VERIFIED`
- `OTP_EXPIRED`
- `OTP_INVALID`
- `OTP_DELIVERY_FAILED`
- `INVALID_CREDENTIALS`
- `ACCOUNT_DEACTIVATED`
- `REFRESH_TOKEN_INVALID`
- `USER_NOT_FOUND`
- `AUTH_VALIDATION_FAILED`

## Important Status Codes

- `200` -> success
- `400` -> validation/business rule failure
- `401` -> invalid login or invalid refresh token
- `404` -> user not found
- `409` -> duplicate email or duplicate phone
- `503` -> OTP delivery issue

## Frontend Handling Guidance

- Use `nextStep` to move between signup screens.
- Use `errorCode` for stable UI logic.
- Do not depend only on raw `message` text.

Suggested behavior:

- `EMAIL_ALREADY_REGISTERED` -> show login and forgot-password actions
- `OTP_EXPIRED` -> show resend OTP
- `OTP_INVALID` -> keep user on OTP screen with inline validation
- `EMAIL_NOT_VERIFIED` -> route user back to OTP verification
- `REFRESH_TOKEN_INVALID` -> clear local auth state and redirect to login
- `ACCOUNT_DEACTIVATED` -> block login and show support/contact guidance

## Notes

- OTP values are cached for 10 minutes.
- Verified-email state is cached for 15 minutes before registration.
- Successful registration publishes a welcome email event.
- Frontend should use `register/request-otp` and `register/verify-otp` only, not the legacy alias endpoints.
