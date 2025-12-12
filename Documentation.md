# FriendNetApp — API Documentation

This document describes the backend HTTP (and SignalR) endpoints frontend developers will use. It focuses on the current stable endpoints implemented in the services and examples showing how to call them from a React app (fetch / axios). Use the Gateway prefix `/friendnet` when testing through the gateway in integration or AppHost environments (integration tests use that prefix). Authenticate using JWT (cookie or Authorization header) as noted below.

---

## Authentication (Auth Service)

Base path: `/auth`

- `POST /auth/register`
 - Description: Register new user and set login cookie.
 - Request JSON body:
 ```json
 { "email": "user@example.com", "password": "Secret123!", "role": "Client" }
 ```
 - Response: `200 OK` with JSON body `{ "token": "<jwt>" }` and sets cookie `jwt` (HttpOnly).
 - Notes: Frontend can either read `token` from response body or rely on cookie (requires credentials included).

- `POST /auth/login`
 - Description: Authenticate a user.
 - Request JSON body:
 ```json
 { "email": "user@example.com", "password": "Secret123!" }
 ```
 - Response: `200 OK` with token (current code returns raw string token). Also sets cookie `jwt`.

- `GET /auth/hello`
 - Description: test endpoint, returns `200 OK` with simple text.

Authentication notes for frontend
- Two supported mechanisms (both used by services):
1. Cookie-based: server sets `jwt` cookie. Include cookies from the browser: with `fetch(..., { credentials: 'include' })` or axios `{ withCredentials: true }`.
2. Header-based: `Authorization: Bearer <token>` — acceptable for SPA clients.
- Ensure `Jwt:Issuer`, `Jwt:Audience`, and `Jwt:SecretKey` match between Auth service and other services.

---

## UserProfile Service

Base path: `/users`

All endpoints with `[Authorize(Roles = "Client,Admin")]` require authentication (cookie or bearer token).

- `GET /users/all`
 - Get all user profiles visible to the caller.
 - Response: `200 OK` JSON array of `UserOutputDto`.

- `GET /users/{id}`
 - Get user profile by id (GUID path param).
 - Response: `200 OK` with `UserOutputDto` or `404`.

- `GET /users/find-by-email?email={email}`
 - Query by email. Returns single user or `null`.

- `GET /users/find-by-username?userName={username}`
 - Query by username. Returns collection.

- `POST /users/create`
 - Create a profile. Request body:
 ```json
 { "userName": "Dextar", "email": "dex@example.com", "age":20 }
 ```
 - Response: `200 OK` returning the created user id as a JSON string (e.g. `"<guid>"`).
 - Note: Publish `UserCreatedEvent` to message bus after creation — consumer services may need time to react.

- `PATCH /users/edit/{id}`
 - Edit profile for `id` (only owner or Admin). Request body same as `UserInputDto`.
 - Response: `200 OK` with updated `UserOutputDto`.

- `DELETE /users/delete/{id}`
 - Delete profile (owner or Admin). Returns `200 OK` or `404`.

### Profiles (photos)
Base path: `/users/profiles`
- `POST /users/profiles/add-photo` (multipart/form-data)
 - Form field `file`: image file.
 - Response: `200 OK` with `PhotoDto`.
- `GET /users/profiles/{userId}/photo` — return profile photo metadata.
- `DELETE /users/profiles/photo` — delete current user's photo.
- `GET /users/profiles/photo/{photoId}` — return raw image bytes.

---

## Messaging Service

Base path: `/messaging/chats`
All endpoints require authentication and appropriate role.

- `GET /messaging/chats/all`
 - Returns all chats for current authenticated user.
 - Response: `200 OK` with array of `ChatDto`.

- `POST /messaging/chats/create`
 - Create a chat between two users. Request JSON body (use this DTO):
 ```json
 { "user1Id": "<guid>", "user2Id": "<guid>" }
 ```
 - Response: `200 OK` with chat id string (GUID quoted). Ensure the `UserReplica` entries exist for the two users (published from UserProfile service). Tests may need to wait for consumer propagation.

- `DELETE /messaging/chats/delete/{chatId}`
 - Delete a chat by id.

- `GET /messaging/chats/{chatId}/history`
 - Get messages for a chat. Response: array of `MessageDto`.

- `POST /messaging/chats/send`
 - Send a message in a chat. Request body (MessageDto):
 ```json
 { "chatId": "<guid>", "senderId": "<guid>", "content": "hello" }
 ```
 - Response: `200 OK` with message id (GUID string).
 - Note: server validates `senderId` matches authenticated user; otherwise returns `403`.

### SignalR (Realtime)
- Hub endpoint: `/hubs/chat`
 - Clients must connect with an authenticated user. Token may be provided as:
 - Cookie `jwt` (HttpOnly) — automatically sent by the browser if `withCredentials` used.
 - `access_token` query parameter for WebSockets/SignalR if needed (server `OnMessageReceived` supports reading `access_token`).
 - Hub methods: `JoinChatGroup(string chatId)`, `LeaveChatGroup(string chatId)`. Server pushes `ReceiveMessage` with `Message` payloads.

---

## Social Service (Matching & Friendships)

The Social service implements matching and friendships. Controller routes may vary; check these controllers in code: `FriendshipsController`, `MatchingController`.

Common operations you will use from frontend:
- Create / remove friendship between two users.
- Request friend-match (inviter introduces two friends).
 - FriendMatch DTO: `FriendMatchRequestDto { userAId, userBId, inviterId? }`.
 - Server validates inviter is friends with both users for friend-match flows.
- Random match (one-per-day) — supply `UserId` and `Filters`.

Ask the backend owner for the exact public paths if you need direct links (they live in `FriendNetApp.SocialService/Controllers`).

---

## Message Contracts (events via RabbitMQ)

Shared contract project: `FriendNetApp.Contracts` (used by publisher and consumers).

- `FriendNetApp.Contracts.Events.UserCreatedEvent` (record)
 - Fields: `Id`, `UserName`, `Email`, `ProfileImageUrl`
 - Published by: `UserProfile` service on user creation.
 - Consumed by: `MessagingService` (creates `UserReplica`) and other services that need user info.

- `FriendNetApp.Contracts.Events.UserUpdatedEvent`
 - Same shape for updates.

If a consumer needs a different payload, a separate contract type should be defined (for example `SocialUserCreatedEvent`) and the publisher should publish both events when appropriate.

---

## Error handling and tips for frontend

- All endpoints return standard HTTP codes: `200 OK`, `400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, `500 Internal Server Error`.
- Where an endpoint returns a raw JSON string (e.g. created id), use `response.json()` or `response.text()` carefully. Prefer to return structured JSON `{ id: "..." }` if you control that endpoint.
- For flows involving RabbitMQ events (profile created -> messaging replica), the messaging consumer processes events asynchronously; allow a small delay or poll the target endpoint until the replica exists.

Examples (fetch)
```js
// login with credentials and reuse cookie
await fetch('/friendnet/auth/login', {
 method: 'POST',
 headers: { 'Content-Type': 'application/json' },
 credentials: 'include',
 body: JSON.stringify({ email, password })
});

// call protected endpoint with bearer token
await fetch('/friendnet/users/all', {
 headers: { 'Authorization': `Bearer ${token}` }
});

// create chat
await fetch('/friendnet/messaging/chats/create', {
 method: 'POST',
 credentials: 'include',
 headers: { 'Content-Type': 'application/json' },
 body: JSON.stringify({ user1Id, user2Id })
});
```
