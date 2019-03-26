using System;
using StructIssueDemo;


namespace StructIssueDemo
{
	public struct TestStruct
	{
		public int N { get; set; }
	}

	public class Program
	{
		static void Main(string[] args)
		{
			var t = new TestStruct();
			t.N = 12;

			var x = t.N;

			Console.WriteLine("Is the value actually readable from the autoprop of the struct?: " + x);
			Console.ReadLine();
		}

	}
}
