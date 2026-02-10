# WSL Setup

https://learn.microsoft.com/en-us/windows/wsl/install

:::note
When updating WSL version with `wsl --update`, you may need to reinstall Docker Desktop and restart your computer to complete the update.
:::

## Install additional Linux distributions

It may be useful to install additional Linux distributions if you want to manage multiple environments to avoid conflicts between projects.

To create a new WSL instance with Ubuntu 24.04 named Ubuntu2, run the following command in PowerShell:
```powershell
wsl --install Ubuntu-24.04 --name Ubuntu2
```

## Google Drive and WSL

Since a recent WSL2 update Google Drive is not working by default. You can find a workaround in this SuperUser answer: https://superuser.com/questions/1781174/google-drive-in-wsl

In short, create the dir (first time only) and mount the drive with the following commands in WSL:
```bash
sudo mkdir -p /mnt/g
sudo mount -t drvfs G: /mnt/g
```

Then you can access your Google Drive files in WSL at `/mnt/g/`.

