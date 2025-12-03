# Manage storage

## Expand Rook-Ceph storage

https://rook.io/docs/rook/v1.9/ceph-osd-mgmt.html

Initially, each agent node in the Kubernetes cluster will have 2 volumes of 100GB each. 6 volumes total.
You can increase this number to 3 per node or any other number of volumes by updating the Pulumi code.
Without additional updates to the code we are always adding the same number of volumes to each agent node.

```csharp
foreach (var agent in k8sAgentServer)
{
    int numberOfVolumesPerAgent = 3; // <-- UPDATE THIS NUMBER TO ADD MORE VOLUMES
    for (var i = 1; i <= numberOfVolumesPerAgent; i++)
    {
        Output.Format($"{agent.Name}_volume_{i}").Apply(volumeName =>
        {
            var volume = new HCloud.Volume(volumeName, new()
            {
                Name = volumeName,
                Size = 100,
                ServerId = agent.Id.Apply(int.Parse),
            });
            return Task.CompletedTask;
        });
    }
}
```

Make sure you run `pulumi preview` before applying the updates. When updating from the default 2 volumes to 3 volumes per node the output should look like this:
```
Previewing update (production):
     Type                    Name                   Plan
     pulumi:pulumi:Stack     hetzner-production
 +   ├─ hcloud:index:Volume  agent1_volume_3        create
 +   ├─ hcloud:index:Volume  agent2_volume_3        create
 +   └─ hcloud:index:Volume  agent3_volume_3        create

Resources:
    + 3 to create
    38 unchanged
```

Run `pulumi up` to apply the changes.

In order to create new OSDs in the ceph cluster Rook operator needs to be restarted by deleting the operator pod
```bash
kubectl -n rook-ceph delete pod $(kubectl -n rook-ceph get pod -l "app=rook-ceph-operator" -o jsonpath='{.items[0].metadata.name}')
```

Confirm that the new OSDs have been added to the cluster and are in `up` status. **This may take a few minutes!**
```bash
kubectl -n rook-ceph exec -it deploy/rook-ceph-tools -- ceph osd tree
```

Confirm that the Ceph cluster is healthy `health: HEALTH_OK` by running

```bash
kubectl -n rook-ceph exec -it deploy/rook-ceph-tools -- ceph status
```

## Expanding PVCs

:::warning
It is easy to expand a PVC. Shrinking is not supported. Expand wisely!
:::

Let's for example expand the PVC of the test `whoami` workload.

Update the file `k8s/whoami/whoami.yaml` as follows:
```yaml
kind: PersistentVolumeClaim
apiVersion: v1
metadata:
  name: busybox-logs
  namespace: whoami
spec:
  storageClassName: ceph-block
  accessModes:
    - ReadWriteOnce
  resources:
    requests:
      storage: 100Mi # <- UPDATE THIS VALUE FROM 50Mi to 100Mi
```

Run `kubectl apply`
```bash
envsubst < k8s/whoami/whoami.yaml | kubectl apply --wait -f -
```

Expansion will happen automatically after a few seconds.

## Shrink Rook-Ceph storage

:::danger
Ensure your new configuration has enough space to hold all the data.

Shrinking storage is a complex operation and may lead to data loss if not done properly.

It is recommended to backup all important data before proceeding.

Also ensure that the number of volumes per node doesn't go bellow 2!

Always remove one volume at a time per node and wait for the Ceph cluster to stabilize before continuing.
:::

Details at https://rook.io/docs/rook/v1.9/ceph-osd-mgmt.html#remove-an-osd

```bash
# Stop Rook operator
kubectl -n rook-ceph scale deployment rook-ceph-operator --replicas=0
```

It is recommended to run the following sequence of commands for each OSD wait for the Ceph cluster to stabilize before continuing on with removals.

:::note
OSD IDs are zero based. Initial 6 OSDs they will be numbered from 0 to 5.
If you are removing the last volume from each node, the OSD IDs will be 6, 7, 8 etc.
:::
```bash
# Stop OSD deployment
kubectl -n rook-ceph scale deployment rook-ceph-osd-{ID} --replicas=0
# Mark OSD as down (may report that the OSD is already down)
kubectl -n rook-ceph exec -it deploy/rook-ceph-tools -- ceph osd down osd.{ID}
# Mark OSDs as out
kubectl -n rook-ceph exec -it deploy/rook-ceph-tools -- ceph osd out osd.{ID}
# Wait for Cepth to finish backfilling to other OSDs
kubectl -n rook-ceph exec -it deploy/rook-ceph-tools -- ceph status
# Purge OSDs
kubectl -n rook-ceph exec -it deploy/rook-ceph-tools -- ceph osd purge {ID} --yes-i-really-mean-it
# Confirm they are removed
kubectl -n rook-ceph exec -it deploy/rook-ceph-tools -- ceph osd tree
# Remove OSD deployment
kubectl delete deployment -n rook-ceph rook-ceph-osd-{ID}
```

Update Pulumi code by reducing the number of volumes. Always remove 1 volume per node per session to allow Ceph cluster to stabilize.

```csharp
int numberOfVolumesPerAgent = 2;
```

Update the infrastructure
```bash
# Preview changes
pulumi preview
# Apply updates
pulumi up
```

Start Rook operator
```bash
kubectl -n rook-ceph scale deployment rook-ceph-operator --replicas=1
```
