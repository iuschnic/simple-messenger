using Dapper;
using DB.Database;
using DB.Tests;

SqlMapper.AddTypeHandler(new GuidTypeHandler());

IntegrationTests.RunAll();