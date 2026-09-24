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
  public Preview(){DoubleBuffered=true;BackColor=Ios.Background;}
  protected override void OnPaint(PaintEventArgs e){
   base.OnPaint(e);var g=e.Graphics;g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
   using(var shape=Ios.Round(new RectangleF(0,0,Width-1,Height-1),24))using(var fill=new System.Drawing.Drawing2D.LinearGradientBrush(ClientRectangle,Color.FromArgb(219,226,242),Color.FromArgb(239,229,236),75f))g.FillPath(fill,shape);
   using(var shadow=new SolidBrush(Color.FromArgb(22,106,98,149)))g.FillEllipse(shadow,Width/2-70,Height-54,140,18);

   if(Value!=null)using(var f=new Font("Segoe UI",20,FontStyle.Bold))TextRenderer.DrawText(g,Value.Name,f,new Rectangle(18,32,Width-36,42),Ios.Ink,TextFormatFlags.HorizontalCenter|TextFormatFlags.EndEllipsis);
   if(Value==null||Width<2||Height<2)return;
   float scale=Math.Min(1.6f,Value.Size/100f);var state=g.Save();g.TranslateTransform((Width-160*scale)/2,Height-40-130*scale);
   Cat.Draw(g,scale,Phase,false,Action=="Sieste",false,Color.FromArgb(Value.Coat),Action,Progress,Value);g.Restore(state);
  }
 }
 class Studio : Form {
  readonly Cat cat;
  IosTabs navigation;
  UpdatePane updates;
  Preferences draft;
  readonly Preview preview=new Preview();
  readonly Timer clock=new Timer();
  readonly Label status=new Label();
  readonly TextBox name=new TextBox();
  readonly IosChoice pattern=new IosChoice();
  readonly IosChoice hat=new IosChoice(),eyeStyle=new IosChoice();
  readonly IosToggle blush=new IosToggle(),whiskers=new IosToggle(),usePng=new IosToggle(),mirrorPng=new IosToggle();
  readonly Label pngInfo=new Label();
  readonly IosToggle collar=new IosToggle();
  readonly IosSlider size=new IosSlider(),speed=new IosSlider(),frequency=new IosSlider();
  readonly Label sizeValue=new Label(),speedValue=new Label(),frequencyValue=new Label();
  readonly Dictionary<string,IosToggle> checks=new Dictionary<string,IosToggle>();
  readonly Dictionary<string,IosColorButton> colorButtons=new Dictionary<string,IosColorButton>();
  readonly List<IosColorButton> paletteButtons=new List<IosColorButton>();
  Button saveButton;
  bool binding,dirty;
  int frame;
  public Studio(Cat owner){
   cat=owner;draft=owner==null?new Preferences():owner.Options.Copy();
   Text="ChatBureau · Personnalisation";Font=new Font("Segoe UI",10);ForeColor=Ios.Ink;BackColor=Ios.Background;
   AutoScaleMode=AutoScaleMode.Dpi;ClientSize=new Size(980,710);MinimumSize=new Size(920,680);StartPosition=FormStartPosition.CenterScreen;
   var root=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(22),ColumnCount=1,RowCount=3};root.RowStyles.Add(new RowStyle(SizeType.Absolute,74));root.RowStyles.Add(new RowStyle(SizeType.Percent,100));root.RowStyles.Add(new RowStyle(SizeType.Absolute,76));Controls.Add(root);
   var header=new Panel{Dock=DockStyle.Fill};header.Controls.Add(new Label{Text="Votre compagnon",Font=new Font("Segoe UI",22,FontStyle.Bold),AutoSize=true,Location=new Point(0,0)});header.Controls.Add(new Label{Text="Son style, ses habitudes, votre touche personnelle.",AutoSize=true,ForeColor=Ios.Muted,Location=new Point(2,42)});root.Controls.Add(header,0,0);
   var body=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=1};body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,296));body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));root.Controls.Add(body,0,1);
   var left=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=4,Margin=new Padding(0,0,18,0)};left.RowStyles.Add(new RowStyle(SizeType.Absolute,0));left.RowStyles.Add(new RowStyle(SizeType.Percent,100));left.RowStyles.Add(new RowStyle(SizeType.Absolute,48));left.RowStyles.Add(new RowStyle(SizeType.Absolute,68));body.Controls.Add(left,0,0);
   preview.Dock=DockStyle.Fill;left.Controls.Add(preview,0,1);
   var demo=new IosChoice{Dock=DockStyle.Fill,DropDownStyle=ComboBoxStyle.DropDownList,Margin=new Padding(0,9,0,0)};demo.Items.AddRange(new object[]{"Marche","Saut","Étirement","Toilette","Salut","Sieste","Danse","Pirouette","Rebonds","Secousse","Bâillement","Porté"});demo.SelectedIndex=0;demo.SelectedIndexChanged+=delegate{preview.Action=(string)demo.SelectedItem;frame=0;};left.Controls.Add(demo,0,2);
   left.Controls.Add(new Label{Text="Essayez une animation dans l’aperçu.\nAppliquez vos réglages quand vous êtes prêt.",Dock=DockStyle.Fill,ForeColor=Ios.Muted,Padding=new Padding(0,10,0,0)},0,3);
   var tabs=new IosTabs{Dock=DockStyle.Fill};navigation=tabs;body.Controls.Add(tabs,1,0);
   var appearance=new Panel{Text="Apparence" ,BackColor=Color.White,AutoScroll=true};var behavior=new Panel{Text="Habitudes" ,BackColor=Color.White,AutoScroll=true};tabs.TabPages.Add(appearance);tabs.TabPages.Add(behavior);
   var details=new Panel{Text="Style" ,BackColor=Color.White};tabs.TabPages.Add(details);
   var images=new Panel{Text="Image PNG" ,BackColor=Color.White};tabs.TabPages.Add(images);
   var updatePage=new Panel{Text="Mises à jour"};tabs.TabPages.Add(updatePage);
   updates=new UpdatePane{Dock=DockStyle.Fill};updates.BeforeInstall=PrepareUpdate;updatePage.Controls.Add(updates);
   var style=Rows(details);var imageRows=Rows(images);
   ConfigureChoice(hat,new string[]{"Aucun","Bonnet","Couronne","Nœud"},delegate{draft.Hat=(string)hat.SelectedItem;});
   Row(style,"Accessoire",Stack(hat,new Label{Text="Couleur : choisissez « Collier » dans Apparence.",AutoSize=true,ForeColor=Ios.Muted}));
   ConfigureChoice(eyeStyle,new string[]{"Ronds","Grands","Endormis"},delegate{draft.EyeStyle=(string)eyeStyle.SelectedItem;});
   ConfigureCheck(blush,"Joues roses",delegate{draft.Blush=blush.Checked;});
   ConfigureCheck(whiskers,"Moustaches",delegate{draft.Whiskers=whiskers.Checked;});Row(style,"Visage",Stack(Inline("Yeux",eyeStyle),blush,whiskers));
   var import=Button("Importer un PNG…",delegate{ImportPng();});import.Width=200;
   pngInfo.AutoSize=true;pngInfo.MaximumSize=new Size(375,0);
   ConfigureCheck(usePng,"Remplacer le chat par mon image",delegate{draft.UsePng=usePng.Checked;});
   ConfigureCheck(mirrorPng,"Retourner l’image selon la direction",delegate{draft.MirrorPng=mirrorPng.Checked;});
   var clear=Button("Revenir au chat",delegate{draft.UsePng=false;Bind();Changed();});clear.Width=170;Row(imageRows,"Votre personnage",Stack(import,pngInfo,usePng,mirrorPng,clear));
   Row(imageRows,"Comment ça marche",new Label{AutoSize=true,MaximumSize=new Size(375,0),ForeColor=Ios.Muted,Text="Choisissez un PNG transparent pour éviter un fond visible.\nLes proportions sont conservées et l’image bouge en entier.\nLes membres ne sont pas animés séparément.\n\nUne copie est conservée à l’enregistrement : vous pouvez\ndéplacer le fichier d’origine. Maximum : 16 Mo, 4 096 × 4 096 px."});
   var look=Rows(appearance);var habits=Rows(behavior);
   name.MaxLength=24;name.Width=280;name.BorderStyle=BorderStyle.FixedSingle;name.BackColor=Ios.Field;name.AccessibleName="Nom du chat";name.Font=new Font("Segoe UI",12);name.TextChanged+=delegate{if(!binding){draft.Name=name.Text;Changed();}};Row(look,"Nom du compagnon",name);
   var presets=new FlowLayoutPanel{AutoSize=true,WrapContents=true,MaximumSize=new Size(420,0)};
   string[] titles={"Ivoire","Charbon","Abricot","Sauge"};Color[] colors={Color.FromArgb(235,234,219),Color.FromArgb(53,55,52),Color.FromArgb(226,173,122),Color.FromArgb(160,184,159)};
   for(int i=0;i<titles.Length;i++){Color color=colors[i];var button=new IosColorButton{Text=titles[i],Swatch=color,Width=110,Margin=new Padding(0,0,6,0)};button.Click+=delegate{draft.Coat=color.ToArgb();draft.Eyes=(color.GetBrightness()<0.4f?Color.FromArgb(235,234,219):Color.FromArgb(52,53,48)).ToArgb();Bind();Changed();};paletteButtons.Add(button);presets.Controls.Add(button);}
   presets.MaximumSize=new Size(490,0);
   var swatches=new FlowLayoutPanel{AutoSize=true};foreach(string key in new string[]{"Pelage","Yeux","Motifs","Collier"}){string k=key;var button=new IosColorButton{Text=k,Width=110,Height=42,Margin=new Padding(0,0,6,0)};button.Click+=delegate{PickColor(k);};colorButtons[k]=button;swatches.Controls.Add(button);}
   pattern.DropDownStyle=ComboBoxStyle.DropDownList;pattern.Width=280;pattern.Items.AddRange(new object[]{"Uni","Tigré","Taches","Bicolore","Pois","Dos sombre"});pattern.SelectedIndexChanged+=delegate{if(!binding){draft.Pattern=(string)pattern.SelectedItem;Changed();}};
   Row(look,"Pelage et couleurs",Stack(presets,swatches,Inline("Motif",pattern)));
   collar.Text="Collier avec clochette";collar.AutoSize=true;collar.CheckedChanged+=delegate{if(!binding){draft.Collar=collar.Checked;Changed();}};
   SetupSlider(size,60,200,20);size.ValueChanged+=delegate{sizeValue.Text=size.Value+" %";if(!binding){draft.Size=size.Value;Changed();}};
   Row(look,"Sur le bureau",Stack(Inline("Taille",SliderPanel(size,sizeValue)),collar));
   SetupSlider(speed,0,5,1);speed.ValueChanged+=delegate{speedValue.Text=speed.Value==0?"Immobile":speed.Value+" / 5";if(!binding){draft.Speed=speed.Value;Changed();}};
   SetupSlider(frequency,3,30,3);frequency.ValueChanged+=delegate{frequencyValue.Text=frequency.Value+" s";if(!binding){draft.Frequency=frequency.Value;Changed();}};Row(habits,"Rythme du compagnon",Stack(Inline("Vitesse",SliderPanel(speed,speedValue)),Inline("Intervalle",SliderPanel(frequency,frequencyValue))));
   var choices=new FlowLayoutPanel{Width=496,Height=225,WrapContents=true};
   foreach(string title in new string[]{"Saut","Étirement","Toilette","Salut","Sieste","Danse","Pirouette","Rebonds","Secousse","Bâillement"}){var check=new IosToggle{Text=title,AutoSize=false,Width=236,Height=40,Margin=new Padding(0,0,10,4)};checks[title]=check;check.CheckedChanged+=delegate{if(!binding){draft.Jump=checks["Saut"].Checked;draft.Stretch=checks["Étirement"].Checked;draft.Groom=checks["Toilette"].Checked;draft.Wave=checks["Salut"].Checked;draft.Sleep=checks["Sieste"].Checked;draft.Dance=checks["Danse"].Checked;draft.Spin=checks["Pirouette"].Checked;draft.Bounce=checks["Rebonds"].Checked;draft.Shake=checks["Secousse"].Checked;draft.Yawn=checks["Bâillement"].Checked;Changed();}};choices.Controls.Add(check);}Row(habits,"Animations spontanées",choices);
   Row(habits,"",new Label{Text="Décochez tout pour garder uniquement la promenade.\nLes animations restent disponibles dans le menu du chat.",AutoSize=true,ForeColor=Ios.Muted});
   var footer=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=3,RowCount=2,Padding=new Padding(0,12,0,0)};footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,110));footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,190));root.Controls.Add(footer,0,2);
   var reset=Button("Réinitialiser",delegate{draft=new Preferences();Bind();Changed();});reset.Width=120;footer.Controls.Add(reset,0,0);
   var close=Button("Fermer",delegate{Close();});footer.Controls.Add(close,1,0);
   var save=Button("Appliquer au chat",delegate{Save();});saveButton=save;save.Enabled=false;save.Width=182;save.BackColor=Ios.Blue;save.ForeColor=Color.White;footer.Controls.Add(save,2,0);
   status.AutoSize=true;status.Text="Vos réglages sont enregistrés";status.ForeColor=Ios.Muted;footer.Controls.Add(status,0,1);footer.SetColumnSpan(status,3);
   StyleCombo(pattern);StyleCombo(demo);
   Bind();clock.Interval=33;clock.Tick+=delegate{preview.Phase+=0.15;frame=(frame+1)%100;preview.Progress=frame/99f;preview.Invalidate();};clock.Start();
   FormClosing+=delegate(object sender,FormClosingEventArgs e){if(updates.Busy){e.Cancel=true;return;}if(dirty){var result=MessageBox.Show(this,"Enregistrer les modifications avant de fermer ?","Personnalisation",MessageBoxButtons.YesNoCancel,MessageBoxIcon.Question);if(result==DialogResult.Cancel)e.Cancel=true;else if(result==DialogResult.Yes && !Save())e.Cancel=true;}};
   FormClosed+=delegate{clock.Stop();clock.Dispose();};
  }
  public void OpenUpdates(Updates.Release release=null){navigation.SelectedIndex=4;if(release!=null)updates.Offer(release);}
  bool PrepareUpdate(){if(!dirty)return true;var choice=MessageBox.Show(this,"Enregistrer vos modifications avant de mettre à jour ?","ChatBureau",MessageBoxButtons.YesNoCancel,MessageBoxIcon.Question);if(choice==DialogResult.Cancel)return false;if(choice==DialogResult.Yes)return Save();dirty=false;return true;}
  void ConfigureChoice(IosChoice box,string[] items,Action change){box.Width=250;box.DropDownStyle=ComboBoxStyle.DropDownList;box.Items.AddRange(items);StyleCombo(box);box.SelectedIndexChanged+=delegate{if(!binding){change();Changed();}};}
  void ConfigureCheck(IosToggle box,string text,Action change){box.Text=text;box.AutoSize=true;box.CheckedChanged+=delegate{if(!binding){change();Changed();}};}
  void ImportPng(){using(var picker=new OpenFileDialog{Title="Choisir un personnage PNG",Filter="Image PNG (*.png)|*.png",CheckFileExists=true})if(picker.ShowDialog(this)==DialogResult.OK){try{using(var image=PngArt.Read(picker.FileName)){}PngArt.Invalidate();draft.PngPath=picker.FileName;draft.UsePng=true;Bind();Changed();}catch(Exception ex){MessageBox.Show(this,"Impossible d’importer cette image : "+ex.Message,"Image PNG",MessageBoxButtons.OK,MessageBoxIcon.Warning);}}}
  static void StyleCombo(IosChoice combo){combo.Height=36;}
  static Control Stack(params Control[] children){var p=new FlowLayoutPanel{AutoSize=true,FlowDirection=FlowDirection.TopDown,WrapContents=false};foreach(var child in children){child.Margin=new Padding(0,0,0,7);p.Controls.Add(child);}return p;}
  static Control Inline(string text,Control control){var p=new FlowLayoutPanel{AutoSize=true};p.Controls.Add(new Label{Text=text,Width=104,Height=36,TextAlign=ContentAlignment.MiddleLeft,ForeColor=Ios.Muted});p.Controls.Add(control);return p;}
  static FlowLayoutPanel Rows(Control parent){var panel=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoScroll=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(2,2,6,10)};parent.Controls.Add(panel);return panel;}
  static void Row(FlowLayoutPanel panel,string title,Control control){var group=new IosCard{AutoSize=true,MinimumSize=new Size(536,0),FlowDirection=FlowDirection.TopDown,WrapContents=false,Margin=new Padding(0,0,0,10)};if(title.Length>0)group.Controls.Add(new Label{Text=title,AutoSize=true,Font=new Font("Segoe UI",9,FontStyle.Bold),ForeColor=Ios.Muted,Margin=new Padding(0,0,0,8)});control.Margin=new Padding(0);if(string.IsNullOrEmpty(control.AccessibleName))control.AccessibleName=title;group.Controls.Add(control);panel.Controls.Add(group);}
  static Button Button(string title,EventHandler click){var b=new IosButton{Text=title,Height=38,Width=100,FlatStyle=FlatStyle.Flat,BackColor=Color.FromArgb(232,235,241),ForeColor=Ios.Ink,Cursor=Cursors.Hand,Margin=new Padding(0,0,6,0)};b.FlatAppearance.BorderColor=Color.FromArgb(216,220,230);b.Click+=click;return b;}
  static void SetupSlider(IosSlider track,int min,int max,int tick){track.Minimum=min;track.Maximum=max;track.TickFrequency=tick;track.Width=280;track.Height=40;}
  static Control SliderPanel(IosSlider track,Label label){var panel=new FlowLayoutPanel{AutoSize=true};panel.Controls.Add(track);label.Width=85;label.Padding=new Padding(0,8,0,0);panel.Controls.Add(label);return panel;}
  void Changed(){dirty=true;if(saveButton!=null)saveButton.Enabled=true;status.ForeColor=Ios.Muted;status.Text="Aperçu modifié · prêt à appliquer";preview.Value=draft;preview.Invalidate();}
  void Bind(){binding=true;name.Text=draft.Name;pattern.SelectedItem=draft.Pattern;collar.Checked=draft.Collar;size.Value=draft.Size;speed.Value=draft.Speed;frequency.Value=draft.Frequency;sizeValue.Text=draft.Size+" %";speedValue.Text=draft.Speed==0?"Immobile":draft.Speed+" / 5";frequencyValue.Text=draft.Frequency+" s";checks["Saut"].Checked=draft.Jump;checks["Étirement"].Checked=draft.Stretch;checks["Toilette"].Checked=draft.Groom;checks["Salut"].Checked=draft.Wave;checks["Sieste"].Checked=draft.Sleep;checks["Danse"].Checked=draft.Dance;checks["Pirouette"].Checked=draft.Spin;checks["Rebonds"].Checked=draft.Bounce;checks["Secousse"].Checked=draft.Shake;checks["Bâillement"].Checked=draft.Yawn;hat.SelectedItem=draft.Hat;eyeStyle.SelectedItem=draft.EyeStyle;blush.Checked=draft.Blush;whiskers.Checked=draft.Whiskers;usePng.Checked=draft.UsePng;mirrorPng.Checked=draft.MirrorPng;pngInfo.Text=string.IsNullOrEmpty(draft.PngPath)?"Aucune image importée":Path.GetFileName(draft.PngPath)+(File.Exists(draft.PngPath)?"":" — introuvable, retour au chat");foreach(var pair in colorButtons)pair.Value.Swatch=Color.FromArgb(GetColor(pair.Key));foreach(var button in paletteButtons)button.Selected=button.Swatch.ToArgb()==draft.Coat;preview.Value=draft;preview.Invalidate();binding=false;}
  int GetColor(string key){return key=="Pelage"?draft.Coat:key=="Yeux"?draft.Eyes:key=="Motifs"?draft.Marking:draft.Accessory;}
  void PickColor(string key){using(var picker=new ColorDialog{FullOpen=true,Color=Color.FromArgb(GetColor(key))})if(picker.ShowDialog(this)==DialogResult.OK){int color=picker.Color.ToArgb();if(key=="Pelage")draft.Coat=color;else if(key=="Yeux")draft.Eyes=color;else if(key=="Motifs")draft.Marking=color;else draft.Accessory=color;Bind();Changed();}}
  bool Save(){try{draft.Validate();if(draft.UsePng)draft.PngPath=PngArt.Keep(draft.PngPath,Path.Combine(Path.GetDirectoryName(Preferences.FilePath),"images"));draft.Save(Preferences.FilePath);if(cat!=null)cat.ApplyOptions(draft);dirty=false;Bind();saveButton.Enabled=false;status.ForeColor=Color.FromArgb(32,116,75);status.Text="Réglages appliqués et enregistrés";return true;}catch(Exception ex){MessageBox.Show(this,"Impossible d’enregistrer : "+ex.Message,"ChatBureau",MessageBoxButtons.OK,MessageBoxIcon.Error);return false;}}
  public static void RunTests(string directory){
   Directory.CreateDirectory(directory);string path=Path.Combine(directory,"test-preferences.xml");
   var p=new Preferences{Name="Réglisse",Size=140,Speed=0,Pattern="Tigré",Collar=true,Jump=false,Sleep=false};p.Save(path);var read=Preferences.LoadFrom(path);
   if(read.Name!=p.Name||read.Size!=140||read.Speed!=0||read.Pattern!="Tigré"||!read.Collar||read.Enabled().Length!=8)throw new Exception("Persistence round-trip failed");
   p.Size=1000;p.Speed=-4;p.Frequency=0;p.Pattern="invalid";p.Validate();if(p.Size!=200||p.Speed!=0||p.Frequency!=3||p.Pattern!="Uni")throw new Exception("Validation failed");
   File.WriteAllText(path,"broken xml");if(Preferences.LoadFrom(path).Name!="Mochi")throw new Exception("Recovery failed");
   using(var studio=new Studio(null)){
    studio.Show();Application.DoEvents();studio.draft=read.Copy();studio.Bind();
    for(int i=0;i<12;i++){
     var menu=studio.pattern.Open();((ToolStripMenuItem)menu.Items[i%6]).PerformClick();menu.Close();Application.DoEvents();
     if(studio.draft.Pattern!=(string)studio.pattern.Items[i%6])throw new Exception("Pattern popup binding failed");
    }
    studio.navigation.SelectedIndex=2;Application.DoEvents();
    for(int i=0;i<12;i++){
     var menu=studio.hat.Open();((ToolStripMenuItem)menu.Items[i%4]).PerformClick();menu.Close();Application.DoEvents();
     if(studio.draft.Hat!=(string)studio.hat.Items[i%4])throw new Exception("Accessory popup binding failed");
    }
    studio.navigation.SelectedIndex=0;Application.DoEvents();
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
    studio.OpenUpdates();Application.DoEvents();
    using(var bitmap=new Bitmap(studio.Width,studio.Height)){studio.DrawToBitmap(bitmap,new Rectangle(0,0,bitmap.Width,bitmap.Height));bitmap.Save(Path.Combine(directory,"mises-a-jour.png"));}
    studio.ClientSize=new Size(920,680);Application.DoEvents();
    using(var bitmap=new Bitmap(studio.Width,studio.Height)){studio.DrawToBitmap(bitmap,new Rectangle(0,0,bitmap.Width,bitmap.Height));bitmap.Save(Path.Combine(directory,"compact.png"));}
    studio.Scale(new SizeF(1.25f,1.25f));Application.DoEvents();
    using(var bitmap=new Bitmap(studio.Width,studio.Height)){studio.DrawToBitmap(bitmap,new Rectangle(0,0,bitmap.Width,bitmap.Height));bitmap.Save(Path.Combine(directory,"scale-125.png"));}
    if(studio.navigation.SelectedIndex!=4||!studio.updates.Visible)throw new Exception("Update category not reachable");
    studio.Close();
   }
   using(var cat=new Cat()){cat.Show();cat.ApplyOptions(read);Application.DoEvents();if(cat.Width!=224||cat.Options.Name!=read.Name)throw new Exception("Desktop application of settings failed");cat.Close();}
   PngArt.Tests(directory);
   InteractionTests.Run(directory);
   File.WriteAllText(Path.Combine(directory,"test-results.txt"),"PASS: settings round-trip, bounds validation, malformed file recovery, UI controls binding, studio render, desktop settings application.");
  }
  static void SelectTab(Control c,int index){var tabs=c as IosTabs;if(tabs!=null)tabs.SelectedIndex=index;foreach(Control child in c.Controls)SelectTab(child,index);}
  static void SelectBehavior(Control control){var tabs=control as IosTabs;if(tabs!=null)tabs.SelectedIndex=1;foreach(Control child in control.Controls)SelectBehavior(child);}
 }
}
