using DrowsinessDetectionServer.Models.DatabaseModels;
using RoleBaseAuthorizationLibrary;

namespace DrowsinessDetectionServer.Datas;

public static class GlobalData
{
    public static List<string> LogDirectories { get; set; } = [];

    public static void SeedUser(ApplicationDbContext dbContext)
    {
        if (dbContext.Users.FirstOrDefault(x => x.Role == Role.Admin) != null) return;
        User seedAdmin = new()
        {
            UserName = "ThaiAn",
            Password = BCrypt.Net.BCrypt.HashPassword("kato@131211#"),
            Role = Role.Admin,
            Email = "daylakato1104@gmail.com",
        };
        dbContext.Users.Add(seedAdmin);
        dbContext.SaveChanges();
    }

    public static void TryGetLogPath(ConfigurationManager config)
    {
        try
        {
            IEnumerable<IConfigurationSection> writeTo = config.GetSection("Serilog:WriteTo").GetChildren();
            foreach (IConfigurationSection logger in writeTo)
            {
                string? path = logger.GetSection("Args:configureLogger:WriteTo:0:Args:path").Value;
                if (!string.IsNullOrEmpty(path))
                {
                    string? directory = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(directory)) LogDirectories.Add(directory);
                }
            }
        }
        catch
        {
            LogDirectories.Clear();
        }
    }
}
