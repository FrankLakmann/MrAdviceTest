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

			Console.WriteLine("Is the value actually in the autoprop of the struct?: " + t.N);
			Console.ReadLine();
		}
	}
}
