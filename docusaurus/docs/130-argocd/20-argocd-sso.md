# ArgoCD SSO

Follow the instructions in the [ArgoCD Keycloak SSO guide](https://argo-cd.readthedocs.io/en/release-2.14/operator-manual/user-management/keycloak/#keycloak-and-argocd-with-client-authentication) to set up Keycloak as an OIDC provider for ArgoCD.

Export `REPLACE_WITH_CLIENT_SECRET` with the client secret you obtained when creating the ArgoCD client in Keycloak.
```bash
export REPLACE_WITH_CLIENT_SECRET=<your-client-secret>
```

Update `argocd-secret` to set the OIDC client secret:
```bash
kubectl -n argocd patch secret argocd-secret --patch="{\"stringData\": { \"oidc.keycloak.clientSecret\": \"$REPLACE_WITH_CLIENT_SECRET\" }}"
```

Update `argocd-cm` configmap to use Keycloak OIDC:
```bash
kubectl -n argocd patch configmap argocd-cm --patch="{\"data\": {
    \"url\": \"https://argocd.${CLUSTER_DOMAIN}\",
    \"oidc.config\": \"name: Keycloak\nissuer: https://account.${CLUSTER_DOMAIN}/realms/nursery\nclientID: argocd\nrequestedScopes: [\\\"openid\\\"]\nclientSecret: \$oidc.keycloak.clientSecret\"
}}"
```

Update `argocd-rbac-cm` configmap to map Keycloak groups to ArgoCD roles:
```bash
kubectl -n argocd patch configmap argocd-rbac-cm --patch="{\"data\": {
    \"policy.csv\": \"g, /Admins, role:admin\",
    \"policy.default\": \"role:''\"
}}"
```

:::note
`role:''` means no access by default.
You can use `role:readonly` to give read-only access by default.
:::