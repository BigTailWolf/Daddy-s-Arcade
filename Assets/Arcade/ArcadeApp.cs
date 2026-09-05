using UnityEngine;

namespace DaddysArcade
{
    public sealed class ArcadeApp : MonoBehaviour
    {
        enum Action { Confirm, Back, Left, Right, Down, Rotate, Pause, Restart }
        BlocksRules game;
        bool leaving;
        static readonly Color[] Colors = { new Color(.12f,.14f,.24f), Color.cyan,
            Color.yellow, new Color(.7f,.4f,1), Color.green, Color.red, Color.blue,
            new Color(1,.55f,.2f) };
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot() { new GameObject("Daddy's Arcade").AddComponent<ArcadeApp>(); }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0)) Dispatch(Action.Confirm);
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1)) Dispatch(Action.Back);
            if (Input.GetKeyDown(KeyCode.LeftArrow)) Dispatch(Action.Left);
            if (Input.GetKeyDown(KeyCode.RightArrow)) Dispatch(Action.Right);
            if (Input.GetKeyDown(KeyCode.DownArrow)) Dispatch(Action.Down);
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space)) Dispatch(Action.Rotate);
            if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.JoystickButton7)) Dispatch(Action.Pause);
            if (Input.GetKeyDown(KeyCode.R)) Dispatch(Action.Restart);
            game?.Tick(Time.unscaledDeltaTime);
        }
        void Dispatch(Action action)
        {
            if (game == null) { if (action == Action.Confirm) StartGame(); return; }
            if (action == Action.Back) { leaving = !leaving; game.Paused = leaving; return; }
            if (leaving) { if (action == Action.Confirm) { game = null; leaving = false; } return; }
            switch (action) {
                case Action.Left: game.Move(-1,0); break;
                case Action.Right: game.Move(1,0); break;
                case Action.Down: game.StepDown(); break;
                case Action.Rotate: game.Rotate(); break;
                case Action.Confirm: if (game.Over) StartGame(); else game.Rotate(); break;
                case Action.Pause: game.Paused = !game.Paused; break;
                case Action.Restart: StartGame(); break;
            }
        }
        void StartGame() { game = new BlocksRules(System.Environment.TickCount); leaving = false; }
        void Box(Rect rect, Color color) { GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = Color.white; }
        void Label(Rect rect, string text, int size, Color color)
        {
            GUI.Label(rect, text, new GUIStyle(GUI.skin.label) { fontSize = size, normal = { textColor = color } });
        }
        void OnGUI()
        {
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1280f, Screen.height / 720f, 1));
            Box(new Rect(0,0,1280,720), new Color(.035f,.04f,.12f));
            Label(new Rect(70,35,1100,75), "DADDY'S ARCADE", 48, new Color(1,.76f,.25f));
            if (game == null) {
                Label(new Rect(75,125,1000,45), "Pick a game. Make a memory.", 25, Color.white);
                Box(new Rect(70,215,620,335), new Color(.8f,.5f,.2f));
                Box(new Rect(75,220,610,325), new Color(.15f,.12f,.3f));
                Label(new Rect(105,250,560,70), "DAD'S BLOCKS", 44, Color.white);
                for (int i=0;i<10;i++) Box(new Rect(110+i*50,425-(i%3)*35,44,44), Colors[i%7+1]);
                if (GUI.Button(new Rect(105,490,220,40), "PLAY  /  ENTER")) StartGame();
                Label(new Rect(75,625,1100,40), "Enter / A: Play      •      Keyboard ready", 22, Color.white);
                return;
            }
            const int cell = 25;
            for (int y=0;y<BlocksRules.Height;y++) for(int x=0;x<BlocksRules.Width;x++)
                Box(new Rect(430+x*cell,145+y*cell,cell-2,cell-2), Colors[game.Board[x,y]]);
            if (!game.Over) for(int i=0;i<4;i++) {
                game.Cell(i,game.Rotation,out int x,out int y);
                Box(new Rect(430+(game.X+x)*cell,145+(game.Y+y)*cell,cell-2,cell-2), Colors[game.Kind+1]);
            }
            Label(new Rect(760,170,400,60), "DAD'S BLOCKS", 30, Color.white);
            Label(new Rect(760,250,400,50), "Score  " + game.Score, 28, Color.yellow);
            Label(new Rect(760,325,460,200), "← →  Move\n↑ / Space  Rotate\n↓  Drop one step\nP  Pause   •   R  Restart\nEsc  Return Home", 21, Color.white);
            if (leaving || game.Over || game.Paused) {
                Box(new Rect(370,300,390,145), new Color(.2f,.12f,.35f));
                Label(new Rect(395,315,350,60), leaving ? "Return Home?" : game.Over ? "Game Over" : "Paused", 32, Color.white);
                Label(new Rect(395,380,350,55), leaving ? "Enter: Yes   Esc: Cancel" : game.Over ? "Enter to play again" : "P to resume", 20, Color.yellow);
            }
        }
    }
}
