using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using NUnit.Framework;

namespace ECommerceTests.Tests
{
    [TestFixture]
    public class RegressionTests : IDisposable
    {
        private IWebDriver? driver;
        private WebDriverWait? wait;

        [SetUp]
        public void Setup()
        {
            Console.WriteLine("Starting Chrome (VISIBLE) with remote debugging...");

            var service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;

            var options = new ChromeOptions();
            // REMOVE --headless=new
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-extensions");
            options.AddArgument("--remote-debugging-port=9292"); // Critical for localhost

            driver = new ChromeDriver(service, options);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15)); // Increased timeout
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Dispose();
        }

        public void Dispose() => TearDown();

        [Test]
        [Description("UAT-001: Valid user can login and complete checkout")]
        public void Checkout_Success()
        {
            driver!.Navigate().GoToUrl("http://localhost:5050");

            var login = wait!.Until(d => d.FindElement(By.Id("login")));
            login.Clear();
            login.SendKeys("test@user.com");

            var password = driver.FindElement(By.Id("password"));
            password.Clear();
            password.SendKeys("Pass123!");

            driver.FindElement(By.Id("submit")).Click();

            // HANDLE THE ALERT
            wait.Until(d =>
            {
                try { d.SwitchTo().Alert(); return true; }
                catch (NoAlertPresentException) { return false; }
            });
            // HANDLE THE SECOND ALERT
            wait.Until(d =>
            {
                try { d.SwitchTo().Alert(); return true; }
                catch (NoAlertPresentException) { return false; }
            });
            driver.SwitchTo().Alert().Accept();

            // HANDLE THE SECOND ALERT
            wait.Until(ExpectedConditions.AlertIsPresent());
            driver.SwitchTo().Alert().Accept();

            driver.FindElement(By.Id("checkout")).Click();

            wait.Until(d => d.PageSource.Contains("Order Confirmed"));

            Assert.That(driver.PageSource.Contains("Order Confirmed"), Is.True);
        }   
    }
}