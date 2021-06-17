using System;
using EShoppy.User_Module.Implementation;
using EShoppy.User_Module.Interfaces;
using System.Collections.Generic;
using EShoppy.Finantial_Module.Interfaces;
using EShoppy.User_Module;
using EShoppy.Finantial_Module.Implementation;
using EShoppy.User_Module.Implementation.Mocks;
using EShoppy.NotificationModule.Implementation;
using EShoppy.Finantial_Module;
using System.Linq;
using EShoppy.NotificationModule.Interfaces;
using NSubstitute;
using NUnit.Framework;
using System.Security.Cryptography;

namespace EBazaar.UnitTest.UserModule
{
    [TestFixture]
    public class ClientManagerTests
    {
        public ClientManagerTests()
        {

        }
        [Test]
        public void RegisterUser_ValidTest()
        {
            IClientManager clientManager = new ClientManager();

            ShoppingClient.Clients.Clear();
            
            clientManager.RegisterUser("Marija", "Kovacevic", "mkovacevic@gmail.com", "sirfa1234", "Novi Sad", new DateTime(1997, 5, 6), new List<IAccount>());

            Assert.IsTrue(ShoppingClient.Clients.Count == 1);
        }

        [Test]
        public void RegisterUser_InvalidTest()
        {
            IClientManager clientManager = new ClientManager();

            ShoppingClient.Clients.Clear();
            
            Assert.Throws<ArgumentNullException>(() => clientManager.RegisterUser("Marija", "", "mkovacevic@gmail.com", "sirfa1234", "Novi Sad", new DateTime(1997, 5, 6), new List<IAccount>()));
        }

        [Test]
        public void RegisterOrg_ValidTest()
        {
            IClientManager clientManager = new ClientManager();
            
            ShoppingClient.Clients.Clear();
            
            clientManager.RegisterOrg("TestBar", "31243254325", "Novi Sad", "info@testbar.com", new DateTime(2010, 1, 1), new List<IAccount>());

            Assert.IsTrue(ShoppingClient.Clients.Count == 1);
        }

        [Test]
        public void RegisterOrg_InvalidTest()
        {
            IClientManager clientManager = new ClientManager();
            
            ShoppingClient.Clients.Clear();
            
            Assert.Throws<ArgumentNullException>(() => clientManager.RegisterOrg("TestBar", null, "Novi Sad", "info@test.com", new DateTime(2010, 1, 1), new List<IAccount>()));
        }

        [Test]
        public void ChangeUserAccount_ValidTest()
        {
            IClientManager clientManager = new ClientManager();
            ShoppingClient.Clients.Clear();
            clientManager.RegisterUser("Marija", "Kovacevic", "mkovacevic@gmail.com", "sirfa1234", "Novi Sad", new DateTime(1997, 5, 6), new List<IAccount>());

            List<IAccount> newList = new List<IAccount>();

            IUser user = ShoppingClient.Clients[0] as IUser;

            clientManager.ChangeUserAccount(user, newList);

            Assert.AreEqual(newList, user.ListOfAccounts);
        }

        [Test]
        public void ChangeUserAccount_InvalidTest()
        {
            IClientManager clientManager = new ClientManager();
            ShoppingClient.Clients.Clear();
            clientManager.RegisterUser("Marija", "Kovacevic", "mkovacevic@gmail.com", "sirfa1234", "Novi Sad", new DateTime(1997, 5, 6), new List<IAccount>());

            List<IAccount> newList = new List<IAccount>();

            IUser user = ShoppingClient.Clients[0] as IUser;
            IUser user2 = new FizickoLice("Marija", "Kovacevic", "mkovacevic@gmail.com", "sirfa1234", new DateTime(1997, 5, 6), "Novi Sad", new List<IAccount>());

            Assert.Throws<KeyNotFoundException>(() => clientManager.ChangeUserAccount(user2, newList));
        }

