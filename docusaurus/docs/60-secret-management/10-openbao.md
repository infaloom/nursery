# OpenBao

:::danger
The following setup may not be suitable for environments requiring the highest level of security, for example apps storing finance or healthcare data. For these environments you may want to use 3rd-party key management system, end to end encryption, etc. Please evaluate your security requirements carefully.
:::

Details at https://openbao.org/docs/platform/k8s/helm/

## Create namespace and unseal key

```bash
kubectl create namespace openbao
```

Generate unseal key and create k8s secret.
```bash
openssl rand -out openbao-unseal-1.key 32
```
```bash
kubectl create secret generic openbao-unseal-key -n openbao --from-file=openbao-unseal-1.key
```
Store base64 encoded unseal key securely in Pulumi.
```bash
pulumi --cwd $PULUMI_CWD config set --secret openbao:unsealKey1Base64 "$(cat openbao-unseal-1.key | base64)"
```
Remove the local unseal key file.
```bash
rm openbao-unseal-1.key
```

Add OpenBao helm repo:
```bash
helm repo add openbao https://openbao.github.io/openbao-helm
```

Install OpenBao in the namespace:
```bash
helm upgrade --install openbao openbao/openbao \
--version 0.19.1 \
--namespace openbao \
--values k8s/openbao/override-values.yaml
```

Initialize OpenBao:
```bash
kubectl exec -ti openbao-0 -n openbao -- bao operator init
```

Store recovery keys. Replace the example keys between quotes below with the actual keys output from the above command.
```bash
pulumi --cwd $PULUMI_CWD config set --secret openbao:recoveryKeys "Recovery Key 1: <your-recovery-key-1>
Recovery Key 2: <your-recovery-key-2>
Recovery Key 3: <your-recovery-key-3>
Recovery Key 4: <your-recovery-key-4>
Recovery Key 5: <your-recovery-key-5>"
```

Store initial root token. Replace the example token between quotes below with the actual initial root token output from the init command.
```bash
pulumi --cwd $PULUMI_CWD config set --secret openbao:initialRootToken "<your-initial-root-token>"
```

### Verify
```bash
kubectl exec -ti openbao-0 -n openbao -- bao login # Then use the root token
kubectl exec -ti openbao-0 -n openbao -- bao operator raft list-peers
kubectl exec -ti openbao-0 -n openbao -- bao operator raft autopilot state
```

### Enable KV secrets engine
```bash
kubectl exec -ti openbao-0 -n openbao -- bao secrets enable -version=2 kv
```

### OpenBao UI

Create OpenBao UI ingress:
```bash
envsubst '$CLUSTER_DOMAIN' < k8s/openbao/openbao-ingress.yaml | kubectl apply -f -
```

Run these commands to access OpenBao UI with initial root token:

```bash
echo "OpenBao UI URL: https://openbao.${CLUSTER_DOMAIN}"
echo "Initial Root Token: $(pulumi --cwd $PULUMI_CWD config get openbao:initialRootToken)"
```

### Keycloak OIDC Authentication

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
The example below allows any user in the "Nursery Admins" group in Keycloak to log in to OpenBao with admin privileges. Adjust the configuration according to your security requirements.
:::

```bash
kubectl exec -ti openbao-0 -n openbao -- bao write auth/oidc/role/admin-sso - <<EOF
{
    "role_type": "oidc",
    "user_claim": "email",
    "token_policies": "admin,default",
    "oidc_scopes": "profile,email",
    "bound_claims": { "groups": ["/Nursery Admins"] },
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