# Cluster Roles & Secrets

This page describes the database roles used by CloudNativePG and how to manage them.

## Roles

File `/k8s/cnpg-system/cnpg-cluster-values.yaml` contains the following configuration for database roles.
```yaml
roles:
    - name: <ROLE_NAME>
        ensure: present # this field can be set to 'present' or 'absent' to create or delete the role respectively
        login: true
        superuser: false # this field can be set to true or false to grant or revoke superuser privileges
        passwordSecret:
            name: <ROLE_SECRET_NAME>
    # START ENVIRONMENT SPECIFIC ROLES SECTION
    # This section can be used to define additional roles specific to your environment. It will be preserved during updates.
    - name: <ROLE_NAME>
        ensure: present
        login: true
        superuser: false
        passwordSecret:
            name: <ROLE_SECRET_NAME>
    # END ENVIRONMENT SPECIFIC ROLES SECTION
```

By default the following roles are created:
- `postgres`: This is the default superuser role created by PostgreSQL.
- `keycloak`: This role is used by the Keycloak.
- `harbor`: This role is used by the Harbor.
- `forgejo`: This role is used by the Forgejo.

You can add additional roles as needed here: 
```yaml
# START ENVIRONMENT SPECIFIC ROLES SECTION
...
# END ENVIRONMENT SPECIFIC ROLES SECTION
```
 This section is preserved during updates, so you can safely add your custom roles there without worrying about them being overwritten. This is where you can add roles for your applications that need to access the database.

## External Secrets

Let's see how to create the secrets for the default roles. The process is the same for any additional roles you want to create.

```bash
kubectl exec -ti openbao-0 -n openbao -- bao kv put -mount=kv cnpg-superuser-secret \
    username=postgres \
    password=$(openssl rand -base64 32)
```

```bash
kubectl exec -ti openbao-0 -n openbao -- bao kv put -mount=kv keycloak-db-credentials \
    username=keycloak \
    password=$(openssl rand -base64 32)
```

```bash
kubectl exec -ti openbao-0 -n openbao -- bao kv put -mount=kv harbor-db-credentials \
    username=harbor \
    password=$(openssl rand -base64 32)
```

```bash
kubectl exec -ti openbao-0 -n openbao -- bao kv put -mount=kv forgejo-db-credentials \
    username=forgejo \
    password=$(openssl rand -base64 32)
```

After creating the secrets in OpenBao, you need to create the corresponding ExternalSecrets in Kubernetes to make them available to the CloudNativePG operator.

Start by creating the `cnpg-system` namespace if it doesn't exist already.

```bash
kubectl apply -f k8s/cnpg-system/cnpg-system-namespace.yaml
```

Then apply the ExternalSecret manifests for each role.

```bash
kubectl apply -f k8s/cnpg-system/cnpg-superuser-secret-external-secret.yaml
kubectl apply -f k8s/cnpg-system/databases/keycloak/keycloak-db-credentials-external-secret.yaml
kubectl apply -f k8s/cnpg-system/databases/harbor/harbor-db-credentials-external-secret.yaml
kubectl apply -f k8s/cnpg-system/databases/forgejo/forgejo-db-credentials-external-secret.yaml
```

:::warning
Every time you update the secrets in OpenBao, make sure they are properly synced to Kubernetes by checking the corresponding ExternalSecrets. The CloudNativePG operator relies on these secrets to manage the database roles and permissions, so it's crucial to keep them up to date.

Additionally everytime you update roles you need to reapply the cluster manifest to ensure the changes are picked up by the operator.
For more details see [Cluster setup](./20_operator_and_cluster.md#cluster-setup).
:::