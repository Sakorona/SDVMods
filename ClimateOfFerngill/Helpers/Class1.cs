using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClimateOfFerngill
{
    /// <summary>
    /// This class allows for a probability distribution of multiple items to be controlled by one class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ProbabilityDistribution<T>
    {
        private Dictionary<double, T> EndPoints { get; set; }
        private double CurrentPoint;

        public ProbabilityDistribution()
        {

        }

        public void AddNewEndPoint(double NewProb, T Entry)
        {
            CurrentPoint += NewProb;
            EndPoints.Add(CurrentPoint,Entry);
        }

        public void Realign()
        {
            EndPoints.OrderBy(endpoint => endpoint.Key);
        }

        public T GetEntryFromProb(double Prob, bool IncludeEnds = true)
        {
            if (IncludeEnds)
            {

            }
        }
    }
}
