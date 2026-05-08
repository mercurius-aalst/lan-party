# Auth0 Setup

Configure the Auth0 application as a Regular Web Application.

## Application URLs

Allowed Callback URLs:

- `https://localhost:7044/callback`
- `http://localhost:5003/callback`
- `https://mercurius-aalst.be/callback`

Allowed Logout URLs:

- `https://localhost:7044`
- `http://localhost:5003`
- `https://mercurius-aalst.be`

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
