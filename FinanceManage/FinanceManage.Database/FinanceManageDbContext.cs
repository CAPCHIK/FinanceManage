﻿using FinanceManage.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace FinanceManage.Database
{
    public class FinanceManageDbContext: DbContext
    {
        public FinanceManageDbContext(DbContextOptions<FinanceManageDbContext> options): base(options)
        {

        }
        public DbSet<Purchase> Purchases { get; set; }
    }
}
