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
                _proxy = File.ReadAllLines(_proxyFileName).ToList();
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
            foreach(var t in th)
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
           
            try
            {
                try
                {
                    td.Driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 0, 35);
                    WebDriverWait  wait = new WebDriverWait(td.Driver, TimeSpan.FromSeconds(20));


                    td.Driver.Url = "http://pinterest.com/logout";

                    //wait.Until(d => d.Title == "Pinterest");
                    //wait.Until(d => d.FindElement(By.Name("id")));

                    td.Driver.FindElementByXPath("//input[@name='id']").SendKeys(email);

                    td.Driver.FindElementByXPath("//input[@name='password']").SendKeys("trance12");


                    bool ageRequrired = true;
                    var ages = td.Driver.FindElementsByCssSelector("[name=age]");
                    if(ages.Count() > 0)
                    {
                        ages[0].SendKeys(new Random().Next(19, 40).ToString());
                        ageRequrired = false;
                    }


                    td.Driver.FindElementByCssSelector("button.red").Click();


                    td.Driver.FindElementByXPath("//input[@name='full_name']").SendKeys(_names[new Random().Next(0, _names.Count())]);

                    if (ageRequrired)
                    td.Driver.FindElementByXPath("//input[@name='age']").SendKeys(new Random().Next(19, 40).ToString());


                    td.Driver.FindElementByXPath("//input[@value='female']").Click();
                    td.Driver.FindElementByXPath("//button[@class='red comeOnInButton active']").Click();




                    if (!_goodProxy.Contains(td.Proxy))
                        _goodProxy.Add(td.Proxy);

                    UpdateList(email);

                }
                catch (Exception ex)
                {

                    

                    if (_goodProxy.Contains(td.Proxy))
                        _goodProxy.Remove(td.Proxy);
                    // proxy is bad :(
                    _CloseThisTask(td);

                }


                try
                {
                   
                  
                    var interests = td.Driver.FindElementsByXPath("//div[@data-grid-item='true']");
                    if (interests != null)
                    {
                        int clicked = 0;
                        foreach (var interes in interests)
                        {
                            interes.Click();
                            clicked++;

                            if (clicked > new Random().Next(5, 10))
                            {

                                var btns = td.Driver.FindElementsByXPath("//span[@class='buttonText']");
                                if (btns != null)
                                {
                                    btns[1].Click();

                                    if (td.Driver.FindElementByXPath("//div[@class='optionalSkip']") != null)
                                    {
                                        td.Driver.FindElementByXPath("//div[@class='optionalSkip']").Click();
                                    }
                                    Thread.Sleep(500);
                                }
                            }



                        }
                    }

                    Thread.Sleep(10000);

                    if (td.Driver.FindElementByXPath("//div[@class='optionalSkip']") != null)
                    {
                        td.Driver.FindElementByXPath("//div[@class='optionalSkip']").Click();
                    }


                    Thread.Sleep(30000);
                }
                catch
                {
                    _CloseThisTask(td);
                }
                finally
                {
                    _CloseThisTask(td);

                }

            }
            catch
            {
                _CloseThisTask(td);
            }
            finally
            {
                _CloseThisTask(td);
            }

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
        private  void _CloseProccess()
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
            option.AddArgument($"--proxy-server={proxy}");
          // option.AddArgument("--headless");
            // option.AddArgument("--no-startup-window");
           // var driver = new ChromeDriver(option);
          
            return  new ChromeDriver(option);
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


    }
    public static class IEnumerableExtensions
    {

        public static IEnumerable<t> Randomize<t>(this IEnumerable<t> target)
        {
            Random r = new Random();

            return target.OrderBy(x => (r.Next()));
        }
    }
}
