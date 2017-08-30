using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Net;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;

/*
суть проекта

    есть arduino модуль подключеный к компьютеру оператора. у него базовая функция - отправить в программу оператора сигнал что нажата кнопка
    
    необходимо программу оператора перевести в WEB (Сейчас она локальная)
    
    работа с COM портом с WEB нереализуема. необходимо на компе к которому подключен arduino установить программу - шлюз, которая будет все сигналы с контроллера отправлять на сервер,
    который в свою очередь будет отправлять сигналы в программу оператора




*/


namespace com_to_http_service
{

    

    class Program
    {
        //path - допущенный к обработке. параметры недопустимы
        private static String[] allow_commands = { "/led_blink_1", "/btnHIGH", "/btnLOW" };

        public static bool stop_read_com_port = false;
        static SerialPort _serialPort = null;



        static void Main(string[] args)
        {
            //запуск читалки COM порта в отдельном потоке
            asyncInitComPort().ContinueWith(t=> {  });

            //запуск HTTP сервера в отдельном потоке
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(ConfigurationManager.AppSettings["ServerUrl"]);
            listener.Start();
            // запуск таска приема соединения
            ProcessAsync(listener).ContinueWith(t=> {});
            
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        static async Task asyncInitComPort()
        {
            ///COM PORT INIT
            
                try
                {

                    _serialPort = new SerialPort(ConfigurationManager.AppSettings["ComPort"], int.Parse(ConfigurationManager.AppSettings["ComPortSpeed"]));


                    //_serialPort.ReadTimeout = 500;
                    //_serialPort.WriteTimeout = 500;

                    Console.WriteLine("try open com port...");
                    await Task.Delay(100);
                    _serialPort.Open();
                    Console.WriteLine("Done.");

                    while (stop_read_com_port == false)
                    {
                            string message = _serialPort.ReadLine(); 
                            System.Diagnostics.Debug.WriteLine(message);

                    //отправил сообщение в обработку - отправка сигнала на сервер
                    
                    ProcessAsyncComPortMessage(message).ContinueWith(t => { });
                    

                    //Form1.thisObject.controllerMessage(message);


                }
                }
                //catch (IOException) { Console.WriteLine("COM PORT IOException, try reconnect"); }
                catch (Exception e)
                {
                    Console.WriteLine("Ошибка открытия COM порта, укажите верный в настройках\n"+e.Message);

                }
                _serialPort.Close();
            
        }

        


        static async Task ProcessAsyncComPortMessage(String message)
        {
            
            await Task.Delay(10);
            

            using (var webClient = new WebClient())
            {
                String url = ConfigurationManager.AppSettings["ServerUrl"]+message;
                /* POST
                // Создаём коллекцию параметров
                var pars = new System.Collections.Specialized.NameValueCollection();

                // Добавляем необходимые параметры в виде пар ключ, значение
                pars.Add("format", "json");
                pars.Add("message", message);

                // Посылаем параметры на сервер
                // Может быть ответ в виде массива байт
                var response = webClient.UploadValues("localhost:8000", pars);
                foreach (var item in response)
                    Console.WriteLine(item);
                    */
                /*GET */

                //webClient.QueryString.Add("key", "value");
                var response = webClient.DownloadString(url);
            }
            

            
        }


        #region встроеный HTTP server - для тестирования
        static async Task ProcessAsync(HttpListener listener)
        {
            HttpListenerContext ctx = await listener.GetContextAsync(); //спит до соединения
            // запуск таска приема следующего соединения
            ProcessAsync(listener).ContinueWith(t => { });

            
            //обработка текущего соединения            
            Perform(ctx);
            
        }

        static void Perform(HttpListenerContext ctx)
        {

            Console.WriteLine("PREFORM REQUEST");
            HttpListenerResponse response = ctx.Response;
            string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
            responseString += ctx.Request.Url.PathAndQuery;

            

            if (! allow_commands.Contains(ctx.Request.Url.PathAndQuery))
            {
                responseString = "NOT ALLOWED COMMAND";
                Console.WriteLine("NOT ALLOWED COMMAND " + ctx.Request.Url.PathAndQuery);
            }
            else
            {
                responseString = onCommand(ctx.Request.Url.PathAndQuery);
            }
            


            //responseString += ctx.Request.QueryString.;
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);

            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);

            // You must close the output stream.
            output.Close();
        }

        static String onCommand(String command)
        {
            String result = "";

            Console.WriteLine("Process command "+ command);

            return result;
        }

        #endregion
    }
}
