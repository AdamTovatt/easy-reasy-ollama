# EasyReasy.Ollama.Server

A minimal .NET API server secured with API key-based JWT authentication.

## Authentication Overview

All requests to protected endpoints must be authenticated using a JWT (JSON Web Token) issued by the server. JWTs are obtained by authenticating with a valid API key.

### How to Authenticate

1. **Obtain a JWT**

   Send a POST request to `/api/auth/apikey` with your API key:

   ```http
   POST /api/auth/apikey
   Content-Type: application/json

   {
     "apiKey": "your-api-key-here"
   }
   ```

   If the API key is valid, the response will be:

   ```json
   {
     "token": "<your-jwt-token>",
     "expiresAt": "2024-06-10T12:34:56Z"
   }
   ```

   - `token`: The JWT to use for authentication
   - `expiresAt`: The expiration date/time of the token (ISO 8601, UTC)

2. **Use the JWT in Requests**

   Add the JWT to the `Authorization` header as a Bearer token:

   ```http
   Authorization: Bearer <your-jwt-token>
   ```

   Example with `curl`:
   ```sh
   curl -H "Authorization: Bearer <your-jwt-token>" https://your-server/api/your-endpoint
   ```

## API Key Management

- API keys and tenant info are configured via environment variables (`TENANT_INFO`, `TENANT_INFO2`, ...), each formatted as:
  ```
  tenant-id, tenant-api-key
  ```
- See the server code for details on how tenants and API keys are loaded.

## Error Handling

- If authentication fails (invalid or missing API key/JWT), the server responds with `401 Unauthorized`.
- After multiple failed attempts from the same IP, a progressive delay is applied to slow down brute-force attacks.

## Security Notes

- Only API key authentication is supported (no username/password login).
- JWTs are signed using a secret configured via the `JWT_SIGNING_SECRET` environment variable.
- For more details on the authentication flow and security, see the [EasyReasy.Auth](../EasyReasy.Auth/README.md) library. 