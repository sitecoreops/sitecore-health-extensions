# Sitecore Health Extensions

Health endpoints to use with for example Docker, Docker Swarm or Kubernetes.

The following endpoints are available after installation:

`/healthz`: returns 200 OK to indicate that the application is running otherwise 50x if Sitecore is not able to initialize.

`/readiness`: returns 200 OK to indicate that the application is ready to accept requests otherwise 503 if connections to SQL, Solr etc. are not responsive.

## Installation

1. Install NuGet: `PM> Install-Package SitecoreHealthExtensions`
2. Add configuration, copy [SitecoreHealthExtensions.config](/src/SitecoreHealthExtensions.Website/App_Config/Include/SitecoreHealthExtensions.config) as reference.