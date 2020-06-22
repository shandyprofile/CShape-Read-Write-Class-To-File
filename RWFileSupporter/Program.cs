using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace RWFileSupporter
{
    class Program
    {
        static void Main(string[] args)
        {
            RWFileSupporter fileSupporter = new RWFileSupporter();

            List<TestClass> testData = new List<TestClass>();
            testData.Add(new TestClass() { Param1 = "Test 1 Param 1", Param2 = "Test 1 Param 2" });
            testData.Add(new TestClass() { Param1 = "Test 2 Param 1", Param2 = "Test 2 Param 2" });
            testData.Add(new TestClass() { Param1 = "Test 3 Param 1", Param2 = "Test 3 Param 2" });
            testData.Add(new TestClass() { Param1 = "Test 4 Param 1", Param2 = "Test 4 Param 2" });

            fileSupporter.Write(AppDomain.CurrentDomain.BaseDirectory + "/test.txt", testData);

            List<TestClass> testRead = fileSupporter.Read<TestClass>(AppDomain.CurrentDomain.BaseDirectory + "/test.txt");

            testRead.ForEach(x => {
            Console.WriteLine(x.Param1 + " " + x.Param2); 
            });
            Console.ReadLine();
        }

        class TestClass { 
            public string Param1 { get; set; }
            public string Param2 { get; set; }
        }
    }
}