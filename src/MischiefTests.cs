using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Automation;

namespace ChatBureau {
 static class MischiefTests {
  sealed class Desktop:IMischiefDesktop {
   public Rectangle Bounds=new Rectangle(20,800,160,140);
   public bool Escape,Busy,Foreground=true,Pending;
   public int Scrolls,Paws;
   public CancellationToken Token;
   public TaskCompletionSource<bool> Completion=new TaskCompletionSource<bool>();
   public Rectangle PetBounds{get{return Bounds;}}
   public Point Pointer{get{return new Point(400,450);}}
   public Rectangle Area(Point p){return new Rectangle(-1920,0,3840,1080);}
   public Rectangle? ForegroundBounds(){return new Rectangle(300,300,600,500);}
   public bool EscapePressed{get{return Escape;}}
   public bool UserBusy{get{return Busy;}}
   public bool IsForeground(PrankWindow window){return Foreground;}
   public void Move(Point p){Bounds.Location=p;}
   public void Paw(){Paws++;}
   public Task<bool> Scroll(PrankWindow target,bool down,CancellationToken token){Scrolls++;Token=token;return Pending?Completion.Task:Task.FromResult(true);}
  }
  static void Advance(Mischief controller,long start,long end){for(long t=start;t<=end;t+=100)controller.Tick(t,false);}
  public static void Run(string folder){
   var area=new Rectangle(-1920,0,1920,1040);var pet=new Size(320,280);var p=MischiefMotion.NearPointer(new Point(-1910,10),pet,area);if(!area.Contains(new Rectangle(p,pet)))throw new Exception("Pointer geometry left the selected monitor");p=MischiefMotion.OnWindow(new Rectangle(-1900,0,1800,1040),pet,area);if(!area.Contains(new Rectangle(p,pet)))throw new Exception("Window perch left the monitor");if(MischiefMotion.Approach(new Point(0,0),new Point(3,4),10)!=new Point(3,4))throw new Exception("Motion overshoot");
   var window=new PrankWindow{Handle=new IntPtr(123),ProcessId=456,ProcessStarted=789,Title="Test window"};
   var desktop=new Desktop();using(var c=new Mischief(desktop)){
    if(c.Active)throw new Exception("Pranks active at startup");Advance(c,0,100000);if(desktop.Scrolls!=0||desktop.Paws!=0)throw new Exception("Inactive mode acted");
    c.Start(false,null,15,0);Advance(c,0,10000);if(desktop.Scrolls!=0||desktop.Paws!=1)throw new Exception("Visual mode sent input or failed to animate");
    c.Stop();if(c.Active)throw new Exception("Stop failed");
    c.Start(true,window,15,0);Advance(c,0,10000);if(desktop.Scrolls!=1)throw new Exception("Interactive prank not dispatched once");Advance(c,10100,15000);if(desktop.Scrolls!=1)throw new Exception("Rate limit failed");
   }
   desktop=new Desktop{Foreground=false};using(var c=new Mischief(desktop)){c.Start(true,window,15,0);Advance(c,0,20000);if(desktop.Scrolls!=0||desktop.Paws!=0)throw new Exception("Unselected foreground received a prank");}
   desktop=new Desktop{Busy=true};using(var c=new Mischief(desktop)){c.Start(true,window,15,0);Advance(c,0,20000);if(desktop.Scrolls!=0)throw new Exception("Prank during user input");}
   desktop=new Desktop();using(var c=new Mischief(desktop)){c.Start(true,window,15,0);c.Tick(3000,false);desktop.Foreground=false;Advance(c,3100,10000);if(desktop.Scrolls!=0)throw new Exception("Focus change during approach was ignored");}
   desktop=new Desktop();using(var c=new Mischief(desktop)){c.Start(false,null,15,0);c.Tick(3000,false);desktop.Escape=true;c.Tick(3100,true);if(c.Active||desktop.Bounds.Location!=new Point(20,800))throw new Exception("Escape did not stop and restore position");}
   desktop=new Desktop{Pending=true};using(var c=new Mischief(desktop)){
    c.Start(true,window,15,0);Advance(c,0,20000);if(desktop.Scrolls!=1)throw new Exception("Pending requests accumulated");c.Tick(20100,true);if(!desktop.Token.IsCancellationRequested)throw new Exception("Suspension did not cancel queued interaction");c.Stop();bool rejected=false;try{c.Start(true,window,15,20200);}catch(InvalidOperationException){rejected=true;}if(!rejected)throw new Exception("Pending interaction allowed restart");desktop.Completion.SetResult(false);
   }
   desktop=new Desktop();using(var c=new Mischief(desktop)){bool rejected=false;try{c.Start(true,null,15,0);}catch(InvalidOperationException){rejected=true;}if(!rejected||c.Active)throw new Exception("Targetless interactive mode accepted");}
   TestOwnedWindow();
   using(var c=new Mischief(new Desktop()))MischiefPane.TestControls(c);
   File.WriteAllText(Path.Combine(folder,"mischief-tests.txt"),"PASS: inactive startup, two modes, rate limit, typing pause, foreground changes, Escape, cancellation, pending request limit, target requirement, multi-monitor bounds. Native scroll and edit-field rejection verified on an owned test window. No input sent to user applications.");
  }
  static bool Wait(Task<bool> task){var watch=System.Diagnostics.Stopwatch.StartNew();while(!task.IsCompleted&&watch.ElapsedMilliseconds<8000){Application.DoEvents();Thread.Sleep(10);}if(!task.IsCompleted)throw new Exception("Owned-window UIA test timed out");return task.GetAwaiter().GetResult();}
  static void TestOwnedWindow(){
   using(var window=new Form{Text="ChatBureau · test de défilement",ClientSize=new Size(400,240),ShowInTaskbar=false})using(var list=new ListBox{Dock=DockStyle.Fill})using(var edit=new TextBox{Dock=DockStyle.Top,Text="Ce texte doit rester inchangé"}){
    for(int i=0;i<100;i++)list.Items.Add("Ligne de test "+i);window.Controls.Add(list);window.Controls.Add(edit);window.Show();Application.DoEvents();
    var target=new PrankWindow{Handle=window.Handle};IntPtr listHandle=list.Handle,editHandle=edit.Handle;string unchanged=edit.Text;
    if(!Wait(MischiefDesktop.ScrollCore(target,true,CancellationToken.None,()=>AutomationElement.FromHandle(listHandle),()=>true))||list.TopIndex==0)throw new Exception("Native scroll did not move the owned test list");
    if(Wait(MischiefDesktop.ScrollCore(target,true,CancellationToken.None,()=>AutomationElement.FromHandle(editHandle),()=>true))||edit.Text!=unchanged)throw new Exception("Editable field was not excluded");
    int previous=list.TopIndex;using(var canceled=new CancellationTokenSource()){canceled.Cancel();if(Wait(MischiefDesktop.ScrollCore(target,true,canceled.Token,()=>AutomationElement.FromHandle(listHandle),()=>true))||list.TopIndex!=previous)throw new Exception("Cancelled native scroll executed");}
    if(Wait(MischiefDesktop.ScrollCore(target,true,CancellationToken.None,()=>AutomationElement.FromHandle(listHandle),()=>false))||list.TopIndex!=previous)throw new Exception("Ineligible native target received input");window.Close();
   }
  }
 }
}
