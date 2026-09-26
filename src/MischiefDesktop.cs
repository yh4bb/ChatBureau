using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;

namespace ChatBureau {
 sealed class MischiefDesktop:IMischiefDesktop {
  readonly Cat cat;
  public MischiefDesktop(Cat cat){this.cat=cat;}
  public Rectangle PetBounds{get{return cat.Bounds;}}
  public Point Pointer{get{return Cursor.Position;}}
  public Rectangle Area(Point point){return Screen.FromPoint(point).WorkingArea;}
  public void Move(Point point){cat.Location=point;}
  public void Paw(){cat.Play("Salut");}
  public bool EscapePressed{get{return (GetAsyncKeyState(0x1B)&0x8000)!=0;}}
  public bool UserBusy{get{return InputBusy();}}
  public Rectangle? ForegroundBounds(){IntPtr window=GetForegroundWindow();uint pid;GetWindowThreadProcessId(window,out pid);if(window==IntPtr.Zero||pid==(uint)Process.GetCurrentProcess().Id||IsIconic(window))return null;RECT r;return GetWindowRect(window,out r)?(Rectangle?)Rectangle.FromLTRB(r.Left,r.Top,r.Right,r.Bottom):null;}
  public bool IsForeground(PrankWindow target){return Matches(target);}
  [StructLayout(LayoutKind.Sequential)] struct RECT {public int Left,Top,Right,Bottom;}
  [StructLayout(LayoutKind.Sequential)] struct LASTINPUTINFO {public uint Size,Time;}
  [StructLayout(LayoutKind.Sequential)] struct GUIINFO {public uint Size,Flags;public IntPtr Active,Focus,Capture,MenuOwner,MoveSize,Caret;public RECT CaretRect;}
  delegate bool EnumWindow(IntPtr handle,IntPtr data);
  [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
  [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr window,out uint pid);
  [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr window,out RECT rect);
  [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr window);
  [DllImport("user32.dll")] static extern bool IsIconic(IntPtr window);
  [DllImport("user32.dll")] static extern IntPtr GetWindow(IntPtr window,uint command);
  [DllImport("user32.dll")] static extern bool EnumWindows(EnumWindow callback,IntPtr parameter);
  [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern int GetWindowText(IntPtr window,StringBuilder text,int length);
  [DllImport("user32.dll")] static extern short GetAsyncKeyState(int key);
  [DllImport("user32.dll")] static extern bool GetLastInputInfo(ref LASTINPUTINFO info);
  [DllImport("user32.dll")] static extern bool GetGUIThreadInfo(uint thread,ref GUIINFO info);
  static bool InputBusy(){var input=new LASTINPUTINFO{Size=(uint)Marshal.SizeOf(typeof(LASTINPUTINFO))};if(!GetLastInputInfo(ref input)||(uint)((uint)Environment.TickCount-input.Time)<1500)return true;var gui=new GUIINFO{Size=(uint)Marshal.SizeOf(typeof(GUIINFO))};if(!GetGUIThreadInfo(0,ref gui)||(gui.Flags&0x1E)!=0||gui.MenuOwner!=IntPtr.Zero||gui.Capture!=IntPtr.Zero||gui.Caret!=IntPtr.Zero)return true;foreach(int key in new int[]{1,2,4,0x10,0x11,0x12,0x1B,0x5B,0x5C})if((GetAsyncKeyState(key)&0x8000)!=0)return true;return false;}
  static bool Matches(PrankWindow target){
   if(target==null||GetForegroundWindow()!=target.Handle||!IsWindowVisible(target.Handle)||IsIconic(target.Handle))return false;
   uint pid;GetWindowThreadProcessId(target.Handle,out pid);if(pid!=target.ProcessId)return false;
   try{using(var process=Process.GetProcessById((int)pid))return process.StartTime.ToUniversalTime().Ticks==target.ProcessStarted;}catch(ArgumentException){return false;}catch(InvalidOperationException){return false;}catch(System.ComponentModel.Win32Exception){return false;}
  }
  public static List<PrankWindow> Windows(){
   var result=new List<PrankWindow>();int own=Process.GetCurrentProcess().Id;
   EnumWindows(delegate(IntPtr window,IntPtr unused){
    if(!IsWindowVisible(window)||GetWindow(window,4)!=IntPtr.Zero)return true;var title=new StringBuilder(256);if(GetWindowText(window,title,title.Capacity)==0)return true;
    uint pid;GetWindowThreadProcessId(window,out pid);if(pid==own||pid==0)return true;
    try{using(var process=Process.GetProcessById((int)pid)){
     string name=process.ProcessName.ToLowerInvariant();
     // These surfaces have no place in a desktop pet's target picker.
     if(name.Contains("chatbureau")||name.Contains("powershell")||name=="cmd"||name=="pwsh"||name.Contains("terminal")||name=="consent"||name.Contains("credential")||name.Contains("keepass")||name.Contains("1password")||name.Contains("bitwarden")||name.Contains("lockapp")||name.Contains("securityhealth"))return true;
     result.Add(new PrankWindow{Handle=window,ProcessId=(int)pid,ProcessStarted=process.StartTime.ToUniversalTime().Ticks,Title=process.ProcessName+" · "+title});
    }}catch(ArgumentException){}catch(InvalidOperationException){}catch(System.ComponentModel.Win32Exception){}return true;
   },IntPtr.Zero);
   result.Sort((a,b)=>string.Compare(a.Title,b.Title,StringComparison.CurrentCultureIgnoreCase));return result;
  }
  public Task<bool> Scroll(PrankWindow target,bool down,CancellationToken token){
   return ScrollCore(target,down,token,()=>AutomationElement.FocusedElement,()=>Matches(target)&&!InputBusy());
  }
  internal static Task<bool> ScrollCore(PrankWindow target,bool down,CancellationToken token,Func<AutomationElement> focus,Func<bool> eligible){
   // UIA stays on a worker thread. At most one request can be in flight. No
   // keystrokes, mouse clicks, focus changes, document text or InvokePattern.
   return Task.Run(()=>{
    try{
     if(token.IsCancellationRequested||!eligible())return false;
     var focused=focus();if(focused==null)return false;
     var element=focused;ScrollPattern scroll=null;
     for(int i=0;element!=null&&i<18;i++){
      if(token.IsCancellationRequested)return false;
      var current=element.Current;
      if(current.IsPassword||current.ControlType==ControlType.Edit||current.ControlType==ControlType.ComboBox||current.ControlType==ControlType.Slider||current.ControlType==ControlType.Spinner||current.ControlType==ControlType.Menu||current.ControlType==ControlType.MenuItem)return false;
      // Only scroll read-only documents/containers. Editable documents (Word,
      // contenteditable, etc.) exposing a ValuePattern are deliberately skipped.
      object value;if(element.TryGetCurrentPattern(ValuePattern.Pattern,out value)&&!((ValuePattern)value).Current.IsReadOnly)return false;
      object pattern;if(element.TryGetCurrentPattern(ScrollPattern.Pattern,out pattern)&&((ScrollPattern)pattern).Current.VerticallyScrollable)scroll=(ScrollPattern)pattern;
      if(current.NativeWindowHandle==target.Handle.ToInt64())break;
      element=TreeWalker.ControlViewWalker.GetParent(element);
     }
     if(element==null||element.Current.NativeWindowHandle!=target.Handle.ToInt64()||scroll==null)return false;
     if(token.IsCancellationRequested||!eligible())return false;
     scroll.Scroll(ScrollAmount.NoAmount,down?ScrollAmount.SmallIncrement:ScrollAmount.SmallDecrement);return true;
    }catch(ElementNotAvailableException){return false;}catch(InvalidOperationException){return false;}catch(ArgumentException){return false;}catch(COMException){return false;}catch(UnauthorizedAccessException){return false;}
   });
  }
 }
}
