-- convert UTC to UnixTime ms
select DATEDIFF_BIG(MILLISECOND, '1970-01-01', GETUTCDATE())

-- convert UTC to Local
select DATEADD(HOUR, DATEDIFF(HOUR, GETUTCDATE(), GETDATE()), CONVERT(DATETIME2, '2023-02-17 10:50:40Z'))

-- convert Local to UTC
select DATEADD(HOUR, DATEDIFF(HOUR, GETDATE(), GETUTCDATE()), CONVERT(DATETIME2, '2023-02-17 10:50:40'))

-- https://stackoverflow.com/a/7389437/924810
-- convert sql getdate to .NET Datetime Ticks
declare @ticksPerDay bigint = 864000000000
declare @date2 datetime2 = '2023-11-01'

declare @dateBinary binary(9) = cast(reverse(cast(@date2 as binary(9))) as binary(9))
declare @days bigint = cast(substring(@dateBinary, 1, 3) as bigint)
declare @time bigint = cast(substring(@dateBinary, 4, 5) as bigint)

select @date2 as [DateTime2], @days * @ticksPerDay + @time as [Ticks]
