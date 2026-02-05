# Redis
https://github.com/bitnami/charts/tree/main/bitnami/redis

:::info
Bitnami ended support for docker images free tier starting 28th Aug 2025. https://github.com/bitnami/charts/issues/35164

Using bitnamilegacy in `values.yaml` and fixing chart version to `20.11.4`
:::

Create the redis password in OpenBao
```bash
kubectl exec -ti openbao-0 -n openbao -- bao kv put -mount=kv redis-password \
redis-password=$(openssl rand -base64 32)
```

Create namespace
```bash
kubectl apply -f k8s/redis-sentinel/redis-sentinel-namespace.yaml
```

Apply external secret to fetch the password
```bash
kubectl apply -f k8s/redis-sentinel/redis-password-external-secret.yaml
```

Add helm repo
```bash
helm repo add bitnami https://charts.bitnami.com/bitnami
```

Install redis sentinel
```bash
helm upgrade --install redis-sentinel bitnami/redis \
--version 20.11.4 \
--namespace redis-sentinel --create-namespace \
--values k8s/redis-sentinel/values.yaml
```

:::info
Read the output of the helm install command carefully. It contains instructions on how to connect to the redis sentinel cluster.        
:::