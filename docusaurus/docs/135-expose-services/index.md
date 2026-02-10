# Expose Services

In this chapter, we will learn how to expose TCP services running in a Kubernetes cluster to the outside world in a secure manner.

While HTTP/HTTPS services can be exposed using Ingress, there are TCP services that require different approach. Until Hetzner LB starts supporting firewall rules, this is the most efficient way I found to expose TCP services while ensuring security. [Learn More](https://docs.hetzner.com/cloud/firewalls/faq#can-firewalls-be-applied-to-my-hetzner-cloud-load-balancers)

We are creating a HAProxy daemon set that runs on each node and forwards traffic to the appropriate service. This way, we can expose any TCP service without needing to configure Ingress or load balancers. The daemon set will listen on a specific port and forward traffic to the service running in the cluster.

First export the source IP what will be allowed to access the service as an environment variable. I'm using the already defined `SSH_SOURCE_IP` variable that contains the IP address of my HQ, but you can replace it with any IP address you want to allow access to the services.

```bash
export HQ_IP=$SSH_SOURCE_IP
```

Create namespace for the TCP proxy:
```bash
kubectl apply -f k8s/tcp-proxy/tcp-proxy-namespace.yaml
```

Then create a ConfigMap for HAProxy with the following content:
```bash
envsubst '$HQ_IP' < k8s/tcp-proxy/haproxy-cfg-configmap.yaml | kubectl apply -f -
```

Finally, create the HAProxy daemon set:
```bash
kubectl apply -f k8s/tcp-proxy/tcp-haproxy-daemonset.yaml
```

With this setup, you can safely expose any TCP service on load balancer by adding the appropriate frontend and backend configuration to the HAProxy ConfigMap. The provided ConfigMap is for exposing:

| Service | Port |
|---------|------|
| Forgejo git SSH service | 22 |
| PostgreSQL CNPG cluster | 5432 |