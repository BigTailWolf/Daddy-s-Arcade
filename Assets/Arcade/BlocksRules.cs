using System;
using System.Collections.Generic;

namespace DaddysArcade
{
    public enum PlayMode { Kid, Daddy }
    // No engine references: time, actions and randomness enter from the host.
    public sealed class BlocksRules
    {
        public const int Width = 10, Height = 20;
        public readonly int[,] Board = new int[Width, Height];
        static readonly int[][] Shapes = {
            new[]{0,1,1,1,2,1,3,1}, new[]{1,0,2,0,1,1,2,1},
            new[]{1,0,0,1,1,1,2,1}, new[]{1,0,2,0,0,1,1,1},
            new[]{0,0,1,0,1,1,2,1}, new[]{0,0,0,1,1,1,2,1},
            new[]{2,0,0,1,1,1,2,1}
        };
        readonly Random random;
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Kind { get; private set; }
        public int NextKind { get; private set; }
        public int Rotation { get; private set; }
        public int Score { get; private set; }
        public bool Over { get; private set; }
        public bool Paused { get; set; }
        public const int GarbageColor = 8;
        readonly Queue<int> incomingRows = new Queue<int>();
        public int PendingRows => incomingRows.Count;
        public event Action<int> LinesCleared;
        public int StackHeight {
            get {
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++) if (Board[x,y] != 0) return Height - y;
                return 0;
            }
        }
        double gravity, grounded;
        public PlayMode Mode { get; }
        public double GravityInterval => Mode == PlayMode.Kid ? 1.4 : .7;
        public static int LineReward(int lines) => lines * (lines + 1) * 50;
        public BlocksRules(int seed, PlayMode mode = PlayMode.Daddy)
        {
            Mode = mode; random = new Random(seed);
            NextKind = random.Next(Shapes.Length);
            Spawn();
        }
        public void NextCell(int index, out int x, out int y)
        {
            x = Shapes[NextKind][index * 2]; y = Shapes[NextKind][index * 2 + 1];
        }
        public void Cell(int index, int rotation, out int x, out int y)
        {
            x = Shapes[Kind][index * 2]; y = Shapes[Kind][index * 2 + 1];
            if (Kind == 1) return;
            int extent = Kind == 0 ? 3 : 2;
            for (int n = 0; n < rotation; n++) { int old = x; x = extent - y; y = old; }
        }
        bool Fits(int px, int py, int rotation)
        {
            for (int i = 0; i < 4; i++) {
                Cell(i, rotation, out int x, out int y); x += px; y += py;
                if (x < 0 || x >= Width || y < 0 || y >= Height || Board[x,y] != 0) return false;
            }
            return true;
        }
        public bool Move(int dx, int dy)
        {
            if (Over || Paused || !Fits(X + dx, Y + dy, Rotation)) return false;
            X += dx; Y += dy; return true;
        }
        public void Rotate(int direction = 1)
        {
            int next = (Rotation + (direction < 0 ? 3 : 1)) % 4;
            if (!Over && !Paused && Fits(X, Y, next)) Rotation = next;
        }
        public void HardDrop()
        {
            if (Over || Paused) return;
            while (Move(0, 1)) { }
            Lock();
        }
        public int ReceiveParentHelp(int parentLines)
        {
            if (Over || Paused || StackHeight <= 8 || parentLines < 3 || parentLines > 4) return 0;
            int rows = parentLines - 2;
            for (int y = Height - 1; y >= rows; y--)
                for (int x = 0; x < Width; x++) Board[x,y] = Board[x,y-rows];
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < Width; x++) Board[x,y] = 0;
            // Carry the falling piece with the stack, clamping it above the floor.
            int target = Y + rows;
            while (target > 0 && !Fits(X, target, Rotation)) target--;
            Y = target;
            grounded = 0;
            return rows;
        }
        public int ReceiveAttack(int clearedLines, int gapColumn)
        {
            if (Over || Paused || clearedLines < 3 || clearedLines > 4) return 0;
            if (gapColumn < 0 || gapColumn >= Width) throw new ArgumentOutOfRangeException(nameof(gapColumn));
            int rows = clearedLines - 2;
            for (int i=0;i<rows;i++) incomingRows.Enqueue(gapColumn);
            return rows;
        }
        void ApplyIncomingRows()
        {
            // Apply attacks between pieces so a rising stack cannot overlap the falling piece.
            // Clearing your own lines never cancels an incoming attack.
            while (incomingRows.Count > 0) {
                int gap = incomingRows.Dequeue();
                for (int x=0;x<Width;x++) {
                    if (Board[x,0] != 0) { Over = true; incomingRows.Clear(); return; }
                }
                for (int y=0;y<Height-1;y++)
                    for (int x=0;x<Width;x++) Board[x,y] = Board[x,y+1];
                for (int x=0;x<Width;x++) Board[x,Height-1] = x == gap ? 0 : GarbageColor;
            }
        }
        public void Tick(double seconds)
        {
            if (Over || Paused) return;
            double delta = Math.Max(0, Math.Min(seconds, .25));
            if (Mode == PlayMode.Kid) {
                grounded = Fits(X, Y + 1, Rotation) ? 0 : grounded + delta;
                if (grounded >= .8) { Lock(); return; }
            }
            gravity += delta;
            if (gravity < GravityInterval) return;
            gravity -= GravityInterval; StepDown();
        }
        public void StepDown()
        {
            if (Over || Paused || Move(0, 1)) return;
            if (Mode == PlayMode.Kid) return;
            Lock();
        }
        void Lock()
        {
            for (int i = 0; i < 4; i++) {
                Cell(i, Rotation, out int x, out int y); Board[X+x,Y+y] = Kind + 1;
            }
            int lines = ClearLines();
            Score += LineReward(lines);
            if (lines > 0) LinesCleared?.Invoke(lines);
            ApplyIncomingRows();
            if (!Over) Spawn();
        }
        public int ClearLines()
        {
            int cleared = 0;
            for (int y = Height - 1; y >= 0; y--) {
                bool full = true;
                for (int x = 0; x < Width; x++) if (Board[x,y] == 0) full = false;
                if (!full) continue;
                for (int row = y; row > 0; row--)
                    for (int x = 0; x < Width; x++) Board[x,row] = Board[x,row-1];
                for (int x = 0; x < Width; x++) Board[x,0] = 0;
                cleared++; y++;
            }
            return cleared;
        }
        void Spawn()
        {
            Kind = NextKind; NextKind = random.Next(Shapes.Length);
            Rotation = 0; X = 3; Y = 0; gravity = 0; grounded = 0;
            Over = !Fits(X,Y,Rotation);
        }
    }
}
