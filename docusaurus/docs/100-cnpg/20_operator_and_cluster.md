# Operator and Cluster

Details at https://cloudnative-pg.io/.

For people not familiar with PostgreSQL, there is a popular tool for managing PostgreSQL databases called pgAdmin. https://www.pgadmin.org/

## Install operator

Add repo
```bash
helm repo add cnpg https://cloudnative-pg.github.io/charts
```

This install cnpg operator in the single namespace mode
```bash
helm upgrade --install cnpg cnpg/cloudnative-pg \
--version 0.23.2 \
--namespace cnpg-system --create-namespace \
--values k8s/cnpg-system/cnpg-values.yaml
```

## Remote backups

For the purpose of remote data copy, I'm running an Amazon S3 compatible Minio Server on premise. The Minio Server setup isn't part of this guide.

Feel free to use any S3 compatible object storage. The instructions won't change except for the endpoint URL and your specific keys.

Ensure you have a bucket named `cnpg-backup` in your S3 compatible storage or adapt accordingly.

Create secret in OpenBao
```bash
kubectl exec -ti openbao-0 -n openbao -- bao kv put -mount=kv cnpg-backup-s3-creds \
    ACCESS_KEY_ID="{REPLACE_WITH_ACCESS_KEY_ID}" \
    ACCESS_SECRET_KEY="{REPLACE_WITH_SECRET_ACCESS_KEY}" \
    ENDPOINT_URL="{REPLACE_WITH_S3_ENDPOINT_URL}"
```

Create external secret
```bash
kubectl apply -f k8s/cnpg-system/cnpg-backup-s3-creds-external-secret.yaml
```

Export endpoint URL variable of Amazon S3 or other S3 compatible storage.
```bash
export CNPG_BACKUP_S3_ENDPOINT_URL=$(kubectl get secret cnpg-backup-s3-creds -n cnpg-system -o jsonpath="{.data.ENDPOINT_URL}" | base64 --decode)
```
:::warning
Altough we are using Rook Ceph as data store which is replicated 3 times, it is not advisible to skip remote backups. But in case you wish to do so you can set the parameter `backups.enabled: false` in `k8s/cnpg-system/cnpg-cluster-values.yaml` which will skip the whole backup configuration.
:::

## Cluster setup

Superuser credentials are created in the previous step.

We can proceed with cluster setup as follows:
```bash
envsubst < k8s/cnpg-system/cnpg-cluster-values.yaml | \
helm upgrade --install cnpg-cluster cnpg/cluster \
--version 0.2.1 \
--namespace cnpg-system --create-namespace \
--values -
```

## Accessing the database

You can use any Postgres client to access the database. For example, you can use `psql` command line tool. Or the great pgAdmin tool mentioned earlier.

Run the following command to get the information needed to connect to the database.
```bash
echo "---- PostgreSQL Connection Info ----"
echo "User: postgres"
echo "Password: $(kubectl get secret cnpg-superuser-secret -n cnpg-system -o jsonpath="{.data.password}" | base64 --decode)"
kubectl port-forward -n cnpg-system svc/cnpg-cluster-rw 5432:5432
```

## cnpg plugin

https://cloudnative-pg.io/documentation/1.15/cnpg-plugin/

Install cnpg plugin
```bash
curl -sSfL \
  https://github.com/cloudnative-pg/cloudnative-pg/raw/main/hack/install-cnpg-plugin.sh | \
  sudo sh -s -- -b /usr/local/bin
```

Check cluster status
```bash
kubectl cnpg status cnpg-cluster -n cnpg-system
```

You can, for example, restart cluster to apply any configuration changes
```bash
kubectl cnpg restart cnpg-cluster -n cnpg-system
```

## Ad-hoc backups

If you didn't make any updates we schedule a daily backup every midnight. You can test the backups by running an ad-hoc backup (optional):
```bash
kubectl cnpg backup cnpg-cluster -n cnpg-system
```

You can list backups with the following cmd:
```bash
kubectl get backups -n cnpg-system --selector cnpg.io/cluster=cnpg-cluster
```