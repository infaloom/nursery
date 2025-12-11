# Keycloak

Keycloak is an open-source identity and access management solution aimed at modern applications and services. It provides features such as single sign-on (SSO), user federation, identity brokering, and social login. Keycloak supports standard protocols like OAuth2, OpenID Connect, and SAML 2.0, making it a versatile choice for managing authentication and authorization in various environments.

## Database Setup

Create openbao secret with random password:
```bash
kubectl exec -ti openbao-0 -n openbao -- bao kv put -mount=kv keycloak-db-credentials \
    username=keycloak \
    password=$(openssl rand -base64 32)
```

Add external secret for Keycloak database credentials:
```bash
kubectl apply -f k8s/cnpg-system/databases/keycloak/keycloak-db-credentials-external-secret.yaml
```

Update the cluster values to include the Keycloak role:
```yaml
  roles:
    - name: keycloak
      ensure: present # <- this line ensures the role is created
      login: true
      superuser: false
      passwordSecret:
        name: keycloak-db-credentials
```

Update CNPG cluster to sync keycloak role:
```bash
kubectl apply -f k8s/cnpg-system/cnpg-cluster-values.yaml
```

Update CNPG cluster to sync keycloak role (ensure environment variables are set):
```bash
envsubst < k8s/cnpg-system/cnpg-cluster-values.yaml | \
helm upgrade --install cnpg-cluster cnpg/cluster \
--version 0.2.1 \
--namespace cnpg-system --create-namespace \
--values -
```

Create Keycloak database:
```bash
kubectl apply -f k8s/cnpg-system/databases/keycloak/keycloak-database.yaml
```

## Deploy Keycloak

Apply Keycloak namespace:
```bash
kubectl apply -f k8s/keycloak/keycloak-namespace.yaml
```

Apply config secret for Keycloak:
```bash
envsubst < k8s/keycloak/keycloak-config-external-secret.yaml | kubectl apply -f -
```

Apply Keycloak deployment:
```bash
kubectl apply -f k8s/keycloak/keycloak.yaml
```

Apply Keycloak ingress:
```bash
envsubst < k8s/keycloak/keycloak-ingress.yaml | kubectl apply -f -
```

Apply Keycloak admin ingress:
```bash
envsubst < k8s/keycloak/keycloak-admin-ingress.yaml | kubectl apply -f -
```

## Access Keycloak

You can access the Keycloak user account console at: `https://account.${CLUSTER_DOMAIN}`

You can access the Keycloak admin console at: `https://keycloak.${CLUSTER_DOMAIN}`

The default admin username is `admin` and the password is `admin`. Create a proper admin user immediately after your first login and delete the default admin user to ensure the security of your Keycloak instance.

## Create Realm, Clients, and Users

You need to create a new realm, clients, and users as needed for your applications. The detailed config is beyond the scope of this documentation, but you can refer to the [Keycloak Documentation](https://www.keycloak.org/documentation) for guidance on how to set up realms, clients, and users according to your requirements.