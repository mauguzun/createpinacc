using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.PhantomJS;

namespace MultiThreadExample
{
    public partial class Form1 : Form
    {
        static int number = 0;

        private int _threadCount = 5;

        List<string> _proxy;
        List<string> _email;
        List<string> _names;
        List<string> _goodProxy;

        string _proxyFileName = "proxy.txt";
        string _emailFileName = "acc.txt";

        List<ThreadAndDriver> th;


        private int waitTask = 180000;

        public Form1()
        {
            InitializeComponent();
            _LoadSettings();
            _CloseProccess();

        }

        private void _LoadSettings()
        {
            this._LoadNames();
            try
            {
                _proxy = new GetProxy.ProxyReader().GetList();
                _email = File.ReadAllLines(_emailFileName).ToList();
                _goodProxy = new List<string>();
                con.Text += "All done" + Environment.NewLine; ;
                //_NotImported();
                File.WriteAllLines(this._emailFileName, _email.ToArray());
            }
            catch
            {
                con.Text += $"not loaded email && proxy " + Environment.NewLine; ;
            }
        }

        private void makeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            while (true)
            {

                th = new List<ThreadAndDriver>();

                for (int i = 0; i < this._threadCount; i++)
                {
                    string proxy = this._ProxyGet();

                    Thread th = new Thread(Make);
                    th.Name = "MAKE AKK";


                    ThreadAndDriver tandd = new ThreadAndDriver()
                    {
                        Driver = OpenAndReturnDriver(proxy),
                        Proxy = proxy,
                        Thread = th,
                    };

                    th.Start(tandd);

                    //_tasks.Add(new Thread(Make));
                    //_tasks[i].Start(_driver);
                }
                Thread.Sleep(this.waitTask);

                this._CloseAll();
                _CloseProccess();
            }


        }
        private void _NotImported()
        {
            var files = Directory.GetFiles(System.AppDomain.CurrentDomain.BaseDirectory);

            foreach (var file in files)
            {
                if (file.Contains("not_imported"))
                {
                    var lines = File.ReadAllLines(file);
                    foreach (string line in lines)
                    {
                        this._email.Add(line.Replace(":trance12", ""));
                    }
                    File.Delete(file);
                }
            }
        }

        private void _CloseAll()
        {
            foreach (var t in th)
            {
                try { t.Driver.Quit(); } catch { }
                try { t.Thread.Abort(); } catch { }


            }

        }
        private string _ProxyGet()
        {
            string proxy;
            if (_proxy.Count > 1)
            {
                _proxy.RemoveAt(0);
                proxy = _proxy[0];
            }
            else
            {



                proxy = _goodProxy.Randomize().FirstOrDefault();

            }
            return proxy;
        }

