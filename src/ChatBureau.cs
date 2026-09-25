using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ChatBureau {
static class Program {
 [STAThread] static void Main(string[] args) {
  if(args.Length>0 && args[0]=="--ui-test")Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
  Application.EnableVisualStyles();
  Application.SetCompatibleTextRenderingDefault(false);
  if(args.Length>0 && args[0]=="--studio-preview"){Application.Run(new Studio(null));return;}
  if(args.Length>0 && args[0]=="--apply-update"){Updates.Install(args);return;}
  if(args.Length>0 && args[0]=="--update-test"){try{Updates.Tests(args[1]);}catch(Exception ex){System.IO.File.WriteAllText(System.IO.Path.Combine(args[1],"update-error.txt"),ex.ToString());Environment.Exit(1);}return;}
  if(args.Length>0 && args[0]=="--animation-test") {
   string[] actions={"Marche","Saut","Étirement","Toilette","Salut","Sieste","Danse","Pirouette","Rebonds","Secousse","Bâillement","Porté"};
   using(var sheet=new Bitmap(640,actions.Length*170))using(var g=Graphics.FromImage(sheet)) {
    g.Clear(Color.FromArgb(29,33,39));
    using(var font=new Font("Segoe UI",10))for(int row=0;row<actions.Length;row++) {
     g.DrawString(actions[row],font,Brushes.White,5,row*170+3);
     for(int col=0;col<4;col++) {var state=g.Save();g.TranslateTransform(col*160,row*170+25);Cat.Draw(g,1,col*0.9,false,actions[row]=="Sieste",false,Color.FromArgb(235,234,219),actions[row],0.15f+col*0.23f);g.Restore(state);}
    }
    sheet.Save(args[1]);
   }
   return;
  }
  if (args.Length > 0 && args[0] == "--self-test") {
   using (Bitmap b = new Bitmap(240, 200)) using (Graphics g = Graphics.FromImage(b)) {
    g.Clear(Color.Transparent); Cat.Draw(g, 1.5f, 0.7, false, false, false, Color.FromArgb(235,234,219));
    b.Save(args[1]);
   }
   return;
  }
  if(args.Length>0 && args[0]=="--ui-test") { try{Studio.RunTests(args[1]);}catch(Exception ex){System.IO.File.WriteAllText(System.IO.Path.Combine(args[1],"error.txt"),ex.ToString());Environment.Exit(1);}return; }
  if(args.Length>0 && args[0]=="--live-update-test"){
   try{var release=Updates.Latest();if(release==null)throw new Exception("No newer release available");string file=Updates.Download(release,args[1]);System.IO.File.WriteAllText(System.IO.Path.Combine(args[1],"live-update-result.txt"),"Verified GitHub release "+release.Version+"; SHA-256 and executable version match.");}catch(Exception ex){System.IO.Directory.CreateDirectory(args[1]);System.IO.File.WriteAllText(System.IO.Path.Combine(args[1],"live-update-error.txt"),ex.ToString());Environment.Exit(1);}return;
  }
  var cat=new Cat();
  if(args.Length==0)cat.Shown+=delegate {cat.ShowStudio();Updates.AutoCheck(cat.ShowUpdates);};
  if(args.Length>0 && args[0]=="--smoke-test")cat.StartSmokeTest();
  Application.Run(cat);
 }
}
class Cat : Form {
 public Preferences Options=Preferences.Load();
 Studio studio;
 public void ShowStudio() {if(studio==null||studio.IsDisposed)studio=new Studio(this);studio.Show();studio.Activate();}
 public void ShowUpdates(Updates.Release release=null){ShowStudio();studio.OpenUpdates(release);}
 public void ApplyOptions(Preferences value) {
  value.Validate();Options=value.Copy();coat=Color.FromArgb(Options.Coat);scale=Options.Size/100f;
  int bottom=Bottom;ClientSize=new Size((int)(160*scale),(int)(140*scale));Top=bottom-Height;
  tray.Text="ChatBureau · "+Options.Name;nextAction=Options.Frequency*30;action="Marche";sleeping=false;
  KeepInside();RenderLayer();
 }
 public void Play(string name) {paused=false;pauseItem.Checked=false;BeginAction(name);}

 [DllImport("user32.dll")] static extern bool SetProcessDPIAware();
 readonly Timer timer = new Timer();
 readonly NotifyIcon tray = new NotifyIcon();
 readonly ContextMenuStrip menu = new ContextMenuStrip();
 readonly Random random = new Random();
 ToolStripMenuItem pauseItem;
 bool paused, dragging, left, sleeping;
 Point grab, original;
 double phase;
 int ticks, affection;
 string action="Marche";
 int actionFrame, actionDuration, nextAction=90;
 float scale = 1;
 Color coat = Color.FromArgb(235,234,219);
 const int NoActivate = 0x08000000;
 protected override bool ShowWithoutActivation { get { return true; } }
 protected override CreateParams CreateParams { get { var p = base.CreateParams; p.ExStyle |= NoActivate | 0x80 | 0x80000; return p; } }
 public Cat() {
  FormBorderStyle = FormBorderStyle.None; ShowInTaskbar = false; TopMost = true;

  DoubleBuffered = true; StartPosition = FormStartPosition.Manual;
  ClientSize = new Size(160,140); Text = "ChatBureau";
  var area = Screen.PrimaryScreen.WorkingArea;
  Location = new Point(area.Right - Width - 80, area.Bottom - Height);
  pauseItem = new ToolStripMenuItem("Mettre en pause", null, delegate { paused = !paused; pauseItem.Checked = paused; });
  menu.Items.Add("ChatBureau · votre petit compagnon").Enabled = false;
  menu.Items.Add("Personnaliser le chat…",null,delegate {ShowStudio();});
  menu.Items.Add("Mises à jour…",null,delegate {ShowUpdates();});
  menu.Items.Add(pauseItem);
  menu.Items.Add("Caresser", null, delegate { affection = 75; sleeping = false; action="Marche"; nextAction=120; });
  var animations = new ToolStripMenuItem("Animations");
  foreach(string name in new string[]{"Saut","Étirement","Toilette","Salut","Sieste","Danse","Pirouette","Rebonds","Secousse","Bâillement","Danse","Pirouette","Rebonds","Secousse","Bâillement"}) {
   string selected=name; animations.DropDownItems.Add(name,null,delegate { paused=false;pauseItem.Checked=false;BeginAction(selected); });
  }
  menu.Items.Add(animations);
  var screens = new ToolStripMenuItem("Déplacer vers un écran");
  for (int i=0; i<Screen.AllScreens.Length;i++) { Screen screen = Screen.AllScreens[i]; screens.DropDownItems.Add("Écran " + (i+1), null, delegate { Rectangle a=screen.WorkingArea; Location=new Point(a.Left+40,a.Bottom-Height); }); }
  menu.Items.Add(screens);
  menu.Items.Add("Ramener en bas de l’écran", null, delegate { var a=Screen.FromPoint(Cursor.Position).WorkingArea; Location=new Point(a.Left+(a.Width-Width)/2,a.Bottom-Height); });
  menu.Items.Add(new ToolStripSeparator());
  menu.Items.Add("Quitter", null, delegate { Close(); });
  ContextMenuStrip = menu;
  tray.Icon = SystemIcons.Information; tray.Text = "ChatBureau — clic droit pour les options"; tray.ContextMenuStrip = menu; tray.Visible = true;
  tray.DoubleClick += delegate { ShowStudio(); };
  MouseDown += delegate(object s, MouseEventArgs e) { if(e.Button!=MouseButtons.Left)return; dragging=true; grab=e.Location; original=Location; Capture=true; sleeping=false; action="Marche"; actionFrame=0; };
  MouseMove += delegate(object s, MouseEventArgs e) { if(dragging) { Point p=Cursor.Position; Location=new Point(p.X-grab.X,p.Y-grab.Y); } };
  MouseUp += delegate(object s, MouseEventArgs e) { if(e.Button!=MouseButtons.Left)return; dragging=false; Capture=false; if(Math.Abs(Left-original.X)+Math.Abs(Top-original.Y)<8)affection=75; else BeginAction("Saut"); KeepInside(); };
  MouseCaptureChanged += delegate { if(!Capture)dragging=false; };
  ApplyOptions(Options);
  timer.Interval=33; timer.Tick += Tick; timer.Start();
  FormClosed += delegate { if(studio!=null)studio.Close();timer.Stop(); timer.Dispose(); tray.Visible=false; tray.Dispose(); menu.Dispose(); };
 }
 
 void AddCoat(ToolStripMenuItem parent,string name,Color color) { parent.DropDownItems.Add(name,null,delegate {coat=color;RenderLayer();}); }
 void KeepInside() { Rectangle a=Screen.FromRectangle(Bounds).WorkingArea; Left=Math.Max(a.Left,Math.Min(Left,a.Right-Width)); Top=Math.Max(a.Top,Math.Min(Top,a.Bottom-Height)); }
 public void StartSmokeTest() {
  int step=0;var test=new Timer();test.Interval=150;
  test.Tick+=delegate {string[] actions={"Saut","Étirement","Toilette","Salut","Sieste","Danse","Pirouette","Rebonds","Secousse","Bâillement"};if(step<actions.Length)BeginAction(actions[step++]);else {test.Stop();test.Dispose();Close();}};test.Start();
 }
 void BeginAction(string name) {
  action=name;actionFrame=0;affection=0;sleeping=name=="Sieste";
  actionDuration=name=="Saut"?36:name=="Sieste"?210:100;
 }
 void Tick(object sender,EventArgs e) {
  ticks++;
  if(!paused && !menu.Visible) {
   phase+=0.15;
   if(affection>0)affection--;
   if(!dragging) {
    if(action!="Marche") {
     actionFrame++;
     if(actionFrame>=actionDuration){action="Marche";sleeping=false;nextAction=Options.Frequency*30;}
    } else if(affection==0) {
     Rectangle a=Screen.FromRectangle(Bounds).WorkingArea;
     Left+=left?-Options.Speed:Options.Speed;
     if(Left<=a.Left){Left=a.Left;left=false;}
     if(Right>=a.Right){Left=a.Right-Width;left=true;}
     if(--nextAction<=0){var enabled=Options.Enabled();if(enabled.Length>0)BeginAction(enabled[random.Next(enabled.Length)]);nextAction=Options.Frequency*30;}
    }
   }
  }
  if(ticks%90==0 && !dragging)KeepInside();
  RenderLayer();
 }

 [StructLayout(LayoutKind.Sequential)] struct NativePoint { public int X,Y; public NativePoint(int x,int y){X=x;Y=y;} }
 [StructLayout(LayoutKind.Sequential)] struct NativeSize { public int Width,Height; public NativeSize(int w,int h){Width=w;Height=h;} }
 [StructLayout(LayoutKind.Sequential,Pack=1)] struct Blend { public byte Operation,Flags,Alpha,Format; }
 [DllImport("user32.dll",SetLastError=true)] static extern bool UpdateLayeredWindow(IntPtr hwnd,IntPtr dst,ref NativePoint pos,ref NativeSize size,IntPtr src,ref NativePoint origin,int key,ref Blend blend,int flags);
 [DllImport("user32.dll")] static extern IntPtr GetDC(IntPtr hwnd);
 [DllImport("user32.dll")] static extern int ReleaseDC(IntPtr hwnd,IntPtr dc);
 [DllImport("gdi32.dll")] static extern IntPtr CreateCompatibleDC(IntPtr dc);
 [DllImport("gdi32.dll")] static extern IntPtr SelectObject(IntPtr dc,IntPtr obj);
 [DllImport("gdi32.dll")] static extern bool DeleteObject(IntPtr obj);
 [DllImport("gdi32.dll")] static extern bool DeleteDC(IntPtr dc);
 protected override void OnShown(EventArgs e) { base.OnShown(e); RenderLayer(); }
 void RenderLayer() {
  if(!IsHandleCreated || IsDisposed)return;
  using(var bitmap = new Bitmap(Width,Height,System.Drawing.Imaging.PixelFormat.Format32bppArgb)) {
   using(var g=Graphics.FromImage(bitmap)) {g.Clear(Color.Transparent);Draw(g,scale,phase,left,sleeping,affection>0,coat,dragging?"Porté":action,actionDuration==0?0:(float)actionFrame/actionDuration,Options);}
   IntPtr screen=GetDC(IntPtr.Zero),dc=CreateCompatibleDC(screen),hb=IntPtr.Zero,old=IntPtr.Zero;
   try {
    hb=bitmap.GetHbitmap(Color.FromArgb(0));old=SelectObject(dc,hb);
    var pos=new NativePoint(Left,Top);var size=new NativeSize(Width,Height);var origin=new NativePoint(0,0);
    var blend=new Blend {Operation=0,Flags=0,Alpha=255,Format=1};
    if(!UpdateLayeredWindow(Handle,screen,ref pos,ref size,dc,ref origin,0,ref blend,2))
     throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
   } finally {if(old!=IntPtr.Zero)SelectObject(dc,old);if(hb!=IntPtr.Zero)DeleteObject(hb);DeleteDC(dc);ReleaseDC(IntPtr.Zero,screen);}
  }
 }
 protected override void OnPaint(PaintEventArgs e) { }
 protected override void OnPaintBackground(PaintEventArgs e) { }
 public static void Draw(Graphics g,float scale,double phase,bool left,bool sleep,bool love,Color coat,string action="Marche",float progress=0,Preferences options=null) {
  g.SmoothingMode=SmoothingMode.AntiAlias;
  g.ScaleTransform(scale,scale);
  if(left && (options==null||!options.UsePng||options.MirrorPng)){g.TranslateTransform(160,0);g.ScaleTransform(-1,1);}
  float envelope=(float)Math.Sin(Math.PI*progress);
  if(action=="Danse"){g.TranslateTransform(80,115);g.RotateTransform((float)Math.Sin(progress*Math.PI*6)*12*envelope);g.TranslateTransform(-80,-115);g.TranslateTransform(0,-8*Math.Abs((float)Math.Sin(progress*Math.PI*3)));}
  if(action=="Pirouette"){g.TranslateTransform(80,85);g.ScaleTransform(1-0.45f*envelope,1-0.45f*envelope);g.RotateTransform(progress*360);g.TranslateTransform(-80,-85);}
  if(action=="Rebonds")g.TranslateTransform(0,-22*Math.Abs((float)Math.Sin(progress*Math.PI*3)));
  if(action=="Secousse")g.TranslateTransform((float)Math.Sin(progress*Math.PI*14)*5*envelope,0);
  if(action=="Bâillement"){g.TranslateTransform(80,128);g.ScaleTransform(1,1+0.07f*envelope);g.TranslateTransform(-80,-128);}
  if(action=="Saut")g.TranslateTransform(0,-28*envelope);
  if(action=="Étirement") {g.TranslateTransform(80,128);g.ScaleTransform(1+0.12f*envelope,1-0.23f*envelope);g.TranslateTransform(-80,-128);}
  if(action=="Porté") {g.TranslateTransform(85,85);g.RotateTransform((float)Math.Sin(phase*0.7)*7);g.TranslateTransform(-85,-85);}
  if(sleep){float breath=(float)Math.Sin(phase*0.45)*0.015f;g.TranslateTransform(80,128);g.ScaleTransform(1,0.73f+breath);g.TranslateTransform(-80,-128);}
  float bob=sleep?3:(float)Math.Sin(phase*2)*0.65f;
  g.TranslateTransform(0,bob);
  if(options!=null && options.UsePng && PngArt.Draw(g,options.PngPath,phase,action,love))return;
  // Rounded side silhouette, upright tail and tiny face, based on the visual reference.
  using(var body=new SolidBrush(coat)) using(var face=new SolidBrush(options!=null?Color.FromArgb(options.Eyes):coat.GetBrightness()<0.4f?Color.FromArgb(228,227,210):Color.FromArgb(52,53,48))) {
   using(var tail=new Pen(coat,11)) {
    tail.StartCap=LineCap.Round;tail.EndCap=LineCap.Round;
    g.DrawBezier(tail,47,96,27,94,22,82,24+(float)Math.Sin(phase*(love?2:0.65))*5,73);
   }
   // Far legs are behind the continuous torso.
   for(int i=0;i<4;i++) {
    float step=(sleep||action!="Marche"||love)?0:(float)Math.Sin(phase+(i%2)*Math.PI)*3.2f;
    if((action=="Toilette"||action=="Salut") && i==3)continue;
    float x=(i<2?43:86)+(i%2)*11+step;
    using(var leg=new Pen(coat,11)){leg.StartCap=LineCap.Round;leg.EndCap=LineCap.Round;g.DrawLine(leg,x,104,x+(sleep?3:step*0.3f),action=="Porté"?131+(float)Math.Sin(phase+i)*2:sleep?119:125-Math.Max(0,step));}
   }
   using(var shape=new GraphicsPath()) {
    shape.StartFigure();
    shape.AddBezier(39,108,33,92,43,85,58,85);
    shape.AddBezier(58,85,67,84,77,85,84,81);
    shape.AddBezier(84,81,90,77,94,63,99,57);
    shape.AddBezier(99,57,102,52,107, 60,111,64);
    shape.AddBezier(111,64,115,64,118,60,121,59);
    shape.AddBezier(121,59,127,56,128,69,130,73);
    shape.AddBezier(130,73,136,81,139,92,133,98);
    shape.AddBezier(133,98,129,104,119,105,112,105);
    shape.AddBezier(112,105,107,108,105,117,99,119);
    shape.AddBezier(99,119,83,122,73,116,64,118);
    shape.AddBezier(64,118,53,121,42,120,39,108);
    shape.CloseFigure();g.FillPath(body,shape);
    if(options!=null && options.Pattern!="Uni") {
     var clip=g.Save();g.SetClip(shape,CombineMode.Intersect);
     using(var marking=new SolidBrush(Color.FromArgb(options.Marking)))using(var stripe=new Pen(marking,5)) {
      stripe.StartCap=LineCap.Round;stripe.EndCap=LineCap.Round;
      if(options.Pattern=="Tigré")for(int x=49;x<90;x+=13)g.DrawBezier(stripe,x,82,x+2,86,x+6,91,x+4,98);
      if(options.Pattern=="Taches"){g.FillEllipse(marking,43,88,21,19);g.FillEllipse(marking,74,98,15,13);g.FillEllipse(marking,99,55,14,21);}
      if(options.Pattern=="Pois")for(int x=47;x<96;x+=14)for(int y=91;y<117;y+=12)g.FillEllipse(marking,x,y,5,5);
      if(options.Pattern=="Dos sombre")g.FillEllipse(marking,32,73,62,31);
      if(options.Pattern=="Bicolore"){g.FillEllipse(marking,81,97,46,38);g.FillEllipse(marking,113,90,30,22);}
     }
     g.Restore(clip);
    }
   }
   if(options!=null && options.Collar) {
    using(var collar=new Pen(Color.FromArgb(options.Accessory),4)){collar.StartCap=LineCap.Round;collar.EndCap=LineCap.Round;g.DrawLine(collar,105,102,115,105);}
    using(var bell=new SolidBrush(Color.FromArgb(242,192,83)))g.FillEllipse(bell,109,106,5,6);
   }
   if(action=="Toilette"||action=="Salut") {
    float wave=(float)Math.Sin(phase*1.8);
    using(var paw=new Pen(coat,10)){paw.StartCap=LineCap.Round;paw.EndCap=LineCap.Round;
     g.DrawBezier(paw,99,109,112,108,119,94,action=="Salut"?143:128,action=="Salut"?78+wave*8:91+wave*3);
    }
    if(action=="Toilette" && wave>0)using(var tongue=new Pen(Color.FromArgb(221,147,154),2.5f))g.DrawLine(tongue,127,94,128,98);
   }
   if(sleep||love||action=="Bâillement"||(options!=null&&options.EyeStyle=="Endormis")||action=="Toilette"||phase%19<0.45) {
    using(var pen=new Pen(face,1.6f)){g.DrawArc(pen,116,85,4,3,0,180);g.DrawArc(pen,130,84,4,3,0,180);}
   } else {float eye=options!=null&&options.EyeStyle=="Grands"?5:3;g.FillEllipse(face,116,85,eye,eye);g.FillEllipse(face,130,84,eye,eye);}
   if(options!=null){
    if(options.Blush)using(var pink=new SolidBrush(Color.FromArgb(215,234,151,159))){g.FillEllipse(pink,112,90,6,3);g.FillEllipse(pink,131,89,6,3);}
    if(options.Whiskers)using(var pen=new Pen(face,0.8f)){g.DrawLine(pen,115,91,105,88);g.DrawLine(pen,115,94,104,95);g.DrawLine(pen,133,92,143,89);g.DrawLine(pen,134,94,144,95);}
    using(var accent=new SolidBrush(Color.FromArgb(options.Accessory))){
     if(options.Hat=="Bonnet"){g.FillPie(accent,96,42,33,30,180,180);g.FillRectangle(accent,94,54,38,6);g.FillEllipse(Brushes.WhiteSmoke,108,36,10,10);}
     if(options.Hat=="Couronne"){g.FillPolygon(accent,new Point[]{new Point(98,58),new Point(95,39),new Point(106,48),new Point(113,34),new Point(121,48),new Point(133,39),new Point(129,58)});}
     if(options.Hat=="Nœud"){g.FillPolygon(accent,new Point[]{new Point(112,57),new Point(99,49),new Point(99,65)});g.FillPolygon(accent,new Point[]{new Point(112,57),new Point(126,49),new Point(126,65)});g.FillEllipse(accent,108,53,8,8);}
    }
   }
   if(action=="Bâillement")g.FillEllipse(face,123,91,5,2+8*envelope);
   g.FillEllipse(face,125,89,2.8f,2);
   using(var pen=new Pen(face,1.05f)){g.DrawArc(pen,121,89,5,4,15,135);g.DrawArc(pen,126,89,5,4,30,135);}
   if(love) {
    using(var heart=new SolidBrush(Color.FromArgb(225,119,135))){g.TranslateTransform(0,-(float)(phase%4)*2);g.FillEllipse(heart,103,32,9,9);g.FillEllipse(heart,110,32,9,9);g.FillPolygon(heart,new Point[]{new Point(104,38),new Point(118,38),new Point(111,47)});}
   }
   if(sleep){using(var f=new Font("Segoe UI",10,FontStyle.Bold))g.DrawString("z",f,face,137, 60-(float)(phase%5)*3);}
  }
 }
}
}








