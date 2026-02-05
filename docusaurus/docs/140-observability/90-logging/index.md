# Logging

## Persistent log storage

Create loki rook-ceph bucket claim
```bash
kubectl apply -f k8s/loki/loki-bucket-claim.yaml
```

## Export ceph-object credentials
```bash
export LOKI_S3_ACCESS_KEY_ID=$(kubectl -n loki get secret loki-bucket-claim -o jsonpath='{.data.AWS_ACCESS_KEY_ID}' | base64 --decode); \
export LOKI_S3_SECRET_ACCESS_KEY=$(kubectl -n loki get secret loki-bucket-claim -o jsonpath='{.data.AWS_SECRET_ACCESS_KEY}' | base64 --decode)
```

## Loki

This installs Loki with 28 day log retention. Depending on your use case you may need longer retention period. For example, SOC 2 compliance requires minimum **365 days** retention for audit logs.

https://grafana.com/docs/loki/latest/setup/install/helm/install-scalable/

:::warning
If loki gets stuck in ContainerCreating you need to remove the pvc on the node where it is stuck. There is probably a better explanation for this but that is how I fixed it during experimentation.
:::

Add repo
```bash
helm repo add grafana https://grafana.github.io/helm-charts
```

Install loki
```bash
envsubst < k8s/loki/loki-values.yaml | \
helm upgrade --install loki grafana/loki \
--version 6.29.0 \
--namespace loki \
--values -
```

## Fluent Bit
https://docs.fluentbit.io/manual/installation/kubernetes

Add repo
```bash
helm repo add fluent https://fluent.github.io/helm-charts
```

Install fluent bit
```bash
helm upgrade --install fluent-bit fluent/fluent-bit \
--version 0.48.9 \
--namespace fluent-bit --create-namespace \
--values k8s/fluent-bit/values.yaml
```

:::note
Loki configures in multi-tenant mode so Grafana Loki datasource needs to be setup with url `http://loki-gateway.loki.svc.cluster.local` and header `X-Scope-OrgID` with value `fluent-bit`
This is done automatically when installing the `kube-prometheus-stack` helm chart.
:::