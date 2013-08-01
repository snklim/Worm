﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Worm
{
    public partial class Form1 : Form
    {
        private enum Direction
        {
            None, Top, Right, Bottom, Left
        }

        private enum Speed
        {
            Low = 6, Normal = 4, High = 2
        }

        private enum GameStatus
        {
            Play, Pause, GameOver, YouWin
        }

        private enum FieldCellState
        {
            Empty, WormHead, WormPart, Prize, Wall
        }

        private class CellChange
        {
            public int PosX { get; set; }
            public int PosY { get; set; }
            public FieldCellState FieldCellStateChangedTo { get; set; }
        }

        private class Part
        {
            private int _id;
            public int PosX { get; set; }
            public int PosY { get; set; }
            public Part NextPart { get; set; }
            public Direction MoveDirection { get; set; }

            private Direction _changeDirection = Direction.None;

            private GameField _gameField;
            private Worm _worm;
            private bool _isHead;

            public Part(int id, bool isHead, int posX, int posY, Direction moveDirection, GameField gameField, Worm worm)
            {
                _id = id;
                _isHead = isHead;
                _worm = worm;

                PosX = posX;
                PosY = posY;
                MoveDirection = moveDirection;
                _gameField = gameField;

                _gameField.SetValueInCell(PosX, PosY, _isHead ? FieldCellState.WormHead : FieldCellState.WormPart);
            }

            public void AddNewPart()
            {
                if (NextPart != null)
                {
                    NextPart.AddNewPart();
                }
                else
                {
                    int x = 0;
                    int y = 0;
                    switch (MoveDirection)
                    {
                        case Direction.Top: x = PosX; y = PosY + 1; break;
                        case Direction.Right: x = PosX - 1; y = PosY; break;
                        case Direction.Bottom: x = PosX; y = PosY - 1; break;
                        case Direction.Left: x = PosX + 1; y = PosY; break;
                    }
                    NextPart = new Part(_id + 1, false, x, y, MoveDirection, _gameField, _worm);
                }
            }

            public void Move()
            {
                if (_gameField.WormGameStatus == GameStatus.GameOver) return;

                if (_changeDirection != Direction.None) MoveDirection = _changeDirection;

                int x = PosX;
                int y = PosY;

                switch (MoveDirection)
                {
                    case Direction.Top: y = PosY - 1; break;
                    case Direction.Right: x = PosX + 1; break;
                    case Direction.Bottom: y = PosY + 1; break;
                    case Direction.Left: x = PosX - 1; break;
                }

                if (_gameField.GetValueInCell(x, y) == FieldCellState.WormPart || _gameField.GetValueInCell(x, y) == FieldCellState.Wall)
                {
                    _gameField.WormGameStatus = GameStatus.GameOver;
                    return;
                }

                _gameField.SetValueInCell(PosX, PosY, FieldCellState.Empty);
                _gameField.SetCellChange(new CellChange { PosX = PosX, PosY = PosY, FieldCellStateChangedTo = FieldCellState.Empty });

                PosX = x; PosY = y;

                if (_isHead && _gameField.GetValueInCell(PosX, PosY) == FieldCellState.Prize)
                {
                    _gameField.NumOfPrize -= 1;
                    AddNewPart();
                    if (_gameField.NumOfPrize == 0)
                    {
                        _gameField.WormGameStatus = GameStatus.YouWin;
                    }
                }

                _gameField.SetValueInCell(PosX, PosY, _isHead ? FieldCellState.WormHead : FieldCellState.WormPart);
                _gameField.SetCellChange(new CellChange { PosX = PosX, PosY = PosY, FieldCellStateChangedTo = _isHead ? FieldCellState.WormHead : FieldCellState.WormPart });

                if (NextPart != null)
                {
                    NextPart.Move();
                    if (_changeDirection != Direction.None) NextPart.ChangeDirection(_changeDirection);
                    _changeDirection = Direction.None;
                }
                else if (_changeDirection != Direction.None)
                {
                    _changeDirection = Direction.None;
                }
            }

            public void ChangeDirection(Direction newDirection)
            {
                _changeDirection = newDirection;
            }
        }

        private class Worm
        {
            public Speed MoveSpeed { get; private set; }
            public Part Head { get; private set; }

            private GameField _gameField;

            public Worm(int headPosX, int headPosY, Direction moveDirection, Speed moveSpeed, int tailLong, GameField gameField)
            {
                _gameField = gameField;

                int partId = 1;
                MoveSpeed = moveSpeed;
                Head = new Part(partId++, true, headPosX, headPosY, moveDirection, gameField, this);
                Part part = Head;

                _gameField.SetCellChange(new CellChange { PosX = headPosX, PosY = headPosY, FieldCellStateChangedTo = FieldCellState.WormHead });

                while (tailLong > 0)
                {
                    headPosX--;
                    Part nextPart = new Part(partId++, false, headPosX, headPosY, moveDirection, gameField, this);
                    part.NextPart = nextPart;
                    part = nextPart;
                    tailLong--;

                    _gameField.SetCellChange(new CellChange { PosX = headPosX, PosY = headPosY, FieldCellStateChangedTo = FieldCellState.WormPart });
                }
            }

            private int _tickCount = 0;
            public void MoveWorm()
            {
                _tickCount++;
                if (_tickCount % (int)MoveSpeed == 0)
                    Head.Move();
            }

            public void ChangeDirection(Direction moveDirection)
            {
                Head.ChangeDirection(moveDirection);
            }
        }

        private class GameField
        {
            private Random m_Rnd;
            private Worm m_Worm;

            private int _width;
            private int _height;

            public int NumOfPrize { get; set; }

            private FieldCellState[,] _field;

            private List<CellChange> _cellChanges;
            private List<GameStatus> _gameStatusChanges = new List<GameStatus>();

            public int FieldWidth { get { return _width; } }
            public int FieldHeight { get { return _height; } }

            GameStatus _wormGameStatus;
            public GameStatus WormGameStatus { get { return _wormGameStatus; } set { _wormGameStatus = value; _gameStatusChanges.Add(value); } }

            public GameField(int width, int height)
            {
                m_Rnd = new Random();

                WormGameStatus = GameStatus.Play;

                _width = width;
                _height = height;

                _field = new FieldCellState[_width, _height];

                _cellChanges = new List<CellChange>();

                for (int i = 0; i < _width; i++)
                {
                    for (int j = 0; j < _height; j++)
                    {
                        _cellChanges.Add(new CellChange { PosX = i, PosY = j, FieldCellStateChangedTo = FieldCellState.Empty });
                    }
                }

                for (int i = 0; i < FieldWidth; i++)
                {
                    SetValueInCell(i, 0, FieldCellState.Wall);
                    SetValueInCell(i, FieldHeight - 1, FieldCellState.Wall);
                    _cellChanges.Add(new CellChange { PosX = i, PosY = 0, FieldCellStateChangedTo = FieldCellState.Wall });
                    _cellChanges.Add(new CellChange { PosX = i, PosY = FieldHeight - 1, FieldCellStateChangedTo = FieldCellState.Wall });
                }

                for (int j = 0; j < FieldHeight; j++)
                {
                    SetValueInCell(0, j, FieldCellState.Wall);
                    SetValueInCell(FieldWidth - 1, j, FieldCellState.Wall);
                    _cellChanges.Add(new CellChange { PosX = 0, PosY = j, FieldCellStateChangedTo = FieldCellState.Wall });
                    _cellChanges.Add(new CellChange { PosX = FieldWidth - 1, PosY = j, FieldCellStateChangedTo = FieldCellState.Wall });
                }

                m_Worm = new Worm(FieldWidth / 2, FieldHeight / 2, Direction.Right, Speed.Normal, 3, this);

                NumOfPrize = FieldWidth * FieldHeight / 100;

                int leftToCreatePrizes = NumOfPrize;

                while (leftToCreatePrizes > 0)
                {
                    int x = m_Rnd.Next(FieldWidth);
                    int y = m_Rnd.Next(FieldHeight);

                    if (GetValueInCell(x, y) == 0)
                    {
                        SetValueInCell(x, y, FieldCellState.Prize);
                        _cellChanges.Add(new CellChange { PosX = x, PosY = y, FieldCellStateChangedTo = FieldCellState.Prize });
                        leftToCreatePrizes--;
                    }
                }
            }

            public FieldCellState GetValueInCell(int x, int y)
            {
                return _field[x, y];
            }

            public void SetValueInCell(int x, int y, FieldCellState value)
            {
                _field[x, y] = value;
            }

            public void Tick()
            {
                if (WormGameStatus == GameStatus.Play)
                    m_Worm.MoveWorm();
            }

            public void ChangeDirection(Direction newDirection)
            {
                m_Worm.ChangeDirection(newDirection);
            }

            public void SetCellChange(CellChange change)
            {
                _cellChanges.Add(change);
            }

            public CellChange[] GetCellChenges()
            {
                CellChange[] changes = _cellChanges.ToArray();
                _cellChanges.Clear();
                return changes;
            }

            public GameStatus[] GetGameStatusChange()
            {
                GameStatus[] changes = _gameStatusChanges.ToArray();
                _gameStatusChanges.Clear();
                return changes;
            }
        }

        private GameField m_Field;
        private Timer m_Timer;

        int m_CellSize, m_DeltaX, m_DeltaY;

        private void InitGame()
        {            
            m_Field = new GameField(60, 30);
        }

        Bitmap m_MainSurface;

        private void DrawGameField()
        {
            CellChange[] chenges = m_Field.GetCellChenges();
            GameStatus[] gameStatusChanges = m_Field.GetGameStatusChange();

            if (chenges.Length == 0 && gameStatusChanges.Length == 0) return;

            int minX, minY, maxX, maxY;
            minX = minY = int.MaxValue;
            maxX = maxY = int.MinValue;

            int minPosX, minPosY, maxPosX, maxPosY;
            minPosX = minPosY = int.MaxValue;
            maxPosX = maxPosY = int.MinValue;
                        
            // draw game field
            foreach (CellChange ch in chenges)
            {
                minX = Math.Min(minX, ch.PosX);
                minY = Math.Min(minY, ch.PosY);
                maxX = Math.Max(maxX, ch.PosX + 1);
                maxY = Math.Max(maxY, ch.PosY + 1);

                minPosX = Math.Min(minPosX, m_DeltaX + minX * m_CellSize);
                minPosY = Math.Min(minPosY, m_DeltaY + minY * m_CellSize);
                maxPosX = Math.Max(maxPosX, m_DeltaX + maxX * m_CellSize);
                maxPosY = Math.Max(maxPosY, m_DeltaY + maxY * m_CellSize);
            }

            Graphics g = Graphics.FromImage(m_MainSurface);

            if (chenges.Length > 0)
            {
                Rectangle updateRect = new Rectangle(minPosX, minPosY, maxPosX - minPosX, maxPosY - minPosY);

                if (minX == 0 && minY == 0 && maxX == m_Field.FieldWidth && maxY == m_Field.FieldHeight)
                    updateRect = new Rectangle(0, 0, m_GameField.Width, m_GameField.Height);

                g.FillRectangle(new SolidBrush(Color.Gray), updateRect);

                for (int i = minX; i < maxX; i++)
                {
                    for (int j = minY; j < maxY; j++)
                    {
                        if (m_Field.GetValueInCell(i, j) == FieldCellState.Empty)
                            g.DrawRectangle(new Pen(Color.Silver), m_DeltaX + i * m_CellSize, m_DeltaY + j * m_CellSize, m_CellSize, m_CellSize);
                        else if (m_Field.GetValueInCell(i, j) == FieldCellState.WormHead)
                            g.FillEllipse(new SolidBrush(Color.Green), m_DeltaX + m_CellSize * i, m_DeltaY + m_CellSize * j, m_CellSize, m_CellSize);
                        else if (m_Field.GetValueInCell(i, j) == FieldCellState.WormPart)
                            g.FillEllipse(new SolidBrush(Color.Orange), m_DeltaX + m_CellSize * i, m_DeltaY + m_CellSize * j, m_CellSize, m_CellSize);
                        else if (m_Field.GetValueInCell(i, j) == FieldCellState.Prize)
                            g.FillRectangle(new SolidBrush(Color.Red), m_DeltaX + i * m_CellSize, m_DeltaY + j * m_CellSize, m_CellSize, m_CellSize);
                        else if (m_Field.GetValueInCell(i, j) == FieldCellState.Wall)
                            g.FillRectangle(new SolidBrush(Color.Black), m_DeltaX + i * m_CellSize, m_DeltaY + j * m_CellSize, m_CellSize, m_CellSize);
                    }
                }

                m_GameField.Invalidate(updateRect);
            }
            else if (gameStatusChanges.Length > 0)
            {
                Rectangle updateRect = new Rectangle(0, 0, m_GameField.Width, m_GameField.Height);

                foreach (GameStatus gCh in gameStatusChanges)
                {
                    if (gCh == GameStatus.GameOver || gCh == GameStatus.YouWin)
                    {
                        string msg = String.Empty;
                        switch (m_Field.WormGameStatus)
                        {
                            case GameStatus.GameOver: msg = "Game Over"; break;
                            case GameStatus.YouWin: msg = "You Win!!!"; break;
                        }

                        SizeF txtSize = g.MeasureString(msg, new Font(FontFamily.GenericSansSerif, 40));
                        float txtPosX, txtPosY;
                        txtPosX = (m_GameField.Width - txtSize.Width) / 2;
                        txtPosY = (m_GameField.Height - txtSize.Height) / 2;
                        g.DrawString(msg, new Font(FontFamily.GenericSansSerif, 40), new SolidBrush(Color.Black), txtPosX, txtPosY);

                        minX = Math.Min(minX, (int)Math.Floor(txtPosX));
                        minY = Math.Min(minY, (int)Math.Floor(txtPosY));
                        maxX = Math.Max(maxX, (int)Math.Ceiling(txtSize.Width + txtPosX));
                        maxY = Math.Max(maxY, (int)Math.Ceiling(txtSize.Height + txtPosY));

                        updateRect = new Rectangle(minX, minY, maxX - minX, maxY - minY);
                    }
                }

                m_GameField.Invalidate(updateRect);
            }
        }

        private void InitGameField()
        {
            int cWidth = m_GameField.Width;
            int cHeight = m_GameField.Height;

            m_CellSize = cHeight / m_Field.FieldHeight;
            m_CellSize = Math.Min(m_CellSize, cWidth / m_Field.FieldWidth);

            m_DeltaX = (cWidth - m_CellSize * m_Field.FieldWidth) / 2;
            m_DeltaY = (cHeight - m_CellSize * m_Field.FieldHeight) / 2;

            m_MainSurface = new Bitmap(m_GameField.Width, m_GameField.Height);
        }

        public Form1()
        {
            InitGame();
            InitializeComponent();

            InitGameField();
            DrawGameField();

            m_Timer = new Timer();
            m_Timer.Interval = 50;
            m_Timer.Tick += m_Timer_Tick;
            m_Timer.Start();
        }

        void m_Timer_Tick(object sender, EventArgs e)
        {
            m_Field.Tick();
            DrawGameField();
        }

        private void m_GameField_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(m_MainSurface, e.ClipRectangle, e.ClipRectangle, GraphicsUnit.Pixel);
        }

        private void m_GameField_Resize(object sender, EventArgs e)
        {
            InitGameField();
            m_Field.SetCellChange(new CellChange { PosX = 0, PosY = 0, FieldCellStateChangedTo = FieldCellState.Empty });
            m_Field.SetCellChange(new CellChange { PosX = m_Field.FieldWidth - 1, PosY = m_Field.FieldHeight - 1, FieldCellStateChangedTo = FieldCellState.Empty });
            DrawGameField();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyValue)
            {
                case 37: m_Field.ChangeDirection(Direction.Left); break;
                case 38: m_Field.ChangeDirection(Direction.Top); break;
                case 39: m_Field.ChangeDirection(Direction.Right); break;
                case 40: m_Field.ChangeDirection(Direction.Bottom); break;
                // pause game. key "P" has 80 code
                case 80: m_Field.WormGameStatus = (m_Field.WormGameStatus == GameStatus.GameOver
                        ? GameStatus.GameOver
                            : (m_Field.WormGameStatus == GameStatus.Play ? GameStatus.Pause : GameStatus.Play));
                    break;
                // new game. kay "N" has 78 code
                case 78: InitGame(); break;
            }
        }
    }
}
