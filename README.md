# Sitecore Health Extensions

Health endpoints to use with for example Docker, Docker Swarm or Kubernetes.

The following endpoints are available after installation:

`/healthz`: returns 200 OK to indicate that the application is running.

`/readiness`: returns 200 OK to indicate that the application is read if connections to SQL, Solr etc. is OK, otherwise 503 Service Unavailable.

## Installation

1. Install NuGet: `PM> Install-Package SitecoreHealthExtensions`
2. Add configuration, copy [SitecoreHealthExtensions.config](/src/SitecoreHealthExtensions.Website/App_Config/Include/SitecoreHealthExtensions.config) as reference.