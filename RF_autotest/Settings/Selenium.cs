using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RF_autotest.Settings
{
    public class Selenium
    {
        private IWebDriver _driver=null;
        public IWebDriver Driver;

        Selenium()
        {
            if (_driver == null)
            {
                _driver = new ChromeDriver();
                _driver.Manage().Cookies.DeleteAllCookies();
                _driver.Manage().Window.Maximize();
            }

        }
            
        
        public IWebDriver GetDriver()
        {
            Driver = _driver;
            return Driver;
        }
    }
}
