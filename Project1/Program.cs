using Microsoft.Extensions.Configuration;
using Serilog;

try
{
    // 建立配置設定
    var config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    string sourceDirectory = config["FileTransferSettings:SourceDirectory"] ?? "C:\\Tmp\\Project\\Project1\\src";
    string destinationDirectory = config["FileTransferSettings:DestinationDirectory"] ?? "C:\\Tmp\\Project\\Project1\\dest";
    string logPath = config["FileTransferSettings:LogPath"] ?? "C:\\Tmp\\Project\\Project1\\log";

    // 設定 Serilog 日誌配置，將日誌寫入到 Console 和檔案
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()  // 設置日誌級別
        .WriteTo.Console()     // 輸出到 Console
        .WriteTo.File($"{logPath}/log.txt", rollingInterval: RollingInterval.Day)  // 輸出到文件
        .CreateLogger();

    // 檢查來源目錄是否存在
    if (!Directory.Exists(sourceDirectory))
    {
        Log.Error($"來源目錄不存在: {sourceDirectory}");
        return;
    }

    // 檢查或創建目標目錄
    if (!Directory.Exists(destinationDirectory))
    {
        Directory.CreateDirectory(destinationDirectory);
    }

    // 取得來源目錄下的所有檔案
    string[] files = Directory.GetFiles(sourceDirectory);

    foreach (string file in files)
    {
        string fileName = Path.GetFileName(file);
        string destFile = Path.Combine(destinationDirectory, fileName);

        try
        {
            // 移動檔案
            File.Move(file, destFile);
            Log.Information($"檔案已移動: {fileName}");
        }
        catch (FileNotFoundException ex)
        {
            Log.Error($"檔案已被刪除: {fileName} - {ex.Message}");
        }
        catch (Exception ex)
        {
            Log.Error($"移動檔案時發生錯誤: {fileName} - {ex.Message}");
        }
    }
}
catch (Exception ex)
{
    Log.Error(ex, "執行檔案移動時發生錯誤");
}
finally
{
    // 確保在程式結束時釋放 Serilog 的資源
    Log.CloseAndFlush();
}
