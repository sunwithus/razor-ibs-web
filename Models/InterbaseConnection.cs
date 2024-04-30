using Microsoft.AspNetCore.Mvc;

namespace RazorInterbaseWeb.Models
{
    public class InterbaseConnection
    {
        public string User { get; set; } = "SYSDBA";
        public string Password { get; set; } = "masterkey";
        public string Database { get; set; } = "c:\\dotnet\\db38.ibs";
        public string DataSource { get; set; } = "localhost";
        public string Port { get; set; } = "3050";

    }
}
