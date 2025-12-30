# Security Considerations

We try to follow best practices when it comes to security. However, there are always trade-offs between security, usability, and cost. Here are some of the important security considerations taken into account when building the stack.

## Vault

We use **Pulumi secrets** with local backend to store sensitive information that is needed during the deployment process. The secrets are encrypted using a passphrase that you provide when running Pulumi commands and are securely stored in the Git repository.

However, for certain use cases where higher levels of security are required, it is recommended to use a dedicated secrets management solution.

- **HashiCorp Vault**: A tool for securely accessing secrets. https://developer.hashicorp.com/vault
- **AWS Secrets Manager**: A service for managing secrets in AWS. https://aws.amazon.com/secrets-manager/
- **Azure Key Vault**: A service for managing secrets in Azure. https://azure.microsoft.com/en-us/services/key-vault/
- **Google Secret Manager**: A service for managing secrets in Google Cloud. https://cloud.google.com/security/products/secret-manager

## OpenBao Secret Management

Once the cluster is up and running, OpenBao is used to manage application secrets. OpenBao encrypts the secrets at rest and in transit. It also provides fine-grained access control to the secrets.

Basically, OpenBao is an alternative to HashiCorp Vault. It takes over the management of application secrets once the cluster is running.

## Git repository visibility

Given that the sensitive information is stored in Pulumi secrets and are commited to the Git repository, it is important to ensure that the repository is private. The secrets are encrypted and cannot be accessed without the Pulumi passphrase but I recommend to keep the repository private to avoid any potential security risks. So...

:::danger
Don't make the Git repository with Pulumi secrets public!
:::

## Administrative access

I use static IP for accessing the cluster nodes via SSH and for accessing the kube api and all the administrative services running in the cluster.

This way, the IP can be whitelisted in the firewall rules of the nodes and applications. Only traffic from the static IP is allowed.

This guide assumes that you have a static IP or a VPN providing a static IP. If your IP changes, you may need to update the firewall rules and IP whitelisting accordingly.

## Team size

As mentioned, this stack is designed for small teams with internal DevOps expertise. If you have a larger team, you may want to consider more advanced security measures, such as:

- Role-based access control (RBAC)
- Audit logging
- etc.

I plan to further enhance security features as the project matures.