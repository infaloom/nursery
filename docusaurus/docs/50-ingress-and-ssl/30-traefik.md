# Traefik & Core DNS
Traefik is a popular open-source edge router that can be used as an Ingress controller in Kubernetes clusters.

Apply traefik middleware for https redirect
```bash
kubectl apply -f k8s/kube-system/traefik-middleware-redirect-https.yaml
```
This enables PROXY protocol on the traefik service to allow the real client IP to be passed to the backend.
```bash
kubectl apply -f k8s/kube-system/traefik.yaml
```

Add SSH Source IP and internal traffic to the traefik whitelist

:::danger
This manifest assumes that you want to limit access to the management services (like Harbor, Grafana, etc.) only from your office IP and the cluster internal network.

Use caution if you decide to modify the IP allowlist as it may expose your management services to unwanted traffic.
:::

```bash
envsubst < k8s/kube-system/traefik-middleware-hq-ipallowlist.yaml | kubectl apply --wait -f -
```

Custom DNS to allow the cluster to access certain services through ingress from the inside.
```bash
envsubst < k8s/kube-system/coredns-custom.yaml | kubectl apply --wait -f -
```
