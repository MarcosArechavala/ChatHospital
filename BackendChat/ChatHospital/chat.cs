using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;

public class ChatHub : Hub
{
    private readonly string _connectionString;

    // Inyectamos IConfiguration para leer la cadena de conexión del appsettings.json
    public ChatHub(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ConexionSQL")!;
    }

    public async Task UnirseAServicio(string codigoServicio)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, codigoServicio);
    }

    public async Task EnviarMensajeAServicio(string servicioOrigen, string servicioDestino, string usuario, string texto, bool esUrgente)
    {
        var fechaActual = DateTime.Now;
        long nuevoId = 0;

        // 1. GUARDAMOS EN SQL SERVER CON ADO.NET
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            string sql = @"
                INSERT INTO Mensajes (ServicioOrigen, ServicioDestino, Usuario, Texto, EsUrgente, Fecha)
                OUTPUT INSERTED.Id
                VALUES (@Origen, @Destino, @Usuario, @Texto, @EsUrgente, @Fecha)";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Origen", servicioOrigen);
                cmd.Parameters.AddWithValue("@Destino", servicioDestino);
                cmd.Parameters.AddWithValue("@Usuario", usuario);
                cmd.Parameters.AddWithValue("@Texto", texto);
                cmd.Parameters.AddWithValue("@EsUrgente", esUrgente);
                cmd.Parameters.AddWithValue("@Fecha", fechaActual);

                nuevoId = (long)(await cmd.ExecuteScalarAsync())!;
            }
        }

        // 2. ARMAMOS EL OBJETO Y LO EMITIMOS EN TIEMPO REAL
        var mensajeDto = new
        {
            Id = nuevoId,
            ServicioOrigen = servicioOrigen,
            ServicioDestino = servicioDestino,
            Usuario = usuario,
            Texto = texto,
            EsUrgente = esUrgente,
            Fecha = fechaActual
        };

        await Clients.Groups(servicioDestino, servicioOrigen).SendAsync("RecibirMensaje", mensajeDto);
    }
}