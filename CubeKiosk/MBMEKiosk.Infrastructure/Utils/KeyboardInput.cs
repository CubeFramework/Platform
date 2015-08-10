using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace MBMEKiosk.Infrastructure.Utils
{
    public class KeyboardInput
    {

        public static char InterpretInput(System.Windows.Input.KeyEventArgs e)
        {
            char keyValue = (char)0;

            if(Keyboard.IsKeyDown(Key.Tab) && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) )
                keyValue = (char)4;
            if (Keyboard.IsKeyDown(Key.D1) && (Keyboard.IsKeyDown(Key.RightShift) || Keyboard.IsKeyDown(Key.LeftShift)))
                keyValue = '!';
            else if (Keyboard.IsKeyDown(Key.D2) && (Keyboard.IsKeyDown(Key.RightShift) || Keyboard.IsKeyDown(Key.LeftShift)))
                keyValue = '@';
            else if (Keyboard.IsKeyDown(Key.D3) && (Keyboard.IsKeyDown(Key.RightShift) || Keyboard.IsKeyDown(Key.LeftShift)))
                keyValue = '#';
            else if (Keyboard.IsKeyDown(Key.D4) && (Keyboard.IsKeyDown(Key.RightShift) || Keyboard.IsKeyDown(Key.LeftShift)))
                keyValue = '$';
            else if (Keyboard.IsKeyDown(Key.D5) && (Keyboard.IsKeyDown(Key.RightShift) || Keyboard.IsKeyDown(Key.LeftShift)))
                keyValue = '%';
            else if (Keyboard.IsKeyDown(Key.D6) && (Keyboard.IsKeyDown(Key.RightShift) || Keyboard.IsKeyDown(Key.LeftShift)))
                keyValue = '^';
            else if (Keyboard.IsKeyDown(Key.D7) && (Keyboard.IsKeyDown(Key.RightShift) || Keyboard.IsKeyDown(Key.LeftShift)))
                keyValue = '&';
            else if (Keyboard.IsKeyDown(Key.D8) && (Keyboard.IsKeyDown(Key.RightShift) || Keyboard.IsKeyDown(Key.LeftShift)))
                keyValue = '*';
            else if (Keyboard.IsKeyDown(Key.D9) && (Keyboard.IsKeyDown(Key.RightShift) || Keyboard.IsKeyDown(Key.LeftShift)))
                keyValue = '(';
            else if (Keyboard.IsKeyDown(Key.D0) && (Keyboard.IsKeyDown(Key.RightShift) || Keyboard.IsKeyDown(Key.LeftShift)))
                keyValue = ')';
            else 
            {
                switch(e.Key)
                {
                    case Key.D0:
                        keyValue = '0';
                    break;
                    case Key.D1:
                        keyValue = '1';
                    break;
                    case Key.D2:
                        keyValue = '2';
                    break;
                    case Key.D3:
                        keyValue = '3';
                    break;
                    case Key.D4:
                        keyValue = '4';
                    break;
                    case Key.D5:
                        keyValue = '5';
                    break;
                    case Key.D6:
                        keyValue = '6';
                    break;
                    case Key.D7:
                        keyValue = '7';
                    break;
                    case Key.D8:
                        keyValue = '8';
                    break;
                    case Key.D9:
                        keyValue = '9';
                    break;
                    case Key.A:
                        keyValue = 'A';
                    break;
                    case Key.B:
                       keyValue = 'B';
                    break;
                    case Key.C:
                       keyValue = 'C';
                    break;
                    case Key.D:
                       keyValue = 'D';
                    break;
                    case Key.E:
                       keyValue = 'E';
                    break;
                    case Key.F:
                       keyValue = 'F';
                    break;
                    case Key.G:
                       keyValue = 'G';
                    break;
                    case Key.H:
                       keyValue = 'H';
                    break;
                    case Key.I:
                       keyValue = 'I';
                    break;
                    case Key.J:
                       keyValue = 'J';
                    break;
                    case Key.K:
                       keyValue = 'K';
                    break;
                    case Key.L:
                       keyValue = 'L';
                    break;
                    case Key.M:
                       keyValue = 'M';
                    break;
                    case Key.N:
                       keyValue = 'N';
                    break;
                    case Key.O:
                       keyValue = 'O';
                    break;
                    case Key.P:
                       keyValue = 'P';
                    break;
                    case Key.Q:
                       keyValue = 'Q';
                    break;
                    case Key.R:
                       keyValue = 'R';
                    break;
                    case Key.S:
                       keyValue = 'S';
                    break;
                    case Key.T:
                       keyValue = 'T';
                    break;
                    case Key.U:
                       keyValue = 'U';
                    break;
                    case Key.V:
                       keyValue = 'V';
                    break;
                    case Key.W:
                       keyValue = 'W';
                    break;
                    case Key.X:
                       keyValue = 'X';
                    break;
                    case Key.Y:
                       keyValue = 'Y';
                    break;
                    case Key.Z:
                       keyValue = 'Z';
                    break;
                    case Key.Back:
                        keyValue = (char)1;
                    break;
                    case Key.Delete:
                        keyValue = (char)2;
                    break;
                    case Key.Tab:
                    keyValue = (char)3;
                    break;
                    case Key.Space:
                    keyValue = ' ';
                    break;
                }
            }
            return keyValue;
        }
    }
}
