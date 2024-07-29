using Dapper;
using Microsoft.Data.SqlClient;
using PresuMVC.Models;

namespace PresuMVC.Services
{
    public interface IRepositorioEgresos
    {
    }
    public class RepositorioEgresos : IRepositorioEgresos
    {
        private readonly string connectionString;

        public RepositorioEgresos(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task CreateEgreso(Egreso egreso)
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
        }


    }
}
