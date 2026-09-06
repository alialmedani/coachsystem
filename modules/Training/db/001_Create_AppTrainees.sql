/* =====================================================================
   Training module - manual schema (NO-MIGRATIONS rule).
   Target: SQL Server, "Default" connection (same DB as the host).
   Table matches ConfigureTraining(): FullAuditedAggregateRoot<Guid> + IMultiTenant.
   Idempotent: safe to run more than once.
   ===================================================================== */

IF OBJECT_ID(N'[dbo].[AppTrainees]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AppTrainees]
    (
        [Id]                   UNIQUEIDENTIFIER   NOT NULL,

        /* Business columns */
        [Code]                 NVARCHAR(32)       NOT NULL,
        [FirstName]            NVARCHAR(64)       NOT NULL,
        [LastName]             NVARCHAR(64)       NOT NULL,
        [Gender]               TINYINT            NOT NULL,   -- Gender enum stored as byte
        [BirthDate]            DATETIME2          NULL,
        [Email]                NVARCHAR(256)      NULL,
        [PhoneNumber]          NVARCHAR(32)       NULL,
        [Address]              NVARCHAR(512)      NULL,
        [IsActive]             BIT                NOT NULL,

        /* Multi-tenancy */
        [TenantId]             UNIQUEIDENTIFIER   NULL,

        /* ABP infrastructure columns (ConfigureByConvention) */
        [ExtraProperties]      NVARCHAR(MAX)      NULL,
        [ConcurrencyStamp]     NVARCHAR(40)       NULL,

        /* Full auditing + soft delete */
        [CreationTime]         DATETIME2          NOT NULL,
        [CreatorId]            UNIQUEIDENTIFIER   NULL,
        [LastModificationTime] DATETIME2          NULL,
        [LastModifierId]       UNIQUEIDENTIFIER   NULL,
        [IsDeleted]            BIT                NOT NULL CONSTRAINT [DF_AppTrainees_IsDeleted] DEFAULT (CAST(0 AS BIT)),
        [DeleterId]            UNIQUEIDENTIFIER   NULL,
        [DeletionTime]         DATETIME2          NULL,

        CONSTRAINT [PK_AppTrainees] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE INDEX [IX_AppTrainees_TenantId_Code] ON [dbo].[AppTrainees] ([TenantId] ASC, [Code] ASC);
END
GO
