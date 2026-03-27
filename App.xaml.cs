using System.Windows;
using Microsoft.EntityFrameworkCore;
using Sensore.Data;
using Sensore.Services;
using Sensore.ViewModels;
using System.IO;

namespace Sensore
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var connectionString =
                Environment.GetEnvironmentVariable("SENSORE_CONNECTION")
                ?? "Server=localhost;Port=3306;Database=sensore_db;User=sensore_user;Password=ChangeMe!123;";

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                .Options;

            var dbContext = new AppDbContext(options);
            try
            {
                dbContext.Database.Migrate();
                RunStartupHealthChecks(dbContext);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Startup health checks failed.\n{ex.Message}\n\nSet SENSORE_CONNECTION and restart.",
                    "Sensore Startup",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                Shutdown(-1);
                return;
            }

            var pressureAnalysisService = new PressureAnalysisService();
            var alertService = new AlertService(dbContext);
            var csvImportService = new CsvImportService(dbContext, pressureAnalysisService, alertService);
            var patientFrameQueryService = new PatientFrameQueryService(dbContext);
            var authService = new AuthService(dbContext);
            var reportService = new ReportService(dbContext);
            var mainViewModel = new MainViewModel(authService, csvImportService, alertService, patientFrameQueryService, reportService);

            var window = new MainWindow(mainViewModel);
            window.Show();
        }

        private static void RunStartupHealthChecks(AppDbContext dbContext)
        {
            if (!dbContext.Database.CanConnect())
            {
                throw new InvalidOperationException("Cannot connect to MySQL database.");
            }

            dbContext.Database.ExecuteSqlRaw("SELECT 1");

            var pending = dbContext.Database.GetPendingMigrations().ToList();
            if (pending.Count > 0)
            {
                throw new InvalidOperationException($"Schema version is not current. Pending migrations: {string.Join(", ", pending)}");
            }

            var appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sensore");
            Directory.CreateDirectory(appDataDir);
            var testFile = Path.Combine(appDataDir, ".startup-write-test");
            File.WriteAllText(testFile, "ok");
            File.Delete(testFile);
        }
    }

}
