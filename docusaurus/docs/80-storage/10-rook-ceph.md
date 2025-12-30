# Rook Ceph Setup

## Add Rook Helm repo

```bash
helm repo add rook-release https://charts.rook.io/release
```

## Install Rook Ceph

https://rook.io/docs/rook/latest-release/Helm-Charts/operator-chart/#installing

Install rook-ceph operator
```bash
helm upgrade --install rook-ceph rook-release/rook-ceph \
--version v1.16.6 \
--namespace rook-ceph --create-namespace \
--values k8s/rook-ceph/rook-ceph-operator-values.yaml
```

Install ceph cluster
:::note
It takes a while for the ceph cluster to stabilize. 5-6 minutes from my experience.
Don't continue with the later commands until the cluster is ready.

```bash
kubectl --namespace rook-ceph get cephcluster
```
PHASE should be `Ready`.
:::

```bash
helm upgrade --install rook-ceph-cluster rook-release/rook-ceph-cluster \
--version v1.16.6 \
--namespace rook-ceph --create-namespace \
--values k8s/rook-ceph/rook-ceph-cluster-values.yaml 
```

## Enable rook orchestrator
```bash
kubectl -n rook-ceph exec -it deploy/rook-ceph-tools -- bash
```

Inside the pod shell, run:
```bash
ceph orch set backend rook
ceph orch status
```
Output should be
```
Backend: rook
Available: Yes
```

## Expose RGW ingress

Optional: If you want to expose the S3 compatible RGW endpoint via ingress, run the following command:
```bash
envsubst < k8s/rook-ceph/rgw-ingress.yaml | kubectl apply --wait -f -
```

## Ceph Dashboard Setup

### Skip certificate verification for Ceph Dashboard

This is required if you are using Traefik as ingress controller and want to access the Ceph Dashboard via HTTPS.

Apply service annotations to skip certificate verification. This is because the service uses a self-signed certificate.
```bash
kubectl apply -f k8s/rook-ceph/skip-verify.yaml
```

Execute the following command to edit the existing service:
```bash
kubectl -n rook-ceph annotate svc rook-ceph-mgr-dashboard \
    traefik.ingress.kubernetes.io/service.serversscheme=https \
    traefik.ingress.kubernetes.io/service.serverstransport=rook-ceph-skip-verify@kubernetescrd \
    --overwrite
```

### Enable Ceph Dashboard ingress

```bash
envsubst < k8s/rook-ceph/ceph-dashboard-ingress.yaml | kubectl apply --wait -f -
```

### Access Ceph Dashboard
Run the following command to get the dashboard URL
```bash
echo "Access the dashboard at https://ceph-dashboard.${CLUSTER_DOMAIN}"
echo "Username: admin"
echo "Password: $(kubectl -n rook-ceph get secret rook-ceph-dashboard-password -o jsonpath="{['data']['password']}" | base64 --decode)"
```