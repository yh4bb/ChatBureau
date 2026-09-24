using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ChatBureau {
 static class InteractionTests {
  public static void Run(string directory){
   int changes=0;
   ContextMenuStrip firstPopup=null,secondPopup=null;
   using(var window=new Form{ClientSize=new Size(560,170),ShowInTaskbar=false,StartPosition=FormStartPosition.Manual,Location=new Point(40,40)})
   using(var pattern=new IosChoice{Location=new Point(15,20)})
   using(var hat=new IosChoice{Location=new Point(285,20)}){
    pattern.Items.AddRange(new object[]{"Uni","Tigré","Taches","Bicolore","Pois","Dos sombre"});
    hat.Items.AddRange(new object[]{"Aucun","Bonnet","Couronne","Nœud"});
    pattern.SelectedIndexChanged+=delegate{changes++;};hat.SelectedIndexChanged+=delegate{changes++;};
    window.Controls.Add(pattern);window.Controls.Add(hat);window.Show();Application.DoEvents();
    for(int i=0;i<100;i++){
     var popup=pattern.Open();if(firstPopup==null)firstPopup=popup;if(popup!=firstPopup)throw new Exception("Dropdown recreated instead of retained");
     ((ToolStripMenuItem)popup.Items[i%pattern.Items.Count]).PerformClick();popup.Close(ToolStripDropDownCloseReason.ItemClicked);Application.DoEvents();
     if(popup.IsDisposed||pattern.SelectedIndex!=i%pattern.Items.Count)throw new Exception("Pattern menu closure regression");
     popup=hat.Open();if(secondPopup==null)secondPopup=popup;
     ((ToolStripMenuItem)popup.Items[i%hat.Items.Count]).PerformClick();popup.Close(ToolStripDropDownCloseReason.AppClicked);Application.DoEvents();
     if(popup.IsDisposed||hat.SelectedIndex!=i%hat.Items.Count)throw new Exception("Accessory menu closure regression");
     if(i%10==0){pattern.Open();hat.Open();Application.DoEvents();hat.Open().Close(ToolStripDropDownCloseReason.Keyboard);Application.DoEvents();}
    }
    pattern.AccessibilityObject.DoDefaultAction();Application.DoEvents();firstPopup.Close();Application.DoEvents();
    if(changes<190)throw new Exception("Selections did not reach bindings");
    pattern.Open();Application.DoEvents();window.Close();Application.DoEvents();
   }
   if(!firstPopup.IsDisposed||!secondPopup.IsDisposed)throw new Exception("Dropdown resources leaked after owner disposal");
   // Every supported coat/pattern/accessory combination must remain paintable.
   string[] patterns={"Uni","Tigré","Taches","Bicolore","Pois","Dos sombre"};string[] hats={"Aucun","Bonnet","Couronne","Nœud"};
   foreach(string pattern in patterns)foreach(string hat in hats)foreach(string eyes in new string[]{"Ronds","Grands","Endormis"})foreach(Color color in new Color[]{Color.Black,Color.White,Color.FromArgb(213,150,80)}){
    var options=new Preferences{Pattern=pattern,Hat=hat,EyeStyle=eyes,Coat=color.ToArgb(),Collar=true,Whiskers=true,Blush=true};
    using(var bitmap=new Bitmap(320,280))using(var graphics=Graphics.FromImage(bitmap))Cat.Draw(graphics,2,1.1,false,false,false,color,"Marche",0.5f,options);
   }
   using(var empty=Ios.Round(new RectangleF(0,0,0,0),12)){if(empty.PointCount!=0)throw new Exception("Zero-size painting guard failed");}
   File.WriteAllText(Path.Combine(directory,"interaction-tests.txt"),"PASS: 200 popup selections, cancellation/reopening/cross-menu focus, owner disposal, 216 coat/accessory render combinations and zero-size layout.");
  }
 }
}
