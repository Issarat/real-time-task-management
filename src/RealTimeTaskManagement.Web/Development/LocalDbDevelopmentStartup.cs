using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace RealTimeTaskManagement.Web.Development;

internal static class LocalDbDevelopmentStartup
{
    private static readonly Regex LocalDbInstancePattern = new(
        @"(?:Data Source|Server)\s*=\s*\(localdb\)\\(?<instance>[^;]+)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static async Task EnsureStartedAsync(
        string? connectionString,
        ILogger logger,
        CancellationToken applicationStopping)
    {
        if (!OperatingSystem.IsWindows()
            || string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var match = LocalDbInstancePattern.Match(connectionString);
        if (!match.Success)
        {
            return;
        }

        var instanceName = match.Groups["instance"].Value.Trim();
        if (string.IsNullOrWhiteSpace(instanceName))
        {
            return;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "SqlLocalDB.exe",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        startInfo.ArgumentList.Add("start");
        startInfo.ArgumentList.Add(instanceName);

        try
        {
            using var process = Process.Start(startInfo)
                ?? throw new InvalidOperationException(
                    "ไม่สามารถเริ่มโปรเซส SqlLocalDB.exe ได้");
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(
                applicationStopping);
            timeout.CancelAfter(TimeSpan.FromSeconds(15));

            var standardOutputTask = process.StandardOutput.ReadToEndAsync();
            var standardErrorTask = process.StandardError.ReadToEndAsync();

            try
            {
                await process.WaitForExitAsync(timeout.Token);
            }
            catch (OperationCanceledException)
                when (!applicationStopping.IsCancellationRequested)
            {
                TryKill(process);
                throw new InvalidOperationException(
                    $"การเปิด LocalDB instance '{instanceName}' ใช้เวลานานเกิน 15 วินาที");
            }

            var standardOutput = (await standardOutputTask).Trim();
            var standardError = (await standardErrorTask).Trim();
            if (process.ExitCode != 0)
            {
                var details = string.IsNullOrWhiteSpace(standardError)
                    ? standardOutput
                    : standardError;
                throw new InvalidOperationException(
                    $"เปิด LocalDB instance '{instanceName}' ไม่สำเร็จ: {details}");
            }

            logger.LogInformation(
                "LocalDB instance {InstanceName} is ready.",
                instanceName);
        }
        catch (Win32Exception exception)
        {
            throw new InvalidOperationException(
                "ไม่พบ SqlLocalDB.exe กรุณาตรวจสอบการติดตั้ง SQL Server LocalDB และ PATH",
                exception);
        }
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
            // The process already exited while the timeout was being handled.
        }
    }
}
