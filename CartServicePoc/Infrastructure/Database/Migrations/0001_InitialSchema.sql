CREATE TABLE Carts (
                       Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                       UserId NVARCHAR(256) NOT NULL,
                       CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                       UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                       CONSTRAINT UQ_Carts_UserId UNIQUE (UserId)
);

CREATE TABLE CartItems (
                           Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                           CartId UNIQUEIDENTIFIER NOT NULL,
                           ProductId NVARCHAR(256) NOT NULL,
                           ProductName NVARCHAR(512) NOT NULL,
                           ImageUrl NVARCHAR(1024) NULL,
                           UnitPrice DECIMAL(18,2) NOT NULL,
                           Quantity INT NOT NULL,
                           AddedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                           CONSTRAINT FK_CartItems_Carts FOREIGN KEY (CartId) REFERENCES Carts(Id) ON DELETE CASCADE,
                           CONSTRAINT UQ_CartItems_CartId_ProductId UNIQUE (CartId, ProductId)
);
