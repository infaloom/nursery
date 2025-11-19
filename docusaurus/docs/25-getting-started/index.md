# Getting Started

## Clone the Repository

To get started with Infastructure, first clone the repository to your local machine using the following commands:
```bash
git clone https://github.com/infaloom/infastructure.git
cd infastructure
```

:::important
All commands are documented to be executed from the **root of the repository**.
:::

## Create Your Own Remote Repository
Push the cloned repository to your preferred Git hosting service. You don't need to do this immediately, but you will eventually want to have your own remote copy. Use the following commands to rename the original remote to `upstream` (so you can pull updates later) and add your own repository as `origin`:
```bash
git remote rename origin upstream
git remote set-url --push upstream DISABLED # Disable pushing to upstream
git remote add origin <your-git-url>
git push -u origin main
```