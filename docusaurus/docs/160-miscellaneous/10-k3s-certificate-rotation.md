# K3s Certificate rotation

K3s automatically renews expired or near-expiry client and server certificates when the service starts. Use manual rotation when you want K3s to issue new client and server certificates and keys instead of extending the existing ones.

The exact auto-renewal window depends on the K3s version. This repository currently pins `v1.32.3+k3s1`, so check the official K3s certificate documentation for version-specific behavior.

## Check certificate expiration

Run this on the node you want to inspect:

```bash
k3s certificate check --output table
```

This shows the certificate files on the node together with their subject, usage, expiration date, residual time, and status.

:::warning
This procedure is node-local and should be performed during a maintenance window. On server nodes the systemd service is `k3s`; on agent nodes it is `k3s-agent`.
:::

## Rotate certificates

Replace `<SERVICE_NAME>` with `k3s` on servers or `k3s-agent` on agents.

```bash
sudo systemctl stop <SERVICE_NAME>
sudo k3s certificate rotate
sudo systemctl start <SERVICE_NAME>
```

Repeat the rotation on each node that needs updated client or server certificates.

## Further reading

For CA rotation, custom or self-signed CA procedures, and service-account issuer key rotation, see the official K3s certificate documentation:
https://docs.k3s.io/cli/certificate
