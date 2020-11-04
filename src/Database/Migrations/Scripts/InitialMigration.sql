CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(95) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
);

CREATE TABLE `Applications` (
    `Name` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Description` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Serial` bigint unsigned NOT NULL,
    `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CONSTRAINT `PK_Applications` PRIMARY KEY (`Name`)
);

CREATE TABLE `OpenIddictApplications` (
    `Id` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `ClientId` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `ClientSecret` longtext CHARACTER SET utf8mb4 NULL,
    `ConcurrencyToken` varchar(50) CHARACTER SET utf8mb4 NULL,
    `ConsentType` longtext CHARACTER SET utf8mb4 NULL,
    `DisplayName` longtext CHARACTER SET utf8mb4 NULL,
    `Permissions` longtext CHARACTER SET utf8mb4 NULL,
    `PostLogoutRedirectUris` longtext CHARACTER SET utf8mb4 NULL,
    `Properties` longtext CHARACTER SET utf8mb4 NULL,
    `RedirectUris` longtext CHARACTER SET utf8mb4 NULL,
    `Type` varchar(25) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_OpenIddictApplications` PRIMARY KEY (`Id`)
);

CREATE TABLE `OpenIddictScopes` (
    `Id` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `ConcurrencyToken` varchar(50) CHARACTER SET utf8mb4 NULL,
    `Description` longtext CHARACTER SET utf8mb4 NULL,
    `DisplayName` longtext CHARACTER SET utf8mb4 NULL,
    `Name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Properties` longtext CHARACTER SET utf8mb4 NULL,
    `Resources` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_OpenIddictScopes` PRIMARY KEY (`Id`)
);

CREATE TABLE `Users` (
    `Id` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `UserName` longtext CHARACTER SET utf8mb4 NULL,
    `NormalizedUserName` longtext CHARACTER SET utf8mb4 NULL,
    `Email` longtext CHARACTER SET utf8mb4 NULL,
    `NormalizedEmail` longtext CHARACTER SET utf8mb4 NULL,
    `EmailConfirmed` tinyint(1) NOT NULL,
    `PasswordHash` longtext CHARACTER SET utf8mb4 NULL,
    `SecurityStamp` longtext CHARACTER SET utf8mb4 NULL,
    `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 NULL,
    `PhoneNumber` longtext CHARACTER SET utf8mb4 NULL,
    `PhoneNumberConfirmed` tinyint(1) NOT NULL,
    `TwoFactorEnabled` tinyint(1) NOT NULL,
    `LockoutEnd` datetime(6) NULL,
    `LockoutEnabled` tinyint(1) NOT NULL,
    `AccessFailedCount` int NOT NULL,
    CONSTRAINT `PK_Users` PRIMARY KEY (`Id`)
);

CREATE TABLE `OpenIddictAuthorizations` (
    `Id` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `ApplicationId` varchar(255) CHARACTER SET utf8mb4 NULL,
    `ConcurrencyToken` varchar(50) CHARACTER SET utf8mb4 NULL,
    `Properties` longtext CHARACTER SET utf8mb4 NULL,
    `Scopes` longtext CHARACTER SET utf8mb4 NULL,
    `Status` varchar(25) CHARACTER SET utf8mb4 NOT NULL,
    `Subject` varchar(450) CHARACTER SET utf8mb4 NOT NULL,
    `Type` varchar(25) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_OpenIddictAuthorizations` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_OpenIddictAuthorizations_OpenIddictApplications_ApplicationId` FOREIGN KEY (`ApplicationId`) REFERENCES `OpenIddictApplications` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `OpenIddictTokens` (
    `Id` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `ApplicationId` varchar(255) CHARACTER SET utf8mb4 NULL,
    `AuthorizationId` varchar(255) CHARACTER SET utf8mb4 NULL,
    `ConcurrencyToken` varchar(50) CHARACTER SET utf8mb4 NULL,
    `CreationDate` datetime(6) NULL,
    `ExpirationDate` datetime(6) NULL,
    `Payload` longtext CHARACTER SET utf8mb4 NULL,
    `Properties` longtext CHARACTER SET utf8mb4 NULL,
    `ReferenceId` varchar(100) CHARACTER SET utf8mb4 NULL,
    `Status` varchar(25) CHARACTER SET utf8mb4 NOT NULL,
    `Subject` varchar(450) CHARACTER SET utf8mb4 NOT NULL,
    `Type` varchar(25) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_OpenIddictTokens` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_OpenIddictTokens_OpenIddictApplications_ApplicationId` FOREIGN KEY (`ApplicationId`) REFERENCES `OpenIddictApplications` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_OpenIddictTokens_OpenIddictAuthorizations_AuthorizationId` FOREIGN KEY (`AuthorizationId`) REFERENCES `OpenIddictAuthorizations` (`Id`) ON DELETE RESTRICT
);

CREATE UNIQUE INDEX `IX_OpenIddictApplications_ClientId` ON `OpenIddictApplications` (`ClientId`);

CREATE INDEX `IX_OpenIddictAuthorizations_ApplicationId_Status_Subject_Type` ON `OpenIddictAuthorizations` (`ApplicationId`, `Status`, `Subject`, `Type`);

CREATE UNIQUE INDEX `IX_OpenIddictScopes_Name` ON `OpenIddictScopes` (`Name`);

CREATE INDEX `IX_OpenIddictTokens_AuthorizationId` ON `OpenIddictTokens` (`AuthorizationId`);

CREATE UNIQUE INDEX `IX_OpenIddictTokens_ReferenceId` ON `OpenIddictTokens` (`ReferenceId`);

CREATE INDEX `IX_OpenIddictTokens_ApplicationId_Status_Subject_Type` ON `OpenIddictTokens` (`ApplicationId`, `Status`, `Subject`, `Type`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20201104142432_InitialMigration', '3.1.9');


