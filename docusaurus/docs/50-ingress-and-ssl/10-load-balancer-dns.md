# Load balancer DNS
You'll need to point your cluster domain to the load balancer address.

Because there are many dashboards, consider using a wildcard domain `*.cluster.domain.tld` and point it to the load balancer address.
That way I can have many apps as subdomains without any DNS changes.

Run the following for instructions to point your DNS to the load balancer IP
```bash
echo "Point your *.${CLUSTER_DOMAIN} DNS record to $(pulumi --cwd $PULUMI_CWD stack output clusterLoadBalancer.Ipv4Address) IP address"
```