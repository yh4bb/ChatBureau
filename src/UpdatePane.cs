using System;
using System.Drawing;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatBureau {
 class UpdatePane:UserControl {
  Updates.Release release;
  readonly Label status=new Label(),detail=new Label();
  readonly IosButton check=new IosButton(),install=new IosButton();
  readonly ProgressBar progress=new ProgressBar();
  public bool Busy {get;private set;}
  public Func<bool> BeforeInstall;
  internal Func<Task<Updates.Release>> CheckRelease=()=>Task.Run(()=>Updates.Latest());
  internal Func<Updates.Release,Task<string>> DownloadRelease=r=>Task.Run(()=>Updates.Download(r));
  public UpdatePane(){
   BackColor=Ios.Background;Font=new Font("Segoe UI",10);AutoScroll=false;
   var layout=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoScroll=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(2)};Controls.Add(layout);
   var hero=new IosCard{Width=536,Height=180,FlowDirection=FlowDirection.TopDown,WrapContents=false,Margin=new Padding(0,0,0,12),Padding=new Padding(20)};layout.Controls.Add(hero);
   
   hero.Controls.Add(new Label{Text="ChatBureau "+BuildInfo.Version,ForeColor=Ios.Ink,AutoSize=true,Font=new Font("Segoe UI",22,FontStyle.Bold),Margin=new Padding(0,0,0,12)});
   status.Text="Prêt à rechercher une nouvelle version";status.Size=new Size(480,48);status.ForeColor=Ios.Ink;hero.Controls.Add(status);
   detail.Text="Vos réglages et vos personnages PNG sont conservés.";detail.Size=new Size(480,40);detail.ForeColor=Ios.Muted;hero.Controls.Add(detail);
   var actions=new IosCard{Width=536,Height=124,FlowDirection=FlowDirection.TopDown,WrapContents=false,Margin=new Padding(0,0,0,12)};layout.Controls.Add(actions);
   var buttons=new FlowLayoutPanel{Width=500,Height=49,Margin=new Padding(0)};
   check.Text="Rechercher";check.Size=new Size(155,40);check.Click+=async delegate{await CheckAsync();};buttons.Controls.Add(check);
   install.Text="Installer et redémarrer";install.Size=new Size(240,40);install.BackColor=Ios.Blue;install.ForeColor=Color.White;install.Enabled=false;install.Click+=async delegate{await InstallAsync();};buttons.Controls.Add(install);actions.Controls.Add(buttons);
   progress.Width=488;progress.Height=7;progress.Style=ProgressBarStyle.Marquee;progress.Visible=false;actions.Controls.Add(progress);
   actions.Controls.Add(new Label{Text="Tout se fait ici, sans ouvrir le navigateur.",AutoSize=true,ForeColor=Ios.Muted,Margin=new Padding(0,8,0,0)});
   var options=new IosCard{Width=536,Height=105,FlowDirection=FlowDirection.TopDown,WrapContents=false,Margin=new Padding(0,0,0,12)};layout.Controls.Add(options);
   var automatic=new IosToggle{Text="Recherche quotidienne au lancement",Width=496,Height=42,Checked=Updates.Automatic};automatic.CheckedChanged+=delegate{try{Updates.Automatic=automatic.Checked;}catch(Exception ex){status.Text="Réglage non enregistré : "+ex.Message;}};options.Controls.Add(automatic);
   options.Controls.Add(new Label{Text="Vous choisissez toujours quand installer.",AutoSize=true,ForeColor=Ios.Muted});
  }
  public void Offer(Updates.Release available){if(Busy)return;release=available;status.Text=available==null?"Votre application est à jour.":"La version "+available.Version+" est disponible.";install.Enabled=available!=null;}
  void SetBusy(bool value){Busy=value;check.Enabled=!value;install.Enabled=!value&&release!=null;progress.Visible=value;}
  public async Task CheckAsync(){
   if(Busy)return;release=null;SetBusy(true);status.Text="Recherche en cours…";
   try{OfferAfterCheck(await CheckRelease());}catch(Exception){status.Text="Connexion impossible pour le moment.";detail.Text="Vérifiez votre connexion, puis cliquez sur Rechercher.";}finally{SetBusy(false);}
  }
  void OfferAfterCheck(Updates.Release available){release=available;status.Text=available==null?"Votre application est à jour.":"La version "+available.Version+" est disponible.";detail.Text="Vos réglages et vos personnages PNG sont conservés.";}
  public async Task InstallAsync(){
   if(Busy||release==null)return;if(BeforeInstall!=null&&!BeforeInstall())return;
   SetBusy(true);status.Text="Téléchargement et vérification…";
   try{string file=await DownloadRelease(release);status.Text="Redémarrage de votre compagnon…";if(BeforeInstall!=null&&!BeforeInstall()){SetBusy(false);return;}
    Process.Start(new ProcessStartInfo(file,"--apply-update \""+Updates.Exe+"\" "+Process.GetCurrentProcess().Id){UseShellExecute=false,CreateNoWindow=true});SetBusy(false);Application.Exit();
   }catch(Exception ex){status.Text="L’installation n’a pas pu être terminée.";detail.Text=ex.Message;SetBusy(false);}
  }
  internal string StatusText {get{return status.Text;}}
  internal bool CanInstall {get{return install.Enabled;}}
  internal bool HasHorizontalOverflow {get{return ((ScrollableControl)Controls[0]).HorizontalScroll.Visible;}}
  public static void TestStates(){
   using(var pane=new UpdatePane()){
    pane.CheckRelease=()=>Task.FromResult<Updates.Release>(null);pane.CheckAsync().GetAwaiter().GetResult();if(pane.CanInstall||pane.Busy||!pane.StatusText.Contains("à jour"))throw new Exception("Current update state failed");
    pane.CheckRelease=()=>Task.FromResult(new Updates.Release{Version=new Version("9.0.0")});pane.CheckAsync().GetAwaiter().GetResult();if(!pane.CanInstall||!pane.StatusText.Contains("9.0.0"))throw new Exception("Available update state failed");
    pane.CheckRelease=()=>{throw new InvalidOperationException("offline");};pane.CheckAsync().GetAwaiter().GetResult();if(pane.CanInstall||pane.Busy)throw new Exception("Offline update state failed");
    pane.Offer(new Updates.Release{Version=new Version("9.0.0")});bool called=false;pane.BeforeInstall=()=>false;pane.DownloadRelease=r=>{called=true;return Task.FromResult("");};pane.InstallAsync().GetAwaiter().GetResult();if(called)throw new Exception("Cancelled install downloaded data");
   }
  }
 }
}
