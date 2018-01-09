using System;
using System.Windows.Forms;
using ChatServer.Utilities;
using ChatServer.Server;


namespace ChatServer
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private Server.ChatService _chatService;

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            try
            {
                _chatService = ChatService.Instance;
                SubscribeToEvents();
                _chatService.StartServer(int.Parse(tbPort.Text));

                tbIP.Text = Utility.GetLocalIP().ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message);
                lbServerStatus.Text = "ERROR";
            }
        }

        private void SubscribeToEvents()
        {
            _chatService.ServerStatusChanged += OnChatServiceStatusChanged;
        }

        /// <summary>
        /// Chat server status changed event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnChatServiceStatusChanged(object sender, EventArgs eventArgs)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action) (() =>
                {
                    lbServerStatus.Text = eventArgs.ToString();
                }));
            }
            else
            {
                lbServerStatus.Text = eventArgs.ToString();
            }
        }
    }
}
