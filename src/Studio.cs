using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace ChatBureau {
 public class Preferences {
  public string Name="Mochi",Pattern="Uni";
  public string PngPath="",Hat="Aucun",EyeStyle="Ronds";
  public bool UsePng=false,MirrorPng=true,Blush=false,Whiskers=false,Dance=true,Spin=true,Bounce=true,Shake=true,Yawn=true;
  public int Coat=Color.FromArgb(235,234,219).ToArgb(),Eyes=Color.FromArgb(52,53,48).ToArgb(),Marking=Color.FromArgb(167,142,111).ToArgb(),Accessory=Color.FromArgb(115,130,211).ToArgb();
  public int Size=100,Speed=2,Frequency=6;
  public bool Collar=false,Jump=true,Stretch=true,Groom=true,Wave=true,Sleep=true;
  public Preferences Copy(){return (Preferences)MemberwiseClone();}
  public void Validate(){
   Name=string.IsNullOrWhiteSpace(Name)?"Mochi":Name.Trim();if(Name.Length>24)Name=Name.Substring(0,24);
   Size=Math.Max(60,Math.Min(200,Size));Speed=Math.Max(0,Math.Min(5,Speed));Frequency=Math.Max(3,Math.Min(30,Frequency));
   if(Array.IndexOf(new string[]{"Uni","Tigré","Taches","Bicolore","Pois","Dos sombre"},Pattern)<0)Pattern="Uni";
   if(Array.IndexOf(new string[]{"Aucun","Bonnet","Couronne","Nœud"},Hat)<0)Hat="Aucun";
   if(Array.IndexOf(new string[]{"Ronds","Grands","Endormis"},EyeStyle)<0)EyeStyle="Ronds";
   PngPath=PngPath??"";
   Coat=Color.FromArgb(255,Color.FromArgb(Coat)).ToArgb();Eyes=Color.FromArgb(255,Color.FromArgb(Eyes)).ToArgb();Marking=Color.FromArgb(255,Color.FromArgb(Marking)).ToArgb();Accessory=Color.FromArgb(255,Color.FromArgb(Accessory)).ToArgb();
  }
  public string[] Enabled(){var names=new List<string>();if(Jump)names.Add("Saut");if(Stretch)names.Add("Étirement");if(Groom)names.Add("Toilette");if(Wave)names.Add("Salut");if(Sleep)names.Add("Sieste");if(Dance)names.Add("Danse");if(Spin)names.Add("Pirouette");if(Bounce)names.Add("Rebonds");if(Shake)names.Add("Secousse");if(Yawn)names.Add("Bâillement");return names.ToArray();}
  public static string FilePath {get{return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"ChatBureau","preferences.xml");}}
  public static Preferences Load(){return LoadFrom(FilePath);}
  public static Preferences LoadFrom(string path){try{using(var file=File.OpenRead(path)){var p=(Preferences)new XmlSerializer(typeof(Preferences)).Deserialize(file);p.Validate();return p;}}catch(IOException){return new Preferences();}catch(InvalidOperationException){return new Preferences();}catch(UnauthorizedAccessException){return new Preferences();}}
  public void Save(string path){Validate();Directory.CreateDirectory(Path.GetDirectoryName(path));string temp=path+".tmp";using(var stream=File.Create(temp))new XmlSerializer(typeof(Preferences)).Serialize(stream,this);File.Copy(temp,path,true);File.Delete(temp);}
 }
 class Preview : Control {
  public Preferences Value;
  public string Action="Marche";
  public double Phase;
  public float Progress;
  public Preview(){DoubleBuffered=true;BackColor=Color.FromArgb(32,38,47);}
  protected override void OnPaint(PaintEventArgs e){
   base.OnPaint(e);var g=e.Graphics;g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
   using(var pen=new Pen(Color.FromArgb(46,54,65)))for(int x=0;x<Width;x+=24)g.DrawLine(pen,x,0,x,Height);
   using(var pen=new Pen(Color.FromArgb(69,78,91)))g.DrawLine(pen,20,Height-40,Width-20,Height-40);
   if(Value==null)return;
   float scale=Math.Min(1.7f,Value.Size/100f);var state=g.Save();g.TranslateTransform((Width-160*scale)/2,Height-40-130*scale);
   Cat.Draw(g,scale,Phase,false,Action=="Sieste",false,Color.FromArgb(Value.Coat),Action,Progress,Value);g.Restore(state);
  }
 }
 class Studio : Form {
  readonly Cat cat;
  Preferences draft;
  readonly Preview preview=new Preview();
  readonly Timer clock=new Timer();
  readonly Label status=new Label();
  readonly TextBox name=new TextBox();
  readonly ComboBox pattern=new ComboBox();
  readonly ComboBox hat=new ComboBox(),eyeStyle=new ComboBox();
  readonly CheckBox blush=new CheckBox(),whiskers=new CheckBox(),usePng=new CheckBox(),mirrorPng=new CheckBox();
  readonly Label pngInfo=new Label();
  readonly CheckBox collar=new CheckBox();
  readonly TrackBar size=new TrackBar(),speed=new TrackBar(),frequency=new TrackBar();
  readonly Label sizeValue=new Label(),speedValue=new Label(),frequencyValue=new Label();
  readonly Dictionary<string,CheckBox> checks=new Dictionary<string,CheckBox>();
  readonly Dictionary<string,Button> colorButtons=new Dictionary<string,Button>();
  bool binding,dirty;
  int frame;
  public Studio(Cat owner){
   cat=owner;draft=owner==null?new Preferences():owner.Options.Copy();
   Text="ChatBureau · Atelier du chat";Font=new Font("Segoe UI",10);BackColor=Color.FromArgb(246,247,251);
   AutoScaleMode=AutoScaleMode.Dpi;ClientSize=new Size(860,650);MinimumSize=new Size(820,650);StartPosition=FormStartPosition.CenterScreen;
   var root=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(22),ColumnCount=1,RowCount=3};root.RowStyles.Add(new RowStyle(SizeType.Absolute,76));root.RowStyles.Add(new RowStyle(SizeType.Percent,100));root.RowStyles.Add(new RowStyle(SizeType.Absolute,78));Controls.Add(root);
   var header=new Panel{Dock=DockStyle.Fill};header.Controls.Add(new Label{Text="Un chat à votre image",Font=new Font("Segoe UI",22,FontStyle.Bold),AutoSize=true,Location=new Point(0,0)});header.Controls.Add(new Label{Text="Choisissez son style et ses petites habitudes.",AutoSize=true,ForeColor=Color.DimGray,Location=new Point(2,45)});root.Controls.Add(header,0,0);
   var body=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=1};body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,290));body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));root.Controls.Add(body,0,1);
   var left=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=4,Margin=new Padding(0,0,18,0)};left.RowStyles.Add(new RowStyle(SizeType.Absolute,26));left.RowStyles.Add(new RowStyle(SizeType.Percent,100));left.RowStyles.Add(new RowStyle(SizeType.Absolute,40));left.RowStyles.Add(new RowStyle(SizeType.Absolute,68));body.Controls.Add(left,0,0);
   left.Controls.Add(new Label{Text="APERÇU ANIMÉ",AutoSize=true,ForeColor=Color.DimGray},0,0);preview.Dock=DockStyle.Fill;left.Controls.Add(preview,0,1);
   var demo=new ComboBox{Dock=DockStyle.Fill,DropDownStyle=ComboBoxStyle.DropDownList,Margin=new Padding(0,9,0,0)};demo.Items.AddRange(new object[]{"Marche","Saut","Étirement","Toilette","Salut","Sieste","Danse","Pirouette","Rebonds","Secousse","Bâillement","Porté","Danse","Pirouette","Rebonds","Secousse","Bâillement"});demo.SelectedIndex=0;demo.SelectedIndexChanged+=delegate{preview.Action=(string)demo.SelectedItem;frame=0;};left.Controls.Add(demo,0,2);
   left.Controls.Add(new Label{Text="L’aperçu change immédiatement.\nEnregistrez pour appliquer au chat.\nClic droit sur le chat pour revenir ici.",Dock=DockStyle.Fill,ForeColor=Color.DimGray,Padding=new Padding(0,10,0,0)},0,3);
   var tabs=new TabControl{Dock=DockStyle.Fill};body.Controls.Add(tabs,1,0);
   var appearance=new TabPage("Apparence"){BackColor=Color.White,AutoScroll=true};var behavior=new TabPage("Comportement"){BackColor=Color.White,AutoScroll=true};tabs.TabPages.Add(appearance);tabs.TabPages.Add(behavior);
   var details=new TabPage("Style"){BackColor=Color.White};tabs.TabPages.Add(details);
   var images=new TabPage("Image PNG"){BackColor=Color.White};tabs.TabPages.Add(images);
   var style=Rows(details);var imageRows=Rows(images);
   ConfigureChoice(hat,new string[]{"Aucun","Bonnet","Couronne","Nœud"},delegate{draft.Hat=(string)hat.SelectedItem;});Row(style,"Accessoire sur la tête",hat);
   Row(style,"",new Label{Text="Les accessoires utilisent la couleur « Collier ».\nLes options de style concernent le chat dessiné.",AutoSize=true,ForeColor=Color.DimGray});
   ConfigureChoice(eyeStyle,new string[]{"Ronds","Grands","Endormis"},delegate{draft.EyeStyle=(string)eyeStyle.SelectedItem;});Row(style,"Expression des yeux",eyeStyle);
   ConfigureCheck(blush,"Joues roses",delegate{draft.Blush=blush.Checked;});Row(style,"Visage",blush);
   ConfigureCheck(whiskers,"Moustaches",delegate{draft.Whiskers=whiskers.Checked;});Row(style,"",whiskers);
   var import=Button("Importer un PNG…",delegate{ImportPng();});import.Width=200;Row(imageRows,"Votre propre personnage",import);
   pngInfo.AutoSize=true;pngInfo.MaximumSize=new Size(375,0);Row(imageRows,"Image sélectionnée",pngInfo);
   ConfigureCheck(usePng,"Remplacer le chat par mon image",delegate{draft.UsePng=usePng.Checked;});Row(imageRows,"Affichage",usePng);
   ConfigureCheck(mirrorPng,"Retourner l’image selon la direction",delegate{draft.MirrorPng=mirrorPng.Checked;});Row(imageRows,"",mirrorPng);
   var clear=Button("Revenir au chat",delegate{draft.UsePng=false;Bind();Changed();});clear.Width=170;Row(imageRows,"",clear);
   Row(imageRows,"Comment ça marche",new Label{AutoSize=true,MaximumSize=new Size(375,0),ForeColor=Color.DimGray,Text="PNG transparent conseillé. Le fond de l’image est conservé.\n\nL’image garde ses proportions et profite des mouvements : promenade, sauts, danse, rotation… Ses membres ne sont pas animés séparément.\n\nUne copie locale est conservée à l’enregistrement. Vous pourrez déplacer le fichier d’origine.\n\nMaximum : 16 Mo et 4 096 × 4 096 pixels."});
   var look=Rows(appearance);var habits=Rows(behavior);
   name.MaxLength=24;name.Width=245;name.TextChanged+=delegate{if(!binding){draft.Name=name.Text;Changed();}};Row(look,"Son petit nom",name);
   var presets=new FlowLayoutPanel{AutoSize=true,WrapContents=true,MaximumSize=new Size(420,0)};
   string[] titles={"Ivoire","Charbon","Abricot","Sauge"};Color[] colors={Color.FromArgb(235,234,219),Color.FromArgb(53,55,52),Color.FromArgb(226,173,122),Color.FromArgb(160,184,159)};
   for(int i=0;i<titles.Length;i++){Color color=colors[i];var button=Button(titles[i],delegate{draft.Coat=color.ToArgb();draft.Eyes=(color.GetBrightness()<0.4f?Color.FromArgb(235,234,219):Color.FromArgb(52,53,48)).ToArgb();Bind();Changed();});button.Width=88;presets.Controls.Add(button);}Row(look,"Palette rapide",presets);
   var swatches=new FlowLayoutPanel{AutoSize=true};foreach(string key in new string[]{"Pelage","Yeux","Motifs","Collier"}){string k=key;var button=Button(k,delegate{PickColor(k);});button.Width=89;colorButtons[k]=button;swatches.Controls.Add(button);}Row(look,"Couleurs sur mesure",swatches);
   pattern.DropDownStyle=ComboBoxStyle.DropDownList;pattern.Width=220;pattern.Items.AddRange(new object[]{"Uni","Tigré","Taches","Bicolore","Pois","Dos sombre"});pattern.SelectedIndexChanged+=delegate{if(!binding){draft.Pattern=(string)pattern.SelectedItem;Changed();}};Row(look,"Motif du pelage",pattern);
   collar.Text="Porter un collier avec une clochette";collar.AutoSize=true;collar.CheckedChanged+=delegate{if(!binding){draft.Collar=collar.Checked;Changed();}};Row(look,"Accessoire",collar);
   SetupSlider(size,60,200,20);size.ValueChanged+=delegate{sizeValue.Text=size.Value+" %";if(!binding){draft.Size=size.Value;Changed();}};Row(look,"Taille sur le bureau",SliderPanel(size,sizeValue));
   SetupSlider(speed,0,5,1);speed.ValueChanged+=delegate{speedValue.Text=speed.Value==0?"Immobile":speed.Value+" / 5";if(!binding){draft.Speed=speed.Value;Changed();}};Row(habits,"Vitesse de promenade",SliderPanel(speed,speedValue));
   SetupSlider(frequency,3,30,3);frequency.ValueChanged+=delegate{frequencyValue.Text=frequency.Value+" s";if(!binding){draft.Frequency=frequency.Value;Changed();}};Row(habits,"Temps entre deux animations",SliderPanel(frequency,frequencyValue));
   var choices=new FlowLayoutPanel{FlowDirection=FlowDirection.TopDown,AutoSize=true,WrapContents=false};
   foreach(string title in new string[]{"Saut","Étirement","Toilette","Salut","Sieste","Danse","Pirouette","Rebonds","Secousse","Bâillement"}){var check=new CheckBox{Text=title,AutoSize=true,Margin=new Padding(3,5,3,5)};checks[title]=check;check.CheckedChanged+=delegate{if(!binding){draft.Jump=checks["Saut"].Checked;draft.Stretch=checks["Étirement"].Checked;draft.Groom=checks["Toilette"].Checked;draft.Wave=checks["Salut"].Checked;draft.Sleep=checks["Sieste"].Checked;draft.Dance=checks["Danse"].Checked;draft.Spin=checks["Pirouette"].Checked;draft.Bounce=checks["Rebonds"].Checked;draft.Shake=checks["Secousse"].Checked;draft.Yawn=checks["Bâillement"].Checked;Changed();}};choices.Controls.Add(check);}Row(habits,"Animations spontanées",choices);
   Row(habits,"",new Label{Text="Décochez tout pour garder uniquement la promenade.\nLes animations restent disponibles dans le menu du chat.",AutoSize=true,ForeColor=Color.DimGray});
   var footer=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=3,RowCount=2,Padding=new Padding(0,12,0,0)};footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,110));footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,190));root.Controls.Add(footer,0,2);
   var reset=Button("Réinitialiser",delegate{draft=new Preferences();Bind();Changed();});reset.Width=120;footer.Controls.Add(reset,0,0);
   var close=Button("Fermer",delegate{Close();});footer.Controls.Add(close,1,0);
   var save=Button("Enregistrer et appliquer",delegate{Save();});save.Width=182;save.BackColor=Color.FromArgb(85,99,193);save.ForeColor=Color.White;footer.Controls.Add(save,2,0);
   status.AutoSize=true;status.ForeColor=Color.DimGray;footer.Controls.Add(status,0,1);footer.SetColumnSpan(status,3);
   StyleCombo(pattern);StyleCombo(demo);
   Bind();clock.Interval=33;clock.Tick+=delegate{preview.Phase+=0.15;frame=(frame+1)%100;preview.Progress=frame/99f;preview.Invalidate();};clock.Start();
   FormClosing+=delegate(object sender,FormClosingEventArgs e){if(dirty){var result=MessageBox.Show(this,"Enregistrer les modifications avant de fermer ?","Personnalisation",MessageBoxButtons.YesNoCancel,MessageBoxIcon.Question);if(result==DialogResult.Cancel)e.Cancel=true;else if(result==DialogResult.Yes && !Save())e.Cancel=true;}};
   FormClosed+=delegate{clock.Stop();clock.Dispose();};
  }
  void ConfigureChoice(ComboBox box,string[] items,Action change){box.Width=250;box.DropDownStyle=ComboBoxStyle.DropDownList;box.Items.AddRange(items);StyleCombo(box);box.SelectedIndexChanged+=delegate{if(!binding){change();Changed();}};}
  void ConfigureCheck(CheckBox box,string text,Action change){box.Text=text;box.AutoSize=true;box.CheckedChanged+=delegate{if(!binding){change();Changed();}};}
  void ImportPng(){using(var picker=new OpenFileDialog{Title="Choisir un personnage PNG",Filter="Image PNG (*.png)|*.png",CheckFileExists=true})if(picker.ShowDialog(this)==DialogResult.OK){try{using(var image=PngArt.Read(picker.FileName)){}PngArt.Invalidate();draft.PngPath=picker.FileName;draft.UsePng=true;Bind();Changed();}catch(Exception ex){MessageBox.Show(this,"Impossible d’importer cette image : "+ex.Message,"Image PNG",MessageBoxButtons.OK,MessageBoxIcon.Warning);}}}
  static void StyleCombo(ComboBox combo){combo.DrawMode=DrawMode.OwnerDrawFixed;combo.ItemHeight=23;combo.DrawItem+=delegate(object sender,DrawItemEventArgs e){e.DrawBackground();if(e.Index>=0)TextRenderer.DrawText(e.Graphics,combo.Items[e.Index].ToString(),combo.Font,new Rectangle(e.Bounds.X+5,e.Bounds.Y,e.Bounds.Width-5,e.Bounds.Height),e.ForeColor,TextFormatFlags.VerticalCenter|TextFormatFlags.Left);e.DrawFocusRectangle();};}
  static FlowLayoutPanel Rows(Control parent){var panel=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoScroll=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(14,12,10,10)};parent.Controls.Add(panel);return panel;}
  static void Row(FlowLayoutPanel panel,string title,Control control){var group=new FlowLayoutPanel{AutoSize=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,Margin=new Padding(0,0,0,12)};if(title.Length>0)group.Controls.Add(new Label{Text=title,AutoSize=true,Font=new Font("Segoe UI",9,FontStyle.Bold),Margin=new Padding(0,0,0,5)});control.Margin=new Padding(0);group.Controls.Add(control);panel.Controls.Add(group);}
  static Button Button(string title,EventHandler click){var b=new Button{Text=title,Height=33,Width=100,FlatStyle=FlatStyle.Flat,BackColor=Color.White,Cursor=Cursors.Hand,Margin=new Padding(0,0,6,0)};b.FlatAppearance.BorderColor=Color.FromArgb(216,220,230);b.Click+=click;return b;}
  static void SetupSlider(TrackBar track,int min,int max,int tick){track.Minimum=min;track.Maximum=max;track.TickFrequency=tick;track.Width=280;track.Height=40;}
  static Control SliderPanel(TrackBar track,Label label){var panel=new FlowLayoutPanel{AutoSize=true};panel.Controls.Add(track);label.Width=85;label.Padding=new Padding(0,8,0,0);panel.Controls.Add(label);return panel;}
  void Changed(){dirty=true;status.Text="Modifications non enregistrées";preview.Value=draft;preview.Invalidate();}
  void Bind(){binding=true;name.Text=draft.Name;pattern.SelectedItem=draft.Pattern;collar.Checked=draft.Collar;size.Value=draft.Size;speed.Value=draft.Speed;frequency.Value=draft.Frequency;sizeValue.Text=draft.Size+" %";speedValue.Text=draft.Speed==0?"Immobile":draft.Speed+" / 5";frequencyValue.Text=draft.Frequency+" s";checks["Saut"].Checked=draft.Jump;checks["Étirement"].Checked=draft.Stretch;checks["Toilette"].Checked=draft.Groom;checks["Salut"].Checked=draft.Wave;checks["Sieste"].Checked=draft.Sleep;checks["Danse"].Checked=draft.Dance;checks["Pirouette"].Checked=draft.Spin;checks["Rebonds"].Checked=draft.Bounce;checks["Secousse"].Checked=draft.Shake;checks["Bâillement"].Checked=draft.Yawn;hat.SelectedItem=draft.Hat;eyeStyle.SelectedItem=draft.EyeStyle;blush.Checked=draft.Blush;whiskers.Checked=draft.Whiskers;usePng.Checked=draft.UsePng;mirrorPng.Checked=draft.MirrorPng;pngInfo.Text=string.IsNullOrEmpty(draft.PngPath)?"Aucune image importée":Path.GetFileName(draft.PngPath)+(File.Exists(draft.PngPath)?"":" — introuvable, retour au chat");foreach(var pair in colorButtons){Color c=Color.FromArgb(GetColor(pair.Key));pair.Value.BackColor=c;pair.Value.ForeColor=c.GetBrightness()<0.5?Color.White:Color.FromArgb(35,35,35);}preview.Value=draft;preview.Invalidate();binding=false;}
  int GetColor(string key){return key=="Pelage"?draft.Coat:key=="Yeux"?draft.Eyes:key=="Motifs"?draft.Marking:draft.Accessory;}
  void PickColor(string key){using(var picker=new ColorDialog{FullOpen=true,Color=Color.FromArgb(GetColor(key))})if(picker.ShowDialog(this)==DialogResult.OK){int color=picker.Color.ToArgb();if(key=="Pelage")draft.Coat=color;else if(key=="Yeux")draft.Eyes=color;else if(key=="Motifs")draft.Marking=color;else draft.Accessory=color;Bind();Changed();}}
  bool Save(){try{draft.Validate();if(draft.UsePng)draft.PngPath=PngArt.Keep(draft.PngPath,Path.Combine(Path.GetDirectoryName(Preferences.FilePath),"images"));draft.Save(Preferences.FilePath);if(cat!=null)cat.ApplyOptions(draft);dirty=false;Bind();status.Text="Enregistré · votre chat est à jour";return true;}catch(Exception ex){MessageBox.Show(this,"Impossible d’enregistrer : "+ex.Message,"ChatBureau",MessageBoxButtons.OK,MessageBoxIcon.Error);return false;}}
  public static void RunTests(string directory){
   Directory.CreateDirectory(directory);string path=Path.Combine(directory,"test-preferences.xml");
   var p=new Preferences{Name="Réglisse",Size=140,Speed=0,Pattern="Tigré",Collar=true,Jump=false,Sleep=false};p.Save(path);var read=Preferences.LoadFrom(path);
   if(read.Name!=p.Name||read.Size!=140||read.Speed!=0||read.Pattern!="Tigré"||!read.Collar||read.Enabled().Length!=8)throw new Exception("Persistence round-trip failed");
   p.Size=1000;p.Speed=-4;p.Frequency=0;p.Pattern="invalid";p.Validate();if(p.Size!=200||p.Speed!=0||p.Frequency!=3||p.Pattern!="Uni")throw new Exception("Validation failed");
   File.WriteAllText(path,"broken xml");if(Preferences.LoadFrom(path).Name!="Mochi")throw new Exception("Recovery failed");
   using(var studio=new Studio(null)){
    studio.Show();Application.DoEvents();studio.draft=read.Copy();studio.Bind();
    studio.size.Value=160;studio.name.Text="Pixel";studio.pattern.SelectedItem="Taches";studio.checks["Salut"].Checked=false;
    if(studio.draft.Size!=160||studio.draft.Name!="Pixel"||studio.draft.Pattern!="Taches"||studio.draft.Wave||!studio.dirty)throw new Exception("UI binding failed");
    studio.draft=read.Copy();studio.Bind();studio.dirty=false;
    using(var bitmap=new Bitmap(studio.Width,studio.Height)){studio.DrawToBitmap(bitmap,new Rectangle(0,0,bitmap.Width,bitmap.Height));bitmap.Save(Path.Combine(directory,"atelier.png"));}
    foreach(Control root in studio.Controls)SelectBehavior(root);
    Application.DoEvents();
    using(var bitmap=new Bitmap(studio.Width,studio.Height)){studio.DrawToBitmap(bitmap,new Rectangle(0,0,bitmap.Width,bitmap.Height));bitmap.Save(Path.Combine(directory,"comportement.png"));}
    studio.hat.SelectedItem="Couronne";studio.eyeStyle.SelectedItem="Grands";studio.blush.Checked=true;studio.whiskers.Checked=true;
    if(studio.draft.Hat!="Couronne"||!studio.draft.Blush||!studio.draft.Whiskers)throw new Exception("Style controls failed");
    studio.dirty=false;foreach(Control root in studio.Controls)SelectTab(root,2);Application.DoEvents();
    using(var bitmap=new Bitmap(studio.Width,studio.Height)){studio.DrawToBitmap(bitmap,new Rectangle(0,0,bitmap.Width,bitmap.Height));bitmap.Save(Path.Combine(directory,"style.png"));}
    foreach(Control root in studio.Controls)SelectTab(root,3);Application.DoEvents();
    using(var bitmap=new Bitmap(studio.Width,studio.Height)){studio.DrawToBitmap(bitmap,new Rectangle(0,0,bitmap.Width,bitmap.Height));bitmap.Save(Path.Combine(directory,"image-png.png"));}
    studio.Close();
   }
   using(var cat=new Cat()){cat.Show();cat.ApplyOptions(read);Application.DoEvents();if(cat.Width!=224||cat.Options.Name!=read.Name)throw new Exception("Desktop application of settings failed");cat.Close();}
   PngArt.Tests(directory);
   File.WriteAllText(Path.Combine(directory,"test-results.txt"),"PASS: settings round-trip, bounds validation, malformed file recovery, UI controls binding, studio render, desktop settings application.");
  }
  static void SelectTab(Control c,int index){var tabs=c as TabControl;if(tabs!=null)tabs.SelectedIndex=index;foreach(Control child in c.Controls)SelectTab(child,index);}
  static void SelectBehavior(Control control){var tabs=control as TabControl;if(tabs!=null)tabs.SelectedIndex=1;foreach(Control child in control.Controls)SelectBehavior(child);}
 }
}






