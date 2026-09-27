using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace ChatBureau {
 sealed class LookTile:IosButton {
  public Preferences Look;
  protected override void DrawContent(Graphics g){TextRenderer.DrawText(g,Text,Font,new Rectangle(10,Height-35,Width-20,26),Ios.Ink,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);if(Look==null)return;var state=g.Save();g.TranslateTransform((Width-128)/2,2);Cat.Draw(g,.8f,0,false,false,false,Color.FromArgb(Look.Coat),"Marche",0,Look);g.Restore(state);}
 }
 sealed class WardrobePane:UserControl {
  readonly Func<Preferences> current;readonly Action<Preferences> apply;readonly string folder,imageFolder;
  readonly FlowLayoutPanel gallery=new FlowLayoutPanel();readonly Label status=new Label();readonly TextBox title=new TextBox();
  public WardrobePane(Func<Preferences> current,Action<Preferences> apply,string folder=null){
   this.folder=folder??Wardrobe.Folder;imageFolder=folder==null?Wardrobe.ImageFolder:Path.Combine(folder,"images");
   this.current=current;this.apply=apply;BackColor=Ios.Background;
   var header=new FlowLayoutPanel{Dock=DockStyle.Top,Height=174,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(2,4,0,0)};Controls.Add(header);
   header.Controls.Add(new Label{Text="Gardez vos créations, retrouvez-les en un clic.",Width=510,Height=28,ForeColor=Ios.Muted});
   var line=new FlowLayoutPanel{Width=530,Height=44};title.Width=250;title.MaxLength=40;title.Text="Mon nouveau style";title.AccessibleName="Nom du style à enregistrer";title.Margin=new Padding(0,8,12,0);line.Controls.Add(title);line.Controls.Add(Button("Enregistrer ce look",delegate{Run(delegate{Wardrobe.Save(title.Text,current(),this.folder);Reload();status.Text="Style ajouté à votre garde-robe.";});},210));header.Controls.Add(line);
   var sharing=new FlowLayoutPanel{Width=530,Height=44};sharing.Controls.Add(Button("Importer un style…",delegate{using(var dialog=new OpenFileDialog{Filter="Style ChatBureau (*.chatlook)|*.chatlook",Title="Importer un style"})if(dialog.ShowDialog(this)==DialogResult.OK)Run(delegate{var package=Wardrobe.Import(dialog.FileName,imageFolder);Wardrobe.Save(package.Title,package.Look,this.folder);Reload();status.Text="Style importé, prêt à essayer.";});},210));sharing.Controls.Add(Button("Exporter le look actuel…",delegate{using(var dialog=new SaveFileDialog{Filter="Style ChatBureau (*.chatlook)|*.chatlook",FileName="Mon style.chatlook"})if(dialog.ShowDialog(this)==DialogResult.OK)Run(delegate{Wardrobe.Export(dialog.FileName,title.Text,current());status.Text="Style exporté avec ses images et animations.";});},250));header.Controls.Add(sharing);
   status.Width=520;status.Height=48;status.ForeColor=Ios.Muted;header.Controls.Add(status);
   gallery.Dock=DockStyle.Fill;gallery.AutoScroll=true;gallery.Padding=new Padding(2,8,2,8);Controls.Add(gallery);gallery.BringToFront();Run(Reload);
  }
  static IosButton Button(string text,EventHandler click,int width){var button=new IosButton{Text=text,Width=width,Height=38,Margin=new Padding(0,0,10,0)};button.Click+=click;return button;}
  void Run(Action action){try{action();}catch(Exception ex){if(ex is OutOfMemoryException)throw;status.Text="Impossible de terminer : "+ex.Message;}}
  void Reload(){while(gallery.Controls.Count>0)gallery.Controls[0].Dispose();if(!Directory.Exists(folder)){status.Text="Aucun style enregistré. Composez un look, puis donnez-lui un nom.";return;}int loaded=0,unreadable=0;foreach(string file in Directory.GetFiles(folder,"*.chatlook")){try{var pack=Wardrobe.Import(file,imageFolder);AddTile(pack);loaded++;}catch(IOException){unreadable++;}catch(InvalidOperationException){unreadable++;}catch(UnauthorizedAccessException){unreadable++;}catch(System.Xml.XmlException){unreadable++;}}status.Text=loaded+" style(s) · cliquez pour essayer"+(unreadable>0?" · "+unreadable+" fichier(s) illisible(s)":"");}
  internal void AddTile(LookPackage pack){var tile=new LookTile{Look=pack.Look,Text=pack.Title,Width=164,Height=152,Margin=new Padding(0,0,12,12),AccessibleName="Essayer le style "+pack.Title};tile.Click+=delegate{apply(pack.Look);status.Text=pack.Title+" dans l’aperçu · cliquez sur Appliquer au chat.";};gallery.Controls.Add(tile);}
 }
}
