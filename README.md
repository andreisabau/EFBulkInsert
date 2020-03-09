![CI](https://github.com/andreisabau/EFBulkInsert/workflows/CI/badge.svg?branch=master)
## EFBulkInsert
#### Provides an extension method over the Entity Framework DbContext for bulk insertion of entities.

### `BulkInsert<T>(IEnumerable<T> entities, int batchSize = 5000)`

Allows fast insertion of entities in bulk with retrieval of the generated identity column.

### Installation:
##### Package Manger
```
Install-Package EFBulkInsert -Version 0.4.0
```
##### .NET CLI
```
dotnet add package EFBulkInsert --version 0.4.0
```
##### PackageReference
```
<PackageReference Include="EFBulkInsert" Version="0.4.0" />
```


### Usage:

```csharp
dbContext.BulkInsert(entites);

dbContext.BulkInsert(entities, 10000); // bulk insert with batch size of 10000
```

Performance improvements

| # of entities | BulkInsert | EF SaveChanges |
| ------------|------------|----------------|
| 1000        | 127 ms     | 493 ms |
| 5000        | 153 ms     | 2573 ms |
| 10000       | 191 ms     | 4214 ms   |
| 50000       | 485 ms     | 22542 ms  |
| 100000      | 871 ms     | 41374 ms   |
| 500000      | 3916 ms    | N/A  |
| 1000000     | 7769 ms    | N/A  |

Tests made with: SQL Server 2016, EntityFramework 6.1.3 and EFBulkExtensions 0.1.0.

### Version history
##### v0.4.0 (2020-03-09)
 - Added: Support for Entity Framework Core 3.1.0
 
##### v0.3.0 (2018-03-27)
 - Added: Support for schemas
 - Fixed: Inserting into tables without identity columns
 - Fixed: Retrieving the table name from EF metadata
 
##### v0.2.0 (2017-05-17)
 - Fixed: Support for transactions

##### v0.1.0 (2017-01-26)
 - Initial release
