# Woodpecker CI

[Woodpecker CI](https://woodpecker-ci.org/) is a self-hosted continuous integration and delivery platform forked from Drone CI. It is designed to be simple, efficient, and scalable, making it a popular choice for developers looking to automate their build, test, and deployment processes.

## Create OAuth2 Application for Woodpecker CI in Forgejo

The specific steps to create a OAuth2 application for Woodpecker CI can be found in the [Forgejo documentation](https://forgejo.org/docs/latest/user/oauth2-provider/).

Once the application is created, make sure to note down the Client ID and Client Secret, as these will be needed for configuring Woodpecker CI.

Create openbao secret with OAuth2 credentials:
```bash
kubectl exec -ti openbao-0 -n openbao -- bao kv put -mount=kv woodpecker-server-env \
WOODPECKER_FORGEJO=true \
WOODPECKER_FORGEJO_URL=https://forgejo.${CLUSTER_DOMAIN} \
WOODPECKER_FORGEJO_CLIENT=<YOUR_FORGEJO_CLIENT> \
WOODPECKER_FORGEJO_SECRET=<YOUR_FORGEJO_CLIENT_SECRET>
```

Create namespace for Woodpecker CI:
```bash
kubectl apply -f k8s/woodpecker/woodpecker-namespace.yaml
```

Add external secret for Woodpecker server environment variables:
```bash
kubectl apply -f k8s/woodpecker/woodpecker-server-env-external-secret.yaml
```

```bash
envsubst < k8s/woodpecker/values.yaml | \
helm upgrade --install woodpecker oci://ghcr.io/woodpecker-ci/helm/woodpecker \
--version 3.4.2 \
--namespace woodpecker --create-namespace \
--values -
```