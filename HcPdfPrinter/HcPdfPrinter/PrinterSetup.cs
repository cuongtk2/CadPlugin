using System;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace HcPdfPrinter
{
    internal static class PrinterSetup
    {
        public static bool IsAdmin()
        {
            using var id = WindowsIdentity.GetCurrent();
            return new WindowsPrincipal(id).IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static void Install()
        {
            AppPaths.EnsureDirs();
            GrantUsersModify();

            string script = $@"
$ErrorActionPreference = 'Stop'
$name   = '{AppPaths.PrinterName}'
$driver = '{AppPaths.DriverName}'
$port   = '{AppPaths.SpoolFile}'

if (-not (Get-PrinterDriver -Name $driver -ErrorAction SilentlyContinue)) {{
    throw ""Không tìm thấy driver '$driver'. Bật tính năng 'Microsoft Print to PDF' trong Windows Features.""
}}
if (-not (Get-PrinterPort -Name $port -ErrorAction SilentlyContinue)) {{
    Add-PrinterPort -Name $port
}}
if (-not (Get-Printer -Name $name -ErrorAction SilentlyContinue)) {{
    Add-Printer -Name $name -DriverName $driver -PortName $port
}}
try {{ Set-PrintConfiguration -PrinterName $name -PaperSize A3 }} catch {{ }}
";
            RunPowerShell(script);
        }

        public static void Uninstall()
        {
            string script = $@"
$ErrorActionPreference = 'Stop'
$name = '{AppPaths.PrinterName}'
$port = '{AppPaths.SpoolFile}'
if (Get-Printer -Name $name -ErrorAction SilentlyContinue) {{ Remove-Printer -Name $name }}
if (Get-PrinterPort -Name $port -ErrorAction SilentlyContinue) {{ Remove-PrinterPort -Name $port }}
";
            RunPowerShell(script);
        }

        /// <summary>
        /// File PDF do spooler (SYSTEM) tạo ra phải được user thường di chuyển/xoá được,
        /// nên cấp Modify cho Users trên thư mục gốc (kế thừa xuống spool, queue).
        /// </summary>
        private static void GrantUsersModify()
        {
            var di = new DirectoryInfo(AppPaths.Root);
            var sec = di.GetAccessControl();
            var users = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
            sec.AddAccessRule(new FileSystemAccessRule(
                users,
                FileSystemRights.Modify,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None,
                AccessControlType.Allow));
            di.SetAccessControl(sec);
        }

        private static void RunPowerShell(string script)
        {
            string encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
            var psi = new ProcessStartInfo("powershell.exe",
                $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -EncodedCommand {encoded}")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardErrorEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8
            };

            using var p = Process.Start(psi) ?? throw new InvalidOperationException("Không chạy được PowerShell.");
            string err = p.StandardError.ReadToEnd();
            p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            if (p.ExitCode != 0)
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(err) ? $"PowerShell lỗi (mã {p.ExitCode})." : err.Trim());
        }
    }
}
