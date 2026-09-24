using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Security.Cryptography;

namespace ChatBureau {
 static class PngArt {
  static string cachedPath;
  static Bitmap cached;
  public static void Invalidate(){if(cached!=null)cached.Dispose();cached=null;cachedPath=null;}
  public static Bitmap Read(string path){
   if(string.IsNullOrWhiteSpace(path)||!File.Exists(path))throw new IOException("Choisissez un fichier PNG existant.");
   using(var stream=File.OpenRead(path)){
    if(stream.Length>16*1024*1024)throw new IOException("Le fichier dépasse 16 Mo.");
    byte[] h=new byte[24];if(stream.Read(h,0,h.Length)!=h.Length)throw new IOException("Fichier PNG incomplet.");
    byte[] sig={137,80,78,71,13,10,26,10};for(int i=0;i<8;i++)if(h[i]!=sig[i])throw new IOException("Ce fichier n’est pas un PNG valide.");
    uint width=Big(h,16),height=Big(h,20);if(width==0||height==0||width>4096||height>4096)throw new IOException("La taille maximale est de 4 096 × 4 096 pixels.");
    stream.Position=0;
    using(var source=Image.FromStream(stream,true,true)){
     var bitmap=new Bitmap(source.Width,source.Height,PixelFormat.Format32bppArgb);
     using(var g=Graphics.FromImage(bitmap)){g.CompositingMode=CompositingMode.SourceCopy;g.DrawImageUnscaled(source,0,0);}
     return bitmap;
    }
   }
  }
  static uint Big(byte[] b,int i){return ((uint)b[i]<<24)|((uint)b[i+1]<<16)|((uint)b[i+2]<<8)|b[i+3];}
  public static string Keep(string source,string folder){
   using(var bitmap=Read(source)){
    Directory.CreateDirectory(folder);
    using(var bytes=new MemoryStream()){
     bitmap.Save(bytes,ImageFormat.Png);byte[] data=bytes.ToArray();string hash;
     using(var sha=SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(data)).Replace("-","").ToLowerInvariant();
     string target=Path.Combine(folder,hash+".png");if(!File.Exists(target))File.WriteAllBytes(target,data);Invalidate();return target;
    }
   }
  }
  public static bool Draw(Graphics g,string path,double phase,string action,bool love){
   if(path!=cachedPath){Invalidate();cachedPath=path;try{cached=Read(path);}catch(IOException){}catch(ArgumentException){}catch(UnauthorizedAccessException){}catch(OutOfMemoryException){}}
   if(cached==null)return false;
   var state=g.Save();
   try {
    if(action=="Toilette"||action=="Salut"){g.TranslateTransform(80,120);g.RotateTransform((float)Math.Sin(phase*1.4)*5);g.TranslateTransform(-80,-120);}
    float ratio=Math.Min(112f/cached.Width,96f/cached.Height);float w=cached.Width*ratio,h=cached.Height*ratio;
    g.InterpolationMode=InterpolationMode.HighQualityBicubic;
    using(var attributes=new ImageAttributes()){
     attributes.SetWrapMode(WrapMode.TileFlipXY);
     g.DrawImage(cached,Rectangle.Round(new RectangleF((160-w)/2,128-h,w,h)),0,0,cached.Width,cached.Height,GraphicsUnit.Pixel,attributes);
    }
    if(love)using(var pink=new SolidBrush(Color.FromArgb(230,125,145))){float y=20-(float)(phase%3)*2;g.FillEllipse(pink,73,y,9,9);g.FillEllipse(pink,80,y,9,9);g.FillPolygon(pink,new PointF[]{new PointF(73,y+6),new PointF(89,y+6),new PointF(81,y+16)});}
   } finally {g.Restore(state);}
   return true;
  }
  public static void Tests(string directory){
   string source=Path.Combine(directory,"source.png");
   using(var b=new Bitmap(64,128)){using(var g=Graphics.FromImage(b)){g.Clear(Color.Transparent);g.FillEllipse(Brushes.Coral,8,8,48,112);}b.Save(source,ImageFormat.Png);}
   string saved=Keep(source,Path.Combine(directory,"images"));File.Delete(source);
   using(var b=Read(saved)){if(b.Width!=64||b.Height!=128||b.GetPixel(0,0).A!=0)throw new Exception("PNG transparency or proportions lost");}
   var p=new Preferences{UsePng=true,PngPath=saved,MirrorPng=false};string prefs=Path.Combine(directory,"png-preferences.xml");p.Save(prefs);p=Preferences.LoadFrom(prefs);
   if(!p.UsePng||p.PngPath!=saved||p.MirrorPng)throw new Exception("PNG preference persistence failed");
   string[] actions={"Marche","Danse","Pirouette","Rebonds","Secousse","Bâillement"};
   using(var sheet=new Bitmap(640,actions.Length*155))using(var canvas=Graphics.FromImage(sheet)){
    canvas.Clear(Color.FromArgb(30,35,43));
    for(int row=0;row<actions.Length;row++)for(int col=0;col<4;col++){
     using(var frame=new Bitmap(160,140))using(var g=Graphics.FromImage(frame)){
      Cat.Draw(g,1,0.8,false,false,false,Color.White,actions[row],0.15f+col*0.23f,p);
      if(frame.GetPixel(0,0).A!=0)throw new Exception("PNG transparent background lost");
      canvas.DrawImageUnscaled(frame,col*160,row*155+15);
     }
    }
    sheet.Save(Path.Combine(directory,"png-animations.png"));
   }
   File.WriteAllText(source,"not a PNG");bool rejected=false;try{using(var b=Read(source)){}}catch(IOException){rejected=true;}if(!rejected)throw new Exception("Invalid PNG accepted");
   using(var b=new Bitmap(160,140))using(var g=Graphics.FromImage(b)){if(Draw(g,Path.Combine(directory,"missing.png"),0,"Marche",false))throw new Exception("Missing PNG fallback failed");}
   Invalidate();
  }
 }
}

