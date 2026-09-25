using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ChatBureau {
 // Supported layered-window opacity works on Windows 10 and 11. It affects the
 // whole studio, never the pet. Do not pretend this is the Windows 11 acrylic API.
 class GlassWindow:Form {
  [DllImport("user32.dll")] static extern bool ReleaseCapture();
  [DllImport("user32.dll")] static extern IntPtr SendMessage(IntPtr h,int message,IntPtr w,IntPtr l);
  public GlassWindow(){FormBorderStyle=FormBorderStyle.None;DoubleBuffered=true;Padding=new Padding(8);}
  protected void DragFrom(Control c){c.MouseDown+=delegate(object sender,MouseEventArgs e){if(e.Button==MouseButtons.Left){ReleaseCapture();SendMessage(Handle,0xA1,new IntPtr(2),IntPtr.Zero);}};}
  protected void SetTransparency(int amount){Opacity=SystemInformation.HighContrast||SystemInformation.TerminalServerSession?1:1-Math.Max(0,Math.Min(16,amount))/100d;}
  protected override void OnSizeChanged(EventArgs e){base.OnSizeChanged(e);if(Width<=0||Height<=0)return;using(var path=Ios.Round(new RectangleF(0,0,Width,Height),WindowState==FormWindowState.Maximized?0:26)){var old=Region;Region=new Region(path);if(old!=null)old.Dispose();}}
  protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;using(var p=Ios.Round(new RectangleF(0.5f,0.5f,Width-1,Height-1),26))using(var pen=new Pen(Color.White))e.Graphics.DrawPath(pen,p);}
  protected override void WndProc(ref Message m){
   base.WndProc(ref m);
   if(m.Msg!=0x84||WindowState==FormWindowState.Maximized)return;
   long packed=m.LParam.ToInt64();var p=PointToClient(new Point((short)(packed&0xffff),(short)((packed>>16)&0xffff)));
   int edge=Math.Max(7,Padding.Left);bool l=p.X<edge,r=p.X>=Width-edge,t=p.Y<edge,b=p.Y>=Height-edge;
   if(l||r||t||b)m.Result=new IntPtr(t?(l?13:r?14:12):b?(l?16:r?17:15):l?10:11);
  }
 }
 class WindowButton:IosButton {
  public bool CloseGlyph;
  protected override void DrawContent(Graphics g){float x=Width/2f,y=Height/2f;using(var pen=new Pen(ForeColor,1.5f)){if(CloseGlyph){g.DrawLine(pen,x-4,y-4,x+4,y+4);g.DrawLine(pen,x+4,y-4,x-4,y+4);}else g.DrawLine(pen,x-5,y,x+5,y);}}
 }
 class RoundedLayout:TableLayoutPanel {
  public RoundedLayout(){DoubleBuffered=true;}
  protected override void OnSizeChanged(EventArgs e){base.OnSizeChanged(e);if(Width<1||Height<1)return;using(var p=Ios.Round(new RectangleF(0,0,Width,Height),20)){var old=Region;Region=new Region(p);if(old!=null)old.Dispose();}}
 }
 // A short, interruptible tween. Timers run only while a control changes state.
 sealed class UiMotion:IDisposable {
  [DllImport("user32.dll")] static extern bool SystemParametersInfo(uint action,uint parameter,[MarshalAs(UnmanagedType.Bool)] out bool value,uint flags);
  static bool AnimationEnabled(){bool enabled;return SystemParametersInfo(0x1042,0,out enabled,0)&&enabled&&!SystemInformation.HighContrast;}
  readonly Timer timer=new Timer{Interval=16};readonly Action changed;float start,target;int began;
  public float Value{get;private set;}
  public UiMotion(Action changed){this.changed=changed;timer.Tick+=delegate{float t=Math.Min(1,(uint)(Environment.TickCount-began)/160f);Value=start+(target-start)*(1-(float)Math.Pow(1-t,3));changed();if(t>=1)timer.Stop();};}
  public void To(float next){timer.Stop();target=next;if(!AnimationEnabled()){Value=next;changed();return;}start=Value;began=Environment.TickCount;timer.Start();}
  public void Dispose(){timer.Dispose();}
 }
}
