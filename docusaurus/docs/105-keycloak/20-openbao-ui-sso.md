# OpenBao UI SSO

:::warning
You need to have [Keycloak](../105-keycloak/10-keycloak.md) and [OpenBao](../60-secret-management/10-openbao.md) up and running before proceeding with this guide.
:::

https://openbao.org/docs/auth/jwt/oidc-providers/keycloak/

Create Client in Keycloak with the following settings. Replace `<CLUSTER_DOMAIN>` with actual value.

| Setting | Value |
|---------|-------|
| Client ID | `openbao` |
| Root URL | `https://openbao.<CLUSTER_DOMAIN>` |
| Redirect URIs | `/ui/vault/auth/oidc/oidc/callback` <br/>  `/v1/auth/oidc/*` <br/>  `http://localhost:8250/oidc/callback` |

Enable OIDC auth method in OpenBao:
```bash
kubectl exec -ti openbao-0 -n openbao -- bao auth enable oidc
```

Configure OIDC auth method in OpenBao. Replace `<CLIENT_SECRET>` and `<CLUSTER_DOMAIN>` with actual values.

```bash
kubectl exec -ti openbao-0 -n openbao -- bao write auth/oidc/config \
  oidc_client_id="openbao" \
  oidc_client_secret="<CLIENT_SECRET>" \
  default_role="admin-sso" \
  oidc_discovery_url="https://account.<CLUSTER_DOMAIN>/realms/nursery"
```

Create OIDC role in OpenBao. Replace `<CLUSTER_DOMAIN>` with actual value.

:::warning
The example below allows any user in the "Admins" group in Keycloak to log in to OpenBao with admin privileges. Adjust the configuration according to your security requirements.
:::

```bash
kubectl exec -ti openbao-0 -n openbao -- bao write auth/oidc/role/admin-sso - <<EOF
{
    "role_type": "oidc",
    "user_claim": "email",
    "token_policies": "admin,default",
    "oidc_scopes": "profile,email",
    "bound_claims": { "groups": ["/Admins"] },
    "allowed_redirect_uris": "https://openbao.<CLUSTER_DOMAIN>/v1/auth/oidc/callback,https://openbao.<CLUSTER_DOMAIN>/ui/vault/auth/oidc/oidc/callback,http://localhost:8250/oidc/callback"
}
EOF
```

Create admin policy in OpenBao that allows managing kv secrets:

```bash
kubectl exec -ti openbao-0 -n openbao -- bao policy write admin - <<EOF
# Allow a token to manage kv secrets
path "kv/*" {
    capabilities = ["create", "read", "update", "delete", "list"]
}
EOF
```