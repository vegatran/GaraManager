-- Drop all IdentityServer and Application tables

-- Disable foreign key checks
SET FOREIGN_KEY_CHECKS = 0;

-- Drop GaraManagement Context tables
DROP TABLE IF EXISTS `Claims`;
DROP TABLE IF EXISTS `SoftDeleteRecords`;

-- Drop Identity tables (AspNet)
DROP TABLE IF EXISTS `AspNetRoleClaims`;
DROP TABLE IF EXISTS `AspNetUserClaims`;
DROP TABLE IF EXISTS `AspNetUserLogins`;
DROP TABLE IF EXISTS `AspNetUserRoles`;
DROP TABLE IF EXISTS `AspNetUserTokens`;
DROP TABLE IF EXISTS `AspNetRoles`;
DROP TABLE IF EXISTS `AspNetUsers`;

-- Drop IdentityServer Configuration tables
DROP TABLE IF EXISTS `ApiResourceClaims`;
DROP TABLE IF EXISTS `ApiResourceProperties`;
DROP TABLE IF EXISTS `ApiResourceScopes`;
DROP TABLE IF EXISTS `ApiResourceSecrets`;
DROP TABLE IF EXISTS `ApiResources`;
DROP TABLE IF EXISTS `ApiScopeClaims`;
DROP TABLE IF EXISTS `ApiScopeProperties`;
DROP TABLE IF EXISTS `ApiScopes`;
DROP TABLE IF EXISTS `ClientClaims`;
DROP TABLE IF EXISTS `ClientCorsOrigins`;
DROP TABLE IF EXISTS `ClientGrantTypes`;
DROP TABLE IF EXISTS `ClientIdPRestrictions`;
DROP TABLE IF EXISTS `ClientPostLogoutRedirectUris`;
DROP TABLE IF EXISTS `ClientProperties`;
DROP TABLE IF EXISTS `ClientRedirectUris`;
DROP TABLE IF EXISTS `ClientScopes`;
DROP TABLE IF EXISTS `ClientSecrets`;
DROP TABLE IF EXISTS `Clients`;
DROP TABLE IF EXISTS `IdentityResourceClaims`;
DROP TABLE IF EXISTS `IdentityResourceProperties`;
DROP TABLE IF EXISTS `IdentityResources`;
DROP TABLE IF EXISTS `IdentityProviders`;

-- Drop IdentityServer Operational tables
DROP TABLE IF EXISTS `DeviceCodes`;
DROP TABLE IF EXISTS `Keys`;
DROP TABLE IF EXISTS `PersistedGrants`;
DROP TABLE IF EXISTS `ServerSideSessions`;
DROP TABLE IF EXISTS `PushedAuthorizationRequests`;

-- Drop Migration history
DROP TABLE IF EXISTS `__EFMigrationsHistory`;

-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS = 1;

SELECT 'All tables dropped successfully!' AS Result;

