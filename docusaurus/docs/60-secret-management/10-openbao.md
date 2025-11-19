# OpenBao

:::danger
The following setup may not be suitable for environments requiring the highest level of security, for example apps storing finance or healthcare data. For these environments you may want to use 3rd-party key management system.
:::

Details at https://openbao.org/docs/platform/k8s/helm/

## Create namespace and unseal key

```bash
kubectl create namespace openbao
```

Generate unseal key and create k8s secret.
```bash
openssl rand -out openbao-unseal-1.key 32
```
```bash
kubectl create secret generic openbao-unseal-key -n openbao --from-file=openbao-unseal-1.key
```
Store base64 encoded unseal key securely in Pulumi.
```bash
pulumi --cwd $PULUMI_CWD config set --secret openbao:unsealKey1Base64 "$(cat openbao-unseal-1.key | base64)"
```
Remove the local unseal key file.
```bash
rm openbao-unseal-1.key
```

Add OpenBao helm repo:
```bash
helm repo add openbao https://openbao.github.io/openbao-helm
```

Install OpenBao in the namespace:
```bash
helm upgrade --install openbao openbao/openbao \
--version 0.19.1 \
--namespace openbao \
--values k8s/openbao/override-values.yaml
```

Initialize OpenBao:
```bash
kubectl exec -ti openbao-0 -n openbao -- bao operator init
```

Store recovery keys. Replace the example keys between quotes below with the actual keys output from the above command.
```bash
pulumi --cwd $PULUMI_CWD config set --secret openbao:recoveryKeys "Recovery Key 1: <your-recovery-key-1>
Recovery Key 2: <your-recovery-key-2>
Recovery Key 3: <your-recovery-key-3>
Recovery Key 4: <your-recovery-key-4>
Recovery Key 5: <your-recovery-key-5>"
```

Store initial root token. Replace the example token between quotes below with the actual initial root token output from the init command.
```bash
pulumi --cwd $PULUMI_CWD config set --secret openbao:initialRootToken "<your-initial-root-token>"
```

### Verify
```bash
kubectl exec -ti openbao-0 -n openbao -- bao login # Then use the root token
kubectl exec -ti openbao-0 -n openbao -- bao operator raft list-peers
kubectl exec -ti openbao-0 -n openbao -- bao operator raft autopilot state
```

### Enable KV secrets engine
```bash
kubectl exec -ti openbao-0 -n openbao -- bao secrets enable -version=2 kv
```

### OpenBao UI

Run these commands to access OpenBao UI initial root token and port-forwarding:

```bash
echo "OpenBao UI URL: http://localhost:8200"
echo "Initial Root Token: $(pulumi --cwd $PULUMI_CWD config get openbao:initialRootToken)"
kubectl port-forward -n openbao svc/openbao 8200:8200
```

:::note
WSL2 users may need to create .wslconfig file in their Windows user profile folder (`C:\Users\<YourUserName>\.wslconfig`) with the following content to enable localhost port forwarding:

```ini
[wsl2]
localhostForwarding=true
```
:::