# Raijin

## Structure

## Development

### Aspire

## Deployment - Azure

Step by step guide to deploy the application to Azure.

### Prerequisites
- Active Azure Subscription
- Azure CLI installed
- Azure CLI logged in
- `kubectl` installed
- Node.js and npm installed
- Static Web Apps CLI installed globally, or install it during the steps below

### Naming convention
`[env]-[location]-[resource_type]-[name](-[suffix])?`
- env: The environment for which the resource is being created (e.g., prod, dev, staging).
- location: The Azure region where the resource will be deployed (e.g., australiaeast, eastus).
- resource_type: A short abbreviation of the type of resource being created (e.g., rg for resource group, swa for static web app, aks for Azure Kubernetes Service, agw for Application Gateway WAF).
- name: A unique identifier for the resource, which can be descriptive of its purpose or function (e.g., raijin for the application name).
- suffix: An optional component that can be added to further differentiate resources.

The only exception to this is the azure container registry, which will be named as follows:
`[name]acr[location]`
- name: A unique identifier for the resource, which can be descriptive of its purpose or function (e.g., raijin for the application name).
- location: The Azure region where the resource will be deployed (e.g., australiaeast, eastus).


## CD Pipeline (GitHub Actions + Azure)
