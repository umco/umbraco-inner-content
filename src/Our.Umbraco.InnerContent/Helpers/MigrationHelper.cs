using System;
using System.Linq;
using System.Web;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Services;

namespace Our.Umbraco.InnerContent.Helpers
{
    public static class MigrationHelper
    {
        public static void ApplyMigrations(
            ApplicationContext applicationContext,
            string migrationName,
            SemVersion currentVersion,
            SemVersion targetVersion)
        {
            // get the latest migration executed
            var latestMigration = applicationContext.Services.MigrationEntryService.GetLatest(migrationName);

            if (latestMigration != null)
                currentVersion = latestMigration.Version;

            if (targetVersion == currentVersion)
                return;

            var migrationsRunner = new MigrationRunner(
                applicationContext.Services.MigrationEntryService,
                applicationContext.ProfilingLogger.Logger,
                currentVersion,
                targetVersion,
                migrationName);

            try
            {
                migrationsRunner.Execute(applicationContext.DatabaseContext.Database);
            }
            catch (HttpException)
            {
                // because umbraco runs some other migrations after the migration runner
                // is executed we get HttpException
                // catch this error, but don't do anything
                // fixed in 7.4.2+ see : http://issues.umbraco.org/issue/U4-8077
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(MigrationHelper), "Error running migration.", ex);
            }
        }

        private static IMigrationEntry GetLatest(this IMigrationEntryService service, string migrationName)
        {
            // TODO: Figure out if we can use the UnitOfWorkProvider here,
            // but it appears to be marked as protected within Umbraco Core.

            return service
                .GetAll(migrationName)
                .OrderByDescending(x => x.Version)
                .FirstOrDefault();
        }
    }
}