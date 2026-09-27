using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace ChatBureau {
 public class LookImage {public string Role;public byte[] Data;}
 public class LookPackage {public int Format=1;public string Title;public Preferences Look=new Preferences();public List<LookImage> Images=new List<LookImage>();}
 static class Wardrobe {
  public static string Folder {get{return Path.Combine(Path.GetDirectoryName(Preferences.FilePath),"wardrobe");}}
  public static string ImageFolder {get{return Path.Combine(Path.GetDirectoryName(Preferences.FilePath),"images");}}
  public static void Apply(Preferences source,Preferences target){Appearance.CopyLook(source,target);target.PngPath=source.PngPath;target.WalkFrames=(string[])source.WalkFrames.Clone();target.RestFrames=(string[])source.RestFrames.Clone();target.FrameRate=source.FrameRate;target.MirrorPng=source.MirrorPng;}
  public static void Export(string path,string title,Preferences value){
   var package=new LookPackage{Title=string.IsNullOrWhiteSpace(title)?"Mon style":title.Trim(),Look=value.Copy()};package.Look.Validate();if(package.Title.Length>40)package.Title=package.Title.Substring(0,40);
   if(value.UsePng){Add(package,"image",value.PngPath);foreach(string frame in value.WalkFrames)Add(package,"walk",frame);foreach(string frame in value.RestFrames)Add(package,"rest",frame);}
   // Only appearance is shared; no machine paths or behavioral preferences.
   var visual=new Preferences();Apply(package.Look,visual);visual.PngPath="";visual.WalkFrames=new string[0];visual.RestFrames=new string[0];package.Look=visual;
   using(var buffer=new MemoryStream()){new XmlSerializer(typeof(LookPackage)).Serialize(buffer,package);if(buffer.Length>48*1024*1024)throw new IOException("Le style dépasse 48 Mo. Réduisez les images avant de l’exporter.");string temp=path+".tmp";File.WriteAllBytes(temp,buffer.ToArray());File.Copy(temp,path,true);File.Delete(temp);}
  }
  static void Add(LookPackage package,string role,string path){using(var b=PngArt.Read(path))using(var bytes=new MemoryStream()){b.Save(bytes,System.Drawing.Imaging.ImageFormat.Png);long total=bytes.Length;foreach(var image in package.Images)total+=image.Data.Length;if(total>32*1024*1024)throw new IOException("Les images dépassent 32 Mo. Réduisez leur taille.");package.Images.Add(new LookImage{Role=role,Data=bytes.ToArray()});}}
  public static LookPackage Import(string path,string imageFolder){
   if(new FileInfo(path).Length>48*1024*1024)throw new IOException("Le fichier dépasse 48 Mo.");LookPackage pack;
   using(var reader=XmlReader.Create(path,new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null,MaxCharactersInDocument=64*1024*1024}))pack=(LookPackage)new XmlSerializer(typeof(LookPackage)).Deserialize(reader);
   if(pack==null||pack.Format!=1||pack.Look==null||pack.Images==null||pack.Images.Count>49)throw new IOException("Ce fichier de style n’est pas compatible.");
   pack.Title=string.IsNullOrWhiteSpace(pack.Title)?"Style importé":pack.Title.Trim();if(pack.Title.Length>40)pack.Title=pack.Title.Substring(0,40);pack.Look.Validate();pack.Look.PngPath="";pack.Look.WalkFrames=new string[0];pack.Look.RestFrames=new string[0];
   var walk=new List<string>();var rest=new List<string>();long pixels=0;string temporary=Path.Combine(Path.GetTempPath(),"ChatBureau-look-"+Guid.NewGuid().ToString("N")+".png");
   try{foreach(var image in pack.Images){if(image==null||image.Data==null||image.Data.Length>16*1024*1024||(image.Role!="image"&&image.Role!="walk"&&image.Role!="rest"))throw new IOException("Image du style invalide.");File.WriteAllBytes(temporary,image.Data);using(var b=PngArt.Read(temporary)){pixels+=(long)b.Width*b.Height;if(pixels>48*1024*1024)throw new IOException("Les images du style sont trop grandes.");if(image.Role!="image"&&(b.Width>1024||b.Height>1024))throw new IOException("Image animée trop grande.");}string saved=PngArt.Keep(temporary,imageFolder);if(image.Role=="image"){if(pack.Look.PngPath.Length>0)throw new IOException("Image principale en double.");pack.Look.PngPath=saved;}else if(image.Role=="walk")walk.Add(saved);else rest.Add(saved);}
   }finally{if(File.Exists(temporary))File.Delete(temporary);}
   if(walk.Count>24||rest.Count>24||walk.Count==1||rest.Count==1||(pack.Look.UsePng&&pack.Look.PngPath.Length==0))throw new IOException("Séquence ou personnage incomplet.");pack.Look.WalkFrames=walk.ToArray();pack.Look.RestFrames=rest.ToArray();pack.Images.Clear();return pack;
  }
  public static string Save(string title,Preferences value,string folder=null){folder=folder??Folder;Directory.CreateDirectory(folder);string path=Path.Combine(folder,Guid.NewGuid().ToString("N")+".chatlook");Export(path,title,value);return path;}
 }
}
