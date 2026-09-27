using System;
using System.Collections.Generic;
using System.Drawing;

namespace ChatBureau {
 // A route advances by elapsed time, with a bounded delta after a stalled frame.
 sealed class PetRoute {
  readonly Queue<Point> stops=new Queue<Point>();
  Point start,end; double elapsed,duration; bool jumping;
  public bool Active {get;private set;}
  public void Clear(){stops.Clear();Active=false;}
  public void Begin(Point origin,IEnumerable<Point> points,bool jump){Clear();start=origin;foreach(var p in points)stops.Enqueue(p);jumping=jump;Next();}
  void Next(){if(stops.Count==0){Active=false;return;}end=stops.Dequeue();elapsed=0;double dx=end.X-start.X,dy=end.Y-start.Y;duration=Math.Max(.25,Math.Sqrt(dx*dx+dy*dy)/180);Active=true;}
  public Point Step(double seconds){if(!Active)return start;elapsed+=Math.Max(0,Math.Min(.05,seconds));double t=Math.Min(1,elapsed/duration);int arc=jumping?(int)(Math.Sin(t*Math.PI)*48):0;var p=new Point((int)Math.Round(start.X+(end.X-start.X)*t),(int)Math.Round(start.Y+(end.Y-start.Y)*t)-arc);if(t>=1){start=end;Next();}return p;}
 }
 static class Character {
  public static readonly string[] Names={"Calme","Joueur","Pot de colle","Farceur"};
  public static int Delay(Preferences p){return p.Frequency*(p.Personality=="Calme"?60:p.Personality=="Farceur"?22:30);}
  public static string Pick(Preferences p,Random random){var enabled=new List<string>(p.Enabled());string[] favored=p.Personality=="Calme"?new[]{"Sieste","Étirement","Bâillement"}:p.Personality=="Pot de colle"?new[]{"Salut","Toilette"}:p.Personality=="Farceur"?new[]{"Secousse","Pirouette","Salut"}:new[]{"Saut","Danse","Rebonds"};foreach(string s in favored)if(enabled.Contains(s)){enabled.Add(s);enabled.Add(s);}return enabled.Count==0?null:enabled[random.Next(enabled.Count)];}
  public static bool ShouldSleep(Preferences p,uint idle){return p.ReactToActivity&&idle>=120000;}
  public static bool CoversScreen(Rectangle window,Rectangle screen){return window.Left<=screen.Left&&window.Top<=screen.Top&&window.Right>=screen.Right&&window.Bottom>=screen.Bottom;}
 }
 sealed class CompanionLife {
  readonly Cat cat; readonly PetRoute route=new PetRoute();
  bool away,returning,perched; Point home; Rectangle? surface; long next=15000,rest,last,surfaceCheck; int visits;
  public bool Resting {get{return away;}}
  public CompanionLife(Cat cat){this.cat=cat;}
  public void Reset(){route.Clear();perched=false;surface=null;returning=false;next=Mischief.Now+15000;away=false;}
  public bool Tick(long now,bool blocked){
   var p=cat.Options;double dt=last==0?.033:(now-last)/1000.0;last=now;
   if(blocked)return route.Active||perched;
   bool absent=Character.ShouldSleep(p,MischiefDesktop.IdleMilliseconds);
   if(absent){if(!away){away=true;route.Clear();perched=false;surface=null;cat.Play("Sieste");}cat.Rest();return true;}
   if(away){away=false;cat.Play("Salut");next=now+8000;return true;}
   if(surface.HasValue&&!returning&&(route.Active||perched)&&now>=surfaceCheck){surfaceCheck=now+1000;if(!MischiefDesktop.Surfaces().Contains(surface.Value)){perched=false;returning=true;surface=null;route.Begin(cat.Location,new[]{MischiefMotion.Clamp(home,cat.Size,System.Windows.Forms.Screen.FromRectangle(cat.Bounds).WorkingArea)},false);}}
   if(route.Active){Point before=cat.Location;cat.Location=MischiefMotion.Clamp(route.Step(dt),cat.Size,System.Windows.Forms.Screen.FromRectangle(cat.Bounds).WorkingArea);cat.Face(cat.Left<before.X);if(!route.Active){if(returning){returning=false;next=now+20000;}else{perched=true;rest=now+5000;}}return true;}
   if(perched){if(now<rest)return true;perched=false;var candidates=MischiefDesktop.Surfaces();Rectangle? other=null;foreach(var r in candidates)if(surface.HasValue&&r!=surface.Value&&Math.Abs(r.Top-surface.Value.Top)<300&&Math.Abs(r.Left-surface.Value.Left)<650){other=r;break;}
    if(other.HasValue&&visits++<2){surface=other;var area=System.Windows.Forms.Screen.FromRectangle(cat.Bounds).WorkingArea;route.Begin(cat.Location,new[]{MischiefMotion.OnWindow(other.Value,cat.Size,area)},true);}else{route.Begin(cat.Location,new[]{MischiefMotion.Clamp(home,cat.Size,System.Windows.Forms.Screen.FromRectangle(cat.Bounds).WorkingArea)},false);returning=true;}return true;}
   if(p.Speed==0)return false;
   if(p.ExploreWindows&&now>=next){next=now+20000;var surfaces=MischiefDesktop.Surfaces();if(surfaces.Count>0){var r=surfaces[0];var area=System.Windows.Forms.Screen.FromRectangle(cat.Bounds).WorkingArea;home=cat.Location;surface=r;visits=0;Point goal=MischiefMotion.OnWindow(r,cat.Size,area);Point foot=MischiefMotion.Clamp(new Point(r.Left-cat.Width/2,cat.Top),cat.Size,area);Point top=new Point(foot.X,goal.Y);route.Begin(cat.Location,new[]{foot,top,goal},false);return true;}}
   if(p.FollowPointer&&(p.Personality=="Pot de colle"||p.Personality=="Farceur")&&MischiefDesktop.IdleMilliseconds<30000){var pointer=System.Windows.Forms.Cursor.Position;int distance=pointer.X-(cat.Left+cat.Width/2);if(Math.Abs(distance)>140&&System.Windows.Forms.Screen.FromPoint(pointer).Bounds==System.Windows.Forms.Screen.FromRectangle(cat.Bounds).Bounds){int step=Math.Min(p.Speed,2)*Math.Sign(distance);cat.Left+=step;cat.Face(step<0);return true;}}
   return false;
  }
 }
}
