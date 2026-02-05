# Databases

In this section, we will set up the databases for Keycloak, Harbor, and Forgejo declaratively using CloudNativePG.

```bash
kubectl apply -f k8s/cnpg-system/databases/keycloak/keycloak-database.yaml
kubectl apply -f k8s/cnpg-system/databases/harbor/harbor-database.yaml
kubectl apply -f k8s/cnpg-system/databases/forgejo/forgejo-database.yaml
```

You can follow the same approach to create databases for any other applications you want to deploy in the future. Just create a new manifest similar to the ones above and apply it.

:::warning
Before applying the database manifests, make sure to create the corresponding roles and ExternalSecrets for the database credentials as described in the previous section [Cluster Roles & Secrets](./10_cluster_roles_and_secrets.md).
:::