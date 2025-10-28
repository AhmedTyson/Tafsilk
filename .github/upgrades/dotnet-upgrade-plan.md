# .NET9.0 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that an .NET9.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET9.0 upgrade.
3. Upgrade TafsilkPlatform.Core\TafsilkPlatform.Core.csproj
4. Upgrade TafsilkPlatform.Application\TafsilkPlatform.Application.csproj
5. Upgrade TafsilkPlatform.Infrastructure\TafsilkPlatform.Infrastructure.csproj
6. Upgrade TafsilkPlatform.Web\TafsilkPlatform.Web.csproj

## Settings

This section contains settings and data used by execution steps.

### Excluded projects

Table below contains projects that do belong to the dependency graph for selected projects and should not be included in the upgrade.

| Project name | Description |
|:-----------------------------------------------|:---------------------------:|

### Aggregate NuGet packages modifications across all projects

NuGet packages used across all selected projects or their dependencies that need version update in projects that reference them.

| Package Name | Current Version | New Version | Description |
|:------------------------------------|:---------------:|:-----------:|:----------------------------------------------|
| Microsoft.EntityFrameworkCore.SqlServer |9.0.9 |9.0.10 | Recommended for .NET9.0 |
| Microsoft.EntityFrameworkCore.Tools |9.0.9 |9.0.10 | Recommended for .NET9.0 |

### Project upgrade details
This section contains details about each project upgrade and modifications that need to be done in the project.

#### TafsilkPlatform.Core\TafsilkPlatform.Core.csproj modifications

Project properties changes:
 - Target framework should be changed from `net8.0` to `net9.0`

#### TafsilkPlatform.Application\TafsilkPlatform.Application.csproj modifications

Project properties changes:
 - Target framework should be changed from `net8.0` to `net9.0`

#### TafsilkPlatform.Infrastructure\TafsilkPlatform.Infrastructure.csproj modifications

Project properties changes:
 - Target framework should be changed from `net8.0` to `net9.0`

NuGet packages changes:
 - Microsoft.EntityFrameworkCore.SqlServer should be updated from `9.0.9` to `9.0.10` (recommended for .NET9.0)
 - Microsoft.EntityFrameworkCore.Tools should be updated from `9.0.9` to `9.0.10` (recommended for .NET9.0)

#### TafsilkPlatform.Web\TafsilkPlatform.Web.csproj modifications

Project properties changes:
 - Target framework should be changed from `net8.0` to `net9.0`
