# Namespace admin service account

Use this guide when a CI job, deployment script, or operator workstation needs to run `kubectl` against one namespace without using the cluster admin kubeconfig.

The example namespace is `namespace1`, and the service account is named `namespace-admin`. Replace these names before applying the manifest if you want different names.

:::warning
The kubeconfig created below contains a bearer token. Store it as a CI secret or protected file, and do not commit it to Git.
:::

## What this creates

The manifest in `k8s/namespace-admin/namespace-admin-rbac.yaml` creates three Kubernetes objects:

- `Namespace` - the namespace that the service account can manage.
- `ServiceAccount` - the identity that CI or `kubectl` will use.
- `RoleBinding` - the permission grant that connects the service account to the built-in `admin` `ClusterRole` inside `namespace1`.

The important part is the `RoleBinding`. Even though it references a `ClusterRole`, it grants those permissions only in the namespace where the `RoleBinding` exists. Because this manifest does not create a `ClusterRoleBinding`, the service account does not get cluster-wide access.

:::note
The built-in `admin` role gives broad admin-level access to namespaced resources. It is suitable for typical deployment work such as applying Deployments, Services, Ingresses, ConfigMaps, Secrets, Jobs, and similar application resources.
:::

## Apply the RBAC manifest

Run these commands from this repository using an existing admin kubeconfig. If your admin kubeconfig is already at `~/.kube/config`, you can skip the `KUBECONFIG` export.

```bash
export KUBECONFIG=/path/to/admin-kubeconfig
kubectl apply -f k8s/namespace-admin/namespace-admin-rbac.yaml
```

If you changed the namespace or service account name in the manifest, use those same values in every command below.

Verify the service account can manage resources in `namespace1`:

```bash
kubectl auth can-i create deployments.apps \
  -n namespace1 \
  --as=system:serviceaccount:namespace1:namespace-admin
```

The output should be:

```text
yes
```

Verify it does not have access to another namespace:

```bash
kubectl auth can-i create deployments.apps \
  -n default \
  --as=system:serviceaccount:namespace1:namespace-admin
```

The output should be:

```text
no
```

## Create a restricted kubeconfig

Generate an expiring token for the service account:

```bash
TOKEN="$(kubectl -n namespace1 create token namespace-admin --duration=8760h)"
```

This asks Kubernetes for a token valid for one year. Your cluster can choose to issue a shorter-lived token, depending on API server settings.

Create a kubeconfig that contains only this service account credential:

```bash
ADMIN_KUBECONFIG="${KUBECONFIG:-$HOME/.kube/config}"
RESTRICTED_KUBECONFIG="$HOME/.kube/config-namespace-admin"
CLUSTER_NAME="$(kubectl config view --kubeconfig "$ADMIN_KUBECONFIG" --minify -o jsonpath='{.clusters[0].name}')"
SERVER_URL="$(kubectl config view --kubeconfig "$ADMIN_KUBECONFIG" --minify -o jsonpath='{.clusters[0].cluster.server}')"
CA_DATA="$(kubectl config view --kubeconfig "$ADMIN_KUBECONFIG" --raw --minify -o jsonpath='{.clusters[0].cluster.certificate-authority-data}')"
CA_FILE="$(mktemp)"

printf '%s' "$CA_DATA" | base64 --decode > "$CA_FILE"

mkdir -p "$HOME/.kube"

kubectl config set-cluster "$CLUSTER_NAME" \
  --kubeconfig "$RESTRICTED_KUBECONFIG" \
  --server "$SERVER_URL" \
  --certificate-authority "$CA_FILE" \
  --embed-certs=true

rm "$CA_FILE"

kubectl config set-credentials namespace-admin \
  --kubeconfig "$RESTRICTED_KUBECONFIG" \
  --token "$TOKEN"

kubectl config set-context namespace1 \
  --kubeconfig "$RESTRICTED_KUBECONFIG" \
  --cluster "$CLUSTER_NAME" \
  --user namespace-admin \
  --namespace namespace1

kubectl config use-context namespace1 \
  --kubeconfig "$RESTRICTED_KUBECONFIG"

chmod 600 "$RESTRICTED_KUBECONFIG"
```

Verify the restricted kubeconfig works:

```bash
kubectl --kubeconfig "$RESTRICTED_KUBECONFIG" auth can-i create deployments.apps -n namespace1
kubectl --kubeconfig "$RESTRICTED_KUBECONFIG" auth can-i create deployments.apps -n default
kubectl --kubeconfig "$RESTRICTED_KUBECONFIG" auth can-i get nodes
```

Expected output:

```text
yes
no
no
```

## Install on the CI server

Copy the restricted kubeconfig to the CI server, for example:

```bash
ssh <ci-user>@<ci-server> 'mkdir -p ~/.kube'
scp ~/.kube/config-namespace-admin <ci-user>@<ci-server>:~/.kube/config-namespace-admin
```

On the CI server, set the file permissions:

```bash
chmod 600 ~/.kube/config-namespace-admin
```

Use it in deployment jobs:

```bash
export KUBECONFIG=~/.kube/config-namespace-admin
kubectl config use-context namespace1
kubectl apply -n namespace1 -f path/to/your-manifest.yaml
```

## Grant access to another namespace

Kubernetes does not have a native RBAC object for "this selected list of namespaces". To grant access to another namespace, create another `RoleBinding` in that namespace.

For example, to grant the same service account access to `namespace2`, apply this:

```yaml
apiVersion: rbac.authorization.k8s.io/v1
kind: RoleBinding
metadata:
  name: namespace-admin
  namespace: namespace2
subjects:
  - kind: ServiceAccount
    name: namespace-admin
    namespace: namespace1
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name: admin
```

The service account still lives in `namespace1`. The new `RoleBinding` lives in `namespace2`, and that is what grants access to `namespace2`.

Optionally add another context to the same restricted kubeconfig:

```bash
kubectl config set-context namespace2 \
  --kubeconfig "$RESTRICTED_KUBECONFIG" \
  --cluster "$CLUSTER_NAME" \
  --user namespace-admin \
  --namespace namespace2
```

## Rotate or revoke access

To rotate the token, generate a new token and recreate or update the restricted kubeconfig:

```bash
TOKEN="$(kubectl -n namespace1 create token namespace-admin --duration=8760h)"
```

To revoke access to one namespace, delete the `RoleBinding` in that namespace:

```bash
kubectl delete rolebinding namespace-admin -n namespace1
```

To revoke the service account completely:

```bash
kubectl delete serviceaccount namespace-admin -n namespace1
```
