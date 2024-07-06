using System.Net.Sockets;
using System.Text;

Console.Write("Local path:");
string folderPath = Console.ReadLine() ?? "";
if (string.IsNullOrEmpty(folderPath)) return;
if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

Console.Write("Server path:");
string serverPath = Console.ReadLine() ?? "";
if (string.IsNullOrEmpty(serverPath)) return;

try
{
    using TcpClient client = new();
    await client.ConnectAsync(serverPath, 9911);
    using NetworkStream stream = client.GetStream();
    using StreamReader reader = new(stream, Encoding.UTF8);
    Console.WriteLine("Connected to server. Starting folder synchronization...");

    string line = "";
    while ((line = await reader.ReadLineAsync() ?? null) != null)
    {
        if (string.IsNullOrEmpty(line)) continue;

        Console.WriteLine(line);

        if (line.StartsWith("DIR:"))
        {
            string dirPath = string.Concat(folderPath, line.AsSpan(4));
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }
        else if (line.StartsWith("FILE:"))
        {
            string filePath = string.Concat(folderPath, line.AsSpan(5));
            string fileData = await reader.ReadLineAsync();
            byte[] fileBytes = Convert.FromBase64String(fileData);
            await File.WriteAllBytesAsync(filePath, fileBytes);
        }
    }

    Console.WriteLine("Folder synchronization completed.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}