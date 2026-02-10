using Grpc.Net.Client.Balancer;
using Pulumi;
using Pulumi.HCloud;
using Pulumi.Std;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HCloud = Pulumi.HCloud;

using Std = Pulumi.Std;

return await Deployment.RunAsync(() =>
{
    var sshConfig = new Pulumi.Config("ssh");

    var defaultSshPublicKeyPath = sshConfig.Require("defaultSshPublicKeyPath");
    var sourceIp = sshConfig.Require("sourceIp");

    var defaultSshKey = new HCloud.SshKey("defaultSshKey", new()
    {
        Name = "defaultSshKey",
        PublicKey = Std.File.Invoke(new()
        {
            Input = defaultSshPublicKeyPath,
        }).Apply(invoke => invoke.Result),
    });

    var clusterNetwork = new HCloud.Network("clusterNetwork", new()
    {
        Name = "clusterNetwork",
        IpRange = "10.0.0.0/8",
    });

    var clusterSubnet = new HCloud.NetworkSubnet("clusterSubnet", new()
    {
        NetworkId = clusterNetwork.Id.Apply(int.Parse),
        Type = "cloud",
        NetworkZone = "eu-central",
        IpRange = "10.0.1.0/24",
    }, new CustomResourceOptions
    {
        DependsOn =
        {
            clusterNetwork,
        }
    });

    var serverPlacementGroup = new HCloud.PlacementGroup("serverPlacementGroup", new()
    {
        Name = "serverPlacementGroup",
        Type = "spread",
    });

    var agentPlacementGroup = new HCloud.PlacementGroup("agentPlacementGroup", new()
    {
        Name = "agentPlacementGroup",
        Type = "spread",
    });

    var createServer = (string serverName, string serverType, string datacenter, string internalIp, PlacementGroup placementGroup, InputList<Resource> dependsOn) =>
        {
            var serverIp = new HCloud.PrimaryIp($"{serverName}Ip", new()
            {
                Name = $"{serverName}Ip",
                Datacenter = datacenter,
                Type = "ipv4",
                AssigneeType = "server",
                AutoDelete = false,
            });

            var server = new HCloud.Server(serverName, new()
            {
                Name = serverName,
                ServerType = serverType,
                Image = "ubuntu-24.04",
                SshKeys = [defaultSshKey.Id],
                Datacenter = datacenter,
                PublicNets = new[]
                {
                    new HCloud.Inputs.ServerPublicNetArgs
                    {
                        Ipv4 = serverIp.Id.Apply(int.Parse),
                        Ipv6Enabled = false,
                    },
                },
                Networks = new[]
                {
                    new HCloud.Inputs.ServerNetworkArgs
                    {
                        NetworkId = clusterNetwork.Id.Apply(int.Parse),
                        Ip = internalIp
                    },
                },
                PlacementGroupId = placementGroup.Id.Apply(int.Parse)
            }, new CustomResourceOptions
            {
                DependsOn = dependsOn,
            });

            return server;
        };

    var server1 = createServer("server1", "cpx41", "nbg1-dc3", "10.0.1.11", serverPlacementGroup, [defaultSshKey, clusterNetwork, serverPlacementGroup]);
    var server2 = createServer("server2", "cpx41", "fsn1-dc14", "10.0.1.12", serverPlacementGroup, [defaultSshKey, clusterNetwork, serverPlacementGroup]);
    var server3 = createServer("server3", "cpx41", "nbg1-dc3", "10.0.1.13", serverPlacementGroup, [defaultSshKey, clusterNetwork, serverPlacementGroup]);

    var serverLb = createServer("serverLb", "cpx11", "nbg1-dc3", "10.0.1.2", serverPlacementGroup, [defaultSshKey, clusterNetwork, serverPlacementGroup]);

    var agent1 = createServer("agent1", "cpx41", "nbg1-dc3", "10.0.1.101", agentPlacementGroup, [defaultSshKey, clusterNetwork, agentPlacementGroup]);
    var agent2 = createServer("agent2", "cpx41", "fsn1-dc14", "10.0.1.102", agentPlacementGroup, [defaultSshKey, clusterNetwork, agentPlacementGroup]);
    var agent3 = createServer("agent3", "cpx41", "nbg1-dc3", "10.0.1.103", agentPlacementGroup, [defaultSshKey, clusterNetwork, agentPlacementGroup]);

    var clusterServers = new[] { server1, server2, server3, agent1, agent2, agent3 };
    var k8sMasterServers = new[] { server1, server2, server3, serverLb };
    var k8sAgentServer = new[] { agent1, agent2, agent3 };
    var allServers = new[] { server1, server2, server3, serverLb, agent1, agent2, agent3 };

    // Create a Load Balancer for the cluster
    // This will be used to route traffic to the servers and agents

    var clusterLoadBalancer = new HCloud.LoadBalancer("clusterLoadBalancer", new()
    {
        Name = "clusterLoadBalancer",
        LoadBalancerType = "lb11",
        Location = "nbg1",
    });

    var loadBalancerServiceHttp = new HCloud.LoadBalancerService("loadBalancerServiceHttp", new()
    {
        LoadBalancerId = clusterLoadBalancer.Id,
        Protocol = "tcp",
        ListenPort = 80,
        DestinationPort = 80,
        Proxyprotocol = true,
    });

    var loadBalancerServiceHttps = new HCloud.LoadBalancerService("loadBalancerServiceHttps", new()
    {
        LoadBalancerId = clusterLoadBalancer.Id,
        Protocol = "tcp",
        ListenPort = 443,
        DestinationPort = 443,
        Proxyprotocol = true
    });

    var loadBalancerServiceForgejoSsh = new HCloud.LoadBalancerService("loadBalancerServiceForgejoSsh", new()
    {
        LoadBalancerId = clusterLoadBalancer.Id,
        Protocol = "tcp",
        ListenPort = 22,
        DestinationPort = 32222,
        Proxyprotocol = true
    });

    var loadBalancerServicePostgresql = new HCloud.LoadBalancerService("loadBalancerServicePostgresql", new()
    {
        LoadBalancerId = clusterLoadBalancer.Id,
        Protocol = "tcp",
        ListenPort = 5432,
        DestinationPort = 5432,
        Proxyprotocol = true
    });

    var loadBalancerNetwork = new HCloud.LoadBalancerNetwork("loadBalancerNetwork", new()
    {
        LoadBalancerId = clusterLoadBalancer.Id.Apply(int.Parse),
        NetworkId = clusterNetwork.Id.Apply(int.Parse),
        Ip = "10.0.1.1",
    }, new CustomResourceOptions
    {
        DependsOn = {
            clusterLoadBalancer,
            clusterSubnet,
        }
    });

    foreach (var server in clusterServers)
    {
        Output.Format($"{clusterLoadBalancer.Name}_{server.Name}_target").Apply(target =>
        {
            var loadBalancerTarget = new HCloud.LoadBalancerTarget(target, new()
            {
                Type = "server",
                LoadBalancerId = clusterLoadBalancer.Id.Apply(int.Parse),
                ServerId = server.Id.Apply(int.Parse),
                UsePrivateIp = true,
            });

            return loadBalancerTarget;
        });
    }

    var sshFirewallRules = new List<HCloud.Inputs.FirewallRuleArgs>
    {
        new HCloud.Inputs.FirewallRuleArgs
        {
            Direction = "in",
            Protocol = "tcp",
            Port = "22",
            SourceIps = new[]
            {
                $"{sourceIp}/32" // HQ IP
            },
        }
    };

    var sshServerFirewall = new HCloud.Firewall("sshServerFirewall", new()
    {
        Name = "sshServerFirewall",
        Rules = sshFirewallRules,
        ApplyTos = allServers.Select(s => new HCloud.Inputs.FirewallApplyToArgs
        {
            Server = s.Id.Apply(int.Parse),
        }).ToList(),
    });

    var k8sFirewallRules = new List<HCloud.Inputs.FirewallRuleArgs>
    {
        new HCloud.Inputs.FirewallRuleArgs
        {
            Direction = "in",
            Protocol = "tcp",
            Port = "6443",
            SourceIps = new[]
            {
                $"{sourceIp}/32" // HQ IP
            },
        }
    };

    var k8sServerFirewall = new HCloud.Firewall("k8sServerFirewall", new()
    {
        Name = "k8sServerFirewall",
        Rules = k8sFirewallRules,
        ApplyTos = k8sMasterServers.Select(s => new HCloud.Inputs.FirewallApplyToArgs
        {
            Server = s.Id.Apply(int.Parse),
        }).ToList(),
    });

    foreach (var agent in k8sAgentServer)
    {
        int numberOfVolumesPerAgent = 2; // <-- UPDATE THIS NUMBER TO ADD MORE VOLUMES
        for (var i = 1; i <= numberOfVolumesPerAgent; i++)
        {
            Output.Format($"{agent.Name}_volume_{i}").Apply(volumeName =>
            {
                var volume = new HCloud.Volume(volumeName, new()
                {
                    Name = volumeName,
                    Size = 100, // Size in GB
                    ServerId = agent.Id.Apply(int.Parse),
                });
                return Task.CompletedTask;
            });
        }
    }

    return new Dictionary<string, object?>
    {
        { "server1.Ipv4Address", server1.Ipv4Address },
        { "server2.Ipv4Address", server2.Ipv4Address },
        { "server3.Ipv4Address", server3.Ipv4Address },

        { "serverLb.Ipv4Address", serverLb.Ipv4Address },

        { "agent1.Ipv4Address", agent1.Ipv4Address },
        { "agent2.Ipv4Address", agent2.Ipv4Address },
        { "agent3.Ipv4Address", agent3.Ipv4Address },

        { "clusterLoadBalancer.Ipv4Address", clusterLoadBalancer.Ipv4 }
    };
});