---
slug: initial-release
title: Initial Release
authors: [jzaric]
---

Today we are open-sourcing **Nursery**, the opinionated blueprint we use to run Infaloom's production workloads on a lean budget.

<!-- truncate -->

The project documents everything we've learned while standing up a six-node [K3s](https://k3s.io) cluster on [Hetzner Cloud](https://www.hetzner.com/) with Pulumi for provisioning, Ansible for day-two automation, and a security-first stance powered by OpenBao + External Secrets. The full stack comes in at **EUR 187.34/month** (November 2025) and is designed for small teams that need production reliability.

## What you get in v1

- **Core platform** – Automated Hetzner bootstrap, HA control plane access via HAProxy, and Rook-Ceph backed storage across three 200 GB nodes for ~200 GB of triple replicated capacity.
- **Data services** – CloudNativePG with scheduled S3-compatible off-site backups and Redis Sentinel (Bitnami legacy images) for stateful workloads.
- **App enablement** – Harbor registry, ArgoCD GitOps, and ready-to-use Helm values for common building blocks.
- **Security + secrets** – OpenBao as the source of truth with ExternalSecrets syncing into the cluster, plus documented ingress, TLS, and IP allowlist patterns.
- **Ops readiness** – Step-by-step guides for monitoring, logging, disaster recovery, and everyday utilities, all pinned to tested tool versions for reproducibility.

## Why it matters

Managed Kubernetes is fantastic, but it quickly eats into the budget of bootstrapped startups. Nursery shows that you can own the stack end-to-end, keep costs predictable, and still follow best practices for backups, observability, and multi-environment automation.

## Dive in

- Start with the [introduction](/) for motivation, audience, and cluster specs.
- Follow the [development environment](/development-environment/) and [getting started](/getting-started/) guides to mirror our toolchain.
- Explore the service deep dives (CNPG, Redis, Harbor, ArgoCD, storage, secrets, DR, etc.) in documentation and keep an eye on the [roadmap](/roadmap) for what is coming next.

Questions, ideas, or battle stories from your own clusters are very welcome — open an issue or start a discussion so we can keep improving the stack together.