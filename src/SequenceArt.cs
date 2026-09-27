using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;

namespace ChatBureau {
 static class SequenceArt {
  public static string[] ValidatePaths(string[] paths){if(paths==null)return new string[0];var result=new List<string>();foreach(string path in paths)if(!string.IsNullOrWhiteSpace(path)&&result.Count<24)result.Add(path);return result.ToArray();}
  public static string Frame(Preferences p,double phase,string action){string[] frames=action=="Sieste"?p.RestFrames:p.WalkFrames;if(frames==null||frames.Length==0)return p.PngPath;return frames[(int)(Math.Max(0,phase)*.22*p.FrameRate)%frames.Length];}
  public static string[] Import(string[] paths,string folder){if(paths.Length<2||paths.Length>24)throw new IOException("Choisissez entre 2 et 24 PNG pour une animation.");paths=(string[])paths.Clone();Array.Sort(paths,StringComparer.OrdinalIgnoreCase);long pixels=0;Size size=Size.Empty;foreach(string path in paths)using(var b=PngArt.Read(path)){if(b.Width>1024||b.Height>1024)throw new IOException("Chaque image animée doit faire au maximum 1 024 × 1 024 pixels.");if(size!=Size.Empty&&size!=b.Size)throw new IOException("Les images d’une séquence doivent avoir les mêmes dimensions.");size=b.Size;pixels+=(long)b.Width*b.Height;}if(pixels>16*1024*1024)throw new IOException("Cette séquence est trop grande. Réduisez les dimensions des images.");var result=new List<string>();foreach(string path in paths)result.Add(PngArt.Keep(path,folder));return result.ToArray();}
  public static void Keep(Preferences p,string folder){if(!p.UsePng)return;p.PngPath=PngArt.Keep(p.PngPath,folder);if(p.WalkFrames.Length>0)p.WalkFrames=KeepFrames(p.WalkFrames,folder);if(p.RestFrames.Length>0)p.RestFrames=KeepFrames(p.RestFrames,folder);}
  static string[] KeepFrames(string[] paths,string folder){var result=new List<string>();foreach(string path in paths)result.Add(PngArt.Keep(path,folder));return result.ToArray();}
 }
}
