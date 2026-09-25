using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ChatBureau {
 static class Appearance {
  public static readonly string[] Patterns={"Uni","Tigré","Taches","Bicolore","Pois","Dos sombre","Siamois","Masque","Ventre clair","Marbré"};
  public static readonly string[] Hats={"Aucun","Bonnet","Couronne","Nœud","Fleur","Casquette","Haut-de-forme","Chapeau de sorcier"};
  public static readonly string[] Eyes={"Ronds","Grands","Endormis","Étoiles","Cœurs","Clin d’œil"};
  public static readonly string[] ColorNames={"Ivoire","Charbon","Abricot","Sauge","Neige","Cacao","Lavande","Pêche","Brume","Miel","Rose","Azur"};
  public static readonly Color[] Colors={Color.FromArgb(235,234,219),Color.FromArgb(53,55,52),Color.FromArgb(226,173,122),Color.FromArgb(160,184,159),Color.FromArgb(246,246,240),Color.FromArgb(120,88,70),Color.FromArgb(179,163,211),Color.FromArgb(239,183,163),Color.FromArgb(164,180,191),Color.FromArgb(220,181,98),Color.FromArgb(225,165,189),Color.FromArgb(141,184,218)};
  public static readonly string[] Styles={"Café crème","Clair de lune","Jardin secret","Petit sorcier","Nuage rose","Capitaine"};
  // A look never changes the name, size, behavior, transparency or PNG path.
  public static void CopyLook(Preferences source,Preferences target){target.Coat=source.Coat;target.Marking=source.Marking;target.Eyes=source.Eyes;target.Accessory=source.Accessory;target.Pattern=source.Pattern;target.Hat=source.Hat;target.EyeStyle=source.EyeStyle;target.Collar=source.Collar;target.Blush=source.Blush;target.Whiskers=source.Whiskers;target.Nose=source.Nose;target.EarTint=source.EarTint;target.InnerEars=source.InnerEars;target.Glasses=source.Glasses;target.Heterochromia=source.Heterochromia;target.OtherEye=source.OtherEye;target.UsePng=source.UsePng;}
  public static void ApplyStyle(Preferences target,int index){
   if(index<0||index>=Styles.Length)throw new ArgumentOutOfRangeException("index");
   var look=new Preferences{InnerEars=true,Whiskers=true};
   switch(index){
    case 0:look.Coat=Colors[0].ToArgb();look.Pattern="Siamois";look.Marking=Colors[5].ToArgb();look.Accessory=Color.FromArgb(155,100,67).ToArgb();look.Collar=true;break;
    case 1:look.Coat=Colors[1].ToArgb();look.Eyes=Colors[9].ToArgb();look.Pattern="Ventre clair";look.Marking=Colors[8].ToArgb();look.Heterochromia=true;look.Hat="Haut-de-forme";break;
    case 2:look.Coat=Colors[3].ToArgb();look.Pattern="Taches";look.Marking=Color.FromArgb(103,137,105).ToArgb();look.Hat="Fleur";look.Accessory=Colors[10].ToArgb();look.Blush=true;break;
    case 3:look.Coat=Colors[6].ToArgb();look.Pattern="Marbré";look.Marking=Color.FromArgb(124,109,162).ToArgb();look.Hat="Chapeau de sorcier";look.EyeStyle="Étoiles";look.Accessory=Color.FromArgb(84,67,133).ToArgb();break;
    case 4:look.Coat=Colors[4].ToArgb();look.Pattern="Ventre clair";look.Marking=Colors[10].ToArgb();look.Hat="Nœud";look.Accessory=Colors[10].ToArgb();look.EyeStyle="Cœurs";look.Blush=true;look.Nose=Color.FromArgb(186,103,136).ToArgb();break;
    case 5:look.Coat=Colors[2].ToArgb();look.Pattern="Masque";look.Marking=Colors[5].ToArgb();look.Eyes=Colors[4].ToArgb();look.Hat="Casquette";look.Accessory=Color.FromArgb(62,112,153).ToArgb();look.Glasses=true;break;
   }
   CopyLook(look,target);
  }
  public static void Randomize(Preferences target,Random random){
   ApplyStyle(target,random.Next(Styles.Length));target.Coat=Colors[random.Next(Colors.Length)].ToArgb();target.Pattern=Patterns[random.Next(Patterns.Length)];target.Hat=Hats[random.Next(Hats.Length)];target.EyeStyle=Eyes[random.Next(Eyes.Length)];target.Eyes=(Color.FromArgb(target.Coat).GetBrightness()<0.4?Colors[4]:Colors[1]).ToArgb();target.Accessory=Colors[random.Next(Colors.Length)].ToArgb();
  }
  public static void DrawPattern(Graphics g,Preferences p,Brush marking,Pen stripe){
   if(p.Pattern=="Siamois"){g.FillEllipse(marking,107,74,38,35);g.FillEllipse(marking,93,49,18,23);g.FillEllipse(marking,116,51,18,23);}
   if(p.Pattern=="Masque")g.FillEllipse(marking,110,76,32,18);
   if(p.Pattern=="Ventre clair")g.FillEllipse(marking,42,106,72,25);
   if(p.Pattern=="Marbré")for(int x=42;x<100;x+=16)g.DrawBezier(stripe,x,83,x-10,102,x+22,93,x+4,125);
  }
  static PointF[] Star(float x,float y,float radius){var points=new PointF[10];for(int i=0;i<10;i++){double a=-Math.PI/2+i*Math.PI/5;float r=i%2==0?radius:radius*0.45f;points[i]=new PointF(x+(float)Math.Cos(a)*r,y+(float)Math.Sin(a)*r);}return points;}
  public static void DrawEye(Graphics g,Brush brush,string style,float x,float y,bool second){
   if(style=="Clin d’œil"&&second){using(var p=new Pen(brush,1.6f))g.DrawArc(p,x,y,5,3,0,180);return;}
   if(style=="Étoiles"){g.FillPolygon(brush,Star(x+2,y+2,4));return;}
   if(style=="Cœurs"){g.FillEllipse(brush,x-1,y-1,4,4);g.FillEllipse(brush,x+2,y-1,4,4);g.FillPolygon(brush,new PointF[]{new PointF(x-1,y+1),new PointF(x+6,y+1),new PointF(x+2.5f,y+6)});return;}
   float d=style=="Grands"?5:3;g.FillEllipse(brush,x,y,d,d);
  }
  public static void DrawHat(Graphics g,Preferences p,Brush accent){
   if(p.Hat=="Fleur"){for(int i=0;i<5;i++){double a=i*Math.PI*2/5;g.FillEllipse(accent,100+(float)Math.Cos(a)*7,51+(float)Math.Sin(a)*7,10,10);}g.FillEllipse(Brushes.Gold,102,53,7,7);}
   if(p.Hat=="Casquette"){g.FillPie(accent,98,40,31,31,180,180);using(var pen=new Pen(accent,5)){pen.StartCap=pen.EndCap=LineCap.Round;g.DrawLine(pen,98,56,140,56);}}
   if(p.Hat=="Haut-de-forme"){using(var shape=Ios.Round(new RectangleF(101,29,25,28),3))g.FillPath(accent,shape);g.FillRectangle(Brushes.WhiteSmoke,101,48,25,4);using(var pen=new Pen(accent,5)){pen.StartCap=pen.EndCap=LineCap.Round;g.DrawLine(pen,94,57,134,57);}}
   if(p.Hat=="Chapeau de sorcier"){g.FillPolygon(accent,new Point[]{new Point(97,57),new Point(117,32),new Point(129,57)});g.FillEllipse(accent,91,53,44,9);g.FillPolygon(Brushes.Gold,Star(116,47,4));}
  }
 }
}
