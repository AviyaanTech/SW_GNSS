using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SW_GNSS.SwMaps
{
	//Correction Information for a single SW Maps Point
	//Avinab Malla 
	//26 September 2021
	public class SwMapsCorrectionPoint
	{
		public string PointUUID { get; set; }
		public string FeatureID { get; set; }
		public string FeatureName { get; set; }
		public int Index { get; set; }
		public string LayerName { get; set; }
		
		
		public double FieldLatitude { get; set; }
		public double FieldLongitude { get; set; }
		public double FieldElevation { get; set; }
		public double FieldOrthoHeight { get; set; }


		public double ProcessedLatitude { get; set; }
		public double ProcessedLongitude { get; set; }
		public double ProcessedElevation { get; set; }
		public double ProcessedOrthoHeight { get; set; }


		public double DeltaE { get; set; }
		public double DeltaN { get; set; }
		public double DeltaU { get; set; }

		public double SDE { get; set; }
		public double SDN { get; set; }
		public double SDU { get; set; }


		public long Time { get; set; }
		public long StartTime { get; set; }
		public double InstrumentHeight { get; set; }

		public int FixID { get; set; }

		public double AverageCount { get; set; }
		public string FixType
		{
			get
			{
				if (FixID == 0) return "Invalid";
				if (FixID == 1) return "Single";
				if (FixID == 2) return "DGPS";
				if (FixID == 3) return "PPP";
				if (FixID == 4) return "Fix";
				if (FixID == 5) return "Float";
				if (FixID == 9) return "SBAS";
				return "Invalid";
			}
		}

	}
}
