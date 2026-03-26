# Step 3 — OTP Flow Implementation Summary

## Overview
Email-based OTP (One-Time Password) verification has been integrated into the registration flow. Users must verify their email with a 6-digit OTP before completing registration.

---

## Implementation Details

### 1. Event Contracts (Shared.Events) ✅
- **OtpRequestedEvent.cs** — Published when user requests OTP
- **WelcomeEmailRequestedEvent.cs** — Published after successful registration

### 2. DTOs (AuthService) ✅
- **SendOtpRequestDto** — Request OTP for email
- **VerifyOtpRequestDto** — Verify OTP before registration

### 3. OTP Storage ✅
- Uses `IMemoryCache` with 10-minute expiry
- Cache key: `otp_{normalized_email}`
- Verification flag: `verified_{normalized_email}` (15-min expiry)

### 4. RabbitMQ Publisher (AuthService) ✅
- **IRabbitMqPublisher** interface
- **RabbitMqPublisher** implementation
- Registered as Singleton for connection pooling

### 5. AuthService Methods ✅
- **SendOtpAsync()** — Generate OTP, store in cache, publish event
  - Generates 6-digit OTP using `Random.Shared`
  - Normalizes email to lowercase
  - Publishes `OtpRequestedEvent` to RabbitMQ
  
- **VerifyOtpAsync()** — Verify OTP, set verified flag
  - Validates OTP exists and matches
  - Removes OTP after verification
  - Sets `verified_` flag for next 15 minutes
  
- **RegisterAsync()** — Modified to require OTP verification
  - Checks `verified_` flag before allowing registration
  - Publishes `WelcomeEmailRequestedEvent` after success
  - Removes verified flag after registration

### 6. API Endpoints (AuthController) ✅
- **POST /api/auth/send-otp** — Send OTP to email
- **POST /api/auth/verify-otp** — Verify OTP and mark email as verified
- **POST /api/auth/register** — Complete registration (now requires verified email)

### 7. Notification Consumers ✅
- **OtpRequestedConsumer** — Listens for OtpRequestedEvent
  - Sends OTP email via EmailService
  - Uses `EmailTemplates.OtpEmail(otp)` template
  
- **WelcomeEmailConsumer** — Listens for WelcomeEmailRequestedEvent
  - Sends welcome email to new users
  - Uses `EmailTemplates.WelcomeEmail(userName)` template

---

## Flow Diagram

```
USER REGISTRATION FLOW:
═══════════════════════

1. User submits email
   ↓
2. POST /api/auth/send-otp {email}
   ↓
3. AuthService generates OTP → stores in cache (10 min)
   ↓
4. Publish OtpRequestedEvent → RabbitMQ
   ↓
5. OtpRequestedConsumer receives event
   ↓
6. NotificationService sends OTP email
   ↓
7. User receives OTP email
   ↓
8. User enters OTP
   ↓
9. POST /api/auth/verify-otp {email, otp}
   ↓
10. AuthService verifies OTP → sets verified_ flag (15 min)
    ↓
11. Response: "Email verified successfully"
    ↓
12. User submits password + phone
    ↓
13. POST /api/auth/register {email, password, phone}
    ↓
14. AuthService checks verified_ flag ✓
    ↓
15. Create user in DB
    ↓
16. Publish WelcomeEmailRequestedEvent → RabbitMQ
    ↓
17. WelcomeEmailConsumer receives event
    ↓
18. NotificationService sends welcome email
    ↓
19. User registered successfully!
```

---

## Key Design Decisions

| Decision | Why | Rationale |
|----------|-----|-----------|
| **MemoryCache** | No DB table needed | OTP is temporary; cache auto-expires |
| **Random.Shared** | Thread-safe OTP generation | Better than `new Random()` |
| **Email normalization** | Case-insensitive lookup | Prevents "test@mail.com" ≠ "test@MAIL.com" |
| **15-min verified flag** | Buffer after verification | User has time to complete registration form |
| **Separate WelcomeEvent** | Clean separation | OTP event ≠ Welcome event |
| **Singleton Publisher** | Connection pooling | Better resource utilization |
| **No in-app notification** | Email is primary | Users won't see notification before email |
| **Skip OTP for admin** | Already has secret key | Extra verification layer not needed |

---

## Configuration Required

Add to `appsettings.json` or `dotnet user-secrets`:

```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  }
}
```

Set secrets:
```powershell
cd AuthService
dotnet user-secrets set "RabbitMQ:Host" "localhost"
dotnet user-secrets set "RabbitMQ:Username" "guest"
dotnet user-secrets set "RabbitMQ:Password" "guest"
```

---

## Testing the OTP Flow

### Test Sequence:

```bash
# 1. Request OTP
POST /api/auth/send-otp
{
  "email": "test@example.com"
}
Response: 200 - "OTP sent to your email..."

# 2. Get OTP from email (check SMTP output or logs)
# OTP format: 6 digits (e.g., 123456)

# 3. Verify OTP
POST /api/auth/verify-otp
{
  "email": "test@example.com",
  "otp": "123456"
}
Response: 200 - "Email verified successfully..."

# 4. Register user
POST /api/auth/register
{
  "email": "test@example.com",
  "password": "Password123",
  "phoneNumber": "9876543210"
}
Response: 200 - "Registration successful."
```

---

## Files Created/Modified

### Created (5 files):
- ✅ `Shared.Events/OtpRequestedEvent.cs`
- ✅ `Shared.Events/WelcomeEmailRequestedEvent.cs`
- ✅ `AuthService/Services/IRabbitMqPublisher.cs`
- ✅ `AuthService/Services/RabbitMqPublisher.cs`
- ✅ `NotificationService/Consumers/OtpRequestedConsumer.cs`
- ✅ `NotificationService/Consumers/WelcomeEmailConsumer.cs`

### Modified (5 files):
- ✅ `AuthService/DTOs/AuthDTOs.cs` — Added SendOtpRequestDto, VerifyOtpRequestDto
- ✅ `AuthService/Services/IAuthService.cs` — Added SendOtpAsync, VerifyOtpAsync
- ✅ `AuthService/Services/AuthServiceImpl.cs` — Implemented OTP methods + cache
- ✅ `AuthService/Extensions/ServiceExtensions.cs` — Registered IRabbitMqPublisher + MemoryCache
- ✅ `AuthService/Controllers/AuthController.cs` — Added send-otp and verify-otp endpoints
- ✅ `NotificationService/Extensions/ServiceExtensions.cs` — Registered both consumers

---

## Build Status
✅ **Solution builds successfully with no errors**

---

## Next Steps

1. **Phase 1, Step 4** — Google OAuth2 (or skip if time tight)
2. **Phase 1, Step 5** — MemoryCache for RewardsService
3. **Phase 1, Step 6** — Polly Resilience patterns
4. **Phase 1, Step 7** — PointsAwarded event (RewardsService)
5. **Phase 2** — NUnit testing

---