        [Test]
        public void ChangeOrgAccount_ValidTest()
        {
            IClientManager clientManager = new ClientManager();
            ShoppingClient.Clients.Clear();
            clientManager.RegisterOrg("TestBar", "31243254325", "Novi Sad", "info@testbar.com", new DateTime(2010, 1, 1), new List<IAccount>());

            List<IAccount> newList = new List<IAccount>();
            IOrganization org = ShoppingClient.Clients[0] as IOrganization;
            clientManager.ChangeOrgAccount(org, newList);

            Assert.AreEqual(newList, org.ListOfAccounts);
        }

        [Test]
        public void ChangeOrgAccount_InvalidTest()
        {
            IClientManager clientManager = new ClientManager();
            ShoppingClient.Clients.Clear();
            clientManager.RegisterOrg("TestBar", "31243254325", "Novi Sad", "info@testbar.com", new DateTime(2010, 1, 1), new List<IAccount>());

            List<IAccount> newList = new List<IAccount>();
            IOrganization org = ShoppingClient.Clients[0] as IOrganization;
            IOrganization org2 = new PravnoLice("Pravnik", "34532453245", "Novi Sad", "pravnik@gmail.com", DateTime.Now, null);

            
            Assert.Throws<KeyNotFoundException>(() => clientManager.ChangeOrgAccount(org2, newList));
        }

        [Test]
        public void GetClientById_ValidTest()
        {
            IClientManager clientManager = new ClientManager();

            ShoppingClient.Clients.Clear();
            clientManager.RegisterUser("Marija", "Kovacevic", "mkovacevic@gmail.com", "sirfa1234", "Novi Sad", new DateTime(1997, 5, 6), new List<IAccount>());

            Guid id = ShoppingClient.Clients[0].ID;
            IClient client = clientManager.GetClientByID(id);

            Assert.AreEqual(client.ID, id);
        }

        [Test]
        public void AddFunds_InvalidArgumentsTest()
        {
            IClientManager clientManager = new ClientManager();
            
            Assert.Throws<ArgumentNullException>(() => clientManager.AddFunds(null, Guid.NewGuid(), 123, null));
        }

        [Test]
        [TestCase(5000)]
        [TestCase(1000)]
        public void AddFunds_ValidTestFizickoLiceRacun(double amount)
        {
            IFinanceManager financeManager = new FinanceManager();
            IClientManager clientManager = new ClientManager(financeManager, new EmailSenderMock());
            FinantialDB.Accounts.Clear();
            financeManager.CreateAccount("7456745885679", new Bank(), 1500, 0, false);
            var account = FinantialDB.Accounts.Values.ToList()[0];
            IClient client = new FizickoLice("Marko", "Markovic","markom@gmail.com", "marko123", DateTime.Now, "Novi Sad", new List<IAccount>() { account });
            ICurrency currency = FinantialDB.Currency["EUR"];

            double oldAmount = account.Balance;

            clientManager.AddFunds(client, account.ID, amount, currency);

            Assert.AreEqual(oldAmount + amount * currency.Value, account.Balance);
        }

        [Test]
        [TestCase(50000)]
        [TestCase(10000)]
        public void AddFunds_ValidTestFizickoLiceKredit(double amount)
        {
            IFinanceManager financeManager = new FinanceManager();
            IClientManager clientManager = new ClientManager(financeManager, new EmailSenderMock());
            financeManager.CreateAccount("7456745885679", new Bank(), 1500, 0, true);
            var account = FinantialDB.Accounts.Values.ToList()[0];
            IClient client = new PravnoLice("Pravnik", "34532453245", "Novi Sad", "pravnik@gmail.com", DateTime.Now, null);
            ICurrency currency = FinantialDB.Currency["EUR"];

            double oldAmount = account.CreditPayment;
            clientManager.AddFunds(client, account.ID, amount, currency);

            Assert.IsTrue(oldAmount > account.CreditPayment);
        }

