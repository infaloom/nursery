# Environment Variables

## Generate secrets

K3s token:
```bash
pulumi --cwd $PULUMI_CWD config set --secret k3s:token $(openssl rand -base64 32)
```
Cluster domain:
```bash
pulumi --cwd $PULUMI_CWD config set --secret cluster:domain {REPLACE_WITH_YOUR_DOMAIN}
```

:::important
Cluster domain is the domain you will use to access services like Harbor, Grafana, etc. In the later steps we will create wildcard DNS records pointing to the load balancer IP.

For example, if your cluster domain is `infrastructure.example.com`, you will create DNS record for:
- `*.infrastructure.example.com` -> Load Balancer IP
:::

## Export necessary environment variables
You can copy and paste the following commands to set the environment variables required for the K3s installation.

```bash
export K3S_TOKEN=$(pulumi --cwd $PULUMI_CWD config get k3s:token); \
export CLUSTER_DOMAIN=$(pulumi --cwd $PULUMI_CWD config get cluster:domain); \
export INSTALL_K3S_VERSION="v1.32.3+k3s1"; \
export K8S_URL=k8s.${CLUSTER_DOMAIN};
```

k3s version can be checked at: https://github.com/k3s-io/k3s/releases/tag/v1.32.3+k3s1