using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
namespace ChatBureau {
 static class AppearanceTests {
  public static void Run(string directory){
   string path=Path.Combine(directory,"appearance-preferences.xml");
   var baseline=new Preferences{Name="Mon chat",Size=160,Speed=0,Frequency=25,Jump=false,Transparency=12,UsePng=true,PngPath="kept.png"};
   for(int i=0;i<Appearance.Styles.Length;i++){
    var p=baseline.Copy();Appearance.ApplyStyle(p,i);p.Save(path);var read=Preferences.LoadFrom(path);
    if(p.Hat!=read.Hat||p.Pattern!=read.Pattern||p.EyeStyle!=read.EyeStyle||p.InnerEars!=read.InnerEars||p.Glasses!=read.Glasses||p.Nose!=read.Nose||p.Heterochromia!=read.Heterochromia||p.OtherEye!=read.OtherEye||p.EarTint!=read.EarTint)throw new Exception("New appearance fields did not persist");
    if(read.Name!=baseline.Name||read.Size!=160||read.Speed!=0||read.Frequency!=25||read.Jump||read.Transparency!=12||read.PngPath!="kept.png"||read.UsePng)throw new Exception("Style overwrote unrelated preferences");
    Appearance.CopyLook(baseline,p);if(!p.UsePng||p.Hat!=baseline.Hat||p.Coat!=baseline.Coat||p.InnerEars!=baseline.InnerEars)throw new Exception("Undo did not restore previous look");
   }
   File.WriteAllText(path,"<Preferences><Name>Ancien chat</Name><Hat>Couronne</Hat><Eyes>-1</Eyes></Preferences>");var legacy=Preferences.LoadFrom(path);if(legacy.Name!="Ancien chat"||legacy.Hat!="Couronne"||legacy.InnerEars||legacy.Glasses||legacy.Heterochromia||legacy.Nose!=0)throw new Exception("Legacy appearance changed");
   var random=new Random(42);for(int i=0;i<100;i++){var p=baseline.Copy();Appearance.Randomize(p,random);string hat=p.Hat,pattern=p.Pattern,eyes=p.EyeStyle;p.Validate();if(p.Hat!=hat||p.Pattern!=pattern||p.EyeStyle!=eyes||p.Name!=baseline.Name)throw new Exception("Random style invalid");}
   foreach(string hat in Appearance.Hats)using(var frame=new Bitmap(160,140)){using(var g=Graphics.FromImage(frame))Cat.Draw(g,1,1.1,false,false,false,Color.Beige,"Saut",0.5f,new Preferences{Hat=hat});for(int x=0;x<160;x++)if(frame.GetPixel(x,0).A!=0)throw new Exception("Hat clipped at jump apex: "+hat);}
   var looks=new List<Preferences>();var titles=new List<string>();
   for(int i=0;i<Appearance.Styles.Length;i++){var p=new Preferences();Appearance.ApplyStyle(p,i);looks.Add(p);titles.Add(Appearance.Styles[i]);}
   for(int i=6;i<Appearance.Patterns.Length;i++){looks.Add(new Preferences{Pattern=Appearance.Patterns[i],InnerEars=true});titles.Add(Appearance.Patterns[i]);}
   for(int i=4;i<Appearance.Hats.Length;i++){looks.Add(new Preferences{Hat=Appearance.Hats[i],InnerEars=true});titles.Add(Appearance.Hats[i]);}
   for(int i=3;i<Appearance.Eyes.Length;i++){looks.Add(new Preferences{EyeStyle=Appearance.Eyes[i],InnerEars=true});titles.Add(Appearance.Eyes[i]);}
   using(var sheet=new Bitmap(1000,((looks.Count+4)/5)*190))using(var g=Graphics.FromImage(sheet))using(var font=new Font("Segoe UI",10)){
    g.Clear(Ios.Background);for(int i=0;i<looks.Count;i++){int x=i%5*200,y=i/5*190;g.DrawString(titles[i],font,Brushes.Black,x+12,y+10);var state=g.Save();g.TranslateTransform(x,y+20);Cat.Draw(g,1.2f,1.1,false,false,false,Color.FromArgb(looks[i].Coat),"Marche",0.5f,looks[i]);g.Restore(state);}sheet.Save(Path.Combine(directory,"new-looks.png"));
   }
   File.WriteAllText(Path.Combine(directory,"appearance-tests.txt"),"PASS: six styles persisted, unrelated preferences preserved, undo restored PNG mode, old preferences retained, 100 random looks validated.");
  }
 }
}
