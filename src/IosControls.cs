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
  bool hover,down;
  public IosButton(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;BackColor=Color.FromArgb(238,242,255);ForeColor=Ios.Blue;Cursor=Cursors.Hand;}
  protected override void OnMouseEnter(EventArgs e){hover=true;Invalidate();base.OnMouseEnter(e);}protected override void OnMouseLeave(EventArgs e){hover=false;Invalidate();base.OnMouseLeave(e);}protected override void OnMouseDown(MouseEventArgs e){down=true;Invalidate();base.OnMouseDown(e);}protected override void OnMouseUp(MouseEventArgs e){down=false;Invalidate();base.OnMouseUp(e);}
  protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.Clear(Parent==null?Ios.Background:Parent.BackColor);g.SmoothingMode=SmoothingMode.AntiAlias;Color fill=Enabled?BackColor:Color.FromArgb(230,230,237);if(Enabled&&(hover||down))fill=ControlPaint.Dark(fill,down?0.1f:0.035f);using(var p=Ios.Round(new RectangleF(1,1,Width-2,Height-2),11))using(var b=new SolidBrush(fill))g.FillPath(b,p);TextRenderer.DrawText(g,Text,Font,ClientRectangle,Enabled?ForeColor:Ios.Muted,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);if(Focused&&ShowFocusCues)using(var p=Ios.Round(new RectangleF(3,3,Width-6,Height-6),9))using(var pen=new Pen(Ios.Blue,1))g.DrawPath(pen,p);}
 }
 class IosColorButton:Button {
  Color swatch=Color.White;bool selected;
  public Color Swatch{get{return swatch;}set{swatch=value;Invalidate();}}
  public bool Selected{get{return selected;}set{selected=value;Invalidate();}}
  public IosColorButton(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;Height=46;Width=110;Cursor=Cursors.Hand;}
  protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.Clear(Parent==null?Color.White:Parent.BackColor);g.SmoothingMode=SmoothingMode.AntiAlias;using(var path=Ios.Round(new RectangleF(1,1,Width-3,Height-3),10)){using(var b=new SolidBrush(Selected?Color.FromArgb(235,242,253):Ios.Field))g.FillPath(b,path);if(Selected||(Focused&&ShowFocusCues))using(var pen=new Pen(Ios.Blue,1.4f))g.DrawPath(pen,path);}using(var b=new SolidBrush(Swatch))g.FillEllipse(b,10,(Height-22)/2,22,22);using(var pen=new Pen(Color.FromArgb(190,198,209),0.8f))g.DrawEllipse(pen,10,(Height-22)/2,22,22);TextRenderer.DrawText(g,Text,Font,new Rectangle(39,0,Width-44,Height),Selected?Ios.Blue:Ios.Ink,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);}
 }
 class IosToggle:CheckBox {
  public IosToggle(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);Cursor=Cursors.Hand;Height=40;}
  public override Size GetPreferredSize(Size proposedSize){return new Size(Math.Min(400,Math.Max(250,TextRenderer.MeasureText(Text,Font).Width+70)),42);}
  protected override void OnCheckedChanged(EventArgs e){Invalidate();base.OnCheckedChanged(e);}
  protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.Clear(Parent==null?Color.White:Parent.BackColor);g.SmoothingMode=SmoothingMode.AntiAlias;TextRenderer.DrawText(g,Text,Font,new Rectangle(0,0,Width-65,Height),Enabled?Ios.Ink:Ios.Muted,TextFormatFlags.VerticalCenter|TextFormatFlags.WordBreak);float x=Width-51,y=(Height-28)/2f;using(var p=Ios.Round(new RectangleF(x,y,50,28),14))using(var b=new SolidBrush(Checked?Color.FromArgb(52,199,89):Color.FromArgb(221,222,229)))g.FillPath(b,p);using(var shadow=new SolidBrush(Color.FromArgb(35,0,0,0)))g.FillEllipse(shadow,Checked?x+25:x+3,y+3,23,23);g.FillEllipse(Brushes.White,Checked?x+24:x+2,y+2,24,24);if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(g,new Rectangle(0,0,Width-60,Height));}
 }
 class IosSlider:Control {
  int minimum,maximum=100,value;
  public int Minimum {get{return minimum;}set{minimum=value;Value=this.value;}}
  public int Maximum {get{return maximum;}set{maximum=value;Value=this.value;}}
  public int TickFrequency {get;set;}
  public int Value {get{return value;}set{int next=Math.Max(minimum,Math.Min(maximum,value));if(next==this.value)return;this.value=next;Invalidate();if(ValueChanged!=null)ValueChanged(this,EventArgs.Empty);}}
  public event EventHandler ValueChanged;
  public IosSlider(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.Selectable,true);TabStop=true;AccessibleRole=AccessibleRole.Slider;Height=40;Cursor=Cursors.Hand;}
  void SetMouse(int x){Value=minimum+(int)Math.Round(Math.Max(0,Math.Min(1,(x-12f)/Math.Max(1,Width-24)))*(maximum-minimum));}
  protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);if(e.Button==MouseButtons.Left){Focus();Capture=true;SetMouse(e.X);}}
  protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);if(Capture)SetMouse(e.X);}
  protected override void OnMouseUp(MouseEventArgs e){Capture=false;base.OnMouseUp(e);}
  protected override bool IsInputKey(Keys k){return k==Keys.Left||k==Keys.Right||k==Keys.Home||k==Keys.End||base.IsInputKey(k);}
  protected override void OnKeyDown(KeyEventArgs e){if(e.KeyCode==Keys.Left)Value--;else if(e.KeyCode==Keys.Right)Value++;else if(e.KeyCode==Keys.Home)Value=Minimum;else if(e.KeyCode==Keys.End)Value=Maximum;else{base.OnKeyDown(e);return;}e.Handled=true;}
  protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.Clear(Parent==null?Color.White:Parent.BackColor);g.SmoothingMode=SmoothingMode.AntiAlias;float y=Height/2f,x=12+(Width-24)*(Value-Minimum)/(float)Math.Max(1,Maximum-Minimum);using(var pen=new Pen(Color.FromArgb(228,228,234),5)){pen.StartCap=pen.EndCap=LineCap.Round;g.DrawLine(pen,12,y,Width-12,y);}using(var pen=new Pen(Ios.Blue,5)){pen.StartCap=pen.EndCap=LineCap.Round;g.DrawLine(pen,12,y,x,y);}using(var b=new SolidBrush(Color.FromArgb(35,0,0,0)))g.FillEllipse(b,x-11,y-9,23,23);g.FillEllipse(Brushes.White,x-11,y-11,22,22);using(var pen=new Pen(Focused?Ios.Blue:Color.FromArgb(217,219,227)))g.DrawEllipse(pen,x-11,y-11,22,22);}
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
 class IosTabs:Panel {
  readonly TableLayoutPanel nav=new TableLayoutPanel();readonly Panel content=new Panel();int selected;
  public sealed class Pages:Collection<Control>{readonly IosTabs owner;public Pages(IosTabs owner){this.owner=owner;}protected override void InsertItem(int index,Control page){base.InsertItem(index,page);owner.AddPage(page,index);}}
  public Pages TabPages {get;private set;}
  public int SelectedIndex {get{return selected;}set{if(value<0||value>=TabPages.Count)return;selected=value;for(int i=0;i<TabPages.Count;i++){TabPages[i].Visible=i==selected;var button=(Button)nav.Controls[i];button.BackColor=i==selected?Color.White:Color.FromArgb(233,234,240);button.ForeColor=i==selected?Ios.Ink:Ios.Muted;}TabPages[selected].BringToFront();}}
  public IosTabs(){BackColor=Ios.Background;TabPages=new Pages(this);nav.Dock=DockStyle.Top;nav.Height=46;nav.Padding=new Padding(3);nav.BackColor=Color.FromArgb(233,234,240);content.Dock=DockStyle.Fill;content.Padding=new Padding(0,12,0,0);Controls.Add(content);Controls.Add(nav);}
  void AddPage(Control page,int index){page.Dock=DockStyle.Fill;page.BackColor=Ios.Background;content.Controls.Add(page);var b=new IosButton{Text=page.Text,Dock=DockStyle.Fill,Font=new Font("Segoe UI",9,FontStyle.Bold),Margin=new Padding(1)};b.Click+=delegate{SelectedIndex=index;};nav.ColumnCount=TabPages.Count;nav.ColumnStyles.Clear();for(int i=0;i<TabPages.Count;i++)nav.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100f/TabPages.Count));nav.Controls.Add(b,index,0);SelectedIndex=selected;}
 }
}
