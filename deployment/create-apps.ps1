$run = 4
$uniqueIdentifier = [guid]::NewGuid()
$applicationName = "Speaker application $($run)"
$applicationIdentifierUri = "api://$($uniqueIdentifier)"
$replyUrl = "https://speaker-application/auth"

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

$waitingDelayInSeconds = 180
Write-Warning "Letting AAD catch up and wait a $($waitingDelayInSeconds) seconds to actually grant the application permission."
Start-Sleep -Seconds $waitingDelayInSeconds
Write-Warning "Continuing with the script."
$enterpriseApplicationDetails = az ad app permission grant `
                                        --id $createdApplication.appId `
                                        --api $graphResourceId
                                        | ConvertFrom-Json
# {
#     "clientId": "aae58ae2-9588-454f-9124-434863cd9b55",
#     "consentType": "AllPrincipals",
#     "expiryTime": "2021-07-21T19:52:40.577023",
#     "objectId": "4orlqoiVT0WRJENIY82bVTfM2zzlRBBBvSC2MpWQ0Z0",
#     "odata.metadata": "https://graph.windows.net/b1f8cb55-7d7a-4e8d-9641-51372b423350/$metadata#oauth2PermissionGrants/@Element",
#     "odatatype": null,
#     "principalId": null,
#     "resourceId": "3cdbcc37-44e5-4110-bd20-b6329590d19d",
#     "scope": "user_impersonation",
#     "startTime": "2020-07-21T19:52:40.577023"
# }

# Delete all test applications
# az ad app list --filter "startswith(displayname, 'Speaker application')" | ConvertFrom-Json| ForEach-Object {  az ad app delete --id $_.appId --verbose }

# 		* Optional:
# 			Add dummy scope
# 			Add authorized client application
# 				Client Id Visual Studio: 872cd9fa-d31f-45e0-9eab-6e460a02d1f1


# * Assign managed identity the roles for the services
# 	Object Id of Enterprise Application
# 	Managed identity object id
# 	Role id

# ```powershell
# az rest `
# --method post `
# --uri https://graph.microsoft.com/beta/servicePrincipals/[ObjectIdVanDeEnterpriseApplication]/appRoleAssignments `
# --headers "{'content-type': 'application/json'}" `
# --body "{'appRoleId': '[role id]', 'principalId': '[managed identity object id]', 'principalType': 'ServicePrincipal', 'resourceId': '[ObjectIdVanDeEnterpriseApplication]'}"
# ```

# * Enterprise Application
# 	* User Assignment Required
