# Harbor SSO

Harbor supports SSO integration via OIDC. This guide will walk you through the steps to configure Harbor to use an external identity provider for authentication.

## Prerequisites

- Create client in Keycloak.
  - Client ID: `harbor`
  - Client Protocol: `OpenID Connect`
  - Root URL: `https://harbor.${CLUSTER_DOMAIN}`
  - Home URL: `/`
  - Valid Redirect URIs: `/c/oidc/callback`
  - Web Origins: `https://harbor.${CLUSTER_DOMAIN}`
- Note client secret down.
- Ensure that you are part of the `/Admins` group in Keycloak's `nursery` realm to have access to Harbor. This will also allow you admin privileges in Harbor.

## Configure Harbor for OIDC

Note that the expected OIDC endpoint format is: `https://account.${CLUSTER_DOMAIN}/realms/nursery`

Use the following command to configure Harbor with OIDC:

:::warning
Ensure that the `HARBOR_OIDC_CLIENT_SECRET` environment variable is set in your shell before running the command.
You can set it using:
```bash
export HARBOR_OIDC_CLIENT_SECRET=<your-client-secret>
```
:::

```bash
curl -X PUT "https://harbor.${CLUSTER_DOMAIN}/api/v2.0/configurations" \
 -H "accept: application/json" \
 -H "Content-Type: application/json" \
 -u "admin:$(kubectl get secret harbor-admin-password -n harbor -o jsonpath="{.data.HARBOR_ADMIN_PASSWORD}" | base64 --decode)" \
 -d "{
  \"oidc_name\": \"KEYCLOAK\",
  \"oidc_endpoint\": \"https://account.${CLUSTER_DOMAIN}/realms/nursery\",
  \"oidc_client_id\": \"harbor\",
  \"oidc_client_secret\": \"${HARBOR_OIDC_CLIENT_SECRET}\",
  \"oidc_groups_claim\": \"groups\",
  \"oidc_admin_group\": \"/Admins\",
  \"oidc_group_filter\": \"/Admins\",
  \"oidc_scope\": \"openid\",
  \"oidc_user_claim\": \"preferred_username\",
  \"oidc_verify_cert\": true,
  \"oidc_auto_onboard\": true
}"
```