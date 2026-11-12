using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    // Cuando una PC abre el chat, se une al canal de su Servicio (ej: "LAB")
    public async Task UnirseAServicio(string codigoServicio)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, codigoServicio);
    }

    // Enviar un mensaje de un servicio a otro
    public async Task EnviarMensajeAServicio(string servicioOrigen, string servicioDestino, string usuario, string texto, bool esUrgente)
    {
        var mensajeDto = new
        {
            ServicioOrigen = servicioOrigen,
            ServicioDestino = servicioDestino, // <-- ¡AGREGA ESTA LÍNEA!
            Usuario = usuario,
            Texto = texto,
            EsUrgente = esUrgente,
            Fecha = DateTime.Now
        };

        await Clients.Groups(servicioDestino, servicioOrigen).SendAsync("RecibirMensaje", mensajeDto);
    }
}