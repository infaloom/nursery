

# Forgejo

[Forgejo](https://forgejo.org/) is a community-driven, lightweight code hosting solution that is a fork of Gitea. It provides a simple and efficient platform for managing Git repositories, making it easy for developers to collaborate on projects.

https://code.forgejo.org/forgejo-helm/forgejo-helm

## Configuration

Create namespace for Forgejo:
```bash
kubectl apply -f k8s/forgejo/forgejo-namespace.yaml
```

Create Object Bucket Claim for Forgejo:
```bash
kubectl apply -f k8s/forgejo/forgejo-bucket-claim.yaml
```

:::info
`forgejo` role and database should have been created with the cnpg cluster. See [Roles and Secrets](../100-cnpg/10_cluster_roles_and_secrets.md) and [Databases](../100-cnpg/30_databases.md).
:::

Add forgejo-db-credentials external secret in forgejo namespace:
```bash
kubectl apply -f k8s/forgejo/forgejo-db-credentials-external-secret.yaml
```

## Install Forgejo

Install Forgejo with Helm:
```bash
envsubst < k8s/forgejo/values.yaml | \
  helm upgrade --install forgejo oci://code.forgejo.org/forgejo-helm/forgejo \
  --version 15.0.3 \
  --namespace forgejo --create-namespace \
  --values -
```