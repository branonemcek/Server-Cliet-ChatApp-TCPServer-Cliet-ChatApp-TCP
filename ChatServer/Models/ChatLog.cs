    
    namespace ChatLogApi.Models;
    public class ConnectionLog

{
    public int Id { get; set; }
    public string? ClientName { get; set; }
    public DateTime ConnectedAt { get; set; }
}