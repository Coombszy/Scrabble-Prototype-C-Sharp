using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace Scrabble_Prototype
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = ("Scrabble Prototype");
            Console.SetWindowSize(Console.WindowWidth - 2, Console.WindowHeight - 4);
            Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
            //Console.TreatControlCAsInput = true;
            GameHandler Game = new GameHandler();
        }
    }
    class Globals//Defines and creates Accessors to Global Variables
    {
        private static string _UsersWord = "ZZZ";
        public static string UsersWord
        {
            get { return _UsersWord; }
            set { _UsersWord = value; }
        }
        private static int _UserX = 0;
        public static int UserX
        {
            get { return _UserX; }
            set { _UserX = value; }
        }
        private static int _UserY = 0;
        public static int UserY
        {
            get { return _UserY; }
            set { _UserY = value; }
        }
        private static bool _DState = false;
        public static bool DState
        {
            get { return _DState; }
            set { _DState = value; }
        }
    }
    class Words
    {
        public static void MakeDict()
        {
            GameEngine.wordList = new Dictionary<string, int>();//Defines Word list as a Dictionary
            var fdata = File.ReadAllLines("Data/WordList.txt");//Loads all data from the WordList.txt
            for (int i = 0; i < fdata.Length; i++)//For each line in the TXT (From above) it stores it into the next line of the Dictionary
            {
                GameEngine.wordList.Add(fdata[i], i);
            }
        }
        public static bool WordCheck(string word)
        {
            bool Return = true;
            try { Return = GameEngine.wordList.ContainsKey(word); } catch {};
            return (Return);// returns 1 if the word is in the wordlist, 0 if it is not
        }
    }//Handles IO and Word Validity Checking
    class GameEngine
    {
        //-------------------------------------
        public const int ROW = 15;//Boards X Coordinate
        public const int COL = 15;//Boards Y Coordinate
        public static string[,] Board = new string[ROW, COL];//2D array to represent the Game Board
        public static List<string> AIHand = new List<string>();//AI's Hand
        public static List<string> Player1Hand = new List<string>();//Player 1 Hand
        public static List<string> Player2Hand = new List<string>();//Player 2 Hand
        public static List<string> LetterPile = new List<string>();//Collection of all the Letters in the game
        public static Dictionary<string, int> wordList;//Defines the Public WordList --> As a Dictionary
        public static Dictionary<char, int> LetterScores = new Dictionary<char, int>();//Creates the Table of scores for each letter
        //Variables for Wordchecking
        //-Variables Specifically for Vertical Word Placing
        public static List<string> P_CharsYB = new List<string>();//Used to store letters that are already on the board on the Y Axis Before the users Target
        public static string P_StringYB;//This the Words/Characters already placed on the Y coordinate before the users target
        public static List<string> P_CharsYA = new List<string>();//Used to store letters that are already on the board on the Y Axis After the users Target
        public static string P_StringYA;//This the Words/Characters already placed on the Y coordinate After the users target

        //-Variables Specifically for Horizontal Word Placing
        public static List<string> P_CharsXB = new List<string>();//Used to store letters that are already on the board on the Y Axis Before the users Target
        public static string P_StringXB;//This the Words/Characters already placed on the Y coordinate before the users target
        public static List<string> P_CharsXA = new List<string>();//Used to store letters that are already on the board on the Y Axis After the users Target
        public static string P_StringXA;//This the Words/Characters already placed on the Y coordinate After the users target

        public static List<string> P_Overlap = new List<string>();//this is used to store values to check for overlap
        public static int Overlapflags = 99;

        public static int ConnectYFlags = 99;
        public static int ConnectXFlags = 99;

        //Move Checking------------
        private bool pass_ = true;
        //Lobby Data
        public static string Player1N = "";//Player 1 Name
        public static string Player2N = "";//Player 2 Name

        //-------------------------------------

        public GameEngine()
        {
            Words.MakeDict();//Loads the Txt into the WordList
            NewGame();
        }
        public void NewGame()
        {
            for (int i = 0; i < ROW; i++)//------------->> This fills the board array with Blank Spaces
            {
                for (int k = 0; k < COL; k++)
                {
                    Board[i, k] = "-";
                }
            }//-------------------------------------

            CreatePile();
            ShufflePile();

            //This section is for filling the board with specific data for testing Purposes
            ///*
            ///
            if (Globals.DState == true)
            {
                Board[1, 0] = "Z";
                Board[1, 1] = "Y";
                Board[1, 2] = "X";

                Board[6, 0] = "X";
                Board[6, 1] = "Y";
                Board[6, 2] = "Z";
                Board[5, 1] = "J";
                Board[5, 3] = "R";

                Board[7, 4] = "A";

                Board[4, 9] = "A";
                Board[5, 9] = "R";
                Board[6, 9] = "E";

                Board[8, 13] = "A";
                Board[9, 11] = "A";
            }
            ///-----------------------------------------------------------------------------

            //Places a starting letter in the center of the board
            Board[7, 7] = ProduceTile();

            //-----------------------------------------------------------------------------
            //Creates a score value for each letter of the Alphabet (Based of the Rules of Scrabble)
            LetterScores[' '] = 0;
            LetterScores['A'] = 1;
            LetterScores['B'] = 3;
            LetterScores['C'] = 3;
            LetterScores['D'] = 2;
            LetterScores['E'] = 1;
            LetterScores['F'] = 4;
            LetterScores['G'] = 2;
            LetterScores['H'] = 4;
            LetterScores['I'] = 1;
            LetterScores['J'] = 8;
            LetterScores['K'] = 5;
            LetterScores['L'] = 1;
            LetterScores['M'] = 3;
            LetterScores['N'] = 1;
            LetterScores['O'] = 1;
            LetterScores['P'] = 3;
            LetterScores['Q'] = 10;
            LetterScores['R'] = 1;
            LetterScores['S'] = 1;
            LetterScores['T'] = 1;
            LetterScores['U'] = 1;
            LetterScores['V'] = 4;
            LetterScores['W'] = 4;
            LetterScores['X'] = 8;
            LetterScores['Y'] = 4;
            LetterScores['Z'] = 10;

            //--------------------------------------------------------------------------------------


        }
        public void ShufflePile()
        {
            Random ran = new Random();
            string tempVal;
            for (int i = 0; i < 99999; i++)//randomises the pile by picking to values and swapping there locations, does this 99999 times
            {
                int val1 = ran.Next(98);
                int val2 = ran.Next(98);
                tempVal = LetterPile[val1];
                LetterPile[val1] = LetterPile[val2];
                LetterPile[val2] = tempVal;
            }
        }
        public void CreatePile()
        {
            //Adds all letters involved in the game (Based of rule book), For loops make editing easier to change letter quantities
            LetterPile.Add("K");
            LetterPile.Add("J");
            LetterPile.Add("X");
            LetterPile.Add("Q");
            LetterPile.Add("Z");

            for (int i = 0; i < 2; i++)//Letters with quantity of 2
            {

                //LetterPile.Add(" ");//Spaces removed at the moment, do not work with current work checks. Remember to increase ShuffleBoard int Val1 and Val2 by 2
                LetterPile.Add("B");
                LetterPile.Add("C");
                LetterPile.Add("M");
                LetterPile.Add("P");

                LetterPile.Add("F");
                LetterPile.Add("H");
                LetterPile.Add("V");
                LetterPile.Add("W");
                LetterPile.Add("Y");
            }

            for (int i = 0; i < 3; i++)//Letters with quantity of 3
            {
                LetterPile.Add("G");
            }

            for (int i = 0; i < 4; i++)//Letters with quantity of 4
            {
                LetterPile.Add("L");
                LetterPile.Add("S");
                LetterPile.Add("U");
                LetterPile.Add("D");
            }

            for (int i = 0; i < 6; i++)//Letters with quantity of 5
            {

                LetterPile.Add("N");
                LetterPile.Add("R");
                LetterPile.Add("T");
            }
            for (int i = 0; i < 8; i++)//Letters with quantity of 8
            {
                LetterPile.Add("O");
            }
            for (int i = 0; i < 9; i++)//Letters with quantity of 9
            {
                LetterPile.Add("A");
                LetterPile.Add("I");
            }

            for (int i = 0; i < 12; i++)//Letters with quantity of 12
            {
                LetterPile.Add("E");

            }

        }
        public void ListPile()
        {
            LetterPile.ForEach(i => Console.WriteLine("{0}", i));//Lists all letters from the pile
        }
        public static string DrawLetter()
        {
            if (LetterPile.Count <= 0)
            {
                return "STOPGAME";
            }
            string temp = LetterPile[LetterPile.Count - 1];// Stores the last letter in the list into Temp
            LetterPile.RemoveAt(LetterPile.Count - 1);//Removes the last letter from the List
            return temp;
        }//--FINISH ME--FINISH ME--FINISH ME--FINISH ME--FINISH ME--FINISH ME--FINISH ME--FINISH ME--FINISH ME--FINISH ME--FINISH ME--FINISH ME--FINISH ME--FINISH ME--FINISH ME--
        public static string ProduceTile()
        {
            string temp = LetterPile[LetterPile.Count - 1];// Stores the last letter in the list into Temp
            return temp;
        }
        public void SetupAIHand()
        {
            for (int i = 0; i < 7; i++)//Adds 7 Letters to the AI's Hand
            {
                AIHand.Add(DrawLetter());
            }
        }
        public void SetupPlayer1Hand()
        {
            for (int i = 0; i < 7; i++)//Adds 7 Letters to the AI's Hand
            {
                Player1Hand.Add(DrawLetter());
            }
        }
        public void SetupPlayer2Hand()
        {
            for (int i = 0; i < 7; i++)//Adds 7 Letters to the AI's Hand
            {
                Player2Hand.Add(DrawLetter());
            }
        }
        public void ListAIHand()
        {
            AIHand.ForEach(i => Console.WriteLine("{0}", i));//Lists all letters from the AI's hand
        }
        public static void ShowBoard()
        {
            Console.WriteLine("    |-A-|-B-|-C-|-D-|-E-|-F-|-G-|-H-|-I-|-J-|-K-|-L-|-M-|-N-|-O-|");//Displays the header above the board
            for (int j = 0; j < COL; j++)//This is to list each letter in the rows 15 times for each column
            {
                if (j < 9)
                {
                    Console.Write("- {0}-| ", (j + 1));
                }//these are to make sure the spacing is correct so the board is neat
                else
                {
                    Console.Write("-{0}-| ", (j + 1));
                }//^^^^^^^^^^^^^^^^
                for (int i = 0; i < ROW; i++)//this Lists the value on the array board as I increases
                {
                    Console.Write(Board[i, j]);
                    if (i < ROW)
                    {
                        Console.Write(" | ");//this is to seperate each letter for neatness
                    }
                }
                Console.WriteLine();
            }
        }
        public void D_GetWordAndTarget()
        {
            TidyUp();
            WSB();
            Console.Write("Your Word -"); Globals.UsersWord = Console.ReadLine();//Gets the users word
            Globals.UsersWord = Globals.UsersWord.ToUpper();
            if (!(Words.WordCheck(Globals.UsersWord)))
            {
                WSB();
                Console.Write("Not a real word, Press enter to try again");
                Console.ReadLine();
                D_GetWordAndTarget();
            }
            WSB();
            Console.Write("Coloumn Postion (A-O)-"); string UserXCoordS = Console.ReadLine().ToUpper();
            ConvertXCoordString(UserXCoordS);//Gets the users X coordinate after converting the Letter into the Int Value
            WSB();
            Console.Write("Row Postion (1-15)-"); Globals.UserY = int.Parse(Console.ReadLine()) - 1;//Gets the Y Value
            WSB();
            Console.Write("Horizontal or Vertical (enter H/V)-"); string HorV = Console.ReadLine().ToUpper();
            WSB();
            Char direction = 'X';
            switch (HorV)
            {
                case "V":
                    direction = 'V';
                    break;
                case "H":
                    direction = 'H';
                    break;
                default:
                    Console.WriteLine("INCORRECT ENTRY");
                    D_stop();
                    break;
            }
            if (LengthCheck(direction))
            {
                switch (HorV)
                {
                    case "V":
                        P_Down();
                        break;
                    case "H":
                        P_Across();
                        break;
                }
            }
        }
        private bool LengthCheck(char direction)
        {
            int WordLength = Globals.UsersWord.Length;
            switch (direction)
            {
                case 'V':
                    if (WordLength + Globals.UserY - 1 > 14)
                    {
                        WSB();
                        Console.Write("Word would go off of board edge, Press enter to try again ");
                        Console.Read();
                        return false;
                    }
                    else { return true; }
                case 'H':
                    if (WordLength + Globals.UserX - 1 > 14)
                    {
                        WSB();
                        Console.Write("Word would go off of board edge, Press enter to try again ");
                        Console.Read();
                        return false;
                    }
                    else { return true; }
                default:
                    Console.WriteLine("LengthCheck ERROR");
                    D_stop();
                    return true;
            }
        }// This checks if the users words is too long and would run of the side of the board
        public void WSB()//Clears the entire Console and then shows the board
        {
            Console.Clear();
            ShowBoard();
            Console.WriteLine(); Console.WriteLine();//Makes Space between the board and user inputs
        }
        private void D_stop()// Debug tool to pause the Program
        {
            Console.WriteLine("D_stop.....");
            Console.ReadLine();
        }
        private void D_stop(string Label)
        {
            Console.WriteLine("D_stop..... "+Label);
            Console.ReadLine();
        }
        public void ConvertXCoordString(string Letter)
        {
            //This Converts Letter value into a coordinate
            switch (Letter)
            {
                case "A":
                    Globals.UserX = 0;
                    break;
                case "B":
                    Globals.UserX = 1;
                    break;
                case "C":
                    Globals.UserX = 2;
                    break;
                case "D":
                    Globals.UserX = 3;
                    break;
                case "E":
                    Globals.UserX = 4;
                    break;
                case "F":
                    Globals.UserX = 5;
                    break;
                case "G":
                    Globals.UserX = 6;
                    break;
                case "H":
                    Globals.UserX = 7;
                    break;
                case "I":
                    Globals.UserX = 8;
                    break;
                case "J":
                    Globals.UserX = 9;
                    break;
                case "K":
                    Globals.UserX = 10;
                    break;
                case "L":
                    Globals.UserX = 11;
                    break;
                case "M":
                    Globals.UserX = 12;
                    break;
                case "N":
                    Globals.UserX = 13;
                    break;
                case "O":
                    Globals.UserX = 14;
                    break;
            }
        }
        public string BeforeX()
        {
            int XDifference = Globals.UserX;
            for (int i = 0; i < XDifference; i++)//----------------------------------------
            {
                P_CharsXB.Add(Board[i, Globals.UserY]);
            }
            try { P_StringXB = P_CharsXB.Aggregate((a, b) => a + b); } catch { P_StringXB = ""; }
            return P_StringXB.Replace("-", "");
        }
        public string AfterX()
        {
            int wordLength = Globals.UsersWord.Length;
            int P_Start = (Globals.UserX) + wordLength;
            for (int i = P_Start; i < 15; i++)
            {
                P_CharsXA.Add(Board[i, Globals.UserY]);
            }
            try { P_StringXA = P_CharsXA.Aggregate((a, b) => a + b); } catch { P_StringXA = ""; }
            P_CharsXA.Clear();
            return P_StringXA.Replace("-", "");
        }
        public bool P_Across()
        {
            pass_ = true;
            int wordLength = Globals.UsersWord.Length;
            //Calculates the contents of tiles Before Target location
            P_StringXB = BeforeX() + Globals.UsersWord;
            //------------------------------------------------------
            //Calculates the contents of tiles After the target location
            P_StringXA = Globals.UsersWord + AfterX();
            //------------------------------------------------------
            //Overlap checking
            bool Val_OverlapCheck = OverlapCheckX(Globals.UsersWord);
            //------------------------------------------------------
            //IsConnectected Checking
            bool Val_IsConnected = IsConnectedX(Globals.UsersWord);
            //------------------------------------------------------
            //PerpendicularCheck 
            bool Val_PerpCheck = PerpCheckX(Globals.UsersWord);
            //------------------------------------------------------
            //Letter after and before  ~~~~  +Edge check (Out of array Error Check)
            string LetterAfter = "";
            try { LetterAfter = Board[((Globals.UserX) + wordLength) + 1, Globals.UserY]; }
            catch { LetterAfter = "-"; }
            string LetterBefore = "";
            try { LetterBefore = Board[Globals.UserX - 1, Globals.UserY]; }
            catch { LetterBefore = "-"; }
            //------------------------------------------------------
            if (Globals.UserX == 0 && LetterAfter == "-" && (Val_OverlapCheck) && (Val_IsConnected) && (Val_PerpCheck))
            {
                if (Words.WordCheck(Globals.UsersWord))
                {
                    for (int i = 0; i < wordLength; i++)
                    {
                        Board[i, Globals.UserY] = CtoString(Globals.UsersWord[i]);
                    }
                }
                //Console.WriteLine("Debug1"); D_stop();
            }//Along X Coordinate - Letter after = "-" - OverlapCheck - PerpCheck
            else if (Words.WordCheck(P_StringXB) && LetterAfter == "-" && (Val_OverlapCheck) && (Val_IsConnected) && (Val_PerpCheck))
            {
                if (Words.WordCheck(P_StringXB))
                {
                    for (int i = 0; i < wordLength; i++)
                    {
                        Board[Globals.UserX + i, Globals.UserY] = CtoString(Globals.UsersWord[i]);
                    }
                }
                //Console.WriteLine("Debug2"); D_stop();
            }//WordCheck(WordBefore+UserWord) - Letter after = "-" - OverlapCheck - PerpCheck
            else if (Words.WordCheck(P_StringXA) && LetterBefore == "-" && (Val_OverlapCheck) && (Val_IsConnected) && (Val_PerpCheck))//WordCheck(UserWord+WordAfter) - Letter after = "-" - OverlapCheck
            {
                if (Words.WordCheck(P_StringXA))
                {
                    for (int i = 0; i < wordLength; i++)
                    {
                        Board[Globals.UserX + i, Globals.UserY] = CtoString(Globals.UsersWord[i]);
                    }
                }
                //Console.WriteLine("Debug3"); D_stop();
            }//WordCheck(UserWord+WordAfter) - Letter after = "-" - OverlapCheck - PerpCheck
            else
            {
                WordCheckDebug(LetterAfter, LetterBefore, Val_OverlapCheck, Val_IsConnected, Val_PerpCheck);
                Console.WriteLine("Final else, PlaceFail-Down");
                D_stop();
                pass_ = false;
            }
            if (Globals.DState == true) { WordCheckDebug(LetterAfter, LetterBefore, Val_OverlapCheck, Val_IsConnected, Val_PerpCheck); }
            TidyUp();
            return pass_;
        }
        public string BeforeY()
        {
            int YDifference = Globals.UserY;
            for (int i = 0; i < YDifference; i++)//----------------------------------------
            {
                P_CharsYB.Add(Board[Globals.UserX, i]);
            }
            try { P_StringYB = P_CharsYB.Aggregate((a, b) => a + b); } catch { P_StringYB = ""; }
            return P_StringYB.Replace("-", "");
        }
        public string AfterY()
        {
            int wordLength = Globals.UsersWord.Length;
            int P_Start = (Globals.UserY) + wordLength;
            for (int i = P_Start; i < 15; i++)
            {
                P_CharsYA.Add(Board[Globals.UserX, i]);
            }
            try { P_StringYA = P_CharsYA.Aggregate((a, b) => a + b); } catch { P_StringYA = ""; }
            P_CharsYA.Clear();
            return P_StringYA.Replace("-", "");
        }
        public bool P_Down()
        {
            pass_ = true;
            int wordLength = Globals.UsersWord.Length;
            //Calculates the contents of tiles Before Target location
            P_StringYB = BeforeY() + Globals.UsersWord;
            //------------------------------------------------------
            //Calculates the contents of tiles After the target location
            P_StringYA = Globals.UsersWord + AfterY();
            //------------------------------------------------------
            //Overlap checking
            bool Val_OverlapCheck = OverlapCheckY(Globals.UsersWord);
            //------------------------------------------------------
            //IsConnectected Checking
            bool Val_IsConnected = IsConnectedY(Globals.UsersWord);
            //------------------------------------------------------
            //PerpendicularCheck 
            bool Val_PerpCheck = PerpCheckY(Globals.UsersWord);
            //------------------------------------------------------
            //Letter after and before  ~~~~  +Edge check (Out of array Error Check)
            string LetterAfter = "";
            try { LetterAfter = Board[Globals.UserX, ((Globals.UserY) + wordLength) + 1]; }
            catch { LetterAfter = "-"; }
            string LetterBefore = "";
            try { LetterBefore = Board[Globals.UserX, Globals.UserY - 1]; }
            catch { LetterBefore = "-"; }
            //------------------------------------------------------
            if (Globals.UserY == 0 && LetterAfter == "-" && (Val_OverlapCheck) && (Val_IsConnected) && (Val_PerpCheck))
            {
                if (Words.WordCheck(Globals.UsersWord))
                {
                    for (int i = 0; i < wordLength; i++)
                    {
                        Board[Globals.UserX, i] = CtoString(Globals.UsersWord[i]);
                    }
                }
                //Console.WriteLine("Debug1"); D_stop();
            }//Along Y Coordinate - Letter after = "-" - OverlapCheck - PerpCheckY
            else if (Words.WordCheck(P_StringYB) && LetterAfter == "-" && (Val_OverlapCheck) && (Val_IsConnected) && (Val_PerpCheck))
            {
                if (Words.WordCheck(P_StringYB))
                {
                    for (int i = 0; i < wordLength; i++)
                    {
                        Board[Globals.UserX, i + Globals.UserY] = CtoString(Globals.UsersWord[i]);
                    }
                }
                //Console.WriteLine("Debug2"); D_stop();
            }//WordCheck(WordBefore+UserWord) - Letter after = "-" - OverlapCheck - PerpCheckY
            else if (Words.WordCheck(P_StringYA) && LetterBefore == "-" && (Val_OverlapCheck) && (Val_IsConnected) && (Val_PerpCheck))//WordCheck(UserWord+WordAfter) - Letter after = "-" - OverlapCheck
            {
                if (Words.WordCheck(P_StringYA))
                {
                    for (int i = 0; i < wordLength; i++)
                    {
                        Board[Globals.UserX, i + Globals.UserY] = CtoString(Globals.UsersWord[i]);
                    }
                }
                //Console.WriteLine("Debug3"); D_stop();
            }//WordCheck(UserWord+WordAfter) - Letter after = "-" - OverlapCheck - PerpCheckY
            else
            {
                Console.WriteLine("Final else, PlaceFail-Down");
                D_stop();
                pass_ = false;
            }
            if (Globals.DState == true) { WordCheckDebug(LetterAfter, LetterBefore, Val_OverlapCheck, Val_IsConnected, Val_PerpCheck); }
            TidyUp();
            return pass_;
        }
        static string CtoString(char value)
        {
            return new string(value, 1);
        }//This Function converts a single Char varible into a new string
        public void D_M_Conflicting()
        {
            WSB();
            Console.Write("Not a valid move, it conflicts with already placed tiles. Press enter to try again");
            Console.ReadLine();
            D_GetWordAndTarget();
        }// This Produces the error that the players move conflicts with placed tiles
        public void D_M_WordFail()
        {
            WSB();
            Console.Write("Placing those tiles here does not create a word. Press enter to try again");
            Console.ReadLine();
            D_GetWordAndTarget();
        }//This Produces an error that the word they tried to create does not exist
        public void TidyUp()
        {
            P_CharsYA.Clear();
            P_CharsYB.Clear();
            P_CharsXA.Clear();
            P_CharsXB.Clear();
            P_Overlap.Clear();
        }//Wipes all lists
        private void WordCheckDebug(string a, string b, bool _1, bool _2, bool _3)
        {
            WSB();
            Console.WriteLine("(YB)  Words.WordCheck({0}) = {1}", P_StringYB, Words.WordCheck(P_StringYB));
            Console.WriteLine("(YA)  Words.WordCheck({0}) = {1}", P_StringYA, Words.WordCheck(P_StringYA));
            Console.WriteLine("(LetterAfter == ' - ') = {0}", a);
            Console.WriteLine("(LetterBefore == ' - ') = {0}", b);
            Console.WriteLine("Val_OverlapCheck = {0} ", (_1));
            Console.WriteLine("Val_IsConnectedY = {0} ", (_2));
            Console.WriteLine("Val_PerpCheck) = {0}", (_3));
            Console.ReadLine();

            //Example (Pre filled in) -: WordCheckDebug(LetterAfter, LetterBefore, Val_Overlap, Val_IsConnectedY, Val_PerpCheck);

        }//Lists all the states of the wordchecks
        private bool OverlapCheckY(string Usersword)
        {
            for (int i = 0; i < Usersword.Length; i++)
            {
                P_Overlap.Add(Board[Globals.UserX, Globals.UserY + i]);
            }
            Overlapflags = 0;
            for (int i = 0; i < Usersword.Length; i++)
            {
                if ((P_Overlap[i] == "-") || (P_Overlap[i] == CtoString(Usersword[i]))) { }
                else { Overlapflags++; }
            }
            if (!(Overlapflags == 0)) { return false; }
            else { return true; }
        }//Checks whether pre placed tiles exist and do the fit with the users word on the Y Axis
        private bool OverlapCheckX(string Usersword)
        {
            for (int i = 0; i < Usersword.Length; i++)
            {
                P_Overlap.Add(Board[Globals.UserX + i, Globals.UserY]);
            }
            Overlapflags = 0;
            for (int i = 0; i < Usersword.Length; i++)
            {
                if ((P_Overlap[i] == "-") || (P_Overlap[i] == CtoString(Usersword[i]))) { }
                else { Overlapflags++; }
            }
            if (!(Overlapflags == 0)) { return false; }
            else { return true; }
        }//Checks whether pre placed tiles exist and do the fit with the users word on the X Axis
        private bool IsConnectedY(string Usersword)
        {
            string temp;
            string LetterAfter = "";
            try { LetterAfter = Board[Globals.UserX, ((Globals.UserY) + Usersword.Length) + 1]; } catch { LetterAfter = "-"; }

            bool ToTheRight = true;
            bool ToTheLeft = true;
            try { temp = Board[Globals.UserX + 1, Globals.UserY]; } catch { ToTheRight = false; }
            try { temp = Board[Globals.UserX - 1, Globals.UserY]; } catch { ToTheLeft = false; }

            string LetterBefore = "";
            try { LetterBefore = Board[Globals.UserX, Globals.UserY - 1]; } catch { LetterBefore = "-"; }


            for (int i = 0; i < Usersword.Length; i++)
            {
                P_Overlap.Add(Board[Globals.UserX, Globals.UserY + i]);
            }
            ConnectYFlags = 0;
            for (int i = 0; i < Usersword.Length; i++)
            {
                if (ToTheRight) { if (!((Board[Globals.UserX + 1, Globals.UserY + i]) == "-")) { ConnectYFlags++; } }
                if (ToTheLeft) { if (!((Board[Globals.UserX - 1, Globals.UserY + i]) == "-")) { ConnectYFlags++; } }
            }
            if (!(LetterBefore == "-")) { ConnectYFlags++; }
            if (!(LetterAfter == "-")) { ConnectYFlags++; }
            for (int i = 0; i < Usersword.Length; i++)
            {
                if (!(P_Overlap[i] == "-")) { ConnectYFlags++; }
            }
            if (!(ConnectYFlags == 0)) { return true; }
            else { return false; }
        }
        private bool IsConnectedX(string Usersword)
        {
            string temp;
            string LetterAfter = "";
            try { LetterAfter = Board[((Globals.UserX) + Usersword.Length) + 1, (Globals.UserY)]; } catch { LetterAfter = "-"; }

            bool ToTheRight = true;
            bool ToTheLeft = true;
            try { temp = Board[Globals.UserX, Globals.UserY + 1]; } catch { ToTheRight = false; }
            try { temp = Board[Globals.UserX, Globals.UserY - 1]; } catch { ToTheLeft = false; }

            string LetterBefore = "";
            try { LetterBefore = Board[Globals.UserX - 1, Globals.UserY]; } catch { LetterBefore = "-"; }


            for (int i = 0; i < Usersword.Length; i++)
            {
                P_Overlap.Add(Board[Globals.UserX + i, Globals.UserY]);
            }
            ConnectXFlags = 0;
            for (int i = 0; i < Usersword.Length; i++)
            {
                if (ToTheRight) { if (!((Board[Globals.UserX + i, Globals.UserY + 1]) == "-")) { ConnectXFlags++; } }
                if (ToTheLeft) { if (!((Board[Globals.UserX + i, Globals.UserY - 1]) == "-")) { ConnectXFlags++; } }
            }
            if (!(LetterBefore == "-")) { ConnectXFlags++; }
            if (!(LetterAfter == "-")) { ConnectXFlags++; }
            for (int i = 0; i < Usersword.Length; i++)
            {
                if (!(P_Overlap[i] == "-")) { ConnectXFlags++; }
            }
            if (!(ConnectXFlags == 0)) { return true; }
            else { return false; }
        }
        private bool PerpCheckY(string Userword)
        {
            int WordLength = Userword.Length;
            int PerpFlags = 0;
            for (int i = 0; i < WordLength; i++)
            {
                string Val_Left = "-"; string Val_Right = "-";
                string TileCollection;
                int Difference = 0;
                string tempChar;
                List<string> Pc_Left = new List<string>(); string LeftString;
                List<string> Pc_Right = new List<string>(); string RightString;
                bool ToTheRight = true;
                bool ToTheLeft = true;
                try { Val_Right = Board[Globals.UserX + 1, Globals.UserY + i]; } catch { ToTheRight = false; }
                try { Val_Left = Board[Globals.UserX - 1, Globals.UserY + i]; } catch { ToTheLeft = false; }

                if (!(Val_Left == "-") && ToTheLeft)//Gets Word to the left
                {
                    for (int j = Globals.UserX - 1; j > 0 && j < Globals.UserX; j--)
                    {
                        Difference = (Globals.UserX - j);
                        tempChar = Board[Globals.UserX - Difference, Globals.UserY + i];
                        if (tempChar == "-") { j = 99; }
                        else { Pc_Left.Add(Board[Globals.UserX - Difference, Globals.UserY + i]); }
                    }
                }
                Pc_Left.Reverse();
                try { LeftString = Pc_Left.Aggregate((a, b) => a + b); } catch { LeftString = ""; }

                if (!(Val_Right == "-") && ToTheRight)//Gets Word to the right
                {
                    for (int j = Globals.UserX + 1; j < 20 && j > Globals.UserX; j++)
                    {
                        Difference = (j - Globals.UserX);
                        tempChar = Board[Globals.UserX + Difference, Globals.UserY + i];
                        if (tempChar == "-") { j = 99; }
                        else { Pc_Right.Add(Board[Globals.UserX + Difference, Globals.UserY + i]); }
                    }
                }
                try { RightString = Pc_Right.Aggregate((a, b) => a + b); } catch { RightString = ""; }

                TileCollection = (LeftString + CtoString(Userword[i]) + RightString);
                if ((LeftString == "") && (RightString == "")) { }
                else
                {
                    if (Words.WordCheck(TileCollection)) { }
                    else { PerpFlags++; }
                }
            }
            if (!(PerpFlags == 0)) { return false; }
            else { return true; }
        }
        private bool PerpCheckX(string Userword)
        {
            int WordLength = Userword.Length;
            int PerpFlags = 0;
            for (int i = 0; i < WordLength; i++)
            {
                string Val_Above = "-"; string Val_Below = "-";
                string TileCollection;
                int Difference = 0;
                string tempChar;
                List<string> Pc_Above = new List<string>(); string AboveString;
                List<string> Pc_Below = new List<string>(); string BelowString;
                bool ToTheBelow = true;
                bool ToTheAbove = true;
                try { Val_Below = Board[Globals.UserX + i, Globals.UserY + 1]; } catch { ToTheBelow = false; }
                try { Val_Above = Board[Globals.UserX + i, Globals.UserY - 1]; } catch { ToTheAbove = false; }

                if (!(Val_Above == "-") && ToTheAbove)//Gets Word to the Above
                {
                    for (int j = Globals.UserY - 1; j > 0 && j < Globals.UserY; j--)
                    {
                        Difference = (Globals.UserY - j);
                        tempChar = Board[Globals.UserX + i, Globals.UserY - Difference];
                        if (tempChar == "-") { j = 99; }
                        else { Pc_Above.Add(Board[Globals.UserX + i, Globals.UserY - Difference]); }
                    }
                }
                Pc_Above.Reverse();
                try { AboveString = Pc_Above.Aggregate((a, b) => a + b); } catch { AboveString = ""; }

                if (!(Val_Below == "-") && ToTheBelow)//Gets Word to the Below
                {
                    for (int j = Globals.UserY + 1; j < 20 && j > Globals.UserY; j++)
                    {
                        Difference = (j - Globals.UserY);

                        WSB();

                        tempChar = Board[Globals.UserX + i, Globals.UserY + Difference];
                        if (tempChar == "-") { j = 99; }
                        else { Pc_Below.Add(Board[Globals.UserX + i, Globals.UserY + Difference]); }
                    }
                }
                try { BelowString = Pc_Below.Aggregate((a, b) => a + b); } catch { BelowString = ""; }

                TileCollection = (AboveString + CtoString(Userword[i]) + BelowString);
                if ((AboveString == "") && (BelowString == "")) { }
                else
                {
                    if (Words.WordCheck(TileCollection)) { }
                    else { PerpFlags++; }
                }
            }
            if (!(PerpFlags == 0)) { return false; }
            else { return true; }
        }
    }//Handles all Game Logic
    class GameHandler//Handles all interactions between user and game engine  
    {
        //Variables----------------
        private int Menustate = 1;
        private int ActivePlayerID = 1;
        public static string Player1N = "";//Player 1 Name
        public static string Player2N = "";//Player 2 Name
        public static Dictionary<int, int> PlayerScoreData = new Dictionary<int, int>();
        public static  string Direction = "";
        private bool pass = false;

        public static List<string> P_CharsXB = new List<string>();//Used to store letters that are already on the board on the X Axis Before the users Target
        public static string P_StringXB;//This the Words/Characters already placed on the X coordinate before the users target
        public static List<string> P_CharsXA = new List<string>();//Used to store letters that are already on the board on the X Axis After the users Target
        public static string P_StringXA;//This the Words/Characters already placed on the X coordinate After the users target
        public static List<string> P_CharsYB = new List<string>();//Used to store letters that are already on the board on the Y Axis Before the users Target
        public static string P_StringYB;//This the Words/Characters already placed on the Y coordinate before the users target
        public static List<string> P_CharsYA = new List<string>();//Used to store letters that are already on the board on the Y Axis After the users Target
        public static string P_StringYA;//This the Words/Characters already placed on the Y coordinate After the users target

        //-------------------------

        public GameHandler()
        {
            Globals globals = new Globals();
            GameEngine game = new GameEngine();
            GameOpen(game);
            MenuScreen(game);
        }
        public void GameOpen(GameEngine game)
        {
            StartScreen();
            //---------DEBUG ZONE-------------

            //Console.Clear();
            //CoopGame(game);

            //D_EngineBasic(game);

            //---------DEBUG ZONE-------------

            ConsoleKeyInfo cki;
            while (true)
            {
                cki = Console.ReadKey();
                if (cki.Key == ConsoleKey.Spacebar)
                {
                    break;
                }
                else if ((cki.Modifiers & ConsoleModifiers.Alt) != 0 & cki.Key == ConsoleKey.E && Globals.DState == true)
                {
                    D_EngineBasic(game);
                }
                else if ((cki.Modifiers & ConsoleModifiers.Alt) != 0 & cki.Key == ConsoleKey.D && Globals.DState == false)
                {
                    Globals.DState = true;
                    StartScreen();
                }
                else if ((cki.Modifiers & ConsoleModifiers.Alt) != 0 & cki.Key == ConsoleKey.D && Globals.DState == true)
                {
                    Globals.DState = false;
                    StartScreen();
                }
                else
                {
                    StartScreen();
                }
            }
            Console.Clear();
        }//Handles The start menu control logic
        private void StartScreen()
        {
            Console.Clear();
            Console.WriteLine("");
            Console.WriteLine(@"                      ______                                 __        __        __ ");
            Console.WriteLine(@"                     /      \                               |  \      |  \      |  \          ");
            Console.WriteLine(@"                    |  $$$$$$\  _______   ______    ______  | $$____  | $$____  | $$  ______  ");
            Console.WriteLine(@"                    | $$___\$$ /       \ /      \  |      \ | $$    \ | $$    \ | $$ /      \ ");
            Console.WriteLine(@"                     \$$    \ |  $$$$$$$|  $$$$$$\  \$$$$$$\| $$$$$$$\| $$$$$$$\| $$|  $$$$$$\");
            Console.WriteLine(@"                     _\$$$$$$\| $$      | $$   \$$ /      $$| $$  | $$| $$  | $$| $$| $$    $$");
            Console.WriteLine(@"                    |  \__| $$| $$_____ | $$      |  $$$$$$$| $$__/ $$| $$__/ $$| $$| $$$$$$$$");
            Console.WriteLine(@"                     \$$    $$ \$$     \| $$       \$$    $$| $$    $$| $$    $$| $$ \$$     \");
            Console.WriteLine(@"                      \$$$$$$   \$$$$$$$ \$$        \$$$$$$$ \$$$$$$$  \$$$$$$$  \$$  \$$$$$$$");
            Console.WriteLine("");
            if (Globals.DState == true)
            {
                Console.WriteLine("                                                  DEBUG IS ACTIVE");
            }
            else { Console.WriteLine(""); }
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.Write("                                              Press Space To Continue ");
        }//Prints out the Start screen logo
        private void D_stop()//Debug tool to pause the Program
        {
            Console.WriteLine("D_stop.....");
            Console.ReadLine();
        }
        private void Screen1()
        {
            Console.Clear();
            Console.WriteLine("");
            Console.WriteLine(@"                      ______                                 __        __        __ ");
            Console.WriteLine(@"                     /      \                               |  \      |  \      |  \          ");
            Console.WriteLine(@"                    |  $$$$$$\  _______   ______    ______  | $$____  | $$____  | $$  ______  ");
            Console.WriteLine(@"                    | $$___\$$ /       \ /      \  |      \ | $$    \ | $$    \ | $$ /      \ ");
            Console.WriteLine(@"                     \$$    \ |  $$$$$$$|  $$$$$$\  \$$$$$$\| $$$$$$$\| $$$$$$$\| $$|  $$$$$$\");
            Console.WriteLine(@"                     _\$$$$$$\| $$      | $$   \$$ /      $$| $$  | $$| $$  | $$| $$| $$    $$");
            Console.WriteLine(@"                    |  \__| $$| $$_____ | $$      |  $$$$$$$| $$__/ $$| $$__/ $$| $$| $$$$$$$$");
            Console.WriteLine(@"                     \$$    $$ \$$     \| $$       \$$    $$| $$    $$| $$    $$| $$ \$$     \");
            Console.WriteLine(@"                      \$$$$$$   \$$$$$$$ \$$        \$$$$$$$ \$$$$$$$  \$$$$$$$  \$$  \$$$$$$$");
            Console.WriteLine("");
            Console.WriteLine("                                                                                          Use Arrow Keys to Navigate!");
            if (Menustate == 1)
            {
                Console.WriteLine("                                                  >-SINGLEPLAYER");
            }
            else { Console.WriteLine("                                                  -Singleplayer"); }
            Console.WriteLine("                                                  -Multiplayer");
            Console.WriteLine("                                                  -Exit");
            Console.WriteLine("");
            Console.WriteLine("");
        }
        private void Screen2()
        {
            Console.Clear();
            Console.WriteLine("");
            Console.WriteLine(@"                      ______                                 __        __        __ ");
            Console.WriteLine(@"                     /      \                               |  \      |  \      |  \          ");
            Console.WriteLine(@"                    |  $$$$$$\  _______   ______    ______  | $$____  | $$____  | $$  ______  ");
            Console.WriteLine(@"                    | $$___\$$ /       \ /      \  |      \ | $$    \ | $$    \ | $$ /      \ ");
            Console.WriteLine(@"                     \$$    \ |  $$$$$$$|  $$$$$$\  \$$$$$$\| $$$$$$$\| $$$$$$$\| $$|  $$$$$$\");
            Console.WriteLine(@"                     _\$$$$$$\| $$      | $$   \$$ /      $$| $$  | $$| $$  | $$| $$| $$    $$");
            Console.WriteLine(@"                    |  \__| $$| $$_____ | $$      |  $$$$$$$| $$__/ $$| $$__/ $$| $$| $$$$$$$$");
            Console.WriteLine(@"                     \$$    $$ \$$     \| $$       \$$    $$| $$    $$| $$    $$| $$ \$$     \");
            Console.WriteLine(@"                      \$$$$$$   \$$$$$$$ \$$        \$$$$$$$ \$$$$$$$  \$$$$$$$  \$$  \$$$$$$$");
            Console.WriteLine("");
            Console.WriteLine("                                                                                          Use Arrow Keys to Navigate!");
            Console.WriteLine("                                                  -Singleplayer");
            if (Menustate == 2)
            {
                Console.WriteLine("                                                  >-MULTIPLAYER");
            }
            else { Console.WriteLine("                                                  -Multiplayer"); }
            Console.WriteLine("                                                  -Exit");
            Console.WriteLine("");
            Console.WriteLine("");
        }
        private void Screen3()
        {
            Console.Clear();
            Console.WriteLine("");
            Console.WriteLine(@"                      ______                                 __        __        __ ");
            Console.WriteLine(@"                     /      \                               |  \      |  \      |  \          ");
            Console.WriteLine(@"                    |  $$$$$$\  _______   ______    ______  | $$____  | $$____  | $$  ______  ");
            Console.WriteLine(@"                    | $$___\$$ /       \ /      \  |      \ | $$    \ | $$    \ | $$ /      \ ");
            Console.WriteLine(@"                     \$$    \ |  $$$$$$$|  $$$$$$\  \$$$$$$\| $$$$$$$\| $$$$$$$\| $$|  $$$$$$\");
            Console.WriteLine(@"                     _\$$$$$$\| $$      | $$   \$$ /      $$| $$  | $$| $$  | $$| $$| $$    $$");
            Console.WriteLine(@"                    |  \__| $$| $$_____ | $$      |  $$$$$$$| $$__/ $$| $$__/ $$| $$| $$$$$$$$");
            Console.WriteLine(@"                     \$$    $$ \$$     \| $$       \$$    $$| $$    $$| $$    $$| $$ \$$     \");
            Console.WriteLine(@"                      \$$$$$$   \$$$$$$$ \$$        \$$$$$$$ \$$$$$$$  \$$$$$$$  \$$  \$$$$$$$");
            Console.WriteLine("");
            Console.WriteLine("                                                                                          Use Arrow Keys to Navigate!");
            Console.WriteLine("                                                  -Singleplayer");
            Console.WriteLine("                                                  -Multiplayer");
            if (Menustate == 3)
            {
                Console.WriteLine("                                                  >-EXIT");
            }
            else { Console.WriteLine("                                                  -exit"); }
            Console.WriteLine("");
            Console.WriteLine("");
        }
        private void MenuScreen(GameEngine game)
        {
            Screen1();
            ConsoleKeyInfo cki;
            while (true)
            {
                cki = Console.ReadKey();

                //Screen1-----------------------------------
                if (cki.Key == ConsoleKey.DownArrow && Menustate == 1)
                {
                    Menustate = 2;
                    Screen2();
                }
                else if (cki.Key == ConsoleKey.UpArrow && Menustate == 1)
                {
                    Menustate = 3;
                    Screen3();
                }


                //Screen2-----------------------------------
                else if (cki.Key == ConsoleKey.DownArrow && Menustate == 2)
                {
                    Menustate = 3;
                    Screen3();
                }
                else if (cki.Key == ConsoleKey.UpArrow && Menustate == 2)
                {
                    Menustate = 1;
                    Screen1();
                }

                //Screen3-----------------------------------
                else if (cki.Key == ConsoleKey.DownArrow && Menustate == 3)
                {
                    Menustate = 1;
                    Screen1();
                }
                else if (cki.Key == ConsoleKey.UpArrow && Menustate == 3)
                {
                    Menustate = 2;
                    Screen2();
                }

                //Hits Enter and catch
                else if (cki.Key == ConsoleKey.Enter)
                {
                    if (Menustate == 1) { SinglePlayerGame(game); }
                    else if (Menustate == 2) { CoopGame(game); }
                    else if (Menustate == 3) { Environment.Exit(0); }
                }
                else if (Menustate == 1)
                {
                    Screen1();
                }
                else if (Menustate == 2)
                {
                    Screen2();
                }
                else if (Menustate == 3)
                {
                    Screen3();
                }
            }
        }
        private void ShowBoard()
        {
            Console.Clear();
            Console.WriteLine("                           |-A-|-B-|-C-|-D-|-E-|-F-|-G-|-H-|-I-|-J-|-K-|-L-|-M-|-N-|-O-|      Player1:{0}|Player2:{1}", PlayerScoreData[1], PlayerScoreData[2]);//Displays the header above the board ----    |-A-|-B-|-C-|-D-|-E-|-F-|-G-|-H-|-I-|-J-|-K-|-L-|-M-|-N-|-O-|"
            for (int j = 0; j < GameEngine.COL; j++)//This is to list each letter in the rows 15 times for each column
            {
                if (j < 9)
                {
                    Console.Write("                       - {0}-| ", (j + 1));
                }//these are to make sure the spacing is correct so the board is neat
                else
                {
                    Console.Write("                       -{0}-| ", (j + 1));
                }//^^^^^^^^^^^^^^^^
                for (int i = 0; i < GameEngine.ROW; i++)//this Lists the value on the array board as I increases
                {
                    Console.Write(GameEngine.Board[i, j]);
                    if (i < GameEngine.ROW)
                    {
                        Console.Write(" | ");//this is to seperate each letter for neatness
                    }
                }
                Console.WriteLine("");
            }
            Console.WriteLine("");
            Console.WriteLine("");
        }
        private void ScreenTextGap()
        {
            Console.Clear();
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
        }
        private void SinglePlayerGame(GameEngine game)
        {
            Console.Clear();
            Console.WriteLine("Debug State = " + Globals.DState);
            Console.WriteLine("Errr, Well this awkward. This isn't Implemented yet, Hit enter to return to the Main menu");
            if (Console.ReadLine() == "D_EngineBasic")
            {
                D_EngineBasic(game);
            }
            MenuScreen(game);
        }
        private void CoopGame(GameEngine game)
        {
            //Game Config
            bool GameActive = true;
            ScreenTextGap();
            Console.Write("                                                  Player 1's Name : ");
            Player1N = Console.ReadLine();
            Console.Write("                                                  Player 2's Name : ");
            Player2N = Console.ReadLine();
            PlayerScoreData[1] = 0; PlayerScoreData[2] = 0;
            //Game Start
            game.SetupPlayer1Hand();
            game.SetupPlayer2Hand();

            Console.Clear();
            ScreenTextGap();
            Console.WriteLine("                                           {0}'s Turn, {1} Look away now", Player1N, Player2N);
            Console.WriteLine("                                   {0} Must look away so they don't see {1}'s Tiles", Player2N, Player1N);
            Console.WriteLine("                                     Do not look at eatch others tiles during rounds");
            Console.WriteLine("                                               Press enter to continue");
            Console.ReadLine();
            ShowBoard();
            Console.WriteLine("Player 1 ({0}) goes first!", Player1N);
            GetUserInputCoopAndPlace(ActivePlayerID, game);
            if (pass = true)
            {
                PlayerScoreData[1] = +CalculateCollectScore(Globals.UsersWord, Direction);
                ShowBoard();
                ActivePlayerID = 2;
            }
            else { Console.WriteLine("Invalid Move -D_FAIL IN P_DOWN/ACROSS - this is CoopGame"); Console.ReadLine(); GetUserInputCoopAndPlace(ActivePlayerID, game); }
            while (GameActive)
            {
                Console.WriteLine("Player {0} ({1}) turn!", ActivePlayerID, ReturnNameUseID(ActivePlayerID));
                if (pass = true)
                {
                    GetUserInputCoopAndPlace(ActivePlayerID, game);
                    PlayerScoreData[ActivePlayerID] = +CalculateCollectScore(Globals.UsersWord, Direction);
                    ShowBoard();
                    if (ActivePlayerID == 1) { ActivePlayerID = 2; }
                    else { ActivePlayerID = 1; }
                }
                else { Console.WriteLine("Invalid Move -D_FAIL IN P_DOWN/ACROSS - this is CoopGame"); Console.ReadLine(); GetUserInputCoopAndPlace(ActivePlayerID, game); }
            }//After the first move has been made

        }
        private string ReturnNameUseID(int ActivePlayerID)
        {
            if (ActivePlayerID == 1) { return Player1N; }
            else { return Player2N; }
            
        }
        private void GetUserInputCoopAndPlace(int ActivePlayer, GameEngine game)
        {
            pass = false;
            string UsersTargetInput;
            if (ActivePlayerID == 1) { Console.Write("Your Tiles: "); ListHand(GameEngine.Player1Hand, game); Console.WriteLine(); }
            else { Console.Write("Your Tiles: "); ListHand(GameEngine.Player2Hand, game); Console.WriteLine(); }
            Console.Write("Your word: ");
            string UsersWordInput = (Console.ReadLine()).ToUpper(); Console.WriteLine();
            Globals.UsersWord = UsersWordInput;
            ShowBoard();
            Console.WriteLine("You Word is '{0}'", UsersWordInput);
            Console.Write("Word Start Location: "); UsersTargetInput = Console.ReadLine().ToUpper(); Console.WriteLine();
            //-----Add Input check-----
            ConvertTargetModGlobals(UsersTargetInput);
            ShowBoard();
            Console.Write("Which direction H/V: "); string UserDirection = Console.ReadLine().ToUpper(); Console.WriteLine();
            //-----Add Direction check-----
            Char direction = 'X';
            switch (UserDirection)
            {
                case "V":
                    direction = 'V';
                    Direction = "V";
                    break;
                case "H":
                    direction = 'H';
                    Direction = "H";
                    break;
                default:
                    Console.WriteLine("INCORRECT ENTRY");
                    D_stop();
                    GetUserInputCoopAndPlace(ActivePlayer, game);
                    break;
            }
            if (LengthCheck(direction))
            {
                switch (UserDirection)
                {
                    case "V":
                        pass = game.P_Down();
                        break;
                    case "H":
                        pass = game.P_Across();
                        break;
                }
            }//Runs the appropriate Placing Algorithm from the Engine
            else { ShowBoard(); GetUserInputCoopAndPlace(ActivePlayer, game); }
        }
        private void ConvertTargetModGlobals(string UsersTargetInput)
        {
            switch (UsersTargetInput[0])
            {
                case 'A':
                    Globals.UserX = 0;
                    break;
                case 'B':
                    Globals.UserX = 1;
                    break;
                case 'C':
                    Globals.UserX = 2;
                    break;
                case 'D':
                    Globals.UserX = 3;
                    break;
                case 'E':
                    Globals.UserX = 4;
                    break;
                case 'F':
                    Globals.UserX = 5;
                    break;
                case 'G':
                    Globals.UserX = 6;
                    break;
                case 'H':
                    Globals.UserX = 7;
                    break;
                case 'I':
                    Globals.UserX = 8;
                    break;
                case 'J':
                    Globals.UserX = 9;
                    break;
                case 'K':
                    Globals.UserX = 10;
                    break;
                case 'L':
                    Globals.UserX = 11;
                    break;
                case 'M':
                    Globals.UserX = 12;
                    break;
                case 'N':
                    Globals.UserX = 13;
                    break;
                case 'O':
                    Globals.UserX = 14;
                    break;
            }
            string[] values = Regex.Split(UsersTargetInput, @"\D+");
            Globals.UserY = int.Parse(values[1]) - 1;
        }
        private bool LengthCheck(char direction)
        {
            int WordLength = Globals.UsersWord.Length;
            switch (direction)
            {
                case 'V':
                    if (WordLength + Globals.UserY - 1 > 14)
                    {
                        ShowBoard();
                        Console.Write("Word would go off of board edge, Press enter to try again ");
                        Console.Read();
                        return false;
                    }
                    else { return true; }
                case 'H':
                    if (WordLength + Globals.UserX - 1 > 14)
                    {
                        ShowBoard();
                        Console.Write("Word would go off of board edge, Press enter to try again ");
                        Console.Read();
                        return false;
                    }
                    else { return true; }
                default:
                    Console.WriteLine("LengthCheck ERROR");
                    D_stop();
                    return true;
            }
        }// This checks if the users words is too long and would run of the side of the board
        private void ListHand(List<string> List, GameEngine game)
        {
            List.ForEach(i => Console.Write("{0} ", i));//Lists all letters from the AI's hand
        }
        private void D_EngineBasic(GameEngine game)
        {
            while (true)
            {
                game.D_GetWordAndTarget();
                game.WSB();
                Console.WriteLine("DEBUG_ENGINEREPLAY");
                if (Console.ReadLine() == "LEAVE")
                {
                    break;
                }
                game.WSB();
            }
            Console.Clear();
            Console.WriteLine("DEBUG_REBOOT ");
            Console.ReadLine();
            GameOpen(game);
        }
        private int CalculateWordScore(string UserWord)
        {
            int ScoreCalc = 0;
            for (int i = 0; i < UserWord.Length; i++)
            {
                ScoreCalc = ScoreCalc + GameEngine.LetterScores[UserWord[i]];
            }
            return ScoreCalc;
        }
        private int CalculateCollectScore(string Userword, string Direction)
        {
            int CollectScore = 0;
            switch (Direction)
            {
                case "H":
                    CollectScore =+ CalculateWordScore(BeforeX(Globals.UserX, Globals.UserY) + Userword + AfterX(Globals.UserX, Globals.UserY, Userword));
                    break;
                case "V":
                    CollectScore =+ CalculateWordScore(BeforeY(Globals.UserX, Globals.UserY) + Userword + AfterY(Globals.UserX, Globals.UserY, Userword));
                    break;
            }
            return CollectScore;
        }
        public string BeforeX(int x, int y)
        {
            int XDifference = x;
            for (int i = 0; i < XDifference; i++)//----------------------------------------
            {
                P_CharsXB.Add(GameEngine.Board[i, y]);
            }
            try { P_StringXB = P_CharsXB.Aggregate((a, b) => a + b); } catch { P_StringXB = ""; }
            return P_StringXB.Replace("-", "");
        }
        public string AfterX(int x, int y, string UserInput)
        {
            int wordLength = UserInput.Length;
            int P_Start = (x) + wordLength;
            for (int i = P_Start; i < 15; i++)
            {
                P_CharsXA.Add(GameEngine.Board[i, y]);
            }
            try { P_StringXA = P_CharsXA.Aggregate((a, b) => a + b); } catch { P_StringXA = ""; }
            P_CharsXA.Clear();
            return P_StringXA.Replace("-", "");
        }
        public string BeforeY(int x, int y)
        {
            int YDifference = y;
            for (int i = 0; i < YDifference; i++)//----------------------------------------
            {
                P_CharsYB.Add(GameEngine.Board[x, i]);
            }
            try { P_StringYB = P_CharsYB.Aggregate((a, b) => a + b); } catch { P_StringYB = ""; }
            return P_StringYB.Replace("-", "");
        }
        public string AfterY(int x, int y, string UserInput)
        {
            int wordLength = UserInput.Length;
            int P_Start = (y) + wordLength;
            for (int i = P_Start; i < 15; i++)
            {
                P_CharsYA.Add(GameEngine.Board[x, i]);
            }
            try { P_StringYA = P_CharsYA.Aggregate((a, b) => a + b); } catch { P_StringYA = ""; }
            P_CharsYA.Clear();
            return P_StringYA.Replace("-", "");
        }
    }
}//--LEFT OfF NOTES
//WORKING ON PASS SYSTEM TO STOP POINTS ADDING EVEN IF P_DOWN/ACROSS FAIL WITHIN THE ENGINE, OTHERWISE CHANGES ACTIVEPLAYERID AND CHEAT SCORE
