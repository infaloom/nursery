# Monitoring

https://github.com/prometheus-community/helm-charts/tree/main/charts/kube-prometheus-stack

## Grafana secrets

Create namespace
```bash
kubectl create namespace kube-prometheus-stack
```

### Admin credentials

Set grafana admin password in OpenBao kv store
```bash
kubectl exec -ti openbao-0 -n openbao -- bao kv put -mount=kv grafana-admin-credentials admin-user=admin admin-password=$(openssl rand -base64 32)
```

Create external secret to read grafana admin password from OpenBao
```bash
kubectl apply -f k8s/kube-prometheus-stack/grafana-admin-credentials-external-secret.yaml
```

### Keycloak OAuth credentials

Create Keycloak OAuth secret for Grafana
```bash
kubectl exec -ti openbao-0 -n openbao -- bao kv put -mount=kv keycloak-grafana-client-credentials client_id=<CLIENT_ID> client_secret=<CLIENT_SECRET>
```

Create external secret to read Keycloak OAuth credentials from OpenBao
```bash
kubectl apply -f k8s/kube-prometheus-stack/keycloak-grafana-client-credentials-external-secret.yaml
```

## Install kube-prometheus-stack

Add helm repositories
```bash
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
```

Install prometheus stack. Note that we are using `envsubst` to replace only the
`CLUSTER_DOMAIN` variable in the values file. This is because envsubst replaces `__file{}`
expressions as well, which we don't want.
```bash
envsubst '$CLUSTER_DOMAIN' < k8s/kube-prometheus-stack/kube-prometheus-stack-values.yaml | \
helm upgrade --install kube-prometheus-stack prometheus-community/kube-prometheus-stack \
--version 70.3.0 \
--namespace kube-prometheus-stack \
--values -
```

## Add Grafana dashboards
Using the `--server-side=true` flag to avoid the error `Too long: must have at most 262144 bytes`
because of certain dashboards that are too large.
```bash
kubectl apply --server-side=true -f k8s/kube-prometheus-stack/dashboards/
```

## Access Grafana

Setup Grafana ingress
```bash
envsubst < k8s/kube-prometheus-stack/grafana-ingress.yaml | kubectl apply --wait -f -
```

Run these commands to see the Grafana admin credentials and default URL.
```bash
echo "Grafana URL: https://grafana.${CLUSTER_DOMAIN}/"
echo "Grafana Admin Username: admin"
echo "Grafana Admin Password: $(kubectl get secret grafana-admin-credentials -n kube-prometheus-stack -o jsonpath="{.data.admin-password}" | base64 --decode)"
```