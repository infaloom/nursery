# Ceph Dashboard SSO

This step is optional. It shows how to enable SAML2 SSO in Ceph Dashboard using Keycloak as Identity Provider.

## Setup Ceph Dashboard SSO with Keycloak

Download the IdP metadata from Keycloak. We are using the `nursery` realm created in the Keycloak setup guide.
```bash
curl -o descriptor.xml -L "https://account.${CLUSTER_DOMAIN}/realms/nursery/protocol/saml/descriptor"
```

Set WantAuthnRequestsSigned to false. I couldn't get it working with signing enabled. But given that the communication is done over HTTPS, it should be secure enough.
```bash
sed -i 's/WantAuthnRequestsSigned="true"/WantAuthnRequestsSigned="false"/' descriptor.xml
```

Copy descriptor.xml to Ceph Manager pod
:::note
It could be that the active Ceph Manager pod is not the one with ceph_daemon_id=a. In that case, find the active pod with the following command and copy the file to that pod instead.
```bash
kubectl -n rook-ceph exec -it deploy/rook-ceph-tools -- ceph mgr dump | grep active_name
```
:::
```bash
kubectl cp descriptor.xml $(kubectl get pods -n rook-ceph -l app=rook-ceph-mgr,ceph_daemon_id=a -o jsonpath='{.items[0].metadata.name}'):/tmp/descriptor.xml -n rook-ceph -c mgr
```

Enable SAML2 SSO in Ceph Dashboard
```bash
kubectl -n rook-ceph exec -it deploy/rook-ceph-tools -- ceph dashboard sso setup saml2 https://ceph-dashboard.${CLUSTER_DOMAIN} file:///tmp/descriptor.xml username
```

Disable redirection to avoid infinite redirect loop when SSO is not working
```bash
kubectl -n rook-ceph exec -it deploy/rook-ceph-tools -- ceph config set mgr mgr/dashboard/standby_behaviour "error"
```

## Create admin user for SAML2 SSO
The password will be randomly generated and stored in a temporary file inside the pod. After creating the user, the temporary file will be deleted.
The password isn't necessary because user authentication will be done via Keycloak, but Ceph Dashboard requires password when creating a user.

:::info
You need to change `{USERNAME}` to the username of the Keycloak user you plan to use for accessing Ceph Dashboard.
:::

```bash
kubectl -n rook-ceph exec -it deploy/rook-ceph-tools -- bash -c "echo $(openssl rand -base64 32) > /tmp/pass"
kubectl -n rook-ceph exec -it deploy/rook-ceph-tools -- ceph dashboard ac-user-create {USERNAME} administrator -i /tmp/pass
kubectl -n rook-ceph exec -it deploy/rook-ceph-tools -- bash -c "rm /tmp/pass"
```

## Configure Keycloak SAML2 client

Download the Service Provider metadata from Ceph Dashboard
```bash
curl -o sp-metadata.xml -L "https://ceph-dashboard.${CLUSTER_DOMAIN}/auth/saml2/metadata"
```

- Import the `sp-metadata.xml` file to Keycloak as new SAML2 client in the nursery realm.
- `username` attribute mapping should be set to `username`. The attribute is created automatically after importing the metadata but the mapping is not set by default.