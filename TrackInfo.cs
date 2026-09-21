using System.Diagnostics;

namespace TerminalRacer;

internal class TrackInfo(string title, uint scale, uint roadWidth, uint bufferZoneSize, string layout, char[] roadTexture, char[] turfTexture)
{
   internal const char RoadChar = '0';
   internal const char TurfChar = '.';
   internal const char StartChar = '>';
   
   private readonly string _layout = layout.Trim() + "\n";
   
   internal readonly string Title = title;
   
   // in order from closest to farthest away
   internal char[] RoadTexture = roadTexture;
   internal char[] TurfTexture = turfTexture;

   internal char[,] GetBlownUp()
   {
      char[,] layoutAsCharArray = GetLayoutAsCharArray();
      char[,] blownUp = new char[Scale((uint) layoutAsCharArray.GetLength(0))+bufferZoneSize, Scale((uint) layoutAsCharArray.GetLength(1))+bufferZoneSize];
      List<(uint, uint)> unscaledRoadPositions = GetUnscaledPositionsOfCharacter(RoadChar, layoutAsCharArray);
      unscaledRoadPositions.AddRange(GetUnscaledPositionsOfCharacter(StartChar, layoutAsCharArray));
      foreach ((uint, uint) point in unscaledRoadPositions)
      {
         blownUp = AddCircle((Scale(point.Item1), Scale(point.Item2)), blownUp);
      }

      List<((uint, uint), (uint, uint))> unscaledConnectedPoints = GetUnscaledPositionsOfConnectedPoints(unscaledRoadPositions);
      foreach (((uint, uint), (uint, uint)) pair in unscaledConnectedPoints)
      {
         (uint, uint) scaledPointOne = (Scale(pair.Item1.Item1), Scale(pair.Item1.Item2));
         (uint, uint) scaledPointTwo = (Scale(pair.Item2.Item1), Scale(pair.Item2.Item2));
         blownUp = AddRect(scaledPointOne, scaledPointTwo, blownUp);
      }
      blownUp = FillEmptyWithTurf(blownUp);
      return blownUp;
   }

   internal Vector3D GetPlayerStartingPosition()
   {
      char[,] layoutAsCharArray = GetLayoutAsCharArray();
      List<(uint, uint)> unscaledRoadPositions = GetUnscaledPositionsOfCharacter(StartChar, layoutAsCharArray);
      Debug.Assert(unscaledRoadPositions.Count == 1);
      (uint, uint) point = unscaledRoadPositions[0];
      return new Vector3D(Scale(point.Item1), 1, Scale(point.Item2));
   }

   private char[,] GetLayoutAsCharArray()
   {
      (uint, uint) layoutDimensions = GetLayoutDimensions();
      char[,] layoutAsCharArray = new char[layoutDimensions.Item1, layoutDimensions.Item2];
      layoutAsCharArray = CopyLayoutIntoArray(layoutAsCharArray);
      return layoutAsCharArray;
   }

   private (uint, uint) GetLayoutDimensions()
   {
      uint columns = 0;
      uint rows = 1;
      uint iterationsThisRow = 0;
      foreach (char character in _layout)
      {
         if (character == '\n')
         {
            rows++;
            if (iterationsThisRow > columns)
            {
               columns = iterationsThisRow;
            }
            iterationsThisRow = 0;
         }
         else
         {
            iterationsThisRow++;
         }
      }
      return (columns, rows);
   }

   private char[,] CopyLayoutIntoArray(char[,] layoutAsCharArray)
   {
      uint row = 0;
      uint col = 0;
      foreach (char character in _layout)
      {
         if (character == '\n')
         {
            row++;
            col = 0;
         }
         else
         {
            layoutAsCharArray[col, row] = character;
            col++;
         }
      }

      return layoutAsCharArray;
   }
   
   private char[,] FillEmptyWithTurf(char[,] layoutAsCharArray)
   {
      for (uint row = 0; row < layoutAsCharArray.GetLength(1); row++)
      {
         for (uint col = 0; col < layoutAsCharArray.GetLength(0); col++)
         {
            if (layoutAsCharArray[col, row] == '\0')
               layoutAsCharArray[col, row] = TurfChar;
         }
      }
      return layoutAsCharArray;
   }

   private List<(uint, uint)> GetUnscaledPositionsOfCharacter(char character, char[,] layoutAsCharArray)
   {
      List<(uint, uint)> scaledCharacterCoordinates = new List<(uint, uint)>();
      for (uint row = 0; row < layoutAsCharArray.GetLength(1); row++)
      {
         for (uint col = 0; col < layoutAsCharArray.GetLength(0); col++)
         {
            if (layoutAsCharArray[col, row] == character)
            {
               scaledCharacterCoordinates.Add((col, row));
            }
         }
      }
      return scaledCharacterCoordinates;
   }

   private char[,] AddCircle((uint, uint) position, char[,] blownUp)
   {
      uint h = position.Item1;
      uint k = position.Item2;
      double r = roadWidth / 2d;
      // always uses floor to prevent creating a position that is not within the circle's position
      // which would create a radical with a negative number under it
      uint x1 = h - (uint) Math.Floor(r);
      uint x2 = h + (uint) Math.Floor(r);
      for (uint x = x1; x <= x2; x++)
      {
         double sqrt = Math.Sqrt(Math.Pow(r, 2) - Math.Pow((int) x - (int) h, 2));
         uint y1 = k - (uint) Math.Floor(sqrt);
         uint y2 = k + (uint) Math.Floor(sqrt);
         for (uint y = y1; y <= y2; y++)
         {
            blownUp[x, y] = RoadChar;
         }
      }
      return blownUp;
   }

