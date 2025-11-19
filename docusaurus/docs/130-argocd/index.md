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