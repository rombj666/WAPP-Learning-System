using System.Collections.Generic;

namespace ILOWLearningSystem.Web.Models
{
    public class StressTrackerViewModel
    {
        public List<StressRecord> Records { get; set; } = new();

        public double AverageStress { get; set; }

        public string Recommendation { get; set; } = string.Empty;

        public string StressStatus { get; set; } = string.Empty;
    }
}