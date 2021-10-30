/*
  Box2DX Copyright (c) 2008 Ihar Kalasouski http://code.google.com/p/box2dx
  Box2D original C++ version Copyright (c) 2006-2007 Erin Catto http://www.gphysics.com

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.
*/


using FixMath.NET;
namespace Box2DX.Common
{
	/// <summary>
	/// A 3-by-3 matrix. Stored in column-major order.
	/// </summary>
	public struct Mat33
	{
		/// <summary>
		/// Construct this matrix using columns.
		/// </summary>
		public Mat33(FVec3 c1, FVec3 c2, FVec3 c3)
		{
			Col1 = c1;
			Col2 = c2;
			Col3 = c3;
		}

		/// <summary>
		/// Set this matrix to all zeros.
		/// </summary>
		public void SetZero()
		{
			Col1.SetZero();
			Col2.SetZero();
			Col3.SetZero();
		}

		/// <summary>
		/// Solve A * x = b, where b is a column vector. This is more efficient
		/// than computing the inverse in one-shot cases.
		/// </summary>
		public FVec3 Solve33(FVec3 b)
		{
			Fix64 det = FVec3.Dot(Col1, FVec3.Cross(Col2, Col3));
			Box2DXDebug.Assert(det != Fix64.Zero);
			det = Fix64.One / det;
			FVec3 x = new FVec3();
			x.X = det * FVec3.Dot(b, FVec3.Cross(Col2, Col3));
			x.Y = det * FVec3.Dot(Col1, FVec3.Cross(b, Col3));
			x.Z = det * FVec3.Dot(Col1, FVec3.Cross(Col2, b));
			return x;
		}

		/// <summary>
		/// Solve A * x = b, where b is a column vector. This is more efficient
		/// than computing the inverse in one-shot cases. Solve only the upper
		/// 2-by-2 matrix equation.
		/// </summary>
		public FVec2 Solve22(FVec2 b)
		{
			Fix64 a11 = Col1.X, a12 = Col2.X, a21 = Col1.Y, a22 = Col2.Y;
			Fix64 det = a11 * a22 - a12 * a21;
			Box2DXDebug.Assert(det != Fix64.Zero);
			det = Fix64.One / det;
			FVec2 x = new FVec2();
			x.X = det * (a22 * b.X - a12 * b.Y);
			x.Y = det * (a11 * b.Y - a21 * b.X);
			return x;
		}

		public FVec3 Col1, Col2, Col3;
	}
}