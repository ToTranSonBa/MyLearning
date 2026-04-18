# Database Migration & Setup Commands

## ?? Table of Contents
1. [Initial Setup](#initial-setup)
2. [Migration Commands](#migration-commands)
3. [Troubleshooting](#troubleshooting)
4. [Rollback](#rollback)
5. [Verification](#verification)

---

## ?? Initial Setup

### Step 1: Install EF Core Tools (if not already installed)

```powershell
# Install globally
dotnet tool install --global dotnet-ef

# Or update if already installed
dotnet tool update --global dotnet-ef
```

### Step 2: Verify Installation

```powershell
dotnet ef --version
```

Expected output: `Entity Framework Core .NET Command-line Tools 10.0.5` or similar

### Step 3: Navigate to Project Directory

```powershell
# Navigate to solution root
cd D:\MyLearning\back-end\MyLeaning\

# Or to Web project
cd D:\MyLearning\back-end\MyLeaning\Web\
```

---

## ?? Migration Commands

### Using Package Manager Console (Visual Studio)

```powershell
# 1. Add new migration
Add-Migration AddAuthenticationFieldsUpdate

# 2. Update database
Update-Database

# 3. Remove last migration (if mistake)
Remove-Migration

# 4. List migrations
Get-Migration
```

### Using .NET CLI (Command Line)

```powershell
# 1. Navigate to Web or Infrastructure.SqlServer project
cd Web
# or
cd Infrastructure.SqlServer

# 2. Add new migration
dotnet ef migrations add AddAuthenticationFieldsUpdate

# 3. Update database
dotnet ef database update

# 4. Remove last migration
dotnet ef migrations remove

# 5. List migrations
dotnet ef migrations list
```

---

## ?? Available Migrations

### Existing Migrations

1. **20260414165024_InitialCreate**
   - Initial database schema
   - Users, Roles tables

2. **20260415000000_AddAuthenticationFieldsUpdate**
   - Authentication fields
   - Password hash, refresh token, etc.

### How to Apply

```powershell
# Apply all pending migrations
Update-Database

# Apply up to specific migration
Update-Database -Migration "20260415000000_AddAuthenticationFieldsUpdate"

# Apply to target database
Update-Database -Migration "20260415000000_AddAuthenticationFieldsUpdate" -Context ApplicationDbContext
```

---

## ?? Check Database Status

### View Pending Migrations

```powershell
# See what migrations haven't been applied
Get-Migration -Status Pending
```

### View Applied Migrations

```powershell
# See all migrations already applied
Get-Migration -Status Applied
```

### Current Database Version

```powershell
# Show latest applied migration
Get-Migration | Select-Object -Last 1
```

---

## ? Troubleshooting

### Issue 1: "No migrations found"

**Cause:** Migration folder or files missing

**Solution:**
```powershell
# Check if Migrations folder exists
# If not, create: Infrastructure.SqlServer\Migrations\

# Verify migration file exists:
# Infrastructure.SqlServer\Migrations\20260415000000_AddAuthenticationFieldsUpdate.cs

# Recreate if needed:
Add-Migration AddAuthenticationFieldsUpdate
```

### Issue 2: "Could not find DbContext"

**Cause:** EF Core can't find your DbContext

**Solution:**
```powershell
# Specify context explicitly
Add-Migration AddAuthenticationFieldsUpdate -Context ApplicationDbContext

Update-Database -Context ApplicationDbContext
```

### Issue 3: "Could not connect to database"

**Cause:** Connection string issue or SQL Server not running

**Solution:**
```powershell
# Check connection string in appsettings.json
# Verify SQL Server is running

# Example valid connection string:
# "Server=localhost;Database=MyLearning;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"

# Test connection from SSMS first
```

### Issue 4: "Migration already exists"

**Cause:** Trying to add migration with same name

**Solution:**
```powershell
# Use different name
Add-Migration AddAuthenticationFieldsUpdate_v2

# Or remove existing first
Remove-Migration
```

### Issue 5: "The type initializer threw an exception"

**Cause:** Configuration or dependency issues

**Solution:**
```powershell
# Clean build
dotnet clean
dotnet build

# Then try again
Add-Migration AddAuthenticationFieldsUpdate
```

---

## ?? Rollback

### Rollback Last Migration

```powershell
# Remove from database
Update-Database -Migration "20260414165024_InitialCreate"

# Remove migration file
Remove-Migration
```

### Rollback Specific Migration

```powershell
# Rollback to specific version
Update-Database -Migration "20260414165024_InitialCreate"
```

### Rollback All Migrations

```powershell
# WARNING: This deletes database!
Update-Database -Migration 0
```

---

## ? Verification

### Verify Migration Applied

```powershell
# Get all applied migrations
Get-Migration

# Should show:
# InitialCreate (Applied)
# AddAuthenticationFieldsUpdate (Applied)
```

### Verify Database Tables

```powershell
# Using SSMS (SQL Server Management Studio):
# 1. Connect to localhost
# 2. Expand MyLearning database
# 3. Expand Tables
# 4. Look for Users table with new columns:
#    - PasswordHash
#    - RefreshToken
#    - PasswordResetToken
#    - LastLoginAt
#    - IsEmailConfirmed
#    - CreatedAt
#    - UpdatedAt
```

### SQL Query to Verify

```sql
-- Run in SQL Server Query Editor
USE MyLearning;

-- Check Users table structure
EXEC sp_columns 'Users';

-- Should show authentication columns
-- PasswordHash
-- RefreshToken
-- PasswordResetToken
-- PasswordResetTokenExpiryTime
-- LastLoginAt
-- IsEmailConfirmed
-- CreatedAt
-- UpdatedAt
```

---

## ??? Common Workflows

### Workflow 1: Initial Setup (Fresh Database)

```powershell
# 1. Ensure DB doesn't exist
# 2. Build solution
dotnet build

# 3. Apply all migrations
Update-Database

# 4. Verify in SSMS
# Tables should be created
```

### Workflow 2: Fix Migration Mistake

```powershell
# 1. Remove incorrect migration
Remove-Migration

# 2. Rollback database
Update-Database -Migration "20260414165024_InitialCreate"

# 3. Create correct migration
Add-Migration AddAuthenticationFieldsUpdate

# 4. Apply
Update-Database
```

### Workflow 3: Add New Fields to Model

```powershell
# 1. Modify entity (e.g., Users.cs)
# Add new property:
# public string? NewField { get; set; }

# 2. Create migration
Add-Migration AddNewFieldToUsers

# 3. Verify migration looks correct

# 4. Apply
Update-Database

# 5. Verify in database
```

### Workflow 4: Deploy to Production

```powershell
# 1. Review all migrations
Get-Migration

# 2. Create migration script
# For manual deployment
dotnet ef migrations script

# 3. Or auto-apply (if you trust your migrations)
Update-Database

# 4. Backup database first (CRITICAL!)
# Use SSMS to backup MyLearning database
```

---

## ?? Full Migration Example

### Step-by-Step Example: Adding New Field

```powershell
# Step 1: Add property to domain model
# File: Domain\Identity\Users.cs
# Add: public string? PhoneNumber { get; set; }

# Step 2: Create migration
Add-Migration AddPhoneNumberToUsers

# Step 3: Review generated migration
# File: Infrastructure.SqlServer\Migrations\[timestamp]_AddPhoneNumberToUsers.cs
# Should contain:
# - Up(): Add PhoneNumber column
# - Down(): Drop PhoneNumber column

# Step 4: Apply migration
Update-Database

# Step 5: Verify
# Run SQL:
# SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
# WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'PhoneNumber'

# Step 6: Update application code
# File: Web\Controllers\AuthController.cs
# Add phoneNumber parameter to register

# Step 7: Test
# Run application and test registration with phone number
```

---

## ?? Pre-Migration Checklist

Before running migrations, verify:

- [ ] Visual Studio is open with solution loaded
- [ ] Build succeeded (no compilation errors)
- [ ] SQL Server is running (check Services or SSMS)
- [ ] Connection string in appsettings.json is correct
- [ ] Database exists or will be created automatically
- [ ] No other instance of application using database
- [ ] You have backup of important data
- [ ] Migration files are in correct location
- [ ] Package Manager Console is open and in correct project

---

## ?? Quick Commands Reference

```powershell
# Most Common Commands
Add-Migration <name>                           # Create migration
Update-Database                                 # Apply all pending
Update-Database -Migration <name>               # Apply specific
Remove-Migration                                # Remove last
Get-Migration                                   # List all
Get-Migration -Status Pending                   # Pending only
Update-Database -Migration 0                    # Remove all (!!!)

# .NET CLI Equivalents
dotnet ef migrations add <name>
dotnet ef database update
dotnet ef migrations list
dotnet ef migrations remove
```

---

## ?? Support

If you encounter issues:

1. Check [Troubleshooting](#troubleshooting) section
2. Review error message carefully
3. Check connection string in appsettings.json
4. Verify SQL Server is running
5. Check authentication fields in Users table
6. Review migration files for issues

---

## ? After Migration

Once migrations are applied:

1. ? Database schema updated with auth fields
2. ? Application can use new features
3. ? Authentication system ready
4. ? Unit of Work pattern active
5. ? Ready for testing

### Next Steps:

```powershell
# 1. Run application
dotnet run --project Web

# 2. Test authentication endpoints
# POST /api/auth/register
# POST /api/auth/login
# POST /api/auth/logout

# 3. Verify database records
# Check Users table for new entries
```

---

## ?? Additional Resources

- EF Core Migrations: https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/
- SQL Server Documentation: https://learn.microsoft.com/en-us/sql/
- Project Migration Files: `Infrastructure.SqlServer/Migrations/`

---

**Migration Setup Complete!** ??

