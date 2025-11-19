# Monitoring

https://github.com/prometheus-community/helm-charts/tree/main/charts/kube-prometheus-stack

Set grafana admin password in OpenBao kv store
```bash
kubectl exec -ti openbao-0 -n openbao -- bao kv put -mount=kv grafana-admin-credentials admin-user=admin admin-password=$(openssl rand -base64 32)
```

Create namespace
```bash
kubectl create namespace kube-prometheus-stack
```

Create external secret to read grafana admin password from OpenBao
```bash
kubectl apply -f k8s/kube-prometheus-stack/grafana-admin-credentials-external-secret.yaml
```

Add helm repositories
```bash
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
```

Install prometheus stack
```bash
helm upgrade --install kube-prometheus-stack prometheus-community/kube-prometheus-stack \
--version 70.3.0 \
--namespace kube-prometheus-stack \
--values k8s/kube-prometheus-stack/kube-prometheus-stack-values.yaml
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