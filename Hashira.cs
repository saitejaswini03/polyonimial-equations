using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Newtonsoft.Json.Linq;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            // Read JSON files as strings
            string json1 = File.ReadAllText("testcases1.json");
            string json2 = File.ReadAllText("testcases2.json");

            Console.WriteLine("Processing testcases1.json:");
            SolveTestCase(json1);

            Console.WriteLine("\nProcessing testcases2.json:");
            SolveTestCase(json2);
        }
        catch (IOException e)
        {
            Console.Error.WriteLine("Error reading file: " + e.Message);
        }
    }

    public static void SolveTestCase(string jsonContent)
    {
        try
        {
            // Parse JSON
            JObject jsonObj = JObject.Parse(jsonContent);
            int k = (int)jsonObj["keys"]["k"];

            var bases = new Dictionary<int, int>();
            var values = new Dictionary<int, string>();

            foreach (var property in jsonObj["keys"])
            {
                if (property is JProperty prop && prop.Name != "k")
                {
                    int key = int.Parse(prop.Name);
                    int baseVal = (int)prop.Value["base"];
                    string value = (string)prop.Value["value"];

                    bases[key] = baseVal;
                    values[key] = value;
                }
            }

            // Convert extracted values (respecting the base)
            List<int> xValues = new List<int>();
            List<BigInteger> yValues = new List<BigInteger>();

            foreach (var kv in bases)
            {
                xValues.Add(kv.Key);
                yValues.Add(ConvertToBigInteger(values[kv.Key], kv.Value));
            }

            // Take only k points
            List<int> xSub = xValues.GetRange(0, k);
            List<BigInteger> ySub = yValues.GetRange(0, k);

            // Compute constant term using Lagrange interpolation
            BigInteger constantTerm = LagrangeInterpolation(xSub, ySub);
            Console.WriteLine("Secret (c) : " + constantTerm);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Error processing JSON: " + e.Message);
        }
    }

    // Function to decode string into BigInteger using given base
    public static BigInteger ConvertToBigInteger(string value, int numberBase)
    {
        BigInteger result = BigInteger.Zero;
        string digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        foreach (char c in value.ToUpper())
        {
            int digit = digits.IndexOf(c);
            if (digit < 0 || digit >= numberBase)
                throw new FormatException($"Invalid digit '{c}' for base {numberBase}");

            result = result * numberBase + digit;
        }

        return result;
    }

    // Lagrange interpolation to find f(0) (constant term)
    public static BigInteger LagrangeInterpolation(List<int> x, List<BigInteger> y)
    {
        BigInteger result = BigInteger.Zero;
        int k = x.Count;

        for (int i = 0; i < k; i++)
        {
            BigInteger term = y[i];
            BigInteger numerator = BigInteger.One;
            BigInteger denominator = BigInteger.One;

            for (int j = 0; j < k; j++)
            {
                if (i != j)
                {
                    numerator *= -x[j];
                    denominator *= (x[i] - x[j]);
                }
            }

            // Ensure exact division (no floating point)
            term = term * numerator / denominator;
            result += term;
        }

        return result;
    }
}