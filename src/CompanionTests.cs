using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Xml.Serialization;

namespace ChatBureau {
 static class CompanionTests {
  static void Require(bool value,string message){if(!value)throw new Exception(message);}
  public static void Run(string folder){
   var p=new Preferences{Personality="Pot de colle",Discreet=false,FrameRate=12};string prefs=Path.Combine(folder,"life-preferences.xml");p.Save(prefs);var read=Preferences.LoadFrom(prefs);Require(read.Personality==p.Personality&&!read.Discreet&&read.FrameRate==12,"New preferences did not persist");
   File.WriteAllText(prefs,"<Preferences><Name>Ancien compagnon</Name><Speed>3</Speed></Preferences>");read=Preferences.LoadFrom(prefs);Require(read.Name=="Ancien compagnon"&&read.Speed==3&&read.Discreet&&read.ReactToActivity&&read.WalkFrames.Length==0,"Legacy preferences migration failed");
   p.FrameRate=200;p.Personality="invalid";p.Validate();Require(p.FrameRate==24&&p.Personality=="Joueur","New preference bounds failed");
   Require(Character.ShouldSleep(p,120000)&&!Character.ShouldSleep(p,119999),"Idle threshold failed");p.ReactToActivity=false;Require(!Character.ShouldSleep(p,uint.MaxValue),"Disabled activity reaction still sleeps");
   var screen=new Rectangle(-1920,0,1920,1080);Require(Character.CoversScreen(screen,screen)&&!Character.CoversScreen(new Rectangle(-1920,0,1800,1040),screen),"Fullscreen multi-monitor detection failed");
   p.Personality="Calme";int calm=Character.Delay(p);p.Personality="Farceur";Require(Character.Delay(p)<calm,"Personality cadence is identical");p.Jump=p.Stretch=p.Groom=p.Wave=p.Sleep=p.Dance=p.Spin=p.Bounce=p.Shake=p.Yawn=false;foreach(string name in Character.Names){p.Personality=name;Require(Character.Pick(p,new Random(1))==null,"Personality ignored disabled animations");}
   var route=new PetRoute();Point previous=new Point(20,800);route.Begin(previous,new[]{new Point(200,800),new Point(200,240),new Point(450,240)},false);int steps=0;while(route.Active&&steps++<1000){Point next=route.Step(.033);Require(Math.Abs(next.X-previous.X)<=7&&Math.Abs(next.Y-previous.Y)<=7,"Climb teleported");previous=next;}Require(!route.Active&&previous==new Point(450,240),"Climb route did not finish");
   route.Begin(previous,new[]{new Point(620,240)},true);int minimum=240;while(route.Active){Point next=route.Step(.033);minimum=Math.Min(minimum,next.Y);previous=next;}Require(minimum<220&&previous==new Point(620,240),"Window jump has no arc");
   route.Begin(Point.Empty,new[]{new Point(500,0)},false);Require(route.Step(15).X<=9,"Stalled frame caused teleport");route.Clear();Require(!route.Active,"Route cancellation failed");
   string images=Path.Combine(folder,"sequence-images");Directory.CreateDirectory(images);string one=Path.Combine(images,"01.png"),two=Path.Combine(images,"02.png");
   using(var b=new Bitmap(32,32)){using(var g=Graphics.FromImage(b))g.Clear(Color.Coral);b.Save(one,ImageFormat.Png);using(var g=Graphics.FromImage(b))g.Clear(Color.SkyBlue);b.Save(two,ImageFormat.Png);}
   string kept=Path.Combine(folder,"kept-frames");var frames=SequenceArt.Import(new[]{two,one},kept);Require(frames.Length==2,"Frame sequence import failed");p=new Preferences{UsePng=true,PngPath=frames[0],WalkFrames=frames,RestFrames=new[]{frames[1],frames[0]},FrameRate=10,Hat="Couronne",Name="Private name",Personality="Calme"};
   var copy=p.Copy();copy.WalkFrames[0]="changed";Require(p.WalkFrames[0]!="changed","Frame arrays were shared by preference copies");
   Require(SequenceArt.Frame(p,0,"Marche")==frames[0]&&SequenceArt.Frame(p,.5,"Marche")==frames[1]&&SequenceArt.Frame(p,0,"Sieste")==frames[1],"Animation frame or action selection failed");
   using(var b=new Bitmap(160,140))using(var g=Graphics.FromImage(b)){PngArt.Draw(g,SequenceArt.Frame(p,0,"Marche"),0,"Marche",false);Color a=b.GetPixel(80,80);g.Clear(Color.Transparent);PngArt.Draw(g,SequenceArt.Frame(p,.5,"Marche"),.5,"Marche",false);Require(b.GetPixel(80,80).ToArgb()!=a.ToArgb(),"Imported animation renders identical frames");}
   string package=Path.Combine(folder,"portable.chatlook");Wardrobe.Export(package,"Vacances",p);string xml=File.ReadAllText(package);Require(!xml.Contains(kept)&&!xml.Contains("Private name"),"Export leaked paths or identity");File.Delete(one);File.Delete(two);foreach(string frame in frames)File.Delete(frame);PngArt.Invalidate();var restored=Wardrobe.Import(package,Path.Combine(folder,"restored"));Require(restored.Title=="Vacances"&&restored.Look.Hat=="Couronne"&&restored.Look.WalkFrames.Length==2&&File.Exists(restored.Look.PngPath),"Portable wardrobe lost PNG or animation");
   var target=new Preferences{Name="Keep me",Speed=5,Personality="Farceur",Transparency=12};Wardrobe.Apply(restored.Look,target);Require(target.Name=="Keep me"&&target.Speed==5&&target.Personality=="Farceur"&&target.Transparency==12&&target.UsePng,"Wardrobe changed non-appearance preferences");
   string bad=Path.Combine(folder,"unsafe.chatlook");File.WriteAllText(bad,"<!DOCTYPE x [<!ENTITY test SYSTEM 'file:///not-allowed'>]><LookPackage><Title>&test;</Title></LookPackage>");bool rejected=false;try{Wardrobe.Import(bad,kept);}catch(InvalidOperationException){rejected=true;}catch(System.Xml.XmlException){rejected=true;}Require(rejected,"External XML entities accepted");
   rejected=false;try{SequenceArt.Import(new[]{restored.Look.PngPath},kept);}catch(IOException){rejected=true;}Require(rejected,"Single-frame sequence accepted");
   File.WriteAllText(Path.Combine(folder,"companion-tests.txt"),"PASS: preference migration and isolation, personalities, disabled animations, idle threshold, multi-monitor fullscreen geometry, continuous climb and jump routes, stalled frame bounds, PNG frame/action selection, portable wardrobe round-trip after source removal, behavior preservation, hostile XML rejection.");
  }
 }
}
