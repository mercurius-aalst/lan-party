# Auth0 Setup

Configure the Auth0 application as a Regular Web Application.

## Application URLs

Allowed Callback URLs:

- `https://localhost:7044/callback`
- `http://localhost:5003/callback`
- `https://lan.mercurius-aalst.be/callback`

Allowed Logout URLs:

- `https://localhost:7044/account/logout/callback`
- `http://localhost:5003/account/logout/callback`
- `https://lan.mercurius-aalst.be/account/logout/callback`

The application always returns from Auth0 through the fixed
`/account/logout/callback` path. Add the exact callback URL for every origin
used in development or production; do not add a dynamic path or query string.
Logout return targets are limited to 1024 characters; longer targets fall back
to `/`. The protected state is limited to 3072 characters as a cookie-size
guard and also falls back to `/` when exceeded. Live logout state is protected
for five minutes in an HttpOnly cookie scoped to `/account/logout`, then
deleted on callback read. No server-side replay store is required.

## Application Settings

Keep `Auth0:ClientSecret` out of committed configuration. Use user-secrets locally:

```powershell
dotnet user-secrets set "Auth0:Domain" "<tenant>.auth0.com" --project src\Mercurius.LAN.Web
dotnet user-secrets set "Auth0:ClientId" "<client-id>" --project src\Mercurius.LAN.Web
dotnet user-secrets set "Auth0:ClientSecret" "<client-secret>" --project src\Mercurius.LAN.Web
dotnet user-secrets set "Auth0:Audience" "<api-audience>" --project src\Mercurius.LAN.Web
```

Use environment variables for deployed environments.

## Roles Claim Action

Create a Post-Login Action and bind it to the Login flow. Keep the Auth0 role name lowercase `admin`.

```javascript
exports.onExecutePostLogin = async (event, api) => {
  const namespace = 'https://mercurius-aalst.be/roles';
  const roles = event.authorization?.roles || [];

  api.idToken.setCustomClaim(namespace, roles);
  api.accessToken.setCustomClaim(namespace, roles);
};
```
