import sys
import json
import yaml # Requires PyYAML: pip install pyyaml
from collections import defaultdict
import re

def convert_to_ansible_inventory(pulumi_output):
    """
    Converts Pulumi output JSON (dict) to an Ansible inventory YAML structure (dict).
    """
    inventory = {
        'all': {
            'vars': {
                'ansible_user': 'root',
                'ansible_ssh_common_args': '-o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null',
                'ansible_python_interpreter': 'auto_silent'
            }
            # Using top-level groups instead of 'children' for this specific output format
        }
    }
    groups = defaultdict(lambda: {'hosts': {}})

    for key, ip_address in pulumi_output.items():
        if '.Ipv4Address' not in key:
            print(f"Warning: Skipping unexpected key format: {key}", file=sys.stderr)
            continue

        hostname_base = key.split('.')[0]
        host_entry = {'ansible_host': ip_address}

        # Determine group based on hostname prefix
        if re.match(r'^agent\d+$', hostname_base):
            groups['agents']['hosts'][hostname_base] = host_entry
        elif re.match(r'^server\d+$', hostname_base):
            groups['servers']['hosts'][hostname_base] = host_entry
        elif hostname_base.startswith('serverLb'):
            groups['serverLbs']['hosts'][hostname_base] = host_entry
        # Add rules for other potential groups if needed, e.g.:
        # elif hostname_base.startswith('clusterLoadBalancer'):
        #     groups['load_balancers']['hosts'][hostname_base] = host_entry
        else:
            # If no specific group matches, add to a generic 'ungrouped' or handle as needed
            # For this specific request, we only care about 'agents' and 'servers'
            print(f"Info: Host '{hostname_base}' doesn't match defined groups ('agents', 'servers'). Skipping group assignment.", file=sys.stderr)


    # Merge the dynamically created groups into the main inventory structure
    inventory.update(groups)

    # Remove empty host groups if any were created but not populated
    # (less likely with defaultdict but good practice)
    keys_to_delete = [k for k, v in inventory.items() if isinstance(v, dict) and 'hosts' in v and not v['hosts']]
    for k in keys_to_delete:
        del inventory[k]


    return inventory

if __name__ == "__main__":
    try:
        # Read JSON data from stdin
        input_data = json.load(sys.stdin)
    except json.JSONDecodeError as e:
        print(f"Error: Invalid JSON received on stdin: {e}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"Error reading from stdin: {e}", file=sys.stderr)
        sys.exit(1)

    # Convert the data
    ansible_inventory_dict = convert_to_ansible_inventory(input_data)

    # Print the YAML output to stdout
    # default_flow_style=False ensures block style (not inline)
    # sort_keys=False attempts to preserve order (though YAML dicts are technically unordered)
    # width=float('inf') prevents line wrapping for ssh args if they were longer
    yaml.dump(
        ansible_inventory_dict,
        sys.stdout,
        default_flow_style=False,
        sort_keys=False,
        width=float('inf')
     )