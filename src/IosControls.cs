using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.ObjectModel;

namespace ChatBureau {
 static class Ios {
  public static readonly Color Background=Color.FromArgb(245,246,248),Blue=Color.FromArgb(0,100,219),Ink=Color.FromArgb(29,32,39),Muted=Color.FromArgb(91,98,113),Field=Color.FromArgb(244,245,248),Line=Color.FromArgb(228,231,238);
  public static GraphicsPath Round(RectangleF r,float radius){var p=new GraphicsPath();if(r.Width<=0||r.Height<=0)return p;float d=Math.Max(0.01f,Math.Min(radius*2,Math.Min(r.Width,r.Height)));p.AddArc(r.X,r.Y,d,d,180,90);p.AddArc(r.Right-d,r.Y,d,d,270,90);p.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);p.AddArc(r.X,r.Bottom-d,d,d,90,90);p.CloseFigure();return p;}
 }
 class IosButton:Button {
  bool hover,down;readonly UiMotion motion;
  public IosButton(){motion=new UiMotion(Invalidate);SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;BackColor=Color.FromArgb(238,242,255);ForeColor=Ios.Blue;Cursor=Cursors.Hand;}
  void Animate(){motion.To(down?1:hover?0.4f:0);}
  protected override void OnMouseEnter(EventArgs e){hover=true;Animate();base.OnMouseEnter(e);}protected override void OnMouseLeave(EventArgs e){hover=false;down=false;Animate();base.OnMouseLeave(e);}protected override void OnMouseDown(MouseEventArgs e){down=true;Animate();base.OnMouseDown(e);}protected override void OnMouseUp(MouseEventArgs e){down=false;Animate();base.OnMouseUp(e);}
  protected virtual void DrawContent(Graphics g){TextRenderer.DrawText(g,Text,Font,ClientRectangle,Enabled?ForeColor:Ios.Muted,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);}
  protected virtual Color SurfaceColor{get{return BackColor;}}
  protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.Clear(Parent==null?Ios.Background:Parent.BackColor);if(Width<3||Height<3)return;g.SmoothingMode=SmoothingMode.AntiAlias;Color fill=Enabled?SurfaceColor:Color.FromArgb(230,232,237);if(Enabled&&motion.Value>0)fill=ControlPaint.Dark(fill,motion.Value*0.09f);using(var p=Ios.Round(new RectangleF(1,1,Width-2,Height-2),14)){using(var b=new LinearGradientBrush(ClientRectangle,ControlPaint.Light(fill,0.15f),fill,90f))g.FillPath(b,p);using(var pen=new Pen(Color.FromArgb(130,255,255,255)))g.DrawPath(pen,p);}DrawContent(g);if(Focused&&ShowFocusCues)using(var p=Ios.Round(new RectangleF(3,3,Width-6,Height-6),12))using(var pen=new Pen(Ios.Blue,1))g.DrawPath(pen,p);}
  protected override void Dispose(bool disposing){if(disposing)motion.Dispose();base.Dispose(disposing);}
 }
 class IosColorButton:Button {
  Color swatch=Color.White;bool selected;
  public Color Swatch{get{return swatch;}set{swatch=value;Invalidate();}}
  public bool Selected{get{return selected;}set{selected=value;Invalidate();}}
  public IosColorButton(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;Height=46;Width=110;Cursor=Cursors.Hand;}
  protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.Clear(Parent==null?Color.White:Parent.BackColor);g.SmoothingMode=SmoothingMode.AntiAlias;using(var path=Ios.Round(new RectangleF(1,1,Width-3,Height-3),10)){using(var b=new SolidBrush(Selected?Color.FromArgb(235,242,253):Ios.Field))g.FillPath(b,path);if(Selected||(Focused&&ShowFocusCues))using(var pen=new Pen(Ios.Blue,1.4f))g.DrawPath(pen,path);}using(var b=new SolidBrush(Swatch))g.FillEllipse(b,10,(Height-22)/2,22,22);using(var pen=new Pen(Color.FromArgb(190,198,209),0.8f))g.DrawEllipse(pen,10,(Height-22)/2,22,22);TextRenderer.DrawText(g,Text,Font,new Rectangle(39,0,Width-44,Height),Selected?Ios.Blue:Ios.Ink,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);}
 }
 class IosToggle:CheckBox {
  readonly UiMotion motion;
  public IosToggle(){motion=new UiMotion(Invalidate);SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);Cursor=Cursors.Hand;Height=40;}
  public override Size GetPreferredSize(Size proposedSize){return new Size(Math.Min(400,Math.Max(250,TextRenderer.MeasureText(Text,Font).Width+70)),42);}
  protected override void OnCheckedChanged(EventArgs e){if(motion!=null)motion.To(Checked?1:0);base.OnCheckedChanged(e);}
  protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.Clear(Parent==null?Color.White:Parent.BackColor);g.SmoothingMode=SmoothingMode.AntiAlias;TextRenderer.DrawText(g,Text,Font,new Rectangle(0,0,Width-65,Height),Enabled?Ios.Ink:Ios.Muted,TextFormatFlags.VerticalCenter|TextFormatFlags.WordBreak);float x=Width-51,y=(Height-28)/2f,v=motion.Value;Color track=Color.FromArgb((int)(221-169*v),(int)(224-25*v),(int)(230-141*v));using(var p=Ios.Round(new RectangleF(x,y,50,28),14))using(var b=new SolidBrush(track))g.FillPath(b,p);using(var shadow=new SolidBrush(Color.FromArgb(28,0,0,0)))g.FillEllipse(shadow,x+3+22*v,y+3,24,24);using(var b=new LinearGradientBrush(new RectangleF(x,y,50,28),Color.White,Color.FromArgb(244,247,250),90f))g.FillEllipse(b,x+2+22*v,y+2,24,24);if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(g,new Rectangle(0,0,Width-60,Height));}
  protected override void Dispose(bool disposing){if(disposing)motion.Dispose();base.Dispose(disposing);}
 }
 class IosSlider:Control {
  int minimum,maximum=100,value;
  public int Minimum {get{return minimum;}set{minimum=value;Value=this.value;}}
  public int Maximum {get{return maximum;}set{maximum=value;Value=this.value;}}
  public int TickFrequency {get;set;}
  public int Value {get{return value;}set{int next=Math.Max(minimum,Math.Min(maximum,value));if(next==this.value)return;this.value=next;Invalidate();if(ValueChanged!=null)ValueChanged(this,EventArgs.Empty);}}
  public event EventHandler ValueChanged;
  public IosSlider(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.Selectable,true);TabStop=true;AccessibleRole=AccessibleRole.Slider;Height=40;Cursor=Cursors.Hand;}
  void SetMouse(int x){Value=minimum+(int)Math.Round(Math.Max(0,Math.Min(1,(x-25f)/Math.Max(1,Width-50)))*(maximum-minimum));}
  protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);if(e.Button==MouseButtons.Left){Focus();Capture=true;SetMouse(e.X);}}
  protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);if(Capture)SetMouse(e.X);}
  protected override void OnMouseUp(MouseEventArgs e){Capture=false;base.OnMouseUp(e);}
  protected override bool IsInputKey(Keys k){return k==Keys.Left||k==Keys.Right||k==Keys.Home||k==Keys.End||base.IsInputKey(k);}
  protected override void OnKeyDown(KeyEventArgs e){if(e.KeyCode==Keys.Left)Value--;else if(e.KeyCode==Keys.Right)Value++;else if(e.KeyCode==Keys.Home)Value=Minimum;else if(e.KeyCode==Keys.End)Value=Maximum;else{base.OnKeyDown(e);return;}e.Handled=true;}
  protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.Clear(Parent==null?Color.White:Parent.BackColor);if(Width<51||Height<4)return;g.SmoothingMode=SmoothingMode.AntiAlias;float y=Height/2f,x=25+(Width-50)*(Value-Minimum)/(float)Math.Max(1,Maximum-Minimum);using(var pen=new Pen(Color.FromArgb(219,223,230),5)){pen.StartCap=pen.EndCap=LineCap.Round;g.DrawLine(pen,25,y,Width-25,y);}using(var pen=new Pen(Color.FromArgb(167,190,218),5)){pen.StartCap=pen.EndCap=LineCap.Round;g.DrawLine(pen,25,y,x,y);}for(int i=3;i>=1;i--)using(var p=Ios.Round(new RectangleF(x-22-i,y-14+i,44+2*i,28),16))using(var b=new SolidBrush(Color.FromArgb(8,35,47,67)))g.FillPath(b,p);using(var p=Ios.Round(new RectangleF(x-22,y-15,44,30),15)){using(var b=new LinearGradientBrush(new RectangleF(x-22,y-15,44,30),Color.White,Color.FromArgb(227,233,240),90f))g.FillPath(b,p);using(var pen=new Pen(Focused?Ios.Blue:Color.White,1.4f))g.DrawPath(pen,p);}using(var p=Ios.Round(new RectangleF(x-18,y-12,36,12),8))using(var b=new SolidBrush(Color.FromArgb(185,255,255,255)))g.FillPath(b,p);using(var b=new SolidBrush(Ios.Blue))g.FillEllipse(b,x-3,y-3,6,6);}
  protected override AccessibleObject CreateAccessibilityInstance(){return new SliderAccessible(this);}
  class SliderAccessible:ControlAccessibleObject {readonly IosSlider owner;public SliderAccessible(IosSlider owner):base(owner){this.owner=owner;}public override string Value{get{return owner.Value.ToString();}set{int parsed;if(int.TryParse(value,out parsed))owner.Value=parsed;}}}
 }
 class IosCard:FlowLayoutPanel {
  public IosCard(){DoubleBuffered=true;BackColor=Color.White;Padding=new Padding(14,12,14,12);}
  protected override void OnPaintBackground(PaintEventArgs e){e.Graphics.Clear(Parent==null?Ios.Background:Parent.BackColor);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;using(var p=Ios.Round(new RectangleF(0,0,Width-1,Height-1),16))using(var b=new SolidBrush(BackColor))e.Graphics.FillPath(b,p);}
  protected override void OnSizeChanged(EventArgs e){base.OnSizeChanged(e);if(Width>0&&Height>0){using(var path=Ios.Round(new RectangleF(0,0,Width,Height),16)){var old=Region;Region=new Region(path);if(old!=null)old.Dispose();}}}
 }
 class IosChoice:Control {
  public class Choices:Collection<object>{public void AddRange(object[] items){foreach(var item in items)Add(item);}}
  readonly Choices items=new Choices();int selected=-1;
  ContextMenuStrip popup;
  public Choices Items{get{return items;}}
  public ComboBoxStyle DropDownStyle{get;set;}
  public int SelectedIndex{get{return selected;}set{if(value<-1||value>=items.Count)throw new ArgumentOutOfRangeException();if(selected==value)return;selected=value;Invalidate();if(SelectedIndexChanged!=null)SelectedIndexChanged(this,EventArgs.Empty);}}
  public object SelectedItem{get{return selected<0?null:items[selected];}set{SelectedIndex=items.IndexOf(value);}}
  public event EventHandler SelectedIndexChanged;
  public IosChoice(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.Selectable,true);Height=36;Width=250;BackColor=Color.FromArgb(242,243,248);TabStop=true;Cursor=Cursors.Hand;AccessibleRole=AccessibleRole.ComboBox;}
  protected override void OnClick(EventArgs e){base.OnClick(e);Focus();Open();}
  // Windows still uses a dropdown after Closed fires. Keep it alive until its owner is disposed.
  internal ContextMenuStrip Open(){
   if(IsDisposed||Disposing||!Enabled||items.Count==0)return null;
   if(popup==null){popup=new ContextMenuStrip{Font=Font,ShowImageMargin=false,ShowCheckMargin=true,Padding=new Padding(4)};}
   if(popup.Visible)return popup;
   if(popup.Items.Count!=items.Count){
    while(popup.Items.Count>0){var obsolete=popup.Items[0];popup.Items.RemoveAt(0);obsolete.Dispose();}
    for(int i=0;i<items.Count;i++){int n=i;var entry=new ToolStripMenuItem(items[i].ToString()){Padding=new Padding(10,6,12,6)};entry.Click+=delegate{if(!IsDisposed&&n<items.Count)SelectedIndex=n;};popup.Items.Add(entry);}
   }
   for(int i=0;i<items.Count;i++){var entry=(ToolStripMenuItem)popup.Items[i];entry.Text=items[i].ToString();entry.Checked=i==selected;}
   popup.MinimumSize=new Size(Width,0);popup.Show(this,new Point(0,Height+3));return popup;
  }
  protected override void Dispose(bool disposing){if(disposing&&popup!=null){popup.Dispose();popup=null;}base.Dispose(disposing);}
  protected override bool IsInputKey(Keys k){return k==Keys.Up||k==Keys.Down||base.IsInputKey(k);}
  protected override void OnKeyDown(KeyEventArgs e){if(e.KeyCode==Keys.Space||e.KeyCode==Keys.Enter){Open();e.Handled=true;}else if(e.KeyCode==Keys.Down&&items.Count>0){SelectedIndex=Math.Min(items.Count-1,selected+1);e.Handled=true;}else if(e.KeyCode==Keys.Up&&items.Count>0){SelectedIndex=Math.Max(0,selected-1);e.Handled=true;}else base.OnKeyDown(e);}
  protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.Clear(Parent==null?Ios.Background:Parent.BackColor);g.SmoothingMode=SmoothingMode.AntiAlias;using(var p=Ios.Round(new RectangleF(0,0,Width-1,Height-1),10))using(var b=new SolidBrush(BackColor))g.FillPath(b,p);TextRenderer.DrawText(g,SelectedItem==null?"Choisir…":SelectedItem.ToString(),Font,new Rectangle(12,0,Width-42,Height),Ios.Ink,TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);using(var pen=new Pen(Ios.Muted,1.5f))g.DrawLines(pen,new Point[]{new Point(Width-25,Height/2-2),new Point(Width-20,Height/2+3),new Point(Width-15,Height/2-2)});if(Focused&&ShowFocusCues)using(var p=Ios.Round(new RectangleF(1,1,Width-3,Height-3),10))using(var pen=new Pen(Ios.Blue))g.DrawPath(pen,p);}
  protected override AccessibleObject CreateAccessibilityInstance(){return new ChoiceAccessible(this);}
  class ChoiceAccessible:ControlAccessibleObject{readonly IosChoice owner;public ChoiceAccessible(IosChoice owner):base(owner){this.owner=owner;}public override string Value{get{return owner.SelectedItem==null?"":owner.SelectedItem.ToString();}}public override string DefaultAction{get{return "Ouvrir";}}public override void DoDefaultAction(){owner.Open();}}
 }
 class IosNavButton:IosButton {
  readonly UiMotion selection;
  bool selected;
  public IosNavButton(){selection=new UiMotion(Invalidate);}
  public bool Selected {get{return selected;}set{if(value==selected)return;selected=value;selection.To(value?1:0);}}
  protected override Color SurfaceColor{get{float v=selection==null?0:selection.Value;return Color.FromArgb((int)(238-20*v),(int)(242-11*v),(int)(247+3*v));}}
  protected override void Dispose(bool disposing){if(disposing)selection.Dispose();base.Dispose(disposing);}
  public int IconIndex;
  protected override void DrawContent(Graphics g){
   using(var pen=new Pen(ForeColor,1.6f)){pen.StartCap=pen.EndCap=LineCap.Round;var s=g.Save();g.TranslateTransform(17,Height/2f-9);
    switch(IconIndex){
     case 0:g.DrawEllipse(pen,2,2,14,14);g.DrawArc(pen,5,5,8,8,10,160);break;
     case 1:g.DrawEllipse(pen,1,1,16,16);g.DrawLines(pen,new Point[]{new Point(9,4),new Point(9,9),new Point(13,11)});break;
     case 2:g.DrawPolygon(pen,new Point[]{new Point(2,5),new Point(6,8),new Point(9,2),new Point(12,8),new Point(16,5),new Point(14,15),new Point(4,15)});break;
     case 3:using(var p=Ios.Round(new RectangleF(1,1,16,16),3))g.DrawPath(pen,p);g.DrawEllipse(pen,10,4,3,3);g.DrawLines(pen,new Point[]{new Point(3,13),new Point(7,9),new Point(12,14),new Point(16,10)});break;
     default:g.DrawArc(pen,1,1,16,16,35,285);g.DrawLines(pen,new Point[]{new Point(12,0),new Point(17,3),new Point(17,-1)});break;
    }g.Restore(s);
   }
   TextRenderer.DrawText(g,Text,Font,new Rectangle(49,0,Width-57,Height),ForeColor,TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
  }
 }
 class IosTabs:Panel {
  readonly TableLayoutPanel nav=new TableLayoutPanel();readonly Panel content=new Panel();int selected;
  public Control Navigation {get{return nav;}}
  public event EventHandler SelectedIndexChanged;
  public sealed class Pages:Collection<Control>{readonly IosTabs owner;public Pages(IosTabs owner){this.owner=owner;}protected override void InsertItem(int index,Control page){base.InsertItem(index,page);owner.AddPage(page,index);}}
  public Pages TabPages {get;private set;}
  public int SelectedIndex {get{return selected;}set{if(value<0||value>=TabPages.Count)return;bool changed=selected!=value;selected=value;for(int i=0;i<TabPages.Count;i++){TabPages[i].Visible=i==selected;var button=(IosNavButton)nav.Controls[i];button.Selected=i==selected;button.ForeColor=i==selected?Ios.Blue:Ios.Muted;}TabPages[selected].BringToFront();if(changed&&SelectedIndexChanged!=null)SelectedIndexChanged(this,EventArgs.Empty);}}
  public IosTabs(){BackColor=Ios.Background;TabPages=new Pages(this);nav.Dock=DockStyle.Fill;nav.ColumnCount=1;nav.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));nav.Padding=new Padding(0);nav.BackColor=Color.FromArgb(238,242,247);content.Dock=DockStyle.Fill;Controls.Add(content);}
  void AddPage(Control page,int index){page.Dock=DockStyle.Fill;page.BackColor=Ios.Background;content.Controls.Add(page);var b=new IosNavButton{Text=page.Text,IconIndex=index,Dock=DockStyle.Fill,Font=new Font("Segoe UI",10),Margin=new Padding(0,0,0,5)};b.Click+=delegate{SelectedIndex=index;};nav.RowCount=TabPages.Count;nav.RowStyles.Add(new RowStyle(SizeType.Absolute,45));nav.Controls.Add(b,0,index);SelectedIndex=selected;}
  protected override void Dispose(bool disposing){if(disposing&&!nav.IsDisposed)nav.Dispose();base.Dispose(disposing);}
 }
}
