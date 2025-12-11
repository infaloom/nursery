# Install K3s

Registry mirroring is enabled because some Hetzner servers are blacklisted. By enabling registry mirroring we
ensure that the image is available to the cluster by being able to pull from any of the nodes.
https://docs.k3s.io/installation/registry-mirror

## Prepare private ssh key

Ensure that the private ssh key corresponding to the public key you provided during pulumi provisioning is available on your local machine.
For example, if you provided `~/.ssh/id_rsa.pub` during pulumi provisioning, ensure that `~/.ssh/id_rsa` is available on your local machine.
Also ensure that the private key has the correct permissions.
```bash
chmod 600 ~/.ssh/id_rsa
```

## Disable ufw on all servers

This is K3s prerequisite. The servers are not accessible from the internet anyway.
```bash
ansible-playbook -i ./ansible/inventory.yaml ./ansible/playbooks/100_disable_ufw.yaml
```

## Init K3s on the first server

```bash
ansible-playbook -i ./ansible/inventory.yaml ./ansible/playbooks/101_k3s_init.yaml
```

## Add other servers

```bash
ansible-playbook -i ./ansible/inventory.yaml ./ansible/playbooks/102_k3s_add_servers.yaml
```

## Add agents
```bash
ansible-playbook -i ./ansible/inventory.yaml ./ansible/playbooks/103_k3s_add_agents.yaml
```

## Add harbor domain to hosts file for internal resolution
This is required for accessing harbor from within the cluster. Ensure that the `CLUSTER_DOMAIN` environment variable is set before running the playbook.

```bash
ansible-playbook -i ./ansible/inventory.yaml ./ansible/playbooks/104_add_internal_services_to_hosts.yaml
```