   private List<((uint, uint), (uint, uint))> GetUnscaledPositionsOfConnectedPoints(List<(uint, uint)> pointList)
   {
      List<((uint, uint), (uint, uint))> connectedPointsList = new List<((uint, uint), (uint, uint))>();
      foreach ((int, int) pointOne in pointList)
      {
         foreach ((int, int) pointTwo in pointList)
         {
            if (
               pointOne.Item1 - 1 <= pointTwo.Item1 && pointTwo.Item1 <= pointOne.Item1 + 1 &&
               pointOne.Item2 - 1 <= pointTwo.Item2 && pointTwo.Item2 <= pointOne.Item2 + 1
            )
            {
               ((uint, uint), (uint, uint)) connectedPoints = (((uint, uint)) pointOne, ((uint, uint)) pointTwo);
               if (IsDuplicate(connectedPointsList, connectedPoints))
                  continue;
               if (pointOne == pointTwo)
                  continue;
               connectedPointsList.Add(connectedPoints);
            }
         }
      }

      return connectedPointsList;
   }

   private bool IsDuplicate(List<((uint, uint), (uint, uint))> pointList, ((uint, uint), (uint, uint)) connectedPoints)
   {
      foreach (((uint, uint), (uint, uint)) pair in pointList)
      {
         // only checks with points in opposite order because there could not possibly be a duplicate in the same order
         if (pair.Item1 == connectedPoints.Item2 && pair.Item2 == connectedPoints.Item1) return true;
      }

      return false;
   }
   
   private char[,] AddRect((uint, uint) pointOne, (uint, uint) pointTwo, char[,] blownUp)
   {
      double theta = GetTheta(pointOne, pointTwo);
      double d = roadWidth / 2.0 + 1.0;
      double rightAngle = Math.PI / 2.0;

      (uint, uint)[] corners =
      [
         TranslateAtAngle(pointOne, theta+rightAngle, d),
         TranslateAtAngle(pointOne, theta-rightAngle, d),
         TranslateAtAngle(pointTwo, theta+rightAngle, d),
         TranslateAtAngle(pointTwo, theta-rightAngle, d)
      ];

      (double, double)[] rotatedCorners =
      [
         RotatePoint(corners[0], -theta),
         RotatePoint(corners[1], -theta),
         RotatePoint(corners[2], -theta),
         RotatePoint(corners[3], -theta)
      ];
      
      // the range in which any point COULD be a part of the rectangle
      uint minX = pointOne.Item1;
      uint minY = pointOne.Item2;
      uint maxX = pointOne.Item1;
      uint maxY = pointOne.Item2;
      
      // the range in which EVERY point is part of the rotated rectangle
      double minRotatedX = rotatedCorners[0].Item1;
      double maxRotatedX = rotatedCorners[0].Item1;
      double minRotatedY = rotatedCorners[0].Item2;
      double maxRotatedY = rotatedCorners[0].Item2;
      
      foreach ((uint, uint) point in corners)
      {
         if (point.Item1 < minX) minX = point.Item1;
         if (point.Item1 > maxX) maxX = point.Item1;
         if (point.Item2 < minY) minY = point.Item2;
         if (point.Item2 > maxY) maxY = point.Item2;
      }

      foreach ((double, double) point in rotatedCorners)
      {
         if (point.Item1 < minRotatedX) minRotatedX = point.Item1;
         if (point.Item1 > maxRotatedX) maxRotatedX = point.Item1;
         if (point.Item2 < minRotatedY) minRotatedY = point.Item2;
         if (point.Item2 > maxRotatedY) maxRotatedY = point.Item2;
      }

      for (uint x = minX; x <= maxX; x++)
      {
         for (uint y = minY; y <= maxY; y++)
         {
            (double, double) rotatedPoint = RotatePoint((x, y), -theta);
            if (
               minRotatedX < rotatedPoint.Item1 && rotatedPoint.Item1 < maxRotatedX 
                                          && minRotatedY < rotatedPoint.Item2 && rotatedPoint.Item2 < maxRotatedY
            )
            {
               blownUp[x, y] = RoadChar;
            }
         }
      }
      return blownUp;
   }

   private (double, double) RotatePoint((uint, uint) point, double theta)
   {
      double x = point.Item1;
      double y = point.Item2;
      double newX = x * Math.Cos(theta) - y * Math.Sin(theta);
      double newY = x * Math.Sin(theta) + y * Math.Cos(theta);
      return (newX, newY);
   }

   private double GetTheta((uint, uint) pointOne, (uint, uint) pointTwo)
   {
      double slope;
      if (pointOne.Item1 == pointTwo.Item1)
      {
         slope = Double.PositiveInfinity;
      }
      else
      {
         slope =  ((double) pointOne.Item2 - (double) pointTwo.Item2) / ((double) pointOne.Item1 - (double) pointTwo.Item1);
      }
      return Math.Atan(slope);
   }

   private (uint, uint) TranslateAtAngle((uint, uint) point, double theta, double distance)
   {
      return (
         (uint) ((int) point.Item1 + (int) (distance * Math.Cos(theta))), 
         (uint) ((int) point.Item2 + (int) (distance * Math.Sin(theta)))
         );
   }

   private uint Scale(uint x)
   {
      return x * scale + bufferZoneSize;
   }
}