# Database Schema Fix: Role Column Size

## Issue
**Error**: "Database error: Data truncated for column 'role' at row 1"

## Root Cause
The `role` column in the `engineers` table is too small (likely CHAR(1), VARCHAR(3), or VARCHAR(5)) to store the longer enum values like "ACCOUNTANT".

Current enum values:
- `ADMIN` (5 chars)
- `JEN` (3 chars)
- `AEN` (3 chars)
- `XEN` (3 chars)
- `ACCOUNTANT` (9 chars) ← **This exceeds small column sizes**

## Solution Applied

### 1. Code Configuration Updated
**File**: `Data/BdaDbContext.cs` (Line 29)

**Before**:
```csharp
e.Property(x => x.Role).HasColumnName("role").HasConversion<string>();
```

**After**:
```csharp
e.Property(x => x.Role).HasColumnName("role").HasMaxLength(20).HasConversion<string>();
```

This tells EF Core that the role column should support strings up to 20 characters.

### 2. Database Migration (Choose One Option)

#### **Option A: Using EF Core Migrations** (Recommended)
```bash
cd "C:\Users\IT Cell\source\repos\Work-Dashboard"
dotnet ef migrations add IncreaseRoleColumnSize
dotnet ef database update
```

#### **Option B: Manual SQL Script** (If migrations fail)
Execute this SQL on your MySQL database:

```sql
-- For MySQL
ALTER TABLE engineers MODIFY COLUMN role VARCHAR(20) NOT NULL DEFAULT 'JEN';

-- Verify the change
DESCRIBE engineers;
```

**For MariaDB**:
```sql
ALTER TABLE engineers CHANGE COLUMN role role VARCHAR(20) NOT NULL DEFAULT 'JEN';
```

## Verification

After applying the fix, try adding a user with role "ACCOUNTANT":

1. Login as Admin
2. Go to Users section
3. Click "Add User"
4. Fill in:
   - Full Name: `PANKAJ CHAUDHARY`
   - Username: `pankaj.chaudhary`
   - Password: `[your password]`
   - Role: `Accountant` ← Should now work!
   - Mobile: `[optional]`
5. Click "Save"

✅ Should save successfully without the truncation error.

## Related Changes

### Code Configuration
- **File**: `Data/BdaDbContext.cs`
- **Change**: Added `.HasMaxLength(20)` to Role property
- **Impact**: Ensures EF Core enforces column constraints correctly

### Database Schema
- **Table**: `engineers`
- **Column**: `role`
- **Old Size**: ~3-5 characters (too small)
- **New Size**: 20 characters (supports all current and future roles)

## Future-Proofing

If new roles are added in the future, they will now be supported as long as they are less than 20 characters. For example:
- `PROJECTMANAGER` (14 chars) ✅
- `DATAANALYST` (11 chars) ✅
- `QUALITYASSURANCE` (15 chars) ✅

All current and planned roles fit within the 20-character limit.

## Testing Checklist

- [ ] Build project successfully (`dotnet build`)
- [ ] Run migrations: `dotnet ef database update`
- [ ] Try adding user with "Accountant" role
- [ ] Try adding user with other roles (JEN, AEN, XEN, ADMIN)
- [ ] Edit existing users and verify role field works
- [ ] Check database: `SELECT COLUMN_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='engineers' AND COLUMN_NAME='role';`

---

**Status**: ✅ Code Fix Applied  
**Next Step**: Apply database migration  
**Estimated Impact**: User management will work correctly with all role types
