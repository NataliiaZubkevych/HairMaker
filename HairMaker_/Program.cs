using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace HairMaker_
{
    class Hairmaker
    {
        static Hairmaker hairmaker;
        const int chairs = 5;
        int waitingCl = 0;
        public static string[] Name = new string[] { "Lui", "X", "Y", "Z", "Sema", "Marfa", "Sony", "Mus'ka" };
        public static  Random random = new Random();
        Mutex mutex = new Mutex(false, "HairmakerMutex");
        Semaphore poolCl = new Semaphore(chairs, 8);
        volatile bool isSleep = false;

        public static Hairmaker Instance()
        {
            if (hairmaker == null)
            {
                hairmaker = new Hairmaker();
            }
            return hairmaker ;
        }
        public void Cut ()
        {
            while (true)
            {
                mutex.WaitOne();
                Thread.Sleep(10000);
                if (waitingCl == 0)
                {
                    Console.WriteLine("Парикмехер проверяет клиентов в зале");
                    Thread.Sleep(500);
                    Console.WriteLine("Клиентов нет. Уснул!");
                    isSleep = true;
                    Thread.Sleep(random.Next(500, 7000));

                }
                else
                {
                    waitingCl--;
                    Thread.Sleep(random.Next(100, 1000));
                    Console.WriteLine("Парикмахер работает");
                    Thread.Sleep(random.Next(500, 10000));
                    Console.WriteLine("Парикмахер освободился, идет в зал");
                    Thread.Sleep(random.Next(100, 500));

                }
                mutex.ReleaseMutex();
            }
        }
        public void Client()
        {
            waitingCl++;
            Console.WriteLine("Kлиент {0} входит в парикмахерскую", Thread.CurrentThread.Name);
            if (isSleep)
            {
                Console.WriteLine("Клиент {0} будит парикмахера",  Thread.CurrentThread.Name );
                Thread.Sleep(random.Next(100, 500));
                isSleep = false;
            }
            else
            {
                Thread.Sleep(random.Next(100, 1000));
                if (waitingCl <= chairs)
                {
                    Console.WriteLine("Kлиент {0} садится в кресло и ждет своей очереди, свободно стульев {1}", Thread.CurrentThread.Name, (chairs - waitingCl));
                }
                else
                {
                    Console.WriteLine("Kлиент {0} вошел и видит, что свобдных стульев нет", Thread.CurrentThread.Name);
                    if(poolCl.WaitOne(6000))
                    {
                        Console.WriteLine("Kлиент {0} думает", Thread.CurrentThread.Name);
                        if (waitingCl <= chairs)
                        {
                            Console.WriteLine("Kлиент {0} садится в кресло и ждет своей очереди, свободно стульев {1}", Thread.CurrentThread.Name, (chairs - waitingCl));
                            Thread.Sleep(4000);
                        }
                        else
                        {
                            Console.WriteLine("{0} уходит, не дождался", Thread.CurrentThread.Name);
                            Thread.Sleep(random.Next(100, 1000));
                            waitingCl--;
                        }
                    }
                    else
                    {
                        Console.WriteLine(Thread.CurrentThread.Name, "уходит, не дождался");
                        Thread.Sleep(random.Next(100, 1000));
                        waitingCl--;
                    }
                    poolCl.Release();
                }
            }
        }
    }
    class Program
    {
       static void Main(string[] args)
        {
            var hairmak = Hairmaker.Instance();
            var hairmakerThread = new Thread(hairmak.Cut);
            hairmakerThread.Start();

            Thread[] clients = new Thread[Hairmaker.Name.Length];

            for (int i=0; i < clients.Length; i++)
            {
                Thread.Sleep(Hairmaker.random.Next(100, 4000));
                clients[i] = new Thread(hairmak.Client);
                clients[i].Name = Hairmaker.Name[i];
                clients[i].Start();               
            }
            Console.ReadLine();
        }
    }
}
