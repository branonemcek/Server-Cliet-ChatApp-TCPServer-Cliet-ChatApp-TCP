using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ChatLogApi.Models;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System.Reflection;




class Program
{
    private static TcpListener server = new TcpListener(IPAddress.Any, 8888);
    private static List<(TcpClient client, string name)> clients = new List<(TcpClient, string)>();
    private static ILogger<Program> logger;

    static async Task Main()
    {
        // Vytvaranie logera
        var loggerCreate = LoggerFactory.Create(builder => {
            builder.AddConsole();
        });

        logger = loggerCreate.CreateLogger<Program>();

        // Nacitavanie Databazi
        using (var dbContext = new ChatDbContext()) {
            await dbContext.Database.EnsureCreatedAsync();
        }

        server.Start();
        logger.LogInformation("Server START");

        while (true) {
            TcpClient client = await server.AcceptTcpClientAsync();
            logger.LogInformation($"New client connected.");

            // Nacitanie MENA klienta
            byte[] nameBytes = new byte[4096];
            int nameBytesRead = await client.GetStream().ReadAsync(nameBytes, 0, nameBytes.Length);
            string clientName = Encoding.ASCII.GetString(nameBytes, 0, nameBytesRead);

            // Databaza uloyenie pripojenia klienta
            using (var dbContext = new ChatDbContext()) {
                dbContext.ConnectionLogs.Add(new ConnectionLog { ClientName = clientName, ConnectedAt = DateTime.Now });
                await dbContext.SaveChangesAsync();
            }

            clients.Add((client, clientName));
            _ = HandleClientAsync((client, clientName)); //discard _ pouzitie ked navratova hodnota neni priradena ziadna hodnota??
        }
    }

    static async Task HandleClientAsync((TcpClient client, string name) clientInfo)
    {
        var (tcpClient, clientName) = clientInfo;
        NetworkStream clientStream = tcpClient.GetStream();

        byte[] message = new byte[4096];
        int bytesRead;

        while (true) { bytesRead = 0;

            try
            {
                bytesRead = await clientStream.ReadAsync(message, 0, 4096);
            }
            catch
            {
                break;
            }

            if (bytesRead == 0)
                break;

            string clientMessage = Encoding.ASCII.GetString(message, 0, bytesRead);
            Console.WriteLine($"{clientName} says: {clientMessage}");

            await BroadcastAsync($"{clientName}: {clientMessage}");
        }

        tcpClient.Close();
        clients.Remove((tcpClient, clientName)); // Odstrániť klienta a jeho meno po odpojení
    }


    static async Task BroadcastAsync(string message) // metoda BroadcastAsync posiela spravu všetkým pripojeným klientom
    {
        byte[] broadcastMessage = Encoding.ASCII.GetBytes(message);

        foreach (var (client, _) in clients) {
            await client.GetStream().WriteAsync(broadcastMessage, 0, broadcastMessage.Length);
        }
    }



public void ConfigureServices(IServiceCollection services)
{
    // ...

    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
    });

    // ...
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // ...

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1");
        c.RoutePrefix = string.Empty; // At root
        c.DocExpansion(DocExpansion.List);
    });

    // ...
}

}
