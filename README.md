 [![andreisabau MyGet Build Status](https://www.myget.org/BuildSource/Badge/andreisabau?identifier=0dd80dc9-0ef7-4b80-a451-42ca280d27f0)](https://www.myget.org/)
## EFBulkInsert
#### Provides an extension method over the Entity Framework DbContext for bulk insertion of entities.

### `BulkInsert<T>(IEnumerable<T> entities, int batchSize = 5000)`

Allows fast insertion of entities in bulk with retrieval of the generated identity column.

Usage:

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

Tests made with: SQL Server 2016, EntityFramework 6.1.3 and EFBulkExtensions 0.1.0;
