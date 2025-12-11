#!/bin/bash
# Usage: source scripts/export_pulumi_env.sh

if [ -z "$PULUMI_CONFIG_PASSPHRASE" ]; then
    echo "PULUMI_CONFIG_PASSPHRASE is not set. Continuing...";

    export PULUMI_CWD="${PWD}/pulumi/hetzner";
    echo "PULUMI_CWD set to $PULUMI_CWD";

    echo "Enter Pulumi passphrase...";
    export PULUMI_CONFIG_PASSPHRASE=$(read -s; echo $REPLY)
    echo "PULUMI_CONFIG_PASSPHRASE exported";
    echo "Run 'unset PULUMI_CONFIG_PASSPHRASE' to clear it from the environment.";
else
  echo "PULUMI_CONFIG_PASSPHRASE is already set";
  echo "To reset it type: unset PULUMI_CONFIG_PASSPHRASE";
  echo "and then run this script again.";
fi
