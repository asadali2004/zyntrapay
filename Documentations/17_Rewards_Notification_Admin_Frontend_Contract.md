# Rewards, Notification, and Admin Frontend Contract

## Purpose
This document defines the remaining frontend-facing contracts after auth, user, and wallet.

## Rewards

### Get summary
- `GET /gateway/rewards/summary`

Success:
```json
{
  "authUserId": 1,
  "totalPoints": 120,
  "tier": "Silver"
}
```

### Get catalog
- `GET /gateway/rewards/catalog`

Success:
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

### Redeem reward
- `POST /gateway/rewards/redeem`

Request:
```json
{
  "rewardCatalogId": 1
}
```

Success:
```json
{
  "message": "Reward redeemed successfully"
}
```

Rewards error shape:
```json
{
  "message": "Readable error message",
  "errorCode": "STABLE_ERROR_CODE"
}
```

Rewards error codes:
- `REWARDS_ACCOUNT_NOT_FOUND`
- `REWARD_ITEM_NOT_FOUND`
- `REWARD_OUT_OF_STOCK`
- `INSUFFICIENT_REWARD_POINTS`
- `REWARDS_VALIDATION_FAILED`

## Notification

### Get all notifications
- `GET /gateway/notification`

Success:
```json
[
  {
    "id": 1,
    "title": "Wallet Top-Up",
    "message": "Your wallet was credited successfully.",
    "isRead": false,
    "createdAt": "2026-03-31T10:00:00Z"
  }
]
```

### Mark notification as read
- `PUT /gateway/notification/{id}/read`

Success:
```json
{
  "message": "Marked as read."
}
```

Notification error codes:
- `NOTIFICATION_NOT_FOUND`
- `NOTIFICATION_VALIDATION_FAILED`

## Admin

These endpoints are mainly for admin UI, not the normal customer flow.

Admin error shape:
```json
{
  "message": "Readable error message",
  "errorCode": "STABLE_ERROR_CODE"
}
```

Admin error codes:
- `KYC_NOT_FOUND`
- `USER_NOT_FOUND`
- `ADMIN_CONFLICT`
- `ADMIN_VALIDATION_FAILED`

## Frontend Notes

- Rewards and Notification are ready for Angular service integration.
- Admin can be added later without changing the public user flow.
- Frontend should rely on `errorCode` first and `message` second for these services too.
- If a route returns middleware/validation fallback codes, handle these as global API errors:
  - `VALIDATION_FAILED`
  - `UNAUTHORIZED`
  - `RESOURCE_NOT_FOUND`
  - `CONFLICT`
  - `INTERNAL_SERVER_ERROR`
