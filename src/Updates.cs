using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace ChatBureau {
 static class Updates {
  public sealed class Release {public Version Version;public string Url,Digest;}
  public static string Root {get{return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"ChatBureau");}}
  public static string Exe {get{return Assembly.GetExecutingAssembly().Location;}}
  public static bool Automatic {get{return !File.Exists(Path.Combine(Root,"updates-disabled"));}set{Directory.CreateDirectory(Root);string p=Path.Combine(Root,"updates-disabled");if(value){if(File.Exists(p))File.Delete(p);}else File.WriteAllText(p,"");}}
  public static bool IsNewer(string tag,string current){Version a,b;return Version.TryParse(tag.TrimStart('v'),out a)&&Version.TryParse(current,out b)&&a>b;}
  public static Release Parse(string json,string repository,string current){
   var obj=new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(json);
   if((bool)obj["draft"]||(bool)obj["prerelease"])return null;
   string tag=(string)obj["tag_name"];if(!Regex.IsMatch(tag,@"^v?\d+\.\d+\.\d+$")||!IsNewer(tag,current))return null;
   foreach(var item in (IEnumerable)obj["assets"]){
    var asset=(Dictionary<string,object>)item;if((string)asset["name"]!="ChatBureau.exe")continue;
    string url=(string)asset["browser_download_url"];Uri uri;
    string expected="/"+repository+"/releases/download/"+tag+"/ChatBureau.exe";
    if(!Uri.TryCreate(url,UriKind.Absolute,out uri)||uri.Scheme!="https"||uri.Host!="github.com"||!string.IsNullOrEmpty(uri.UserInfo)||uri.AbsolutePath!=expected||!uri.IsDefaultPort)throw new InvalidDataException("Adresse de téléchargement inattendue.");
    string digest=asset.ContainsKey("digest")?asset["digest"] as string:null;
    if(digest==null||!Regex.IsMatch(digest,@"^sha256:[a-fA-F0-9]{64}$"))throw new InvalidDataException("GitHub n’a pas fourni l’empreinte de cette version. Réessayez plus tard.");
    long size=Convert.ToInt64(asset["size"]);if(size<=0||size>50*1024*1024)throw new InvalidDataException("Taille de mise à jour invalide.");
    return new Release{Version=new Version(tag.TrimStart('v')),Url=url,Digest=digest.Substring(7)};
   }
   throw new InvalidDataException("La version publiée ne contient pas ChatBureau.exe.");
  }
  class Client:WebClient {
   public Client(){Headers[HttpRequestHeader.UserAgent]="ChatBureau/"+BuildInfo.Version;Headers[HttpRequestHeader.Accept]="application/vnd.github+json";}
   protected override WebRequest GetWebRequest(Uri address){var request=base.GetWebRequest(address);request.Timeout=30000;var http=request as HttpWebRequest;if(http!=null)http.ReadWriteTimeout=30000;return request;}
  }
  public static Release Latest(){
   if(!Regex.IsMatch(BuildInfo.Repository,@"^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$"))throw new InvalidOperationException("Cette compilation n’est pas encore reliée à un dépôt GitHub.");
   ServicePointManager.SecurityProtocol=SecurityProtocolType.Tls12;
   using(var client=new Client())return Parse(client.DownloadString("https://api.github.com/repos/"+BuildInfo.Repository+"/releases/latest"),BuildInfo.Repository,BuildInfo.Version);
  }
  public static string Hash(string path){using(var sha=SHA256.Create())using(var stream=File.OpenRead(path))return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","").ToLowerInvariant();}
  public static string Download(Release release,string storage=null){
   string folder=Path.Combine(storage??Path.Combine(Root,"updates"),release.Version.ToString());Directory.CreateDirectory(folder);string path=Path.Combine(folder,"ChatBureau.exe"),part=path+".part";
   using(var client=new Client())client.DownloadFile(release.Url,part);
   if(!string.Equals(Hash(part),release.Digest,StringComparison.OrdinalIgnoreCase)){File.Delete(part);throw new InvalidDataException("Le téléchargement est incomplet ou altéré. L’installation a été annulée.");}
   var info=FileVersionInfo.GetVersionInfo(part);if(info.FileMajorPart!=release.Version.Major||info.FileMinorPart!=release.Version.Minor||info.FileBuildPart!=release.Version.Build){File.Delete(part);throw new InvalidDataException("Le fichier ne correspond pas à la version annoncée.");}
   File.Copy(part,path,true);File.Delete(part);return path;
  }
  public static void CopyWithBackup(string source,string target){
   source=Path.GetFullPath(source);target=Path.GetFullPath(target);
   if(string.Equals(source,target,StringComparison.OrdinalIgnoreCase)||Path.GetExtension(target).ToLowerInvariant()!=".exe")throw new IOException("Destination de mise à jour invalide.");
   File.Copy(target,target+".previous",true);
   try{File.Copy(source,target,true);}catch{File.Copy(target+".previous",target,true);throw;}
  }
  public static void Install(string[] args){
   try{
    if(args.Length!=3)throw new ArgumentException("Arguments d’installation invalides.");
    string target=Path.GetFullPath(args[1]);int pid=int.Parse(args[2]);
    Process old=null;try{old=Process.GetProcessById(pid);}catch(ArgumentException){}
    if(old!=null)using(old){if(!string.Equals(old.MainModule.FileName,target,StringComparison.OrdinalIgnoreCase))throw new IOException("L’application à remplacer ne correspond pas.");if(!old.WaitForExit(60000))throw new IOException("Fermez ChatBureau avant de relancer la mise à jour.");}
    Exception last=null;
    for(int i=0;i<10;i++){try{CopyWithBackup(Exe,target);last=null;break;}catch(IOException ex){last=ex;Thread.Sleep(500);}}
    if(last!=null)throw last;
    try{Process.Start(new ProcessStartInfo(target){UseShellExecute=false,WorkingDirectory=Path.GetDirectoryName(target)});}catch{File.Copy(target+".previous",target,true);throw;}
   }catch(Exception ex){MessageBox.Show("Installation interrompue : "+ex.Message+"\nLe fichier téléchargé reste disponible dans :\n"+Path.GetDirectoryName(Exe),"ChatBureau",MessageBoxButtons.OK,MessageBoxIcon.Warning);}
  }
  public static async void AutoCheck(){
   if(string.IsNullOrEmpty(BuildInfo.Repository)||!Automatic)return;
   try{
    string file=Path.Combine(Root,"last-update-check");if(File.Exists(file)&&DateTime.UtcNow-File.GetLastWriteTimeUtc(file)<TimeSpan.FromDays(1))return;
    Directory.CreateDirectory(Root);File.WriteAllText(file,DateTime.UtcNow.ToString("O"));
    var release=await Task.Run(()=>Latest());if(release!=null)UpdateWindow.Open(release);
   }catch{ /* Offline startup must not interrupt the companion. Manual check shows details. */ }
  }
  public static void Tests(string directory){
   Directory.CreateDirectory(directory);
   if(!IsNewer("v1.10.0","1.9.0")||IsNewer("v1.0.0","1.0.0")||IsNewer("invalid","1.0.0"))throw new Exception("Version comparison failed");
   string digest=new string('a',64);
   string json="{\"draft\":false,\"prerelease\":false,\"tag_name\":\"v1.1.0\",\"assets\":[{\"name\":\"ChatBureau.exe\",\"size\":1024,\"digest\":\"sha256:"+digest+"\",\"browser_download_url\":\"https://github.com/test/ChatBureau/releases/download/v1.1.0/ChatBureau.exe\"}]}";
   if(Parse(json,"test/ChatBureau","1.0.0").Digest!=digest)throw new Exception("Release parsing failed");
   if(Parse(json,"test/ChatBureau","1.1.0")!=null)throw new Exception("Same version offered");
   bool rejected=false;try{Parse(json.Replace("https://github.com/","https://example.com/"),"test/ChatBureau","1.0.0");}catch(InvalidDataException){rejected=true;}if(!rejected)throw new Exception("External URL accepted");
   string source=Path.Combine(directory,"new.exe"),target=Path.Combine(directory,"installed.exe");File.WriteAllText(source,"new");File.WriteAllText(target,"old");CopyWithBackup(source,target);
   if(File.ReadAllText(target)!="new"||File.ReadAllText(target+".previous")!="old")throw new Exception("Backup/replace failed");
   if(Hash(source)!=Hash(target))throw new Exception("Hash failed");
   rejected=false;try{Parse(json.Replace("sha256:","sha512:"),"test/ChatBureau","1.0.0");}catch(InvalidDataException){rejected=true;}if(!rejected)throw new Exception("Invalid digest accepted");
   if(Parse(json.Replace("\"prerelease\":false","\"prerelease\":true"),"test/ChatBureau","1.0.0")!=null)throw new Exception("Prerelease accepted");
   File.WriteAllText(target,"preserve");try{CopyWithBackup(Path.Combine(directory,"missing.exe"),target);}catch(IOException){}
   if(File.ReadAllText(target)!="preserve")throw new Exception("Failed replacement damaged current executable");
   File.WriteAllText(Path.Combine(directory,"update-tests.txt"),"PASS: versions, releases, origin validation, replacement, backup and SHA-256.");
  }
 }
 class UpdateWindow:Form {
  static UpdateWindow current;
  Updates.Release release;
  readonly Label status=new Label();readonly Button install=new Button();readonly Button check=new Button();bool busy;
  public static void Open(Updates.Release available=null){if(current==null||current.IsDisposed)current=new UpdateWindow(available);current.Show();current.Activate();}
  UpdateWindow(Updates.Release available){
   release=available;Text="ChatBureau · Mises à jour";Font=new Font("Segoe UI",10);ClientSize=new Size(520,265);FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;StartPosition=FormStartPosition.CenterScreen;
   Controls.Add(new Label{Text="ChatBureau "+BuildInfo.Version,Font=new Font("Segoe UI",18,FontStyle.Bold),AutoSize=true,Location=new Point(20,16)});
   status.SetBounds(22,62,475,80);status.Text=available==null?"Vérifiez si une nouvelle version est disponible.":"Version "+available.Version+" disponible.";Controls.Add(status);
   var auto=new CheckBox{Text="Vérifier chaque jour au lancement",Checked=Updates.Automatic,AutoSize=true,Location=new Point(22,153)};auto.CheckedChanged+=delegate{try{Updates.Automatic=auto.Checked;}catch(Exception ex){status.Text=ex.Message;}};Controls.Add(auto);
   check.Text="Vérifier";check.SetBounds(22,204,125,35);check.Click+=async delegate{busy=true;check.Enabled=false;status.Text="Consultation de GitHub…";try{release=await Task.Run(()=>Updates.Latest());status.Text=release==null?"Vous disposez de la dernière version stable.":"Version "+release.Version+" disponible.";install.Enabled=release!=null;}catch(Exception ex){status.Text="Vérification impossible : "+ex.Message;}finally{check.Enabled=true;busy=false;}};Controls.Add(check);
   install.Text="Installer";install.SetBounds(162,204,145,35);install.Enabled=release!=null;Controls.Add(install);
   install.Click+=async delegate{
    busy=true;install.Enabled=false;check.Enabled=false;status.Text="Téléchargement et vérification…";
    try{string file=await Task.Run(()=>Updates.Download(release));status.Text="Installation : le chat va redémarrer.";
     Process.Start(new ProcessStartInfo(file,"--apply-update \""+Updates.Exe+"\" "+Process.GetCurrentProcess().Id){UseShellExecute=false,CreateNoWindow=true});busy=false;Application.Exit();
    }catch(Exception ex){status.Text="Mise à jour impossible : "+ex.Message;busy=false;install.Enabled=true;check.Enabled=true;}
   };
   FormClosing+=delegate(object sender,FormClosingEventArgs e){if(busy)e.Cancel=true;};
  }
 }
}