        [Test]
        [TestCase(50000)]
        [TestCase(10000)]
        public void AddFunds_InvalidTestClientDb(double amount)
        {
            IFinanceManager financeManager = new FinanceManager();
            IClientManager clientManager = new ClientManager(financeManager, new EmailSenderMock());
            financeManager.CreateAccount("7456745885679", new Bank(), 1500, 0, true);
            var account = FinantialDB.Accounts.Values.ToList()[0];
            IClient client = new PravnoLice("Pravnik", "34532453245", "Novi Sad", "pravnik@gmail.com", DateTime.Now, null);
            ICurrency currency = FinantialDB.Currency["EUR"];

            double oldAmount = account.CreditPayment;

            Assert.Throws<KeyNotFoundException>(() => clientManager.AddFunds(client, account.ID, amount, currency), "Korisnicki nalog nije nadjen u bazi.");
        }

        [Test]
        [TestCase(50000)]
        [TestCase(10000)]
        public void AddFunds_InvalidTestDb(double amount)
        {
            IFinanceManager financeManager = new FinanceManager();
            IClientManager clientManager = new ClientManager(financeManager, new EmailSenderMock());
            IAccount account = new Account("7456745885679", new Bank(), 1500, 0, true);
            IClient client = new PravnoLice("Pravnik", "34532453245", "Novi Sad", "pravnik@gmail.com", DateTime.Now, null);
            ICurrency currency = FinantialDB.Currency["EUR"];

            double oldAmount = account.CreditPayment;
            
            Assert.Throws<KeyNotFoundException>(() => clientManager.AddFunds(client, account.ID, amount, currency), "Racun nije nadjen u bazi.");
        }

        [Test]
        [TestCase(500)]
        [TestCase(100)]
        public void AddFunds_ValidTestPravnoLiceKredit(double amount)
        {
            IFinanceManager financeManager = new FinanceManager();
            IEmailSender emailSender = Substitute.For<IEmailSender>();
            IClientManager clientManager = new ClientManager(financeManager, emailSender);
            FinantialDB.Accounts.Clear();
            financeManager.CreateAccount("7456745885679", new Bank(), 150000, 500000, true);
            var account = FinantialDB.Accounts.Values.ToList()[0];
            IClient client = new PravnoLice("Pravnik", "34532453245", "Novi Sad", "pravnik@gmail.com", DateTime.Now, new List<IAccount>() { account });
            ICurrency currency = FinantialDB.Currency["EUR"];


            double oldAmount = account.CreditPayment;

            clientManager.AddFunds(client, account.ID, amount, currency);
            emailSender.Received();
            Assert.IsTrue(oldAmount > account.CreditPayment);
        }

        [Test]
        [TestCase(500)]
        [TestCase(1000)]
        public void AddFunds_ValidTestPravnoLiceRacun(double amount)
        {
            IFinanceManager financeManager = new FinanceManager();
            IClientManager clientManager = new ClientManager(financeManager, new EmailSenderMock());
            FinantialDB.Accounts.Clear();
            financeManager.CreateAccount("7456745885679", new Bank(), 1500, 0, false);
            var account = FinantialDB.Accounts.Values.ToList()[0];
            IClient client = new PravnoLice("Pravnik", "34532453245", "Novi Sad", "pravnik@gmail.com", DateTime.Now, new List<IAccount>() { account });
            ICurrency currency = FinantialDB.Currency["EUR"];


            double oldAmount = account.Balance;
            clientManager.AddFunds(client, account.ID, amount, currency);

            Assert.AreEqual(oldAmount + amount * currency.Value, account.Balance);
        }

        [Test]
        [TestCase(5000)]
        [TestCase(10)]
        [TestCase(0)]
        public void AddFunds_InvalidTestPravnoLiceRacun(double amount)
        {
            IFinanceManager financeManager = new FinanceManagerMock();
            IClientManager clientManager = new ClientManager(financeManager, new EmailSenderMock());
            IClient client = new PravnoLice("Pravnik", "34532453245", "Novi Sad", "pravnik@gmail.com", DateTime.Now, new List<IAccount>());

            Assert.Throws<Exception>(() => clientManager.AddFunds(client, Guid.NewGuid(), amount, new CurrencyStub()), "Iznos mora biti veci 10000 RSD.");
        }
    }
}
