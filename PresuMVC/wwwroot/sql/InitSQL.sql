CREATE DATABASE Presu

USE Presu

-- Crear la tabla TipoIngreso
CREATE TABLE TipoIngreso (
    IdTipoIngreso INT PRIMARY KEY IDENTITY(1,1),
    Tipo NVARCHAR(50) NOT NULL 
);

-- Crear la tabla Ingreso
CREATE TABLE Ingreso (
    IdIngreso INT PRIMARY KEY IDENTITY(1,1),
    Valor FLOAT NOT NULL,
    FechaRegistro DATE NOT NULL,
    IdTipoIngreso INT NOT NULL,
    FOREIGN KEY (IdTipoIngreso) REFERENCES TipoIngreso(IdTipoIngreso)
);

-- Crear la tabla TipoEgreso
CREATE TABLE TipoEgreso (
    IdTipoEgreso INT PRIMARY KEY IDENTITY(1,1),
    Tipo NVARCHAR(50) NOT NULL 
);

-- Crear la tabla Egreso
CREATE TABLE Egreso (
    IdEgreso INT PRIMARY KEY IDENTITY(1,1),
    Valor FLOAT NOT NULL,
    FechaRegistro DATE NOT NULL,
    CuotaNro INT,
    CantCuotas INT,
    FechaCuota DATE,
    ValorTotal FLOAT,
    IdTipoEgreso INT NOT NULL,
    IdEgresoOriginal INT,
    FOREIGN KEY (IdTipoEgreso) REFERENCES TipoEgreso(IdTipoEgreso),
    FOREIGN KEY (IdEgresoOriginal) REFERENCES Egreso(IdEgreso)
);

ALTER TABLE Ingreso
ADD Nombre NVARCHAR(100);

ALTER TABLE Egreso
ADD Nombre NVARCHAR(100);

INSERT INTO TipoIngreso(tipo)
VALUES ('Mensual'),('Único')

INSERT INTO TipoEgreso(tipo)
VALUES ('Suscripción'),('Único'),('Cuota')

--SELECT * FROM Ingreso
--SELECT * FROM Egreso
--SELECT * FROM TipoIngreso
--SELECT * FROM TipoEgreso
