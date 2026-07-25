IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [NombreCompleto] nvarchar(max) NOT NULL,
        [Activo] bit NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE TABLE [Auditorias] (
        [Id] bigint NOT NULL IDENTITY,
        [UsuarioId] nvarchar(max) NULL,
        [NombreUsuario] nvarchar(256) NOT NULL,
        [Accion] nvarchar(100) NOT NULL,
        [Entidad] nvarchar(100) NOT NULL,
        [ClavePrimaria] nvarchar(100) NOT NULL,
        [FechaHora] datetime2 NOT NULL,
        [ValoresAnteriores] nvarchar(max) NULL,
        [ValoresNuevos] nvarchar(max) NULL,
        [Motivo] nvarchar(max) NULL,
        [DireccionIp] nvarchar(max) NULL,
        [UserAgent] nvarchar(max) NULL,
        [CorrelationId] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Auditorias] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE TABLE [MotivosMerma] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Descripcion] nvarchar(500) NULL,
        [RequiereEvidencia] bit NOT NULL,
        [RequiereAutorizacion] bit NOT NULL,
        [Estado] int NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_MotivosMerma] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_MotivosMerma_Estado] CHECK ([Estado] IN (1,2))
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE TABLE [TiposRegistro] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [UnidadMedida] nvarchar(30) NOT NULL,
        [Estado] int NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_TiposRegistro] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_TiposRegistro_Estado] CHECK ([Estado] IN (1,2))
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE TABLE [Trabajadores] (
        [Id] int NOT NULL IDENTITY,
        [Rut] nvarchar(20) NOT NULL,
        [NombreCompleto] nvarchar(150) NOT NULL,
        [Area] nvarchar(100) NOT NULL,
        [Estado] int NOT NULL,
        [CreadoPor] nvarchar(450) NOT NULL,
        [ModificadoPor] nvarchar(450) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Trabajadores] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Trabajadores_Estado] CHECK ([Estado] IN (1,2))
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE TABLE [Entradas] (
        [Id] int NOT NULL IDENTITY,
        [TipoRegistroId] int NOT NULL,
        [FechaHora] datetime2 NOT NULL,
        [CantidadInicial] decimal(18,3) NOT NULL,
        [DocumentoOrigen] nvarchar(100) NOT NULL,
        [Observacion] nvarchar(1000) NULL,
        [Estado] int NOT NULL,
        [UsuarioResponsableId] nvarchar(450) NOT NULL,
        [FechaUltimoMovimiento] datetime2 NOT NULL,
        [AnuladaPorId] nvarchar(max) NULL,
        [FechaAnulacion] datetime2 NULL,
        [MotivoAnulacion] nvarchar(500) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Entradas] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Entradas_Cantidad] CHECK ([CantidadInicial] > 0),
        CONSTRAINT [CK_Entradas_Estado] CHECK ([Estado] IN (1,2)),
        CONSTRAINT [FK_Entradas_TiposRegistro_TipoRegistroId] FOREIGN KEY ([TipoRegistroId]) REFERENCES [TiposRegistro] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE TABLE [Asignaciones] (
        [Id] int NOT NULL IDENTITY,
        [EntradaId] int NOT NULL,
        [TrabajadorId] int NOT NULL,
        [FechaHora] datetime2 NOT NULL,
        [Cantidad] decimal(18,3) NOT NULL,
        [Observacion] nvarchar(1000) NULL,
        [Estado] int NOT NULL,
        [UsuarioResponsableId] nvarchar(450) NOT NULL,
        [AnuladaPorId] nvarchar(max) NULL,
        [FechaAnulacion] datetime2 NULL,
        [MotivoAnulacion] nvarchar(500) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Asignaciones] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Asignaciones_Cantidad] CHECK ([Cantidad] > 0),
        CONSTRAINT [CK_Asignaciones_Estado] CHECK ([Estado] IN (1,2)),
        CONSTRAINT [FK_Asignaciones_Entradas_EntradaId] FOREIGN KEY ([EntradaId]) REFERENCES [Entradas] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Asignaciones_Trabajadores_TrabajadorId] FOREIGN KEY ([TrabajadorId]) REFERENCES [Trabajadores] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE TABLE [Mermas] (
        [Id] int NOT NULL IDENTITY,
        [EntradaId] int NOT NULL,
        [MotivoMermaId] int NOT NULL,
        [FechaHora] datetime2 NOT NULL,
        [Cantidad] decimal(18,3) NOT NULL,
        [Observacion] nvarchar(1000) NULL,
        [EvidenciaReferencia] nvarchar(500) NULL,
        [Estado] int NOT NULL,
        [UsuarioResponsableId] nvarchar(450) NOT NULL,
        [AnuladaPorId] nvarchar(max) NULL,
        [FechaAnulacion] datetime2 NULL,
        [MotivoAnulacion] nvarchar(500) NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaModificacion] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Mermas] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Mermas_Cantidad] CHECK ([Cantidad] > 0),
        CONSTRAINT [CK_Mermas_Estado] CHECK ([Estado] IN (1,2)),
        CONSTRAINT [FK_Mermas_Entradas_EntradaId] FOREIGN KEY ([EntradaId]) REFERENCES [Entradas] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Mermas_MotivosMerma_MotivoMermaId] FOREIGN KEY ([MotivoMermaId]) REFERENCES [MotivosMerma] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Asignaciones_EntradaId] ON [Asignaciones] ([EntradaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Asignaciones_Estado] ON [Asignaciones] ([Estado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Asignaciones_FechaHora] ON [Asignaciones] ([FechaHora]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Asignaciones_TrabajadorId] ON [Asignaciones] ([TrabajadorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Auditorias_Entidad_ClavePrimaria] ON [Auditorias] ([Entidad], [ClavePrimaria]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Auditorias_FechaHora] ON [Auditorias] ([FechaHora]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Entradas_DocumentoOrigen_TipoRegistroId] ON [Entradas] ([DocumentoOrigen], [TipoRegistroId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Entradas_Estado] ON [Entradas] ([Estado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Entradas_FechaHora] ON [Entradas] ([FechaHora]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Entradas_TipoRegistroId] ON [Entradas] ([TipoRegistroId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Mermas_EntradaId] ON [Mermas] ([EntradaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Mermas_Estado] ON [Mermas] ([Estado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Mermas_FechaHora] ON [Mermas] ([FechaHora]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Mermas_MotivoMermaId] ON [Mermas] ([MotivoMermaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MotivosMerma_Nombre] ON [MotivosMerma] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TiposRegistro_Nombre] ON [TiposRegistro] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Trabajadores_Rut] ON [Trabajadores] ([Rut]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260725203532_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260725203532_InitialCreate', N'8.0.22');
END;
GO

COMMIT;
GO

