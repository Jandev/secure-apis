function Add-AadApplicationWithServicePrincipal {
<#
.SYNOPSIS
    Creates an Azure Active Directory App Registration and corresponding
    Enterprise Application with the User.Read permission from the 
    Microsoft Graph

.DESCRIPTION
    Uses Azure CLI commands to create an Azure Active Direcotry App Registration
    and corresponding Enterprise Application.

.PARAMETER DisplayName
    The display name of the application.

.PARAMETER IdentifierUris
    Space-separated unique URIs that Azure AD can use for this app.

.PARAMETER ReplyUrls
    Space-separated URIs to which Azure AD will redirect in response to an OAuth 2.0 request. 
    The value does not need to be a physical endpoint, but must be a valid URI.

.PARAMETER AppRoles
    Declare the roles you want to associate with your application. Should be in manifest json format.

.EXAMPLE
    Add-AadApplicationWithServicePrincipal -DisplayName 'MyApplication' -IdentifierUris 'api://myapplication' -ReplyUrls 'https://myapplication/auth'

.OUTPUTS
    Created Enterprise Application client identifier

.NOTES
    Author:  Jan de Vries
    Website: http://jan-v.nl
    Twitter: @Jan_de_V
#>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$DisplayName,
        [Parameter(Mandatory)]
        [string]$IdentifierUris,
        [Parameter(Mandatory)]
        [string]$ReplyUrls,
        [Parameter(Mandatory)]
        [string]$AppRoles
    )

    Write-Verbose -Message "Creating $($DisplayName)."
    # Output matches a manifest file
    $createdApplication = az ad app create `
                            --display-name $DisplayName `
                            --identifier-uris $IdentifierUris `
                            --reply-urls $ReplyUrls `
                            --available-to-other-tenants false `
                            --oauth2-allow-implicit-flow false `
                            --app-roles $AppRoles
                            | ConvertFrom-Json
    Write-Verbose -Message "Created application $($createdApplication.appId)."

    # Got code from https://github.com/Azure/azure-cli/issues/11168#issuecomment-593385804
    Write-Verbose -Message "Setting access token version to 2.0 for application $($createdApplication.appId)."
    az rest `
    --method PATCH `
    --headers "Content-Type=application/json" `
    --uri https://graph.microsoft.com/v1.0/applications/$($createdApplication.objectId)/ `
    --body '{"api":{"requestedAccessTokenVersion": 2}}'

    Write-Verbose -Message "Adding User.Read permissions to Microsoft Graph for application $($createdApplication.appId)."
    $graphResourceId = "00000003-0000-0000-c000-000000000000"
    $userReadPermission = "e1fe6dd8-ba31-4d61-89e7-88639da4683d=Scope"
    az ad app permission add `
                            --id $createdApplication.appId `
                            --api $graphResourceId `
                            --api-permissions $userReadPermission
    Write-Verbose -Message "Added User.Read permissions to Microsoft Graph for application $($createdApplication.appId)"
    $waitingDelayInSeconds = 30

    Write-Verbose -Message "Waiting $($waitingDelayInSeconds) seconds."
    Start-Sleep -Seconds $waitingDelayInSeconds
    # According to the answer over here: https://stackoverflow.com/a/62890221/352640
    # We need to do an admin consent first, this will create a service principal and granting permissions is now possible.
    Write-Verbose -Message "Running Admin Consent to application $($createdApplication.appId)."
    az ad app permission admin-consent --id $createdApplication.appId

    # While creating this script I noticed it takes a bit of time for the service principal to be created.
    # Running the next commands immediatly will cause them to fail, or at least when I tested them.
    Write-Verbose -Message "Waiting $($waitingDelayInSeconds) seconds."
    Start-Sleep -Seconds $waitingDelayInSeconds
    # Granting the permission, though this should not be necessary anymore as it's already granted via `admin-consent` above.
    Write-Verbose -Message "Adding permission grant to application $($createdApplication.appId)."
    $enterpriseApplicationDetails = az ad app permission grant `
                                            --id $createdApplication.appId `
                                            --api $graphResourceId
                                            | ConvertFrom-Json

                                        
    Write-Verbose -Message "Waiting $($waitingDelayInSeconds) seconds."
    Start-Sleep -Seconds $waitingDelayInSeconds

    Write-Verbose -Message "Running Admin Consent to application $($createdApplication.appId) to get admin consent on set permissions."
    az ad app permission admin-consent --id $createdApplication.appId

    Write-Information "Created application with Application Id: '$($createdApplication.appId)' and Enterprise Application object id: '$($enterpriseApplicationDetails.objectId)'."

    return $enterpriseApplicationDetails.clientId
}

function Add-Role{
<#
.SYNOPSIS
    Adds app role to the Azure Active Directory identity for the specified Enteprise Application

.DESCRIPTION
    Uses Azure CLI commands to assign the specified App Role to an Azure Active Directory service principal,
    like a Managed Identity or User, for the specified Enterprise Application.

.PARAMETER EnterpriseApplicationClientId
    The client identifier of the Enteprise Application where the role has to be assigned for.

.PARAMETER AppRoleId
    The role identifier

.PARAMETER IdentityDisplayName
    The display name of the identity for which the role needs to be assigned to.

.PARAMETER IdentityType
    The type of identity. Should be either 'ServicePrincipal' or 'User'

.EXAMPLE
    Add-Role -EnterpriseApplicationClientId 'b2ff7cd3-5680-447f-8ef5-6b60b2faa26f' -AppRoleId 'd70bc7a8-664e-4647-a881-bfa4a4d21a67' -IdentityDisplayName 'my-enterprise-application' -IdentityType 'ServicePrincipal'

.EXAMPLE
    Add-Role -EnterpriseApplicationClientId 'b2ff7cd3-5680-447f-8ef5-6b60b2faa26f' -AppRoleId 'd70bc7a8-664e-4647-a881-bfa4a4d21a67' -IdentityDisplayName 'my-username' -IdentityType 'User'

.NOTES
    Author:  Jan de Vries
    Website: http://jan-v.nl
    Twitter: @Jan_de_V
#>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$EnterpriseApplicationClientId,
        [Parameter(Mandatory)]
        [string]$AppRoleId,
        [Parameter(Mandatory)]
        [string]$IdentityDisplayName,
        [Parameter(Mandatory)]
        [ValidateSet('ServicePrincipal', 'User')]
        [string]
        $IdentityType
    )
    Write-Verbose -Message "Retrieving details from service principal  $($EnterpriseApplicationClientId)."
    $enterpriseApplication = az ad sp show --id $EnterpriseApplicationClientId | ConvertFrom-Json # `clientId` matches the `Object Id` in the portal
    Write-Verbose -Message "Retrieving details for the managed identity  $($IdentityDisplayName)."
    $managedIdentityNeedingPermission = az ad sp list --display-name $IdentityDisplayName | ConvertFrom-Json | Select-Object -First 1

    # Assign managed identity the roles for the services
    Write-Verbose -Message "Assigning role $($AppRoleId) to identity $($managedIdentityNeedingPermission.objectId) for service principal $($enterpriseApplication.objectId)."
    az rest `
        --method post `
        --uri https://graph.microsoft.com/beta/servicePrincipals/$($enterpriseApplication.objectId)/appRoleAssignments `
        --headers "{'content-type': 'application/json'}" `
        --body "{'appRoleId': '$($AppRoleId)', 'principalId': '$($managedIdentityNeedingPermission.objectId)', 'principalType': '$($IdentityType)', 'resourceId': '$($enterpriseApplication.objectId)'}"

    Write-Output "Assigned role $($AppRoleId) to identity $($managedIdentityNeedingPermission.objectId) for service principal $($enterpriseApplication.objectId)."
}

$speakerApplicationUriId = [guid]::NewGuid()
$speakerApplicationName = "Speaker application"
$speakerApplicationIdentifierUri = "api://$($speakerApplicationUriId)"
$speakerApplicationReplyUrl = "https://speaker-application/auth"
$createdSpeakerApplicationEntepriseApplicationClientId = Add-AadApplicationWithServicePrincipal -DisplayName $speakerApplicationName -IdentifierUris $speakerApplicationIdentifierUri -ReplyUrls $speakerApplicationReplyUrl -AppRoles @speaker-manifest.json -Verbose

$speakerReaderRoleId = "42ee5891-7e50-4db9-a6d9-75ffc8cc1e9b"
$nameOfTheManagedIdentityNeedingPermission = "janv-secureapi-api"
Add-Role -EnterpriseApplicationClientId $createdSpeakerApplicationEntepriseApplicationClientId -AppRoleId $speakerReaderRoleId -IdentityDisplayName $nameOfTheManagedIdentityNeedingPermission -IdentityType "ServicePrincipal" -Verbose

$conferenceApplicationUriId = [guid]::NewGuid()
$conferenceApplicationName = "Conferences application"
$conferenceApplicationIdentifierUri = "api://$($conferenceApplicationUriId)"
$conferenceApplicationReplyUrl = "https://conference-application/auth"
$createdconferenceApplicationEntepriseApplicationClientId = Add-AadApplicationWithServicePrincipal -DisplayName $conferenceApplicationName -IdentifierUris $conferenceApplicationIdentifierUri -ReplyUrls $conferenceApplicationReplyUrl -AppRoles @conference-manifest.json -Verbose

$conferenceReaderRoleId = "2866fc2a-f9a0-4fc3-a0d4-c5ec8d287b3b"
Add-Role -EnterpriseApplicationClientId $createdconferenceApplicationEntepriseApplicationClientId -AppRoleId $conferenceReaderRoleId -IdentityDisplayName $nameOfTheManagedIdentityNeedingPermission -IdentityType "ServicePrincipal" -Verbose

###################
# Delete all test applications
###################
# az ad app list --filter "startswith(displayname, 'Speaker application')" | ConvertFrom-Json| ForEach-Object {  az ad app delete --id $_.appId --verbose }
# Shouldn't be necessary, as removing the App Registration also removes the Enterprise Application, but for completeness sake.
# az ad sp list --filter "startswith(displayname, 'Speaker application')" | ConvertFrom-Json| ForEach-Object {  az ad sp delete --id $_.appId --verbose }
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