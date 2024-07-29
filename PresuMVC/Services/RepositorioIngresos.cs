using Dapper;
using Microsoft.Data.SqlClient;
using PresuMVC.Models;
using System.Runtime.CompilerServices;

namespace PresuMVC.Services
{
    public interface IRepositorioIngresos
    {
        Task<Ingreso> GetIngresoByID(int idIngreso);
        Task<IEnumerable<Ingreso>> GetIngresos();
        Task UpdateIngreso(Ingreso ingreso);
        Task<IEnumerable<Ingreso>> GetIngresosTotales(DateTime date);
        Task CreateIngreso(Ingreso ingreso);
        Task DeleteIngreso(int id);
    }
    public class RepositorioIngresos : IRepositorioIngresos
    {
        private readonly string connectionString;

        public RepositorioIngresos(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Ingreso>> GetIngresos()
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Ingreso>
                (@"SELECT * FROM Ingreso");
        }

        public async Task<IEnumerable<Ingreso>> GetIngresosTotales(DateTime date)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Ingreso>
                (@"SELECT DISTINCT *
                FROM Ingreso
                WHERE
                (MONTH(FechaRegistro) = @Mes AND YEAR(FechaRegistro) = @Año) -- Ingresos del mismo mes y también se incluyen los de tipo único
                OR
                (FechaRegistro <= @Fecha AND 
                (FechaFin IS NULL OR FechaFin >= @Fecha)  -- Ingresos mensuales que se registraron en una fecha posterior 
                AND IdTipoIngreso = 1)                    -- y no vencieron, o si lo hicieron, fue luego de este mes
                ORDER BY IdTipoIngreso DESC, Valor DESC, FechaRegistro;",
                new { Mes = date.Month, Año = date.Year, Fecha = date});
        }

        public async Task UpdateIngreso(Ingreso ingreso)
        {
            using var connection = new SqlConnection(connectionString);

            var query = @"
            UPDATE Ingreso
            SET 
                Valor = @Valor,
                FechaRegistro = @FechaRegistro, 
                IdTipoIngreso = @IdTipoIngreso, 
                Nombre = @Nombre, 
                FechaFin = @FechaFin
            WHERE 
                IdIngreso = @IdIngreso";

            var parameters = new
            {
                ingreso.Valor,
                ingreso.FechaRegistro,
                ingreso.IdTipoIngreso,
                ingreso.Nombre,
                ingreso.FechaFin,
                ingreso.IdIngreso
            };

            await connection.ExecuteAsync(query, parameters);
        }

        public async Task CreateIngreso(Ingreso ingreso)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO Ingreso (Valor,FechaRegistro,IdTipoIngreso,Nombre,FechaFin) 
                                                            VALUES (@Valor,@FechaRegistro,@IdTipoIngreso,@Nombre,@FechaFin)
                                                            SELECT SCOPE_IDENTITY()",
                                                            new { ingreso.Valor, ingreso.FechaRegistro, ingreso.IdTipoIngreso, ingreso.Nombre, ingreso.FechaFin });
            ingreso.IdIngreso=id;
        }

        public async Task DeleteIngreso (int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"DELETE Ingreso WHERE IdIngreso = @Id", new {id});
        }

        public async Task<Ingreso> GetIngresoByID(int idIngreso)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Ingreso>
                (@"SELECT * FROM Ingreso Where IdIngreso=@idIngreso", new {idIngreso});
        }

    }
}