        private void Make(object state)
        {

            var td = (ThreadAndDriver)state;
            string email = _email[number];
            number++;

            ChromeDriver driver = td.Driver; 
            
            driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 30);
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(50));



            try
            {
                driver.Url = "http://pinterest.com/logout";
                driver.FindElementByXPath("//input[@name='id']").SendKeys(email);
                driver.FindElementByXPath("//input[@name='password']").SendKeys("trance12");


                //  if we have special age
                bool ageRequrired = true;
                var ages = driver.FindElementsByCssSelector("[name=age]");
                if (ages.Count() > 0)
                {
                    ages[0].SendKeys(new Random().Next(19, 40).ToString());
                    ageRequrired = false;
                }

                driver.FindElementByCssSelector("button.red").Click();

                // fill age

                UpdateList(email);

                driver.FindElementByXPath("//input[@name='full_name']").Clear();
                driver.FindElementByXPath("//input[@name='full_name']").SendKeys(_names[new Random().Next(0, _names.Count())]);


                if (ageRequrired)
                    driver.FindElementByXPath("//input[@name='age']").SendKeys(new Random().Next(19, 40).ToString());


                driver.FindElementByXPath("//input[@value='female']").Click();
                driver.FindElementByXPath("//button[@class='red comeOnInButton active']").Click();

                // put proxy 

             
            }
            catch (Exception ex)
            {

                driver.Quit();
               
                return;
            }


            try
            {
                if (driver.FindElementsById("newUserCountry").Count > 0)
                    driver.FindElementByCssSelector("button[type = 'submit']").Click();

            }
            catch { }





            try
            {
                string selector = "div[data-grid-item='true']";
                //   td.Driver.ExecuteScript()
                wait.Until((d) => d.FindElements(By.CssSelector(selector)).Count > 1);
                var interests = driver.FindElementsByCssSelector(selector);
                if (interests != null)
                {
                    int clicked = 0;
                    foreach (var interes in interests)
                    {
                        try
                        {
                            interes.Click();
                            clicked++;
                        }
                        catch { }


                        if (clicked > 5)
                        {

                            var btns = driver.FindElementsByXPath("//span[@class='buttonText']");
                            if (btns != null)
                            {

                                //
                                btns[1].Click();

                                try
                                {
                                    string buttonSkip = "body > div:nth-child(11) > div > div > form > div.formFooter > div > button:nth-child(1)";
                                    string xpathSelector = "/html/body/div[2]/div/div/div/div[2]/div/div[3]/button";
                                    //    wait.Until((d) => d.FindElements(By.XPath(xpathSelector)).Count > 1);
                                    driver.FindElementByXPath(xpathSelector).Click();

                                    //    wait.Until((d) => d.FindElements(By.CssSelector(buttonSkip)).Count > 1);
                                    driver.FindElementByXPath(buttonSkip).Click();
                                    Thread.Sleep(new TimeSpan(0, 0, 30));
                                }

                                catch { }




                            }
                        }
                    }
                }
            }
            catch { }
            //interestCardWrapper

            driver.ExecuteScript(@"setInterval(function () {
            var x = document.querySelector('.optionalSkip');
            if (x != null){x.Click(); } }, 500); ");
            Thread.Sleep(10000);

        }

        private static void _CloseThisTask(ThreadAndDriver td)
        {

            td.Driver.Quit();
            try
            {
                td.Thread.Abort();
            }
            catch { }
        }
        private void _CloseProccess()
        {
            //    var proccess = Process.GetProcesses();
            //    foreach (Process pr in proccess)
            //    {

            //        if (pr.ProcessName == "chromedriver")
            //            pr.Kill();

            //        //File.AppendAllText("text.txt", pr.MainWindowTitle+":" + pr.ProcessName + Environment.NewLine);
            //    }
        }



        private ChromeDriver OpenAndReturnDriver(string proxy)
        {
            ChromeOptions option = new ChromeOptions();
            option.AddArgument($"--proxy-server={proxy}");  //
        //    option.AddArgument($"--window--size=100,100");
           //  option.AddArgument("--headless");
            //  option.AddArgument("--no-startup-window");
            // var driver = new ChromeDriver(option);

            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
        

            ChromeDriver  driver = new ChromeDriver(driverService,option);
           

            return driver;
            //var serviceJs = PhantomJSDriverService.CreateDefaultService();
            //serviceJs.HideCommandPromptWindow = true;
            //serviceJs.Proxy = proxy;
            //var driver = new PhantomJSDriver(serviceJs);
            //return driver;

        }



        private void UpdateList(string email)
        {
            _email.Remove(email);
            File.AppendAllText("result.txt", email + ":trance12" + Environment.NewLine);
            File.WriteAllLines(this._emailFileName, _email.ToArray());
        }

        private void _LoadNames()
        {
            try
            {
                _names = File.ReadAllLines("names.txt").ToList();
                con.Text += $"names loaded,  {this._names.Count()}  " + Environment.NewLine; ;
            }
            catch
            {
                con.Text += $"names can`t loaded " + Environment.NewLine; ;
            }


        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this._CloseAll();
            _CloseProccess();
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChromeDriver driver = this.OpenAndReturnDriver(_proxy[17]);
            
            string email = _email[0];

            driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 30);
          
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(50));



            try
            {
                driver.Url = "http://pinterest.com/logout";
                driver.FindElementByXPath("//input[@name='id']").SendKeys(email);
                driver.FindElementByXPath("//input[@name='password']").SendKeys("trance12");


                //  if we have special age
                bool ageRequrired = true;
                var ages = driver.FindElementsByCssSelector("[name=age]");
                if (ages.Count() > 0)
                {
                    ages[0].SendKeys(new Random().Next(19, 40).ToString());
                    ageRequrired = false;
                }

                driver.FindElementByCssSelector("button.red").Click();

                // fill age

                driver.FindElementByXPath("//input[@name='full_name']").Clear();
                driver.FindElementByXPath("//input[@name='full_name']").SendKeys(_names[new Random().Next(0, _names.Count())]);


                if (ageRequrired)
                    driver.FindElementByXPath("//input[@name='age']").SendKeys(new Random().Next(19, 40).ToString());


                driver.FindElementByXPath("//input[@value='female']").Click();
                driver.FindElementByXPath("//button[@class='red comeOnInButton active']").Click();

                // put proxy 

                UpdateList(email);
            }
            catch (Exception ex)
            {

                driver.Quit();
                return;
            }


            try
            {

                var fem = driver.FindElementByLinkText("female");
                if (fem != null)
                    fem.Click();

                driver.FindElementByXPath("//input[@value='female']").Click();
                driver.FindElementByXPath("//button[@class='red comeOnInButton active']").Click();
            }
            catch { }


            try
            {
                if (driver.FindElementsById("newUserCountry").Count > 0)
                    driver.FindElementByCssSelector("button[type = 'submit']").Click();

            }
            catch { }



            driver.ExecuteScript(@"setInterval(function () {
            var x = document.querySelector('.optionalSkip');
            if (x != null){x.Click(); } }, 500); ");

            driver.ExecuteScript(@"setInterval(function () {
            var y = document.querySelector('.NuxPickerFooter__buttonWrapper button');
            if (y != null){y.Click(); } }, 500) ");

            try
            {
                string selector = "div[data-grid-item='true']";
                //   td.Driver.ExecuteScript()
                wait.Until((d) => d.FindElements(By.CssSelector(selector)).Count > 1);
                var interests = driver.FindElementsByCssSelector(selector);
                if (interests != null)
                {
                    int clicked = 0;
                    foreach (var interes in interests)
                    {
                        try
                        {
                            interes.Click();
                            clicked++;
                        }
                        catch { }


                        if (clicked > 5)
                        {

                            var btns = driver.FindElementsByCssSelector(".NuxPickerFooter__buttonWrapper button");
                            if (btns != null)
                            {

                                //
                               
                                try
                                {
                                    btns[1].Click();

                                    string buttonSkip = "body > div:nth-child(11) > div > div > form > div.formFooter > div > button:nth-child(1)";
                                    string xpathSelector = "/html/body/div[2]/div/div/div/div[2]/div/div[3]/button";
                               //    wait.Until((d) => d.FindElements(By.XPath(xpathSelector)).Count > 1);
                                    driver.FindElementByXPath(xpathSelector).Click();

                                //    wait.Until((d) => d.FindElements(By.CssSelector(buttonSkip)).Count > 1);
                               driver.FindElementByXPath(buttonSkip).Click();
                                    Thread.Sleep(new TimeSpan(0, 0, 30));
                                }

                                catch { }
                               
                              

                              
                            }
                        }
                    }
                }
            }
            catch { }
            //interestCardWrapper

       


            Thread.Sleep(10000);
            //                td.Driver.FindElementByCssSelector("body").Click();

            //                ///pinLink 
            //                ///
            //                var save = td.Driver.FindElementsByCssSelector(".pinLink");
            //                if (save.Count > 0)
            //                {
            //                    save[0].Click();
            //                    var g = 7;

            //                }

            //                Thread.Sleep(30000);
            //            }
            //            catch
            //            {
            //                _CloseThisTask(td);
            //            }
            //            finally
            //            {
            //                _CloseThisTask(td);

            //            }

            //        }
            //            catch
            //            {
            //                _CloseThisTask(td);
            //    }
            //            finally
            //            {
            //                _CloseThisTask(td);
            //}

        }
    }
}
public static class IEnumerableExtensions
{

    public static IEnumerable<t> Randomize<t>(this IEnumerable<t> target)
    {
        Random r = new Random();

        return target.OrderBy(x => (r.Next()));
    }
}


