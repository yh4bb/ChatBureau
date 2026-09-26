using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace ChatBureau {
 sealed class PrankWindow {
  public IntPtr Handle;
  public int ProcessId;
  public long ProcessStarted;
  public string Title;
  public override string ToString(){return Title;}
 }
 interface IMischiefDesktop {
  Rectangle PetBounds {get;}
  Point Pointer {get;}
  Rectangle Area(Point point);
  Rectangle? ForegroundBounds();
  bool EscapePressed {get;}
  bool UserBusy {get;}
  bool IsForeground(PrankWindow target);
  void Move(Point point);
  void Paw();
  Task<bool> Scroll(PrankWindow target,bool down,CancellationToken token);
 }
 sealed class Mischief:IDisposable {
  readonly IMischiefDesktop desktop;
  CancellationTokenSource cancellation;
  Task<bool> scrolling;
  Point home,destination;
  long next,started;
  int interval,number,stage;
  bool interactive,disposed;
  bool wasSuspended;
  PrankWindow target;
  public bool Active {get;private set;}
  public bool Performing {get{return stage!=0;}}
  public bool Interactive {get{return interactive;}}
  public int IntervalSeconds {get{return interval==0?45:interval/1000;}}
  public PrankWindow SelectedWindow {get{return target;}}
  static readonly System.Diagnostics.Stopwatch clock=System.Diagnostics.Stopwatch.StartNew();
  public static long Now {get{return clock.ElapsedMilliseconds;}}
  public string Status {get;private set;}
  public Mischief(IMischiefDesktop desktop){this.desktop=desktop;Status="Au repos. Choisissez un mode pour commencer.";}
  public void Start(bool interactive,PrankWindow target,int intervalSeconds,long now){
   if(disposed)throw new ObjectDisposedException("Mischief");
   if(interactive&&target==null)throw new InvalidOperationException("Choisissez d’abord une fenêtre ouverte.");
   if(scrolling!=null&&!scrolling.IsCompleted)throw new InvalidOperationException("La fenêtre termine encore la dernière interaction. Réessayez dans un instant.");
   if(scrolling!=null&&scrolling.IsFaulted){var observed=scrolling.Exception;}scrolling=null;
   Stop();this.interactive=interactive;this.target=target;interval=Math.Max(15,Math.Min(180,intervalSeconds))*1000;next=now+3000;number=0;stage=0;wasSuspended=false;home=desktop.PetBounds.Location;Active=true;cancellation=new CancellationTokenSource();Status="Activé pour cette session · Échap pour arrêter.";
  }
  public void Stop(bool restorePosition=true){if(cancellation!=null){cancellation.Cancel();cancellation.Dispose();cancellation=null;}if(restorePosition&&Active&&stage!=0)desktop.Move(MischiefMotion.Clamp(home,desktop.PetBounds.Size,desktop.Area(home)));Active=false;stage=0;Status="Farces arrêtées.";}
  public void Tick(long now,bool suspended){
   if(!Active)return;
   if(desktop.EscapePressed){Stop();return;}
   if(suspended){if(!wasSuspended){cancellation.Cancel();cancellation.Dispose();cancellation=new CancellationTokenSource();}wasSuspended=true;if(stage!=0){desktop.Move(MischiefMotion.Clamp(home,desktop.PetBounds.Size,desktop.Area(home)));stage=0;}next=now+interval;Status="En pause pendant vos réglages ou le déplacement du chat.";return;}wasSuspended=false;
   if(scrolling!=null){
    if(!scrolling.IsCompleted){Status="La fenêtre répond… Échap pour arrêter.";return;}
    bool success=scrolling.Status==TaskStatus.RanToCompletion&&scrolling.Result;
    if(scrolling.IsFaulted){var observed=scrolling.Exception;}
    scrolling=null;Status=success?"Petit coup de patte : la page a défilé.":"Défilement indisponible ici. Le chat attend son prochain tour.";
   }
   if(stage==0){
    if(now<next)return;
    if(desktop.UserBusy){next=now+1500;Status="Le chat attend que vous ayez fini de taper.";return;}
    if(interactive&&!desktop.IsForeground(target)){next=now+1500;Status="En attente de la fenêtre choisie au premier plan.";return;}
    home=desktop.PetBounds.Location;var bounds=desktop.ForegroundBounds();
    if(interactive&&!bounds.HasValue){next=now+interval;return;}
    Point pointer=desktop.Pointer;bool windowPrank=(interactive||number%2==1)&&bounds.HasValue;var area=desktop.Area(windowPrank?new Point(bounds.Value.Left+bounds.Value.Width/2,bounds.Value.Top+bounds.Value.Height/2):pointer);
    destination=windowPrank?MischiefMotion.OnWindow(bounds.Value,desktop.PetBounds.Size,area):MischiefMotion.NearPointer(pointer,desktop.PetBounds.Size,area);
    stage=1;started=now;number++;Status=interactive?"Il s’approche de votre fenêtre…":"Il vient réclamer votre attention…";
   }
   if(interactive&&!desktop.IsForeground(target)){Finish(now);Status="Fenêtre changée : farce annulée.";return;}
   if(stage==1){
    var point=MischiefMotion.Approach(desktop.PetBounds.Location,destination,10);desktop.Move(point);
    if(point==destination||now-started>3000){desktop.Move(destination);desktop.Paw();stage=2;started=now;}
   }else if(stage==2&&now-started>=800){
    stage=3;started=now;
    if(interactive&&!desktop.UserBusy&&desktop.IsForeground(target))scrolling=desktop.Scroll(target,number%2==1,cancellation.Token);
   }else if(stage==3&&now-started>=1400){Finish(now);}
  }
  void Finish(long now){desktop.Move(MischiefMotion.Clamp(home,desktop.PetBounds.Size,desktop.Area(home)));stage=0;next=now+interval;}
  public void Dispose(){if(disposed)return;Stop();disposed=true;}
 }
}
