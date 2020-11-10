# My Secure APIs demo project

This project contains some of the best-practices, known to me, for securing your web applications in Azure.  
It's a work-in-progress, so nowhere near perfect.

# Goals

The things I want to do in this project.

* Have an access restriction on the backend services, only to allow traffic from within the VNet
* VNet integrate the frontend service (API) to communicate with the backend services (Speaker API & Conference API)
* Have authentication & authorization in place on backend services
* Only the frontend service (API) can communicate with the backend services
  Handled via app roles assigned to managed identity
* Use Key Vault references for accessing secrets
* Get & assign keys & connection strings via ARM Template functions during deployment
* Use and assign RBAC roles for managed identities to allow access to other resources
* Create and assign Azure Policies for compliancy of the resource group
* ...

# Want to help?

If you think something is missing from my list or think of something else, feel free to add new issues.  
I'm using this project as a learning exercise and would love to learn more from you.

# Badges
![CodeQL](https://github.com/Jandev/secure-apis/workflows/CodeQL/badge.svg)