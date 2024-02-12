USE [$(dbName)]
GO

---------------------------------------------------------------
--Tables
---------------------------------------------------------------
:r $(rootPath)/Create/Tables/System/QueueMessages.sql
:r $(rootPath)/Create/Tables/System/UpdateScripts.sql

:r $(rootPath)/Create/Tables/Audit/RegixReportsAuditLog.sql
:r $(rootPath)/Create/Tables/Audit/TimeStampRequestsAuditLog.sql

:r $(rootPath)/Create/Tables/Identity/AdminUsers.sql
:r $(rootPath)/Create/Tables/Identity/AdminRoles.sql
:r $(rootPath)/Create/Tables/Identity/AdminRoleClaims.sql
:r $(rootPath)/Create/Tables/Identity/AdminUserClaims.sql
:r $(rootPath)/Create/Tables/Identity/AdminUserLogins.sql
:r $(rootPath)/Create/Tables/Identity/AdminUserRoles.sql
:r $(rootPath)/Create/Tables/Identity/AdminUserTokens.sql

:r $(rootPath)/Create/Tables/Blob/MalwareScanResult.sql
:r $(rootPath)/Create/Tables/Blob/Blobs.sql
:r $(rootPath)/Create/Tables/Blob/BlobSignatures.sql

:r $(rootPath)/Create/Tables/Login/Logins.sql
:r $(rootPath)/Create/Tables/Login/Roles.sql
:r $(rootPath)/Create/Tables/Login/LoginsRoles.sql

:r $(rootPath)/Create/Tables/TargetGroup/TargetGroups.sql
:r $(rootPath)/Create/Tables/TargetGroup/TargetGroupMatrix.sql

:r $(rootPath)/Create/Tables/Template/LoginSecurityLevels.sql
:r $(rootPath)/Create/Tables/Template/Templates.sql
:r $(rootPath)/Create/Tables/Template/TemplateTargetGroups.sql

:r $(rootPath)/Create/Tables/Profile/Countries.sql
:r $(rootPath)/Create/Tables/Profile/Addresses.sql
:r $(rootPath)/Create/Tables/Profile/Profiles.sql
:r $(rootPath)/Create/Tables/Profile/Individuals.sql
:r $(rootPath)/Create/Tables/Profile/LegalEntities.sql
:r $(rootPath)/Create/Tables/Profile/ProfileKeys.sql
:r $(rootPath)/Create/Tables/Profile/ProfileBlobAccessKeys.sql
:r $(rootPath)/Create/Tables/Profile/ProfileQuotas.sql
:r $(rootPath)/Create/Tables/Profile/ProfileEsbUsers.sql
:r $(rootPath)/Create/Tables/Profile/ProfilesHistory.sql
:r $(rootPath)/Create/Tables/Profile/LoginsProfiles.sql
:r $(rootPath)/Create/Tables/Profile/LoginProfilePermissions.sql

:r $(rootPath)/Create/Tables/TargetGroup/TargetGroupProfiles.sql
:r $(rootPath)/Create/Tables/Template/TemplateProfiles.sql

:r $(rootPath)/Create/Tables/Message/Messages.sql
:r $(rootPath)/Create/Tables/Message/MessageRecipients.sql
:r $(rootPath)/Create/Tables/Message/MessageAccessKeys.sql
:r $(rootPath)/Create/Tables/Message/MessageBlobs.sql
:r $(rootPath)/Create/Tables/Message/MessageBlobAccessKeys.sql
:r $(rootPath)/Create/Tables/Message/MessagesAccessCodes.sql
:r $(rootPath)/Create/Tables/Message/ForwardedMessages.sql
:r $(rootPath)/Create/Tables/Message/MessageTranslations.sql
:r $(rootPath)/Create/Tables/Message/MessageTranslationRequests.sql

:r $(rootPath)/Create/Tables/RecipientGroup/RecipientGroups.sql
:r $(rootPath)/Create/Tables/RecipientGroup/RecipientGroupProfiles.sql

:r $(rootPath)/Create/Tables/Administration/AdminsProfiles.sql
:r $(rootPath)/Create/Tables/Administration/RegistrationRequests.sql
:r $(rootPath)/Create/Tables/Administration/StatisticsMessagesByMonth.sql

:r $(rootPath)/Create/Tables/Ticket/Tickets.sql
:r $(rootPath)/Create/Tables/Ticket/TicketStatuses.sql

:r $(rootPath)/Create/Tables/Report/EmailDelivery.sql
:r $(rootPath)/Create/Tables/Report/SmsDelivery.sql
:r $(rootPath)/Create/Tables/Report/TicketDelivery.sql
:r $(rootPath)/Create/Tables/Report/ViberDelivery.sql

:r $(rootPath)/Create/Tables/__ForRemoval/CertificateFlags.sql
:r $(rootPath)/Create/Tables/__ForRemoval/ExternalLogins.sql
:r $(rootPath)/Create/Tables/__ForRemoval/LoginsClaims.sql
:r $(rootPath)/Create/Tables/__ForRemoval/Notifications.sql

:r $(rootPath)/Create/Views/ProfileStorageSpace_Indexed.sql

:r $(rootPath)/Create/StoredProcedures/spDeleteOrphanedBlobs.sql
