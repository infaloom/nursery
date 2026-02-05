# Harbor Setup

https://goharbor.io/

Harbor is an open-source container image registry that secures images with role-based access control, scans images for vulnerabilities, signs images as trusted, etc.

## Create external secrets

Create namespace
```bash
kubectl apply -f k8s/harbor/harbor-namespace.yaml
```

### Admin password

Create OpenBao secret with a random password for Harbor admin user
```bash
kubectl exec -ti openbao-0 -n openbao -- bao kv put -mount=kv harbor-admin-password \
    HARBOR_ADMIN_PASSWORD=$(openssl rand -base64 32)
```

Create ExternalSecret to pull the admin password
```bash
kubectl apply -f k8s/harbor/harbor-admin-password-external-secret.yaml
```

### Harbor database credentials

:::info
Harbor role and OpenBao secret should have been created in the [Cluster Roles and Secrets](../100-cnpg/10_cluster_roles_and_secrets.md).
:::

Create ExternalSecret to pull the Harbor db credentials into harbor namespace
```bash
kubectl apply -f k8s/harbor/harbor-db-credentials-external-secret.yaml
```

## Install

By default the harbor is configured to be served from `https://harbor.${CLUSTER_DOMAIN}` but you can change it in the `k8s/harbor/values.yaml` file.

```bash
export CLUSTER_DOMAIN=$(pulumi --cwd $PULUMI_CWD config get cluster:domain)
```

It will be accessable from the ssh source IP address you set in the pulumi config.

:::warning
Harbor has issues with Redis Sentinel support at the time of writing.
We are using the Harbor helm chart built-in redis instance.
:::

:::info
`harbor` database should have been created earlier as part of the [Databases](../100-cnpg/30_databases.md).
:::

Add helm repo
```bash
helm repo add harbor https://helm.goharbor.io
```

Install Harbor with Helm
```bash
envsubst < k8s/harbor/harbor-values.yaml | \
helm upgrade --install harbor harbor/harbor \
--version 1.16.2 \
--namespace harbor --create-namespace \
--values -
```

## Accessing Harbor

Run the following to get instructions to access Harbor UI
```bash
echo "---- Harbor Access Instructions ----"
echo "Harbor url: https://$(kubectl --namespace harbor get ingress harbor-ingress -n harbor -o jsonpath='{.spec.rules[0].host}')"
echo "Username: admin"
echo "Password: $(kubectl get secret harbor-admin-password -n harbor -o jsonpath="{.data.HARBOR_ADMIN_PASSWORD}" | base64 --decode)"
echo "------------------------------------"
```

## Pulling an image from the registry

Update the robot prefix to `robot-` to avoid bash issues with `robot$` using the harbor API:
```bash
curl -X PUT "https://harbor.${CLUSTER_DOMAIN}/api/v2.0/configurations" \
 -H "accept: application/json" \
 -H "Content-Type: application/json" \
 -u "admin:$(kubectl get secret harbor-admin-password -n harbor -o jsonpath="{.data.HARBOR_ADMIN_PASSWORD}" | base64 --decode)" \
 -d "{\"robot_name_prefix\":\"robot-\"}"
```
You can also do it via Harbor UI in the Configuration section.

Then proceed to create a project and a robot user.

In the end, use the following command to create a secret for the robot user in the k8s cluster.
```bash
kubectl create secret docker-registry regcred -n {namespace} --docker-server=harbor.${CLUSTER_DOMAIN} --docker-username=robot-{your robot name} --docker-password=<your-password>
```
More on this in a dedicate section [TBD].