# Roadmap

- ~~Migration to https://docusaurus.io/~~
- ~~Refactor secrets to use OpenBao and external secrets~~
- ~~Fix services versions (Helm charts, etc.)~~
- ~~Continuous Integration / Continuous Deployment (CI/CD)~~
- ~~Keycloak for user management and authentication~~
- ~~Migrate internal services authentication to Keycloak~~
- ~~Migrate harbor registry db to dedicated PostgreSQL role (as with forgejo and keycloak)~~
- ~~Refactor docs to ensure services cnpg roles and dbs are created with the cnpg cluster and not each individually~~
- ~~Document Forgejo SSO with Keycloak~~
- Alert rules and notifications (Prometheus, Grafana, Alertmanager)
- DB services access behind a firewall (without kubectl port-forward)
- Look for alternatives to MinIO for remote backups (going closed source)
- Add tutorials for configuring keycloak Nursery realm
- Network policies
- Full cluster disaster recovery (restoring to a new Hetzner project)
- Bash/Python Jupyter notebook for deploying the cluster and workloads