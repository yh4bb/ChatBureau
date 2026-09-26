using System;
using System.Drawing;

namespace ChatBureau {
 // Geometry is independent of Windows and of input injection so it can be tested
 // without touching another application or the user's mouse.
 static class MischiefMotion {
  public static Point Clamp(Point point,Size pet,Rectangle area){return new Point(Math.Max(area.Left,Math.Min(point.X,area.Right-pet.Width)),Math.Max(area.Top,Math.Min(point.Y,area.Bottom-pet.Height)));}
  public static Point NearPointer(Point pointer,Size pet,Rectangle area){return Clamp(new Point(pointer.X-pet.Width+24,pointer.Y-pet.Height+18),pet,area);}
  public static Point OnWindow(Rectangle window,Size pet,Rectangle area){return Clamp(new Point(window.Left+Math.Min(window.Width/3,180),window.Top-pet.Height+12),pet,area);}
  public static Point Approach(Point current,Point target,int step){int dx=target.X-current.X,dy=target.Y-current.Y;double distance=Math.Sqrt((double)dx*dx+(double)dy*dy);if(distance<=step||distance==0)return target;return new Point(current.X+(int)Math.Round(dx/distance*step),current.Y+(int)Math.Round(dy/distance*step));}
 }
}
