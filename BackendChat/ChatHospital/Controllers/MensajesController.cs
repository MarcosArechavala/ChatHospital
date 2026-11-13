using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

[ApiController]
[Route("api/[controller]")]
public class MensajesController : ControllerBase
{
    private readonly string _connectionString;

    public MensajesController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ConexionSQL")!;
    }

    // Trae todos los mensajes en los que participó este servicio (enviados o recibidos)
    [HttpGet("{miServicio}")]
    public async Task<IActionResult> ObtenerHistorial(string miServicio)
    {
        var lista = new List<object>();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            string sql = @"
                SELECT TOP 200 Id, ServicioOrigen, ServicioDestino, Usuario, Texto, EsUrgente, Fecha
                FROM Mensajes
                WHERE ServicioOrigen = @Servicio OR ServicioDestino = @Servicio
                ORDER BY Fecha ASC";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Servicio", miServicio);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new
                        {
                            Id = reader.GetInt64(0),
                            ServicioOrigen = reader.GetString(1),
                            ServicioDestino = reader.GetString(2),
                            Usuario = reader.GetString(3),
                            Texto = reader.GetString(4),
                            EsUrgente = reader.GetBoolean(5),
                            Fecha = reader.GetDateTime(6)
                        });
                    }
                }
            }
        }

        return Ok(lista);
    }
}