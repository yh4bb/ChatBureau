using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChatBureau {
 sealed class MischiefPane:UserControl {
  readonly Mischief host;
  readonly Func<System.Collections.Generic.List<PrankWindow>> windowProvider;
  readonly IosChoice mode=new IosChoice(),windows=new IosChoice();
  readonly IosSlider interval=new IosSlider();
  readonly Label state=new Label(),seconds=new Label(),help=new Label();
  readonly IosButton start=new IosButton(),refresh=new IosButton();
  readonly Timer ticker=new Timer{Interval=250};
  readonly IosCard applicationsGroup;
  string notice;long noticeUntil;
  public MischiefPane(Mischief host,Func<System.Collections.Generic.List<PrankWindow>> windowProvider=null){
   this.host=host;this.windowProvider=windowProvider??MischiefDesktop.Windows;BackColor=Ios.Background;
   var rows=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=true,Padding=new Padding(2)};Controls.Add(rows);
   var behavior=Card(rows,"Le petit fauteur de troubles");
   mode.Width=480;mode.Items.AddRange(new object[]{"Visuel · souris et bords des fenêtres","Interactif · petits coups de défilement"});mode.SelectedIndex=0;mode.AccessibleName="Mode de farce";behavior.Controls.Add(mode);
   help.Width=480;help.Height=60;help.ForeColor=Ios.Muted;help.Margin=new Padding(0,10,0,0);behavior.Controls.Add(help);
   var apps=Card(rows,"Fenêtre autorisée en mode interactif");applicationsGroup=apps;windows.Width=480;windows.AccessibleName="Fenêtre autorisée";apps.Controls.Add(windows);
   refresh.Text="Actualiser les fenêtres";refresh.Width=200;refresh.Height=38;refresh.Margin=new Padding(0,8,0,0);refresh.Click+=delegate{LoadWindows();};apps.Controls.Add(refresh);
   apps.Controls.Add(new Label{Text="Le défilement agit uniquement au premier plan, hors saisie.\nCertaines applications ne proposent pas de défilement compatible.",Width=485,Height=44,ForeColor=Ios.Muted,Margin=new Padding(0,8,0,0)});
   var rhythm=Card(rows,"À son rythme");var line=new FlowLayoutPanel{Width=490,Height=44};interval.Minimum=15;interval.Maximum=180;interval.Value=45;interval.Width=330;interval.AccessibleName="Intervalle minimum entre deux farces";line.Controls.Add(interval);seconds.Width=130;seconds.Height=40;seconds.TextAlign=ContentAlignment.MiddleLeft;line.Controls.Add(seconds);rhythm.Controls.Add(line);interval.ValueChanged+=delegate{seconds.Text=interval.Value+" secondes";};seconds.Text="45 secondes";
   var actions=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=142,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(2,8,0,0)};Controls.Add(actions);
   start.Text="Activer pour cette session";start.Width=260;start.Height=40;start.BackColor=Ios.Blue;start.ForeColor=Color.White;start.Enabled=host!=null;start.Margin=new Padding(0,0,0,8);actions.Controls.Add(start);
   state.Width=500;state.Height=34;state.ForeColor=Ios.Muted;actions.Controls.Add(state);
   actions.Controls.Add(new Label{Text="Fermez l’atelier pour le laisser jouer. Échap arrête les farces.\nDésactivé au lancement. Aucun clic ni texte saisi.",Width=500,Height=42,ForeColor=Ios.Muted});
   if(host!=null){mode.SelectedIndex=host.Interactive?1:0;interval.Value=host.IntervalSeconds;if(host.SelectedWindow!=null){windows.Items.Add(host.SelectedWindow);windows.SelectedIndex=0;}}
   mode.SelectedIndexChanged+=delegate{if(mode.SelectedIndex==1&&windows.Items.Count==0&&host!=null)LoadWindows();RefreshState();};start.Click+=delegate{
    if(host==null)return;
    try{if(host.Active)host.Stop();else host.Start(mode.SelectedIndex==1,windows.SelectedItem as PrankWindow,interval.Value,Mischief.Now);notice=null;}catch(InvalidOperationException ex){ShowNotice(ex.Message);return;}
    RefreshState();
   };
   ticker.Tick+=delegate{RefreshState();};VisibleChanged+=delegate{ticker.Enabled=Visible;};RefreshState();
  }
  static IosCard Card(FlowLayoutPanel rows,string title){var card=new IosCard{Width=536,AutoSize=true,MinimumSize=new Size(536,0),FlowDirection=FlowDirection.TopDown,WrapContents=false,Margin=new Padding(0,0,0,12)};card.Controls.Add(new Label{Text=title,AutoSize=true,Font=new Font("Segoe UI",10,FontStyle.Bold),ForeColor=Ios.Ink,Margin=new Padding(0,0,0,10)});rows.Controls.Add(card);return card;}
  void ShowNotice(string text){notice=text;noticeUntil=Mischief.Now+6000;state.Text=text;}
  void LoadWindows(){var selected=windows.SelectedItem as PrankWindow;windows.SelectedIndex=-1;windows.Items.Clear();var available=windowProvider();foreach(var window in available)windows.Items.Add(window);if(selected!=null)for(int i=0;i<available.Count;i++)if(available[i].Handle==selected.Handle)windows.SelectedIndex=i;ShowNotice(available.Count==0?"Aucune fenêtre disponible. Ouvrez une application puis actualisez.":"Choisissez explicitement la fenêtre où le chat peut jouer.");}
  void RefreshState(){bool active=host!=null&&host.Active;mode.Enabled=!active;applicationsGroup.Visible=mode.SelectedIndex==1;windows.Enabled=refresh.Enabled=!active&&mode.SelectedIndex==1;interval.Enabled=!active;start.Text=active?"Arrêter les farces":"Activer pour cette session";state.Text=notice!=null&&Mischief.Now<noticeUntil?notice:host==null?"Activez ce mode depuis le chat de votre bureau.":host.Status;help.Text=mode.SelectedIndex==0?"Il s’approche du pointeur, puis joue sur le bord de votre fenêtre.\nVos applications et votre souris gardent leur fonctionnement habituel.":"Il donne un coup de patte pour faire défiler la fenêtre choisie.\nIl attend une pause dans vos frappes et ne prend jamais le focus.";}
  protected override void Dispose(bool disposing){if(disposing)ticker.Dispose();base.Dispose(disposing);}
  internal static void TestControls(Mischief host){
   var target=new PrankWindow{Handle=new IntPtr(123),Title="Fenêtre de test"};
   using(var form=new Form{ClientSize=new Size(600,540),ShowInTaskbar=false})using(var pane=new MischiefPane(host,()=>new System.Collections.Generic.List<PrankWindow>{target}){Dock=DockStyle.Fill}){
    form.Controls.Add(pane);form.Show();Application.DoEvents();pane.start.PerformClick();if(!host.Active||pane.mode.Enabled)throw new Exception("Visual mode activation UI failed");pane.start.PerformClick();if(host.Active)throw new Exception("Stop button failed");
    pane.mode.SelectedIndex=1;pane.start.PerformClick();if(host.Active)throw new Exception("Interactive mode selected a window without user choice");pane.windows.SelectedIndex=0;pane.start.PerformClick();if(!host.Active||pane.windows.Enabled)throw new Exception("Selected interactive target not activated");pane.start.PerformClick();form.Close();
   }
   host.Start(true,target,60,Mischief.Now);using(var reopened=new MischiefPane(host,()=>new System.Collections.Generic.List<PrankWindow>{target})){if(reopened.mode.SelectedIndex!=1||reopened.interval.Value!=60||reopened.windows.SelectedItem!=target||reopened.mode.Enabled)throw new Exception("Reopened panel lost the active mode");}host.Stop();
  }
 }
}
