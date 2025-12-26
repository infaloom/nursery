# Roadmap

- ~~Migration to https://docusaurus.io/~~
- ~~Refactor secrets to use OpenBao and external secrets~~
- ~~Fix services versions (Helm charts, etc.)~~
- ~~Continuous Integration / Continuous Deployment (CI/CD)~~
- ~~Keycloak for user management and authentication~~
- Migrate internal services authentication to Keycloak
- Migrate harbor registry db to dedicated PostgreSQL role (as with forgejo and keycloak)
- Look for alternatives to MinIO for remote backups (going closed source)
- Alert rules and notifications (Prometheus, Grafana, Alertmanager)
- Add tutorials for configuring keycloak Nursery realm
- Network policies
- Full cluster disaster recovery (restoring to a new Hetzner project)
- Bash/Python Jupyter notebook for deploying the cluster and workloads