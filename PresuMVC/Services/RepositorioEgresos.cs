using Dapper;
using Microsoft.Data.SqlClient;
using PresuMVC.Models;
using System.Numerics;

namespace PresuMVC.Services
{
    public interface IRepositorioEgresos
    {
        Task<int> CreateEgreso(Egreso egreso);
        Task DeleteEgreso(int id);
        Task DeleteEgresoCuotasSiguientes(int idEgresoOriginal, int nroCuota);
        Task DeleteEgresoTodasCuotas(int id);
        Task<Egreso> GetEgresoByID(int idEgreso);
        Task<IEnumerable<Egreso>> GetEgresos();
        Task<IEnumerable<Egreso>> GetEgresosTotales(DateTime date);
        Task UpdateEgreso(Egreso egreso);
        Task UpdateEgresoCuotaNro(string nombre, float valor, int cuotaNro, int idEgresoOriginal);
    }
    public class RepositorioEgresos : IRepositorioEgresos
    {
        private readonly string connectionString;

        public RepositorioEgresos(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> CreateEgreso(Egreso egreso)
        {
            using var connection = new SqlConnection(connectionString);

            var query = @"
            INSERT INTO Egreso (Nombre, Valor, FechaRegistro, FechaFin, CuotaNro, CantCuotas, FechaCuota, ValorTotal, IdTipoEgreso, IdEgresoOriginal)
            VALUES (@Nombre, @Valor, @FechaRegistro, @FechaFin, @CuotaNro, @CantCuotas, @FechaCuota, @ValorTotal, @IdTipoEgreso, @IdEgresoOriginal);
            SELECT CAST(SCOPE_IDENTITY() as int);";

            var parameters = new
            {
                egreso.Nombre,
                egreso.Valor,
                egreso.FechaRegistro,
                egreso.FechaFin,
                egreso.CuotaNro,
                egreso.CantCuotas,
                egreso.FechaCuota,
                egreso.ValorTotal,
                egreso.IdTipoEgreso,
                egreso.IdEgresoOriginal
            };

            egreso.IdEgreso = await connection.QuerySingleAsync<int>(query, parameters);
            return egreso.IdEgreso;
        }

        public async Task<IEnumerable<Egreso>> GetEgresos()
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Egreso>
               (@"SELECT * FROM Egreso");

        }

        public async Task<IEnumerable<Egreso>> GetEgresosTotales(DateTime date)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Egreso>
                (@"SELECT DISTINCT *
                FROM Egreso
                WHERE
                (MONTH(FechaRegistro) = @Mes AND YEAR(FechaRegistro) = @Año AND IdTipoEgreso=2) -- Ingresos del mismo mes y también se incluyen los de tipo único
                OR
                (MONTH(FechaCuota) = @Mes AND YEAR(FechaCuota) = @Año)
                OR
                (FechaRegistro <= @Fecha AND 
                (FechaFin IS NULL OR FechaFin >= @Fecha)  -- Egresos mensuales que se registraron en una fecha posterior 
                AND IdTipoEgreso = 1)                     -- y no vencieron, o si lo hicieron, fue luego de este mes
                ORDER BY IdTipoEgreso DESC, Valor DESC, FechaRegistro;",
                new { Mes = date.Month, Año = date.Year, Fecha = date });
        }

        public async Task DeleteEgreso(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"DELETE Egreso WHERE IdEgreso = @Id", new { id });
        }

        public async Task DeleteEgresoCuotasSiguientes(int idEgresoOriginal, int nroCuota)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"DELETE Egreso
                                            WHERE 
                                            (IdEgresoOriginal = @IdEgresoOriginal OR IdEgreso = @IdEgresoOriginal)AND
                                            CuotaNro >= @NroCuota", new { idEgresoOriginal, nroCuota });

        }

        public async Task DeleteEgresoTodasCuotas(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"DELETE Egreso
                                            WHERE 
                                            (IdEgresoOriginal = @Id OR IdEgreso = @Id)", new { id });

        }

        public async Task<Egreso> GetEgresoByID(int idEgreso)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Egreso>
                (@"SELECT * FROM Egreso Where IdEgreso=@idEgreso", new { idEgreso });
        }

        public async Task UpdateEgreso(Egreso egreso)
        {
            using var connection = new SqlConnection(connectionString);

            var query = @"
            UPDATE Egreso
            SET 
                Nombre = @Nombre,
                Valor = @Valor,
                FechaRegistro = @FechaRegistro, 
                FechaFin = @FechaFin,
                CuotaNro = @CuotaNro,
                CantCuotas = @CantCuotas,
                FechaCuota = @FechaCuota,
                ValorTotal = @ValorTotal,
                IdTipoEgreso = @IdTipoEgreso,
                IdEgresoOriginal = @IdEgresoOriginal
            WHERE 
                IdEgreso = @IdEgreso";

            var parameters = new
            {
                egreso.Nombre,
                egreso.Valor,
                egreso.FechaRegistro,
                egreso.FechaFin,
                egreso.CuotaNro,
                egreso.CantCuotas,
                egreso.FechaCuota,
                egreso.ValorTotal,
                egreso.IdTipoEgreso,
                egreso.IdEgresoOriginal,
                egreso.IdEgreso
            };

            await connection.ExecuteAsync(query, parameters);
        }

        public async Task UpdateEgresoCuotaNro(string nombre, float valor, int cuotaNro, int idEgresoOriginal)
        {
            using var connection = new SqlConnection(connectionString);

            var query = @"
            UPDATE Egreso
            SET 
                Nombre = @Nombre,
                Valor = @Valor
	        WHERE 
	            (IdEgresoOriginal = @IdEgresoOriginal OR IdEgreso = @IdEgresoOriginal)AND
	            CuotaNro = @CuotaNro";

            var parameters = new
            {
                nombre,
                valor,
                cuotaNro,
                idEgresoOriginal
            };

            await connection.ExecuteAsync(query, parameters);
        }

    }
}
