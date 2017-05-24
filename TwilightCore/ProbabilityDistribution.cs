using System;
using System.Collections.Generic;
using System.Linq;

namespace TwilightCore
{
    /// <summary>
    /// This class allows for a probability distribution of multiple items to be controlled by one class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ProbabilityDistribution<T>
    {
        private Dictionary<double, T> EndPoints { get; set; }
        private double CurrentPoint;
        private T OverflowResult;

        public ProbabilityDistribution()
        {
            EndPoints = new Dictionary<double, T>();
        }

        public ProbabilityDistribution(T Overflow)
        {
            EndPoints = new Dictionary<double, T>();
            OverflowResult = Overflow;
        }

        public double GetCurrentEndPoint()
        {
            return CurrentPoint;
        }

        public void SetOverflowResult(T Overflow)
        {
            OverflowResult = Overflow;
        }

        public void AddNewEndPoint(double NewProb, T Entry)
        {
            if (NewProb < 0)
                throw new ArgumentOutOfRangeException("The probability being added must be positive.");
            if (NewProb + CurrentPoint > 1)
                throw new ArgumentOutOfRangeException("The argument being added would cause the probability to exceed 100%.");

            CurrentPoint += NewProb;
            EndPoints.Add(CurrentPoint,Entry);
        }

        public void Realign()
        {
            EndPoints.OrderBy(endpoint => endpoint.Key);
        }

        public bool GetEntryFromProb(double Prob, out T Result, bool IncludeEnds = true)
        {
            if (EndPoints.Keys.Count() == 0)
                throw new InvalidOperationException("No probabilities have been added to this distribution");

            if (Prob > 1 || Prob < 0)
                throw new ArgumentOutOfRangeException("The Probability must be greather than 0 and less than or equal to 1.");


            double[] KeyValues = EndPoints.Keys.ToArray();
            for (int i = 0; i < KeyValues.Count(); i++)
            {
                if (i == 0 && IncludeEnds)
                {
                    if (Prob >= 0 && Prob <= KeyValues[i])
                    {
                        Result = EndPoints[KeyValues[i]];
                        return true;
                    }
                }
                else if (i == 0 && !IncludeEnds)
                {
                    if (Prob > 0 && Prob < KeyValues[i])
                    {
                        Result = EndPoints[KeyValues[i]];
                        return true;
                    }
                }

                else if (i != 0 && IncludeEnds)
                {
                    if (Prob > KeyValues[i - 1] && Prob <= KeyValues[i])
                    {
                        Result = EndPoints[KeyValues[i]];
                        return true;
                    }

                }

                else if (i != 0 && !IncludeEnds)
                {
                    if (Prob > KeyValues[i - 1] && Prob < KeyValues[i])
                    {
                       Result = EndPoints[KeyValues[i]];
                        return true;
                    }
                }               

            }

            if (OverflowResult != null && !OverflowResult.Equals(default(T)))
            {
                if (Prob > KeyValues.Last())
                {
                    Result = OverflowResult;
                    return true;
                }
            }

            Result = default(T);
            return false;
        }
    }
}
