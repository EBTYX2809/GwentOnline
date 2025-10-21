using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Gwent_Release.Models.CardsNS;

namespace Gwent_Release.Models
{
    public class Client
    {
        private TcpClient _client;        
        private NetworkStream _stream;

        public void Connect(string serverIp, int port)
        {
            _client = new TcpClient();
            try
            {
                _client.Connect(serverIp, port);
            }            
            catch(SocketException ex)
            {
                MessageBox.Show("Server is offline.");
                return;
            }

            MessageBox.Show("Connected to server.");

            _stream = _client.GetStream();            
        }

        public void SendInfo(string info) 
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(info);
                if (_stream != null) _stream.Write(data, 0, data.Length);
                else return;
            }
            catch (Exception ex) { }
        }

        public async Task<string> ReceiveInfo()
        {
            string info = null;
            var buffer = new byte[1024];

            try
            {
                int bytesRead = 0;
                if (_stream != null) bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                else return null;

                info = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            }
            catch(Exception ex)  
            {
                MessageBox.Show($"Error: {ex.Message}");                                
            }

            if (info == "Disconnect")
            {
                MessageBox.Show("Oponent disconnected.");
                GameContext.Instance.ReturnToMenuWindow(this);
            }

            return info;
        }

        public async Task SwapDeck(List<Card> cards)
        {
            string myDeck = $"{GameContext.Instance.Player1.Leader.JsonNameKey}|" + string.Join("|", cards.Select(card => card.JsonNameKey));

            SendInfo(myDeck);

            string enemyDeck = await ReceiveInfo();

            GameContext.Instance.Player2.CreateDeck(enemyDeck);            
        }

        public async Task TossCoin()
        {
            SendInfo("Toss coin.");

            string info = await ReceiveInfo();

            if(info == "active")
            {
                GameContext.Instance.ActivePlayer = GameContext.Instance.Player1;
                GameContext.Instance.StarterPlayer = GameContext.Instance.Player1;
                GameContext.Instance.PassivePlayer = GameContext.Instance.Player2;
                GameContext.Instance.IsPlayer1Turn = true;
            }
            else if(info == "passive")
            {
                GameContext.Instance.ActivePlayer = GameContext.Instance.Player2;
                GameContext.Instance.StarterPlayer = GameContext.Instance.Player2;
                GameContext.Instance.PassivePlayer = GameContext.Instance.Player1;
                GameContext.Instance.IsPlayer1Turn = false;
            }
        }

        public async Task<bool> WaitingSecondPlayer()
        {
            SendInfo($"Ready|{GameContext.Instance.Player1.Name}");

            string info = await ReceiveInfo() ?? "";
            
            if(info.Contains("Ready"))
            {                    
                GameContext.Instance.Player2.Name = info.Replace("Ready|", "");
                return true;
            }  
            else if (info == "Timeout")
            {
                MessageBox.Show("The session search time has ended. The enemy player was not found.");
                return false;
            }

            return false;
        }
    }
}
