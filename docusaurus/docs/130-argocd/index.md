# ArgoCD

https://argo-cd.readthedocs.io/en/release-2.14/

```bash
kubectl create namespace argocd && \
kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/v2.14.6/manifests/install.yaml
```

Disable internal TLS to avoid redirect loop.
```bash
kubectl -n argocd patch configmap argocd-cmd-params-cm \
--type merge -p '{"data": {"server.insecure": "true"}}'
```

Restart server deployments to ensure the changes are applied.
```bash
kubectl rollout restart deployment argocd-server -n argocd && \
kubectl rollout restart deployment argocd-dex-server -n argocd
```

Ensure `CLUSTER_DOMAIN` env var is set before applying ingress
```bash
envsubst < k8s/argocd/argocd-ingress.yaml | kubectl apply --wait -f -
```

Access ArgoCD UI
```bash
echo "---- ArgoCD Access Instructions ----"
echo "ArgoCD url: https://$(kubectl --namespace argocd get ingress argocd-ingress -n argocd -o jsonpath='{.spec.rules[0].host}')"
echo "Username: admin"
echo "Password: $(kubectl get secret argocd-initial-admin-secret -n argocd -o jsonpath="{.data.password}" | base64 --decode)"
echo "------------------------------------"
```

## Configure ArgoCD to use Keycloak SSO

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