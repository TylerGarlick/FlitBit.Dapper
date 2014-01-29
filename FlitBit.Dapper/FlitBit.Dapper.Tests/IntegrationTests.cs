using System;
using System.Data;
using System.Linq;
using Dapper;
using FlitBit.Dapper.Tests.Infrastructure;
using FlitBit.Dto;
using FlitBit.IoC;
using Xunit;

namespace FlitBit.Dapper.Tests
{
    [DTO]
    public interface ICustomer
    {
        int Customer_Id { get; set; }
    }

    public class Customer
    {
        public int Customer_Id { get; set; }
    }

    [DTO]
    public interface ICustomerProper
    {
        int CustomerId { get; set; }
    }

    [DTO]
    public interface ICustomerFailed
    {
        Guid CustomerId { get; set; }
    }

    public class IntegrationTests : AbstractTest
    {
        [Fact]
        public void CanReadFromExistingSqlDatabase()
        {
            using (Create.SharedOrNewContainer())
            using (var connection = Db.GetConnectionAndOpen(ConnectionString))
            {
                var customers = connection.Query<ICustomer>("Select top 10 * from customer").ToList();
                Assert.Equal(10, customers.Count());

                for (int i = 0; i < customers.Count(); i++)
                    Assert.True(customers.ElementAt(i).Customer_Id > 0);


                var customers2 = connection.Query<Customer>("Select top 10 * from customer").ToList();
                Assert.Equal(10, customers2.Count());

                for (int i = 0; i < customers2.Count(); i++)
                    Assert.True(customers2.ElementAt(i).Customer_Id > 0);

            }
        }

        [Fact]
        public void ShouldTranslateColumnNameToCamelCasing()
        {
            using (Create.SharedOrNewContainer())
            using (var connection = Db.GetConnectionAndOpen(ConnectionString))
            {
                var customers = connection.Query<ICustomerProper>("Select top 10 * from customer").ToList();
                Assert.Equal(10, customers.Count());

                for (int i = 0; i < customers.Count(); i++)
                    Assert.True(customers.ElementAt(i).CustomerId > 0);
            }
        }

        [Fact]
        public void ShouldThrowErrorIfColumnIsNotTypedProperly()
        {
            using (Create.SharedOrNewContainer())
            using (var connection = Db.GetConnectionAndOpen(ConnectionString))
            {
                Assert.Throws<DataException>(() =>
                                             {
                                                 connection.Query<ICustomerFailed>("Select top 10 * from customer");
                                             });
            }
        }

    }
}
