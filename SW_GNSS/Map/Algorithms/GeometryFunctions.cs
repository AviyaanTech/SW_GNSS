using SwCad.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwCad.Algorithms
{
	public class GeometryFunctions
	{
		/// <summary>
		/// Determines if the given point is inside the polygon
		/// </summary>
		/// <param name="polygon">the vertices of polygon</param>
		/// <param name="testPoint">the given point</param>
		/// <returns>true if the point is inside the polygon; otherwise, false</returns>
		public static bool IsPointInPolygon(Vector2D testPoint, params Vector2D[] polygon)
		{
			bool result = false;
			int j = polygon.Count() - 1;
			for (int i = 0; i < polygon.Count(); i++)
			{
				if (polygon[i].Y < testPoint.Y && polygon[j].Y >= testPoint.Y || polygon[j].Y < testPoint.Y && polygon[i].Y >= testPoint.Y)
				{
					if (polygon[i].X + (testPoint.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < testPoint.X)
					{
						result = !result;
					}
				}
				j = i;
			}
			return result;
		}

		public static Vector2D IntersectionPoint(Vector2D l1Pt1, Vector2D l1pt2, Vector2D l2pt1, Vector2D l2pt2)
		{

			Vector2D l1dir = new Vector2D(l1pt2.X - l1Pt1.X, l1pt2.Y - l1Pt1.Y);
			Vector2D l2dir = new Vector2D(l2pt2.X - l2pt1.X, l2pt2.Y - l2pt1.Y);

			double m1 = l1dir.Y / l1dir.X;
			double m2 = l2dir.Y / l2dir.X;

			double c1 = l1Pt1.Y - m1 * l1Pt1.X;
			double c2 = l2pt1.Y - m2 * l2pt1.X;

			double x = (c2 - c1) / (m1 - m2);
			double y = m1 * x + c1;

			Vector2D l1pt = new Vector2D(l1Pt1.X, l1Pt1.Y);
			Vector2D l2pt = new Vector2D(l2pt1.X, l2pt1.Y);
			Vector2D ipt = new Vector2D(x, y);

			if (Math.Abs(ipt.X - l1pt2.X) < 1E-09)
				return null;
			if (Math.Abs(ipt.X - l2pt2.X) < 1E-09)
				return null;


			if (IsBetween(l1Pt1.X, ipt.X, l1pt2.X))
			{
				if (IsBetween(l1Pt1.Y, ipt.Y, l1pt2.Y))
				{
					if (IsBetween(l2pt1.X, ipt.X, l2pt2.X))
					{
						if (IsBetween(l2pt1.Y, ipt.Y, l2pt2.Y))
						{
							return ipt;
						}
					}
				}
			}

			return null;
		}
		public static bool DoLineSegmentsIntersect(Vector2D l1Pt1, Vector2D l1pt2, Vector2D l2pt1, Vector2D l2pt2)
		{
			Vector2D l1dir = new Vector2D(l1pt2.X - l1Pt1.X, l1pt2.Y - l1Pt1.Y);
			Vector2D l2dir = new Vector2D(l2pt2.X - l2pt1.X, l2pt2.Y - l2pt1.Y);

			double m1 = l1dir.Y / l1dir.X;
			double m2 = l2dir.Y / l2dir.X;


			double c1 = l1Pt1.Y - m1 * l1Pt1.X;
			double c2 = l2pt1.Y - m2 * l2pt1.X;


			double x = (c2 - c1) / (m1 - m2);
			double y = m1 * x + c1;

			Vector2D l1pt = new Vector2D(l1Pt1.X, l1Pt1.Y);
			Vector2D l2pt = new Vector2D(l2pt1.X, l2pt1.Y);
			Vector2D ipt = new Vector2D(x, y);

			if (Math.Abs(ipt.X - l1pt2.X) < 1E-09)
				return false;
			if (Math.Abs(ipt.X - l2pt2.X) < 1E-09)
				return false;


			if (IsBetween(l1Pt1.X, ipt.X, l1pt2.X))
			{
				if (IsBetween(l1Pt1.Y, ipt.Y, l1pt2.Y))
				{
					if (IsBetween(l2pt1.X, ipt.X, l2pt2.X))
					{
						if (IsBetween(l2pt1.Y, ipt.Y, l2pt2.Y))
						{
							return true;
						}
					}
				}
			}

			return false;

		}
		public static bool IsBetween(double n1, double nv, double n2)
		{

			if (nv >= n1 & nv <= n2)
				return true;
			if (nv >= n2 & nv <= n1)
				return true;
			return false;
		}

		public static List<Vector2D> RemoveLineIntersections(List<Vector2D> line)
		{
			var ret = new List<Vector2D>();
			ret.Add(line[0]);

			for (int i = 0; i < line.Count - 1; i++)
			{
				bool intersectionFound = false;
				for (int j = i + 1; j < line.Count - 1; j++)
				{
					var ipt = IntersectionPoint(line[i], line[i + 1], line[j], line[j + 1]);

					if (ipt != null)
					{
						ret.Add(ipt);
						ret.Add(line[j + 1]);
						i = j + 1;
						intersectionFound = true;
						break;
					}
				}
				if (!intersectionFound)
				{
					ret.Add(line[i + 1]);
				}
			}

			return ret;
		}
	}
}
