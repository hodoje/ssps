﻿namespace BankService.Migrations.DomainContext
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class DomainConfiguration : DbMigrationsConfiguration<BankService.DatabaseManagement.BankDomainContext>
    {
        public DomainConfiguration()
        {
            MigrationsDirectory = @"Migrations\DomainContext";
        }

        protected override void Seed(BankService.DatabaseManagement.BankDomainContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.
        }
    }
}
