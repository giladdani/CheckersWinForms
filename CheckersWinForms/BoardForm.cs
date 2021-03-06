﻿using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CheckersLogic;

namespace CheckersWinForms
{
    public partial class BoardForm : Form
    {
        // Private Members
        private const string k_TopPlayerSign = "O";
        private const string k_BottomPlayerSign = "X";
        private const string k_TopPlayerKingSign = "Q";
        private const string k_BottomPlayerKingSign = "K";
        private Game m_Game;
        private SquareButton[,] m_SquareButtons;
        private StringBuilder m_CurrentMove;
        private SquareButton m_SelectedSquare;

        // Constructors
        public BoardForm(Game i_Game)
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedDialog; // disable window resizing
            m_Game = i_Game;
            initBoardSettings(m_Game.Board);
            ResetBoardSettings(m_Game.Board);
            UpdatePlayersNamesAndScores();
            m_CurrentMove = new StringBuilder();
        }

        // Private Methods
        // Setup first time board settings
        private void initBoardSettings(Board i_Board)
        {
            m_SquareButtons = new SquareButton[i_Board.Size, i_Board.Size];
            for (int row = 0; row < i_Board.Size; row++)
            {
                for (int col = 0; col < i_Board.Size; col++)
                {
                    m_SquareButtons[row, col] = new SquareButton(row, col) { Width = 60, Height = 60, Left = col * 60, Top = (row + 1) * 60, TabStop = false, Font = new Font("Arial", 24, FontStyle.Bold) };
                    if ((row % 2 == 0 && col % 2 != 0) || (row % 2 != 0 && col % 2 == 0))
                    {
                        m_SquareButtons[row, col].SquareClicked += squareClicked;
                        m_SquareButtons[row, col].BackColor = Color.BurlyWood;
                        m_Game.Board.GameBoard[row, col].CurrentPieceChanged += updateSquareSymbol;
                    }
                    else
                    {
                        m_SquareButtons[row, col].BackColor = Color.SaddleBrown;
                        m_SquareButtons[row, col].Enabled = false;
                    }

                    this.Controls.Add(m_SquareButtons[row, col]);
                }
            }
        }

        // Public Methods
        // Reset basic board settings needed for a new round
        public void ResetBoardSettings(Board i_Board)
        {
            for (int row = 0; row < i_Board.Size; row++)
            {
                for (int col = 0; col < i_Board.Size; col++)
                {
                    if ((row % 2 == 0 && col % 2 != 0) || (row % 2 != 0 && col % 2 == 0))
                    {
                        m_Game.Board.GameBoard[row, col].CurrentPieceChanged += updateSquareSymbol;
                        updateSquareSymbol(m_Game.Board.GameBoard[row, col]);
                    }
                }
            }

            HighlightCurrentPlayerLabel();
        }

        // Refreshes players labels with updated scores
        public void UpdatePlayersNamesAndScores()
        {
            this.LabelPlayerOneName.Text = string.Format(
                "{0}({1}): {2}",
                m_Game.PlayerOne.Name,
                k_TopPlayerSign,
                m_Game.PlayerOne.TotalScore);
            this.LabelPlayerTwoName.Text = string.Format(
                "{0}({1}): {2}",
                m_Game.PlayerTwo.Name,
                k_BottomPlayerSign,
                m_Game.PlayerTwo.TotalScore);
        }

        // Changes players label color according to current turn
        public void HighlightCurrentPlayerLabel()
        {
            LabelPlayerOneName.ForeColor = m_Game.CurrentPlayer == m_Game.PlayerOne ? Color.Red : Color.Black;
            LabelPlayerTwoName.ForeColor = m_Game.CurrentPlayer == m_Game.PlayerTwo ? Color.Red : Color.Black;
        }

        // Event Handlers
        protected override void OnSizeChanged(EventArgs e)
        {
            this.LabelPlayerTwoName.Left = this.Width - 175;
        }

        // Shows starting player message
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            string startingPlayerSign = m_Game.CurrentPlayer.Side == ePlayerSide.Up ? k_TopPlayerSign : k_BottomPlayerSign;
            MessageBox.Show(string.Format("{0}({1}) goes first", m_Game.CurrentPlayer.Name, startingPlayerSign));
            HighlightCurrentPlayerLabel();
        }

        // Starts building a move according to chosen square
        private void squareClicked(SquareButton i_SquareButton)
        {
            string currentSquareLocationString;
            Square square = m_Game.Board.GameBoard[i_SquareButton.RowIndex, i_SquareButton.ColIndex];
            Piece pieceAtSquare = square.CurrentPiece;

            // First click- build the move's starting position
            if (m_CurrentMove.ToString() == string.Empty && pieceAtSquare != null)
            {
                if (pieceAtSquare.Side == m_Game.CurrentPlayer.Side)
                {
                    i_SquareButton.BackColor = Color.Cyan;
                    currentSquareLocationString = MoveValidator.ConvertLocationToString(
                        i_SquareButton.RowIndex,
                        i_SquareButton.ColIndex);
                    m_CurrentMove.Append(currentSquareLocationString);
                    m_SelectedSquare = i_SquareButton;
                }
            }
            // Second click- add the move's ending position and execute it
            else if (m_CurrentMove.ToString() != string.Empty && pieceAtSquare == null)
            {
                m_SelectedSquare.BackColor = Color.BurlyWood;
                currentSquareLocationString = MoveValidator.ConvertLocationToString(
                    i_SquareButton.RowIndex,
                    i_SquareButton.ColIndex);
                m_CurrentMove.Append(">");
                m_CurrentMove.Append(currentSquareLocationString);
                Move selectedMove = MoveValidator.ConvertStringToMove(m_CurrentMove.ToString());
                m_Game.ExecuteMove(selectedMove);
                m_CurrentMove.Clear();
            }
            // Second click on the same square- cancel the chosen square (reset the move)
            else if (pieceAtSquare != null && pieceAtSquare == m_Game.Board.GameBoard[m_SelectedSquare.RowIndex, m_SelectedSquare.ColIndex].CurrentPiece)
            {
                m_SelectedSquare.BackColor = Color.BurlyWood;
                m_SelectedSquare = null;
                m_CurrentMove.Clear();
            }
        }

        // Change square symbol according to the piece it holds
        private void updateSquareSymbol(Square i_SquareToUpdate)
        {
            int row = i_SquareToUpdate.RowIndex;
            int col = i_SquareToUpdate.ColIndex;
            Piece pieceAtSquare = m_Game.Board.GameBoard[row, col].CurrentPiece;
            if (pieceAtSquare != null)
            {
                if (pieceAtSquare.IsKing)
                {
                    m_SquareButtons[row, col].Text =
                        pieceAtSquare.Side == ePlayerSide.Up ? k_TopPlayerKingSign : k_BottomPlayerKingSign;
                }
                else
                {
                    m_SquareButtons[row, col].Text =
                        pieceAtSquare.Side == ePlayerSide.Up ? k_TopPlayerSign : k_BottomPlayerSign;
                }
            }
            else
            {
                m_SquareButtons[row, col].Text = string.Empty;
            }
        }
    }
}
