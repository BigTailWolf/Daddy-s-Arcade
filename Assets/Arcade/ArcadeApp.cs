using UnityEngine;
using UnityEngine.InputSystem;

namespace DaddysArcade
{
    public sealed class ArcadeApp : MonoBehaviour
    {
        enum ScreenState { Arcade, MainMenu, Playing }
        ScreenState screen;
        int selectedMode;
        MatchSession session;
        ArcadePauseMenu pause;
        readonly ArcadeInput[] inputs = { new ArcadeInput(), new ArcadeInput(), new ArcadeInput() };
        readonly ArcadeInputFrame[] frames = new ArcadeInputFrame[3];
        readonly Gamepad[] pads = new Gamepad[3];
        readonly float[] dropWait = new float[3];
        static readonly string[] ModeNames = { "KID", "PARENT", "PARENT + CHILD", "PK", "WHOLE FAMILY" };
        bool controllerMissing;
        Texture2D homeArt;
        ArcadeMusic music;
        static readonly Color[] Colors = { new Color(.12f,.14f,.24f), Color.cyan,
            Color.yellow, new Color(.7f,.4f,1), Color.green, Color.red, Color.blue,
            new Color(1,.55f,.2f), new Color(.5f,.55f,.65f) };
        bool Finished => session != null && session.Finished;
        void Awake()
        {
            homeArt = Resources.Load<Texture2D>("ArcadeHome");
            music = gameObject.AddComponent<ArcadeMusic>();
        }
        void LateUpdate() { music.SetContext(screen == ScreenState.Playing, pause != null || Finished); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindAnyObjectByType<ArcadeApp>() != null) return;
            var root = new GameObject("Daddy's Arcade");
            DontDestroyOnLoad(root);
            root.AddComponent<ArcadeApp>();
        }
        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (screen != ScreenState.Playing) {
                for (int i=0;i<pads.Length;i++) pads[i] = Gamepad.all.Count > i ? Gamepad.all[i] : null;
            }
            else {
                foreach (var connected in Gamepad.all) {
                    if (System.Array.IndexOf(pads,connected) >= 0) continue;
                    for (int i=0;i<session.Players.Length;i++)
                        if (pads[i] == null) { pads[i] = connected; break; }
                }
            }
            bool confirm = false, back = false, menu = false;
            int horizontal = 0, vertical = 0;
            int inputCount = screen == ScreenState.Playing ? session.Players.Length : 3;
            for (int i=0;i<inputCount;i++) {
                frames[i] = inputs[i].Read(pads[i],i,dt);
                confirm |= frames[i].A; back |= frames[i].B; menu |= frames[i].Pause;
                if (horizontal == 0) horizontal = frames[i].Horizontal;
                if (vertical == 0) vertical = frames[i].Vertical;
            }
            if (screen == ScreenState.Arcade) {
                if (confirm) screen = ScreenState.MainMenu;
                return;
            }
            if (screen == ScreenState.MainMenu) {
                if (back || menu) { screen = ScreenState.Arcade; return; }
                int direction = horizontal != 0 ? horizontal : vertical;
                if (direction != 0) selectedMode = (selectedMode + direction + ModeNames.Length) % ModeNames.Length;
                if (confirm) StartGame();
                return;
            }
            controllerMissing = false;
            for (int i=0;i<session.Players.Length;i++) controllerMissing |= pads[i] != null && !pads[i].added;
            if (controllerMissing && pause == null) OpenPause();
            if (pause != null) {
                if (vertical != 0) pause.Move(vertical);
                if (menu || back) Resume();
                else if (confirm) pause.Confirm();
                return;
            }
            if (menu) { OpenPause(); return; }
            if (Finished) { if (confirm) StartGame(); return; }
            for (int i=0;i<session.Players.Length && !Finished;i++) Apply(session.Players[i],frames[i],i,dt);
            for (int i=0;i<session.Players.Length && !Finished;i++) session.Players[i].Tick(dt);
        }
        void Apply(BlocksRules target, ArcadeInputFrame frame, int player, float dt)
        {
            if (frame.Horizontal != 0) target.Move(frame.Horizontal, 0);
            if (frame.A) target.Rotate(1);
            if (frame.B) target.Rotate(-1);
            if (frame.Drop) { target.HardDrop(); dropWait[player] = .05f; return; }
            if (!frame.Down) { dropWait[player] = 0; return; }
            dropWait[player] -= dt;
            if (dropWait[player] <= 0) { target.StepDown(); dropWait[player] = .05f; }
        }
        void StartGame()
        {
            int seed = System.Environment.TickCount;
            session = new MatchSession(seed,(MatchMode)selectedMode);
            for (int i=0;i<pads.Length;i++) {
                pads[i] = i < session.Players.Length && Gamepad.all.Count > i ? Gamepad.all[i] : null;
                dropWait[i] = 0;
                inputs[i] = new ArcadeInput();
            }
            pause = null; controllerMissing = false; screen = ScreenState.Playing;
        }
        void OpenPause()
        {
            session.SetPaused(true);
            var musicOption = new ArcadePauseMenu.Option("Music: " + (music.MusicEnabled ? "On" : "Off"), ToggleMusic);
            pause = new ArcadePauseMenu(Resume, () => LeaveGame(ScreenState.MainMenu),
                () => LeaveGame(ScreenState.Arcade), new ArcadePauseMenu.Option("Restart Round", StartGame), musicOption);
        }
        void ToggleMusic()
        {
            music.Toggle();
            if (pause != null) pause.Options[pause.Options.Count-1].Label = "Music: " + (music.MusicEnabled ? "On" : "Off");
        }
        void Resume()
        {
            if (controllerMissing) return;
            pause = null; session.SetPaused(false);
        }
        void LeaveGame(ScreenState destination) { pause = null; session = null; screen = destination; }
        void OnApplicationFocus(bool focused)
        {
            if (!focused && screen == ScreenState.Playing && pause == null) OpenPause();
        }
        void Box(Rect rect, Color color) { GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = Color.white; }
        void Label(float x, float y, float width, string text, int size = 22, Color? color = null)
        {
            GUI.Label(new Rect(x,y,width,100), text, new GUIStyle(GUI.skin.label) {
                fontSize = size, wordWrap = true, normal = { textColor = color ?? Color.white } });
        }
        bool Button(Rect rect, string text, bool selected = false)
        {
            var previous = GUI.backgroundColor;
            GUI.backgroundColor = selected ? new Color(1,.72f,.25f) : new Color(.35f,.4f,.6f);
            bool clicked = GUI.Button(rect, text, new GUIStyle(GUI.skin.button) { fontSize = 23 });
            GUI.backgroundColor = previous;
            return clicked;
        }
        void DrawBoard(BlocksRules board, float left, string title)
        {
            Label(left,105,380,title,27);
            Label(left,140,380,"Score " + board.Score + "    Height " + board.StackHeight,19,Color.yellow);
            const int cell = 23;
            for (int y=0;y<BlocksRules.Height;y++) for(int x=0;x<BlocksRules.Width;x++)
                Box(new Rect(left+x*cell,185+y*cell,cell-2,cell-2), Colors[board.Board[x,y]]);
            if (!board.Over) for(int i=0;i<4;i++) {
                board.Cell(i,board.Rotation,out int x,out int y);
                Box(new Rect(left+(board.X+x)*cell,185+(board.Y+y)*cell,cell-2,cell-2), Colors[board.Kind+1]);
            }
            DrawNext(board, left + 244);
        }
        void DrawNext(BlocksRules board, float left)
        {
            Box(new Rect(left,185,96,125), new Color(.13f,.14f,.25f));
            Label(left+15,195,80,"NEXT",22,Color.yellow);
            int minX = 4, minY = 4, maxX = 0, maxY = 0;
            for (int i=0;i<4;i++) {
                board.NextCell(i,out int x,out int y);
                minX = Mathf.Min(minX,x); minY = Mathf.Min(minY,y);
                maxX = Mathf.Max(maxX,x); maxY = Mathf.Max(maxY,y);
            }
            const int cell = 20;
            float originX = left + (96-(maxX-minX+1)*cell)/2f;
            float originY = 235 + (60-(maxY-minY+1)*cell)/2f;
            for (int i=0;i<4;i++) {
                board.NextCell(i,out int x,out int y);
                Box(new Rect(originX+(x-minX)*cell,originY+(y-minY)*cell,cell-2,cell-2),Colors[board.NextKind+1]);
            }
        }
        void CenterLabel(Rect rect, string text, int size, Color color)
        {
            GUI.Label(rect,text,new GUIStyle(GUI.skin.label) {
                fontSize=size, alignment=TextAnchor.MiddleCenter, wordWrap=false,
                fontStyle=FontStyle.Bold, normal={textColor=color}
            });
        }
        void DrawPerson(float center, float top, float unit, Color color)
        {
            // Small block-built characters keep the mode icons in the game's visual language.
            Box(new Rect(center-3*unit,top,6*unit,5*unit),color);
            Box(new Rect(center-2*unit,top+2*unit,unit,unit),new Color(.05f,.07f,.15f));
            Box(new Rect(center+unit,top+2*unit,unit,unit),new Color(.05f,.07f,.15f));
            Box(new Rect(center-unit,top+4*unit,2*unit,unit),Color.white);
            Box(new Rect(center-3*unit,top+6*unit,6*unit,5*unit),color);
            Box(new Rect(center-5*unit,top+6*unit,2*unit,4*unit),color);
            Box(new Rect(center+3*unit,top+6*unit,2*unit,4*unit),color);
            Box(new Rect(center-3*unit,top+12*unit,2*unit,3*unit),color);
            Box(new Rect(center+unit,top+12*unit,2*unit,3*unit),color);
        }
        void DrawModeCards()
        {
            CenterLabel(new Rect(60,120,1160,85),"DAD'S BLOCKS",46,Color.white);
            Color[] accents = { new Color(.3f,.9f,.8f), new Color(.4f,.65f,1),
                new Color(1,.72f,.3f), new Color(1,.4f,.5f), new Color(.75f,.5f,1) };
            string[] titles = { "KID", "PARENT", "PARENT + KID", "PK", "WHOLE FAMILY" };
            for (int i=0;i<5;i++) {
                float x=60+i*236;
                bool selected=selectedMode==i;
                float y=selected ? 248 : 258;
                var card=new Rect(x,y,216,300);
                Box(new Rect(x+5,y+9,216,300),new Color(.015f,.02f,.065f));
                Box(card,selected ? accents[i] : new Color(.2f,.23f,.36f));
                Box(new Rect(x+3,y+3,210,294),selected ? new Color(.15f,.17f,.29f) : new Color(.09f,.105f,.2f));
                Box(new Rect(x+20,y+22,176,5),accents[i]);
                float center=x+108, top=y+68;
                if (i==0) DrawPerson(center,top+25,5,accents[i]);
                if (i==1) DrawPerson(center,top,7,accents[i]);
                if (i==2) {
                    DrawPerson(center-40,top+8,5.5f,new Color(.4f,.65f,1));
                    DrawPerson(center+38,top+38,3.8f,accents[0]);
                    CenterLabel(new Rect(center-14,top+1,28,35),"+",27,accents[i]);
                }
                if (i==3) {
                    DrawPerson(center-49,top+20,4.5f,new Color(.4f,.65f,1));
                    DrawPerson(center+49,top+20,4.5f,accents[i]);
                    CenterLabel(new Rect(center-22,top+40,44,35),"VS",19,Color.white);
                }
                if (i==4) {
                    DrawPerson(center-55,top+6,4.5f,new Color(.4f,.65f,1));
                    DrawPerson(center+55,top+6,4.5f,accents[i]);
                    DrawPerson(center,top+58,3.5f,accents[0]);
                }
                CenterLabel(new Rect(x+8,y+192,200,46),titles[i],i==2 || i==4 ? 22 : 29,Color.white);
                if (selected) CenterLabel(new Rect(x+25,y+248,166,34),"A  PLAY",20,accents[i]);
                if (card.Contains(Event.current.mousePosition)) Box(new Rect(x+3,y+3,210,294),new Color(1,1,1,.04f));
                if (GUI.Button(card,GUIContent.none,GUIStyle.none)) { selectedMode=i; StartGame(); }
            }
            CenterLabel(new Rect(60,622,1160,42),"SELECT  <  >       A  PLAY       B  BACK",20,new Color(.68f,.73f,.85f));
        }
        void OnGUI()
        {
            GUI.matrix = Matrix4x4.identity;
            Box(new Rect(0,0,Screen.width,Screen.height), new Color(.02f,.02f,.06f));
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280*scale)/2, (Screen.height - 720*scale)/2,0), Quaternion.identity, new Vector3(scale,scale,1));
            if (screen == ScreenState.Arcade && homeArt != null) {
                GUI.DrawTexture(new Rect(0,0,1280,720), homeArt, ScaleMode.StretchToFill);
                if (GUI.Button(new Rect(154,279,555,335), GUIContent.none, GUIStyle.none) ||
                    GUI.Button(new Rect(460,642,160,50), GUIContent.none, GUIStyle.none)) screen = ScreenState.MainMenu;
                return;
            }
            Box(new Rect(0,0,1280,720), new Color(.035f,.04f,.12f));
            Label(55,25,1100,"DADDY'S ARCADE",44,new Color(1,.76f,.25f));
            if (screen == ScreenState.Arcade) {
                if (Button(new Rect(100,250,500,150),"DAD'S BLOCKS  /  A to play",true)) screen = ScreenState.MainMenu;
                return;
            }
            if (screen == ScreenState.MainMenu) {
                DrawModeCards();
                return;
            }
            int count = session.Players.Length;
            for (int i=0;i<count;i++) {
                // Whole Family: Parent 1, Kid, Parent 2; player/controller IDs stay fixed.
                int column = count == 3 ? (i == 1 ? 2 : i == 2 ? 1 : 0) : i;
                float left = count == 3 ? 55+column*410 : count == 2 ? 75+i*360 : 325;
                var board = session.Players[i];
                DrawBoard(board,left,session.PlayerTitle(i) + (count>1 ? " / P"+(i+1) : ""));
                if (session.Mode == MatchMode.PK || session.Mode == MatchMode.WholeFamily) {
                    Label(left+244,335,96,"INCOMING\n"+board.PendingRows,18,board.PendingRows>0 ? Color.yellow : Color.white);
                    if (i<2) Label(left+244,420,96,"SENT\n"+session.SentRows[i],18);
                }
            }
            if (count < 3) {
                Label(780,140,440,ModeNames[(int)session.Mode],31);
                GUI.Label(new Rect(780,220,440,205),"D-pad / Stick: move\nA: rotate clockwise\nB: rotate counterclockwise\nHold Down: fall faster\nY: hard drop\nMenu: pause",new GUIStyle(GUI.skin.label) { fontSize=23 });
                if (session.Mode == MatchMode.ParentChild) {
                    Label(780,455,430,"TEAM HELP: " + session.HelpedRows + " rows removed",24,Color.yellow);
                    Label(780,505,420,"Clear 3 / 4 -> help Kid by 1 / 2 rows. Active only above 8 rows.",22);
                } else if (session.Mode == MatchMode.PK)
                    Label(780,465,420,"Clear 3 / 4 -> attack with 1 / 2 rows. Incoming rows rise after your piece locks.",22,Color.yellow);
            } else {
                Label(775,35,470,"WHOLE FAMILY | Helped " + session.HelpedRows + " rows",23,Color.yellow);
            }
            Label(70,665,1130,"A / B rotate    |    Down accelerates    |    Y lands    |    Menu pauses",22);
            if (pause != null) {
                Box(new Rect(0,0,1280,720),new Color(0,0,0,.78f));
                Box(new Rect(390,155,500,440),new Color(.14f,.12f,.25f));
                Label(425,175,440,"Paused",36);
                var current = pause;
                for (int i=0;i<current.Options.Count;i++)
                    if (Button(new Rect(425,245+i*58,430,48),current.Options[i].Label,current.Selected == i)) { current.Options[i].Invoke(); break; }
                Label(405,605,650,controllerMissing ? "Reconnect your controller to resume, or restart." : "Up / Down: choose   A: select   B / Menu: resume",20);
            } else if (Finished) {
                Box(new Rect(350,255,580,230),new Color(.14f,.12f,.25f));
                Label(390,275,510,session.Result,29);
                if (Button(new Rect(400,345,480,50),"A: Play again",true)) StartGame();
                Label(405,410,500,"Menu: Main Menu / Exit to Arcade",22);
            }
        }
    }
}
