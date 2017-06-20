using System;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace ClimateDataExaminer
{
    class Program
    {
        private readonly static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ObjectCreationHandling = ObjectCreationHandling.Replace, // avoid issue where default ICollection<T> values are duplicated each time the config is loaded
        };

        static void Main(string[] args)
        {
            FerngillClimate OurClimate = ReadJsonFile<FerngillClimate>("enhanced.json");
            StringBuilder outputString = new StringBuilder();

            Console.WriteLine("Parsing Data..");
            outputString.AppendLine("Parsing Data..");

            //the goal of this is to produce a write up per time span of outputs.
            //Example: Time Span is Season: X K to Y Z 
            //<newline>
            //Weather Parameters [1..n]: Type 'K', Equation: XD + Y , Variability: -A to +B
            //      Effective Range: [-C to +E] 

            Console.WriteLine($"Settings: Allow Rain in Winter set to {OurClimate.AllowRainInWinter}");
            outputString.AppendLine($"Settings: Allow Rain in Winter set to {OurClimate.AllowRainInWinter}");

            foreach (FerngillClimateTimeSpan span in OurClimate.ClimateSequences)
            {
                //pull the time span.
                Console.WriteLine($"Time Span is Season {span.BeginSeason} {span.BeginDay} to " +
                    $"{span.EndSeason} {span.EndDay}");
                Console.WriteLine();

                outputString.AppendLine($"Time Span is Season {span.BeginSeason} {span.BeginDay} to " +
                    $"{span.EndSeason} {span.EndDay}");
                outputString.AppendLine();

                //verify the span is valid
                if (SDVDate.LesserThan(span.EndSeason, span.EndDay, span.BeginSeason, span.BeginDay))
                {
                    Console.WriteLine("INVALID SPAN. The Ending date must be equal or greater than the beginning date.");
                    outputString.AppendLine("INVALID SPAN. The Ending date must be equal or greater than the beginning date.");
                    continue;
                }

                //pull the parameters
                for (int i = 0; i < span.WeatherChances.Count; i++)
                {
                    Console.Write($"Weather Parameters {i}: Type {span.WeatherChances[i].WeatherType}, ");                    
                    Console.WriteLine($" Equation: {span.WeatherChances[i].ChangeRate}x + {span.WeatherChances[i].BaseValue} ");
                    Console.WriteLine($"Varibility: {span.WeatherChances[i].VariableLowerBound} to {span.WeatherChances[i].VariableHigherBound}");

                    outputString.Append($"Weather Parameters {i}: Type {span.WeatherChances[i].WeatherType}, ");
                    outputString.AppendLine($" Equation: {span.WeatherChances[i].ChangeRate}x + {span.WeatherChances[i].BaseValue} ");
                    outputString.AppendLine($"Varibility: {span.WeatherChances[i].VariableLowerBound} to {span.WeatherChances[i].VariableHigherBound}");

                    //calc effective range
                    double min = 9000;
                    double max = -9000;
                    int start = span.BeginDay;
                    int end = span.EndDay;

                    if (span.BeginSeason != span.EndSeason)
                    {
                        for (int j = span.BeginDay; j <= 28; j++)
                        {
                            double val = span.WeatherChances[i].ChangeRate * j + span.WeatherChances[i].BaseValue;
                            double lVal = val + span.WeatherChances[i].VariableLowerBound;
                            double hVal = val + span.WeatherChances[i].VariableHigherBound;

                            Console.WriteLine($"Testing: Generated Value for day [{j}] is {lVal} and {hVal}");
                            outputString.AppendLine($"Testing: Generated Value for day [{j}] is {lVal} and {hVal}");

                            if (lVal < min)
                                min = lVal;
                            if (hVal > max)
                                max = hVal;
                            
                        }

                        for (int j = 1; j <= span.EndDay; j++)
                        {
                            double val = span.WeatherChances[i].ChangeRate * j + span.WeatherChances[i].BaseValue;
                            double lVal = val + span.WeatherChances[i].VariableLowerBound;
                            double hVal = val + span.WeatherChances[i].VariableHigherBound;
                            Console.WriteLine($"Testing: Generated Value for day [{j}] is {lVal} and {hVal}");
                            outputString.AppendLine($"Testing: Generated Value for day [{j}] is {lVal} and {hVal}");

                            if (lVal < min)
                                min = lVal;
                            if (hVal > max)
                                max = hVal;
                        }
                    }
                    else
                    {
                        for (int j = start; j <= end; j++)
                        {
                            double val = span.WeatherChances[i].ChangeRate * j + span.WeatherChances[i].BaseValue;
                            double lVal = val + span.WeatherChances[i].VariableLowerBound;
                            double hVal = val + span.WeatherChances[i].VariableHigherBound;
                            Console.WriteLine($"Testing: Generated Value for day [{j}] is {lVal} and {hVal}");
                            outputString.AppendLine($"Testing: Generated Value for day [{j}] is {lVal} and {hVal}");

                            if (lVal < min)
                                min = lVal;
                            if (hVal > max)
                                max = hVal;
                        }
                    }


                    Console.WriteLine($"      Effective Range: [{min.ToString("F2")} to {max.ToString("F2")}]");
                    Console.WriteLine();

                    outputString.AppendLine($"      Effective Range: [{min.ToString("F2")} to {max.ToString("F2")}]");
                    outputString.AppendLine();
                }
            }

            File.WriteAllText(@"output.txt", outputString.ToString());
            Console.ReadLine();

        }

        public static TModel ReadJsonFile<TModel>(string fullPath)
            where TModel : class
        {
            // validate
            if (string.IsNullOrWhiteSpace(fullPath))
                throw new ArgumentException("The file path is empty or invalid.", nameof(fullPath));

            // read file
            string json;
            try
            {
                json = File.ReadAllText(fullPath);
            }
            catch (Exception ex) when (ex is DirectoryNotFoundException || ex is FileNotFoundException)
            {
                return null;
            }

            // deserialise model
            return JsonConvert.DeserializeObject<TModel>(json, JsonSettings);
        }
    }
}
