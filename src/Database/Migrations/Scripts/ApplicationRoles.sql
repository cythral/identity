CREATE TABLE `Roles` (
    `Name` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Description` longtext CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_Roles` PRIMARY KEY (`Name`)
);

CREATE TABLE `ApplicationRoles` (
    `ApplicationName` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `RoleName` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_ApplicationRoles` PRIMARY KEY (`ApplicationName`, `RoleName`),
    CONSTRAINT `FK_ApplicationRoles_Applications_ApplicationName` FOREIGN KEY (`ApplicationName`) REFERENCES `Applications` (`Name`) ON DELETE CASCADE,
    CONSTRAINT `FK_ApplicationRoles_Roles_RoleName` FOREIGN KEY (`RoleName`) REFERENCES `Roles` (`Name`) ON DELETE CASCADE
);

CREATE INDEX `IX_ApplicationRoles_RoleName` ON `ApplicationRoles` (`RoleName`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20201107204842_ApplicationRoles', '3.1.9');

