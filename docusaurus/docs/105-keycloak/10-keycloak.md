# Keycloak

Keycloak is an open-source identity and access management solution aimed at modern applications and services. It provides features such as single sign-on (SSO), user federation, identity brokering, and social login. Keycloak supports standard protocols like OAuth2, OpenID Connect, and SAML 2.0, making it a versatile choice for managing authentication and authorization in various environments.

## Deploy Keycloak

:::info
Database setup is complete as part of the Cloud Native PostgreSQL deployment.
:::

You can now deploy Keycloak using the provided Kubernetes manifests.

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

You need to create a new realm called `nursery`, users and clients as needed for your applications. The detailed config is beyond the scope of this documentation, but you can refer to the [Keycloak Documentation](https://www.keycloak.org/documentation) for guidance on how to set up realms, clients, and users according to your requirements.

## Create Groups

You can create groups in Keycloak to manage user permissions and access control more efficiently. You should create a the following groups at minimum:

- `Forgejo Contributors` group for users who should have access to Forgejo
- `Forgejo Admins` group for users who should have administrative privileges in Forgejo
- `Admins` group for users who should have administrative privileges in cluster-wide applications

:::warning
Add `groups` scope to the clients that you will be using for SSO, otherwise group information will not be included in the tokens issued by Keycloak and SSO configuration will not work as expected.
:::