$run = 10
$uniqueIdentifier = [guid]::NewGuid()
$applicationName = "Speaker application $($run)"
$applicationIdentifierUri = "api://$($uniqueIdentifier)"
$replyUrl = "https://speaker-application/auth"

$speakerReaderRoleId = "42ee5891-7e50-4db9-a6d9-75ffc8cc1e9b"
$nameOfTheManagedIdentityNeedingPermission = "janv-secureapi-api"

Write-Output "Creating $($applicationName)."
# Output matches a manifest file
$createdApplication = az ad app create `
                        --display-name $applicationName `
                        --identifier-uris $applicationIdentifierUri `
                        --reply-urls $replyUrl `
                        --available-to-other-tenants false `
                        --oauth2-allow-implicit-flow false `
                        --app-roles @speaker-manifest.json
                        | ConvertFrom-Json
Write-Output "Created application $($createdApplication.appId)."

Write-Output "Adding User.Read permissions to Microsoft Graph for application $($createdApplication.appId)."
$graphResourceId = "00000003-0000-0000-c000-000000000000"
$userReadPermission = "e1fe6dd8-ba31-4d61-89e7-88639da4683d=Scope"
az ad app permission add `
                        --id $createdApplication.appId `
                        --api $graphResourceId `
                        --api-permissions $userReadPermission
Write-Output "Added User.Read permissions to Microsoft Graph for application $($createdApplication.appId)"
$waitingDelayInSeconds = 30

Write-Output "Waiting $($waitingDelayInSeconds) seconds."
Start-Sleep -Seconds $waitingDelayInSeconds
# According to the answer over here: https://stackoverflow.com/a/62890221/352640
# We need to do an admin consent first, this will create a service principal and granting permissions is now possible.
Write-Output "Running Admin Consent to application $($createdApplication.appId)."
az ad app permission admin-consent --id $createdApplication.appId

# While creating this script I noticed it takes a bit of time for the service principal to be created.
# Running the next commands immediatly will cause them to fail, or at least when I tested them.
Write-Output "Waiting $($waitingDelayInSeconds) seconds."
Start-Sleep -Seconds $waitingDelayInSeconds
# Granting the permission, though this should not be necessary anymore as it's already granted via `admin-consent` above.
Write-Output "Adding permission grant to application $($createdApplication.appId)."
$enterpriseApplicationDetails = az ad app permission grant `
                                        --id $createdApplication.appId `
                                        --api $graphResourceId
                                        | ConvertFrom-Json

                                    
Write-Output "Waiting $($waitingDelayInSeconds) seconds."
Start-Sleep -Seconds $waitingDelayInSeconds

Write-Output "Running Admin Consent to application $($createdApplication.appId) to get admin consent on set permissions."
az ad app permission admin-consent --id $createdApplication.appId

Write-Output "Retrieving details from service principal  $($enterpriseApplicationDetails.clientId)."
$enterpriseApplication = az ad sp show --id $enterpriseApplicationDetails.clientId | ConvertFrom-Json # `clientId` matches the `Object Id` in the portal
Write-Output "Retrieving details for the managed identity  $($nameOfTheManagedIdentityNeedingPermission)."
$managedIdentityNeedingPermission = az ad sp list --display-name "janv-secureapi-api" | ConvertFrom-Json | Select-Object -First 1

# Assign managed identity the roles for the services
Write-Output "Assigning role $($speakerReaderRoleId) to managed identity $($managedIdentityNeedingPermission.objectId) for service principal $($enterpriseApplication.Id)."
az rest `
    --method post `
    --uri https://graph.microsoft.com/beta/servicePrincipals/$($enterpriseApplication.objectId)/appRoleAssignments `
    --headers "{'content-type': 'application/json'}" `
    --body "{'appRoleId': '$($speakerReaderRoleId)', 'principalId': '$($managedIdentityNeedingPermission.objectId)', 'principalType': 'ServicePrincipal', 'resourceId': '$($enterpriseApplication.objectId)'}"

###################
# Delete all test applications
###################
# az ad app list --filter "startswith(displayname, 'Speaker application')" | ConvertFrom-Json| ForEach-Object {  az ad app delete --id $_.appId --verbose }
###################

###################
# Nice to haves
###################

# Set `User Assignment Required` for the $enterpriseApplication.
# According to this answer on Stack Overflow it's not possible to set `User Assignment Required` via the Azure CLI at this time: https://stackoverflow.com/a/49547344/352640
# You can use the `Set-AzureADServicePrincipal` (https://docs.microsoft.com/en-us/powershell/module/azuread/set-azureadserviceprincipal?view=azureadps-2.0&WT.mc_id=AZ-MVP-5003246) 
# to set the -AppRoleAssignmentRequired $true, but I don't want to use the old Azure PowerShell commands over here.

# Add authorized client application
# For Visual Studio or the Azure CLI to connect, you need to have their identifiers added to the 
# authorized client application list.
# - Visual Studio: 872cd9fa-d31f-45e0-9eab-6e460a02d1f1
# - Azure CLI: 04b07795-8ddb-461a-bbee-02f9e1bf7b46
# Details over here: https://github.com/Azure/azure-sdk-for-net/issues/6172