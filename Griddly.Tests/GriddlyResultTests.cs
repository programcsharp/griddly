﻿using Griddly.Mvc.Results;
using System.Data.SQLite;
using Dapper;

namespace Griddly.Tests
{
    [TestClass]
    public sealed class GriddlyResultTests
    {
        private SQLiteConnection CreateTestDatabase(string databaseName)
        {
            SQLiteConnection.CreateFile($"{databaseName}.sqlite");
            var conn = new SQLiteConnection($"Data Source={databaseName}.sqlite;Version=3;");
            conn.Open();

            conn.Execute("CREATE TABLE TestTable (Id bigint, Name varchar(50))");
            conn.Execute("INSERT INTO TestTable (Id, Name) values (1, 'Test1')");
            conn.Execute("INSERT INTO TestTable (Id, Name) values (2, 'Test2')");
            conn.Execute("INSERT INTO TestTable (Id, Name) values (3, 'Test3')");

            return conn;
        }

        [TestMethod]
        public void DapperResult_GetAllForProperty_Filters()
        {
            using (var conn = CreateTestDatabase("DapperResult_GetAllForProperty_Filters"))
            {
                //Arrange
                var dapperResult = new DapperSql2012Result<object>(() => conn, "SELECT Id, Name from TestTable", null, null);

                //Act
                var results = dapperResult.GetAllForProperty<long>("Id", null, new long[] { 1, 3 }).ToList();

                //Assert
                Assert.AreEqual(2, results.Count);
                Assert.IsTrue(results.Contains(1));
                Assert.IsTrue(results.Contains(3));
            }
        }


        [TestMethod]
        public void QueryableResult_GetAllForProperty_Filters()
        {
            //Arrange
            var data = new Model[] { new Model { Id = 1, Name = "Test1" }, new Model { Id = 2, Name = "Test2" }, new Model { Id = 3, Name = "Test3" } };
            var query = data.AsQueryable();
            var queryableResult = new QueryableResult<object>(query, null);

            //Act
            var results = queryableResult.GetAllForProperty<long>("Id", null, new long[] { 1, 3 }).ToList();

            //Assert
            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.Contains(1));
            Assert.IsTrue(results.Contains(3));
        }

        private class Model
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }
    }
}
