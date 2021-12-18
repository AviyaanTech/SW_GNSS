using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwCad.Primitives
{
	internal class TransformationMatrix
	{
		public double[,] Matrix = new double[3, 3];
		public double[,] InverseMatrix = new double[3, 3];

		public void print()
		{
			Console.WriteLine(Matrix[0, 0] + "," + Matrix[0, 1] + "," + Matrix[0, 2]);
			Console.WriteLine(Matrix[1, 0] + "," + Matrix[1, 1] + "," + Matrix[1, 2]);
			Console.WriteLine(Matrix[2, 0] + "," + Matrix[2, 1] + "," + Matrix[2, 2]);
		}
		public TransformationMatrix()
		{
			Matrix[0, 0] = 1;
			Matrix[1, 1] = 1;
			Matrix[2, 2] = 1;

			InverseMatrix[0, 0] = 1;
			InverseMatrix[1, 1] = 1;
			InverseMatrix[2, 2] = 1;
		}

		public static double[,] mmult(double[,] A, double [,] B)
		{
			var C = new double[3, 3];
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					C[i, j] = 0;
					for (int k = 0; k < 3; k++)
					{
						C[i, j] += A[i, k] * B[k, j];
					}
				}
			}
			return C;
		}


		public static TransformationMatrix Identity()
		{
			return new TransformationMatrix();
		}


		public static TransformationMatrix operator *(TransformationMatrix m1, TransformationMatrix m2)
		{
			var ret = new TransformationMatrix();
			ret.Matrix = mmult(m1.Matrix, m2.Matrix);
			ret.InverseMatrix = mmult(m2.InverseMatrix, m1.InverseMatrix);
			return ret;
		}

		public static TransformationMatrix CreateRotation(double RotationDegrees)
		{
			var r = MathHelper.ToRadians(RotationDegrees);
			var ret = new TransformationMatrix();
			ret.Matrix[0, 0] = Math.Cos(r);
			ret.Matrix[0, 1] = -Math.Sin(r);
			ret.Matrix[1, 0] = Math.Sin(r);
			ret.Matrix[1, 1] = Math.Cos(r);


			ret.InverseMatrix[0, 0] = Math.Cos(-r);
			ret.InverseMatrix[0, 1] = -Math.Sin(-r);
			ret.InverseMatrix[1, 0] = Math.Sin(-r);
			ret.InverseMatrix[1, 1] = Math.Cos(-r);

			return ret;
		}


		public static Vector2D operator *(TransformationMatrix m1, Vector2D v)
		{
			var ret = new Vector2D();
			ret.X = m1.Matrix[0, 0] * v.X + m1.Matrix[0, 1] * v.Y + m1.Matrix[0, 2];
			ret.Y = m1.Matrix[1, 0] * v.X + m1.Matrix[1, 1] * v.Y + m1.Matrix[1, 2];
			return ret;
		}


		public BoundingBox Transform(BoundingBox b)
		{
			var topLeft = Transform(new Vector2D(b.MinX, b.MaxY));
			var bottomRight = Transform(new Vector2D(b.MaxX, b.MinY));
			return new BoundingBox(topLeft, bottomRight);
		}

		public Vector2D Transform(Vector2D v)
		{
			var ret = new Vector2D();
			ret.X = Matrix[0, 0] * v.X + Matrix[0, 1] * v.Y + Matrix[0, 2];
			ret.Y = Matrix[1, 0] * v.X + Matrix[1, 1] * v.Y + Matrix[1, 2];
			return ret;
		}


		public Vector2D InvertTransform(Vector2D v)
		{
			var ret = new Vector2D();
			ret.X = InverseMatrix[0, 0] * v.X + InverseMatrix[0, 1] * v.Y + InverseMatrix[0, 2];
			ret.Y = InverseMatrix[1, 0] * v.X + InverseMatrix[1, 1] * v.Y + InverseMatrix[1, 2];
			return ret;
		}

		public static TransformationMatrix CreateScale(double s)
		{
			var ret = new TransformationMatrix();
			ret.Matrix[0, 0] = s;
			ret.Matrix[1, 1] = s;

			ret.InverseMatrix[0, 0] = 1/s;
			ret.InverseMatrix[1, 1] = 1/s;
			return ret;
		}

		public static TransformationMatrix CreateScale(double sx,double sy)
		{
			var ret = new TransformationMatrix();
			ret.Matrix[0, 0] = sx;
			ret.Matrix[1, 1] = sy;
			ret.InverseMatrix[0, 0] = 1/sx;
			ret.InverseMatrix[1, 1] = 1/sy;
			return ret;
		}

		public static TransformationMatrix CreateScale(double CenterX, double CenterY, double factor)
		{
			return CreateTranslation(CenterX, CenterY) * CreateScale(factor) * CreateTranslation(-CenterX, -CenterY);
		}

		public static TransformationMatrix CreateScale(double CenterX, double CenterY, double factorX, double factorY)
		{
			return CreateTranslation(CenterX, CenterY) * CreateScale(factorX,factorY) * CreateTranslation(-CenterX, -CenterY);
		}

		public static TransformationMatrix CreateTranslation(double tx, double ty)
		{
			var ret = new TransformationMatrix();
			ret.Matrix[ 0,2] = tx;
			ret.Matrix[ 1,2] = ty;

			ret.InverseMatrix[0, 2] = -tx;
			ret.InverseMatrix[1, 2] = -ty;
			return ret;
		}


	}
}
