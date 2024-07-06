using System.Net;
using System.Net.Sockets;
using System.Text;

Console.Write("Path to share:");
string folderPath = Console.ReadLine() ?? "";

if (!Directory.Exists(folderPath))
{
    Console.WriteLine("The specified folder does not exist.");
    return;
}

TcpListener listener = new(IPAddress.Any, 9911);
listener.Start();
Console.WriteLine("Server started. Waiting for client connections...");

while (true)
{
    TcpClient client = await listener.AcceptTcpClientAsync();
    Console.WriteLine("Client connected.");
    _ = Task.Run(() => HandleClientAsync(client, folderPath));
}



static async Task HandleClientAsync(TcpClient client, string folderPath)
{
    using (client)
    using (NetworkStream stream = client.GetStream())
    {
        try
        {
            await SendFolderAsync(folderPath, stream);
            Console.WriteLine("Folder synchronization completed for a client.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

static async Task SendFolderAsync(string folderPath, NetworkStream stream)
{
    foreach (string directory in Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories))
    {
        await SendDataAsync($"DIR:{directory.Replace(folderPath, string.Empty)}", stream);
    }

    foreach (string file in Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories))
    {
        await SendDataAsync($"FILE:{file.Replace(folderPath, string.Empty)}", stream);
        byte[] fileBytes = await File.ReadAllBytesAsync(file);
        await SendDataAsync(Convert.ToBase64String(fileBytes), stream);
    }
}

static async Task SendDataAsync(string data, NetworkStream stream)
{
    byte[] dataBytes = Encoding.UTF8.GetBytes(data + Environment.NewLine);
    await stream.WriteAsync(dataBytes);
}
