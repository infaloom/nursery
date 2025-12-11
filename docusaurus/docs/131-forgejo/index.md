

# Forgejo

[Forgejo](https://forgejo.org/) is a community-driven, lightweight code hosting solution that is a fork of Gitea. It provides a simple and efficient platform for managing Git repositories, making it easy for developers to collaborate on projects.

https://code.forgejo.org/forgejo-helm/forgejo-helm

## Create CNPG Database

Create openbao secret with random password:
```bash
kubectl exec -ti openbao-0 -n openbao -- bao kv put -mount=kv forgejo-db-credentials \
    username=forgejo \
    password=$(openssl rand -base64 32)
```

Add external secret for Forgejo database credentials:
```bash
kubectl apply -f k8s/cnpg-system/databases/forgejo/forgejo-db-credentials-external-secret.yaml
```

Update the cluster values to include the Forgejo role:
```yaml
  roles:
    - name: forgejo
      ensure: present # <- this line ensures the role is created
      login: true
      superuser: false
      passwordSecret:
        name: forgejo-db-credentials
```

Update CNPG cluster to sync forgejo role (ensure environment variables are set):
```bash
envsubst < k8s/cnpg-system/cnpg-cluster-values.yaml | \
helm upgrade --install cnpg-cluster cnpg/cluster \
--version 0.2.1 \
--namespace cnpg-system --create-namespace \
--values -
```

Create Forgejo database:
```bash
kubectl apply -f k8s/cnpg-system/databases/forgejo/forgejo-database.yaml
```

## Configuration

Create namespace for Forgejo:
```bash
kubectl apply -f k8s/forgejo/forgejo-namespace.yaml
```

Create Object Bucket Claim for Forgejo:
```bash
kubectl apply -f k8s/forgejo/forgejo-bucket-claim.yaml
```

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