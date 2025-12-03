# Development environment

:::note
We are fixing the version of the tools used in this guide to ensure reproducibility.
:::

## Linux
**Ubuntu 24.04.1 LTS** (We used wsl on Windows)

### If running on Windows

Ensure you use `wsl` to run the commands because of the `bash` scripts.
Follow the instructions on https://learn.microsoft.com/en-us/windows/wsl/install

Install Ubuntu 24.04 distro.
```powershell
wsl --install Ubuntu-24.04 --name nursery
```

For later use you can run it with:
```powershell
wsl -d nursery
```

## Update list of packages
```bash
sudo apt update
```

## Git
https://git-scm.com/
```bash
sudo apt install git=1:2.43.0-1ubuntu7.3 -y
```

## .NET
https://dotnet.microsoft.com/
```bash
sudo apt install dotnet-sdk-8.0 -y
```

## Pulumi
https://www.pulumi.com/
```bash
curl -fsSL https://get.pulumi.com | sh -s -- --version 3.205.0
```

## Python
https://www.python.org/
```bash
sudo apt install python3=3.12.3-0ubuntu2.1 python3.12-venv=3.12.3-1ubuntu0.8 -y
```

### Ansible

#### Virtual environment
```bash
python3 -m venv ~/ansible-venv
```
Activate the virtual environment.
:::info
This is needed every time you open a new terminal session before using Ansible.
:::
```bash
source ~/ansible-venv/bin/activate
```

#### Install Ansible
https://docs.ansible.com/

The reason for using Ansible is to automate the setup of the K3s cluster as some commands are executed the same on multiple servers.
Where Ansible comes in handy.

Install Ansible with pip:
```bash
pip install ansible==12.2.0
```

## Kubectl

https://kubernetes.io/docs/tasks/tools/install-kubectl/
```bash
sudo snap install kubectl --channel=1.32/stable --classic
```

## Helm

https://helm.sh/docs/intro/install
```bash
sudo apt-get install curl gpg apt-transport-https -y
curl -fsSL https://packages.buildkite.com/helm-linux/helm-debian/gpgkey | gpg --dearmor | sudo tee /usr/share/keyrings/helm.gpg > /dev/null
echo "deb [signed-by=/usr/share/keyrings/helm.gpg] https://packages.buildkite.com/helm-linux/helm-debian/any/ any main" | sudo tee /etc/apt/sources.list.d/helm-stable-debian.list
sudo apt-get update
sudo apt-get install helm=3.19.2-1 -y
```